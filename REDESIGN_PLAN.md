# Implementation Plan — MaxwellCalc Notebook Sheet UI

This plan turns the design in [`REDESIGN_UI.md`](REDESIGN_UI.md) into a concrete, step-by-step
implementation. Read `REDESIGN_UI.md` first for the *what* and *why*; this document is the *how*
and the *in what order*.

## Guiding decisions

1. **New UI project, Core untouched.** We create a brand-new Avalonia app project
   (`MaxwellCalc.Notebook`) that references the existing `MaxwellCalc.Core` unchanged. The current
   `MaxwellCalc` UI project is left intact as a working fallback/reference until the redesign is
   complete, then retired (Step 12). This satisfies the "make a new project" requirement and lets us
   build the notebook UI without breaking the shipping app.

2. **Everything in the UI layer is re-implemented from scratch.** We do **not** copy, file-link, or
   otherwise reuse any view-model or control from the old `MaxwellCalc` project. All view-models
   (shell, sheet, workspace/shared state, the overlay's collection view-models, settings) and all
   controls (including the quantity renderer) are written fresh in `MaxwellCalc.Notebook`, driving the
   `MaxwellCalc.Core` API directly. The old project's files may be consulted as a *reference* for how a
   given Core call is used, but no code is carried over. This keeps the new UI clean and free of the
   old shell's assumptions (panes, single-line history, etc.).

3. **Target the versions already in the repo, not the handoff doc's.** The doc says "Avalonia 11";
   the repo is on **Avalonia 12.0.4**, **Material.Icons.Avalonia 3.0.2**, **CommunityToolkit.Mvvm 8.4.2**,
   **.NET 8**. Match the repo.

4. **Base theme: Avalonia FluentTheme, not Material.** This is a from-scratch, fully token-driven
   design with its own color system (accent + unit hue + `bg`/`surf`/`bar`/`ink`/… tokens), so it sits
   on Avalonia's neutral built-in `FluentTheme` rather than the opinionated Material theme the old app
   used. Light/dark switching uses the **built-in** `RequestedThemeVariant` (`ThemeVariant.Light` /
   `ThemeVariant.Dark`) — no Material dependency. `Material.Icons.Avalonia` is kept purely as a
   standalone **icon pack** (glyphs like `Magnify`, `WeatherNight`, `Close`, `MenuDown`, `Plus`); it is
   independent of the Material theme. The handoff doc's mention of `Theme.MaterialLight/Dark` reflects
   the old app and is superseded here.

5. **Sheet evaluation must not mutate the workspace.** Confirmed in
   `MaxwellCalc.Core/Workspaces/Workspace.cs:271-306`: resolving an `Assign` node writes to
   `Scope.Local[name]`, and a function-definition assign writes to `UserFunctions`. Evaluating the
   sheet top-to-bottom against the live workspace would therefore permanently pollute it as the user
   types. Step 4 evaluates each pass into a **transient scope seeded from the workspace**, so the
   persistent workspace only ever changes through the overlay's explicit add/remove commands. Sheet-
   defined names are surfaced in the overlay tagged `from sheet` (read-only there).

## Naming & locations (new project)

```
MaxwellCalc.Notebook/
  MaxwellCalc.Notebook.csproj      # Avalonia 12 WinExe, refs MaxwellCalc.Core
  Program.cs, App.axaml(.cs)       # DI container + Material theme + ViewLocator
  ViewLocator.cs
  Assets/                          # Theme.axaml (tokens), Fonts/ (optional IBM Plex)
  Controls/
    QuantityView.*                 # quantity renderer, written fresh (superscript runs)
    HighlightedExpressionBox.*     # inline-highlighting editor
    Chip.*                         # access-bar chip
  ViewModels/
    ViewModelBase.cs               # fresh MVVM base
    WorkspaceState.cs              # active-workspace + shared state (fresh; ~ old SharedModel role)
    ShellViewModel.cs              # top-level shell
    SheetViewModel.cs              # Lines collection + evaluation loop
    LineViewModel.cs               # per-line model
    CommandPaletteViewModel.cs     # overlay host
    Overlay/                       # fresh collection VMs driving Core dictionaries directly:
      VariablesPanelViewModel.cs, ConstantsPanelViewModel.cs,
      InputUnitsPanelViewModel.cs, OutputUnitsPanelViewModel.cs,
      FunctionsPanelViewModel.cs, BuiltInFunctionsPanelViewModel.cs
    SettingsViewModel.cs           # theme, unit hue, workspace list, persistence
  Views/
    ShellWindow.axaml(.cs)         # single-window shell (title bar + sheet + access bar + overlay)
    SheetView.axaml(.cs)
    CommandPaletteView.axaml(.cs)
```

Nothing above is copied from the old project — every file is authored fresh against Core. Each step
below is independently implementable, compiles on its own, and lists concrete acceptance criteria.
Steps 1–2 are prerequisites; 3–11 can largely be parallelized by area once the shell exists.

---

## Step 1 — Scaffold the new project

**Goal:** a new, clean Avalonia app that builds, runs, shows an empty window, and can reach Core.

- Create `MaxwellCalc.Notebook/MaxwellCalc.Notebook.csproj` mirroring `MaxwellCalc.csproj`'s package
  set and settings (`WinExe`, `net8.0`, `Nullable`, `AvaloniaUseCompiledBindingsByDefault`,
  `ExtendClientAreaToDecorationsHint` usage). Add `PackageReference`s: Avalonia 12.0.4,
  Avalonia.Desktop, Avalonia.Themes.Fluent, Avalonia.Fonts.Inter, Material.Icons.Avalonia 3.0.2
  (icon pack only), CommunityToolkit.Mvvm 8.4.2, Microsoft.Extensions.DependencyInjection. Do **not**
  reference Material.Avalonia. Add `<ProjectReference Include="..\MaxwellCalc.Core\MaxwellCalc.Core.csproj" />`
  — the only dependency on existing repo code.
- Add the project to `MaxwellCalc.sln`.
- Author fresh, minimal boilerplate: `Program.cs`, `App.axaml`/`App.axaml.cs` (register the Material
  theme, build a fresh DI `ServiceCollection` for the new VMs, register the `System.Text.Json`
  converters from Core — `WorkspaceJsonConverter`, `WorkspaceJsonConverterFactory`), a `ViewLocator`
  implementing the `*ViewModel`→`*View` convention (re-written, not copied), a fresh `ViewModelBase`,
  and an empty `ShellWindow` set as `MainWindow`.
- **No files are copied or linked from `MaxwellCalc`.** The old project is only opened for reference.

**Acceptance:** `dotnet build MaxwellCalc.Notebook` succeeds; running it opens a blank window with the
FluentTheme; the new DI container builds without throwing. No compile unit originates from the old
UI project.

---

## Step 2 — Design tokens & theming resources

**Goal:** all colors, brushes, font families, radii, and the selectable "unit hue" exist as theme
resources usable via `DynamicResource`, switching correctly between light and dark.

- Add `Assets/Theme.axaml` with two `ThemeVariant` resource dictionaries (Light/Dark) defining brushes
  for every token in `REDESIGN_UI.md` → **Design Tokens**: `bg`, `surf`, `bar`, `barFooter`, `ink`,
  `mut`, `faint`, `line`, `card`, `chip`, `chipBorder`, `activeTint`, `err`, `overlay`, `accent`.
  Use the exact hex / `rgba` / `oklch` values listed. Avalonia 12 `Color` supports parsing these; for
  `oklch` precompute sRGB hex if the parser rejects it.
- Add the **unit hue** as three named brush sets (amber H=60 default, green H=150, violet H=300), each
  with light `oklch(0.52 0.13 H)` and dark `oklch(0.82 0.11 H)` values, plus a single dynamic
  `UnitHueBrush` that the app points at the selected preset (wired in Step 10).
- Define pill styles (`const` pill, `ƒ-defined`/`auto` pill) and shared radii/spacing as resources.
- Register `Theme.axaml` in `App.axaml` (after `FluentTheme` so tokens override base brushes). Wire
  theme switching to the built-in mechanism (`Application.Current.RequestedThemeVariant =
  ThemeVariant.Light / ThemeVariant.Dark`); the fresh `SettingsViewModel` (Step 10) drives it.
- **Typography:** add `Fonts/` and either bundle IBM Plex Sans + IBM Plex Mono (OFL) as
  `AvaloniaResource` and expose `UiFontFamily` / `MonoFontFamily` resources, or fall back to the system
  UI font + `Cascadia Code`/`Consolas`. Editor and gutter **must** share the same mono family/metrics.

**Acceptance:** a throwaway test page bound to these brushes renders the right colors in both variants;
toggling `RequestedThemeVariant` recolors live; the mono font is applied to a sample `TextBlock`.

---

## Step 3 — App shell layout (static)

**Goal:** the three-region single-window layout renders with correct chrome, spacing, and drag
behavior — no evaluation or overlay logic yet.

- `WorkspaceState` (fresh shared-state VM): wraps the active `IWorkspace` and its output format, raises
  change notifications when the active workspace switches. This is the single source the sheet and the
  overlay VMs read from — the role the old `SharedModel` played, rebuilt clean.
- `ShellViewModel` (top-level): holds `WorkspaceState`, the `SheetViewModel`, the
  `CommandPaletteViewModel`, and exposes the active workspace name for the title caption + Physics
  button. Register in DI; set as `ShellWindow.DataContext`.
- `ShellWindow.axaml`: a `Grid` with rows `Auto,*,Auto`:
  - **Title bar (44px):** left cluster (15×15 accent app-mark, "MaxwellCalc" 600/12.5px, em-dash
    caption bound to workspace name), right cluster (theme-toggle button placeholder + real OS caption
    buttons via `ExtendClientAreaToDecorationsHint="True"`). `bar` background, 1px bottom `line`, whole
    bar is the window drag handle.
  - **Sheet region (`*`):** a `ScrollViewer` hosting `SheetView` (placeholder rows for now).
  - **Access bar (`Auto`):** "Physics ▾" accent button, divider, three `Chip`s (Variables/Units/
    Functions with count placeholders), spacer, `⌘K` key-hint pill. Build the `Chip` control here.
- Use Material.Icons for `MenuDown`, `Magnify`, `WeatherNight`/`WhiteBalanceSunny`, `Close`, `Plus`.

**Acceptance:** window matches the prototype's layout and spacing (title 44px, access bar padding
`10px 16px`, chip gap 9px); dragging the title bar moves the window; OS caption buttons work; nothing
is wired yet but nothing throws.

---

## Step 4 — Sheet data model & evaluation loop

**Goal:** typing in a plain (unhighlighted) multi-line editor produces correct per-line results in a
transient scope, without mutating the workspace.

- `LineViewModel` : `[ObservableProperty] string Text`; derived (recomputed) result state:
  `enum LineKind { Empty, Value, Assign, FuncDef, Error }`, `Quantity<string> Quantity`,
  `bool IsConstBadge`, `bool AutoUnitSelected`, `string? ErrorMessage`.
- `SheetViewModel` (written fresh): `ObservableCollection<LineViewModel> Lines`,
  `int? FocusedLineIndex`. Re-evaluate the whole sheet on any `Text` change (optional debounce).
- **Evaluation loop** (`EvalDoc`), mirroring the prototype's `evalDoc`, threading a running scope
  top-to-bottom:
  1. Read the active `IWorkspace` from `WorkspaceState`; bail if null.
  2. **Build a transient scope seeded from the workspace** so assignments/funcdefs on the sheet don't
     persist. Because `TryResolve` on an `Assign` writes to `Scope.Local`/`UserFunctions`
     (`Workspace.cs:271-306`), evaluate against a scoped copy: snapshot with `Restrict(...)` +
     `Restore(oldState)` around the pass, or clone the variable scope per pass. Verify the chosen
     mechanism leaves `workspace.Variables`/`UserFunctions` untouched after typing.
  3. For each line, in order: trim → empty ⇒ `Empty`. Else `new Lexer(text)` →
     `Parser.Parse(lexer, workspace)`.
  4. Detect node shape:
     - `BinaryNode { Type: Assign, Left: FunctionNode }` ⇒ `FuncDef` (register into transient function
       scope; gutter shows `ƒ defined` pill).
     - `BinaryNode { Type: Assign, Left: VariableNode }` ⇒ `Assign` (bind in transient scope; gutter
       shows the value).
     - otherwise ⇒ `Value`: `workspace.TryResolveAndFormat(node, WorkspaceState.OutputFormat,
       CultureInfo.InvariantCulture, out result)`. Tag `IsConstBadge` if the whole line is a single
       constant identifier (check against `workspace.Constants`).
  5. Collect diagnostics via `workspace.DiagnosticMessagePosted` (subscribe before parse/resolve,
     unsubscribe in `finally`); any diagnostic ⇒ `Error` with joined message. (The old
     `CalculatorViewModel.Evaluate` is a useful reference for this subscribe/finally shape.)
  6. Capture `AutoUnitSelected` when `TryResolveOutputUnits` picked the ≥1 unit (for the Step 6 caption).

**Acceptance:** unit tests (or manual) confirm: `1 m/s in km/hour` formats correctly; `mass = 70 kg`
then `mass * 2` on the next line yields `140 kg`; `f(x)=x^2` then `f(3)` yields `9`; after typing all
that, `workspace.Variables` and `workspace.UserFunctions` are **unchanged**; an error line reports the
Core diagnostic.

---

## Step 5 — Result gutter rendering

**Goal:** the right gutter renders each line's result exactly per spec.

- Write a fresh `Controls/QuantityView` that renders a `Quantity<string>` as colored `Run` inlines: the
  scalar in `ink`, each unit symbol in the unit hue, exponents as superscript runs at `0.75×FontSize`.
  (The old `FormattedQuantity` may be read as a reference for the `Run`/superscript approach, but this
  is authored new.) Core's formatter already produces the scalar + unit dimension and the
  scientific-notation rule, so `QuantityView` only lays out what it's given.
- `SheetView` row template: baseline-aligned `flex`-like layout, `gap 24px`, padding `8px 26px`, 1px
  bottom `line`, focused row gets `activeTint` with a 120ms background transition.
- Gutter content by `LineKind`:
  - `Empty` ⇒ nothing.
  - `Value`/`Assign` ⇒ `QuantityView` bound to `Quantity`, `UnitForeground={DynamicResource
    UnitHueBrush}`, mono 15px/600 right-aligned.
  - `IsConstBadge` ⇒ prefix the accent-tinted `const` pill.
  - `FuncDef` ⇒ unit-hue `ƒ defined` pill instead of a value.
  - `Error` ⇒ right-aligned, wrapping, `err` color, 12.5px **sans**, ⚠ prefix.

**Acceptance:** the four result kinds and the `const` pill render matching the prototype; numbers in the
gutter line up (shared mono metrics); unit symbols use the unit hue.

---

## Step 6 — Inline syntax highlighting + editor control

**Goal:** the editable line field highlights tokens live and re-highlights when workspace dictionaries
change.

- New `Controls/HighlightedExpressionBox`. **Recommended route (same `Run`-rebuild technique as
  `QuantityView`):** a custom control that rebuilds colored `Run` inlines from lexer output on each
  `TextChanged`, over an editable text surface, with caret color = accent. Fall back to a `TextBox`
  subclass painting token
  spans only if caret handling fails.
- Tokenize with `MaxwellCalc.Core.Parsers.Lexer`; map `TokenTypes` → classes per the table in
  `REDESIGN_UI.md` → **Inline syntax highlighting**. Classification rule (mirror exactly): for each
  `Word`, if the next non-space token is `(` ⇒ `func` (exists in user/built-in funcs) else `unknown`;
  otherwise `var` (in `Variables`) / `const` (in `Constants`) / `unit` (in `InputUnits`) / else
  `unknown`. `in` keyword ⇒ `kw` (accent).
- Recompute on every keystroke **and** subscribe to the workspace dictionaries' `DictionaryChanged`
  (and `WorkspaceState` active-workspace change) so adding a unit recolors the sheet.
- **Auto-caption (behind a setting, default on):** when the focused line's output unit was auto-selected
  (Step 4's `AutoUnitSelected`), show the tiny faint caption *"output unit auto-selected (value ≥ 1) —
  type `in <unit>` to override"*. Add the toggle to `SettingsViewModel` (Step 10).

**Acceptance:** typing `2 m + 3 kg` colors `m`/`kg` in unit hue, numbers in ink, `+` faint; `sin(` colors
`sin` as func; an unknown identifier is `err`-colored; adding an input unit via the overlay recolors an
existing line that used it.

---

## Step 7 — Keyboard model in the sheet

**Goal:** the sheet behaves like a notebook, replacing the old history recall.

- **Enter** ⇒ insert an empty `LineViewModel` below the current, move caret into it.
- **Backspace at column 0** ⇒ merge current line into previous (caret at the join point).
- **Arrow Up/Down** ⇒ move caret to previous/next line, preserving column where possible.
- Focus: clicking a row focuses its editor and applies `activeTint`; keyboard actions move focus and set
  caret position (`FocusedLineIndex` + a caret-index property per line).
- Remove the old `cls`/`clc`/`clear` commands and up/down `history.json` recall (they don't apply to the
  live sheet model).

**Acceptance:** Enter/Backspace-merge/Arrow navigation work with correct caret placement; the sheet can
grow, shrink, and never loses focus tracking.

---

## Step 8 — Command palette overlay (read/search)

**Goal:** ⌘K / chip clicks open a centered modal that lists workspace contents with live search.

- **Build the panel view-models from scratch.** Author a small fresh `FilteredPanelViewModel<TItem>`
  base that: reads a Core observable dictionary from the active workspace (via `WorkspaceState`),
  projects entries into item VMs, keeps them in sync by subscribing to `DictionaryChanged`, re-projects
  when the active workspace switches, and exposes a `Filter` string with `MatchesFilter`. This replaces
  the old `FilteredCollectionViewModel<>` — reading it as a reference for the sync/filter mechanics is
  fine, but the new one is written clean and only exposes what the overlay needs. On top of it, author:
  `VariablesPanelViewModel` (→ `workspace.Variables`), `ConstantsPanelViewModel` (→ `Constants`,
  read-only), `InputUnitsPanelViewModel` (→ `InputUnits`), `OutputUnitsPanelViewModel` (→ `OutputUnits`,
  grouped by physical quantity), `FunctionsPanelViewModel` (→ `UserFunctions`),
  `BuiltInFunctionsPanelViewModel` (→ `BuiltInFunctions`, read-only). Register in DI.
- `CommandPaletteViewModel`: `bool PaletteOpen`, `enum PaletteSection { Variables, Units, Functions }`,
  `enum UnitTab { Input, Output }`, and a `Search` string pushed into each hosted panel VM's `Filter`.
  Also surfaces the sheet-defined names (from `SheetViewModel`'s transient scope) as read-only
  `from sheet` rows merged into the Variables/Functions lists.
- `CommandPaletteView.axaml`: 640px (max 92vw) centered card, 14px radius, big soft shadow, over a
  dimmed `overlay` scrim. Header: ⌕ glyph + autofocused search input (mono 15px) + right-aligned section
  title. Body scrolls; content per section:
  - **Variables & constants:** "Variables — you define these" (name bold + value + × remove) and
    "Constants — fixed, can't be reassigned" (name + description + value, no remove). Include sheet-
    defined vars tagged `from sheet` (read-only). Empty state string per spec.
  - **Units:** segmented Input/Output tabs. Input rows: symbol (unit hue) `= definition` + × ; caption
    "resolve to base SI". Output rows grouped by physical quantity with section labels; row = label
    `= definition` + × ; caption "candidates the app auto-picks from".
  - **Functions:** "Your functions" (`name(params) = body` + ×) then "Built-in — always available"
    (signature + description, no remove); sheet-defined funcs tagged `from sheet`.
- Open on **⌘K/Ctrl+K** (Units by default); close on **Esc** or scrim click. Chips open pre-scoped to a
  section with `Filter` cleared; clear `Filter` on tab/section change.

**Acceptance:** ⌘K opens the overlay; typing filters rows live (drives `Filter`); switching section/tab
resets search; Esc/scrim closes; built-ins and constants show no × button.

---

## Step 9 — Overlay add/remove flows

**Goal:** the overlay footer can add each entity type and rows can remove, all implemented fresh
against the Core API.

- Footer add-row: "＋ New …" label, one or two mono fields, an `=` separator, accent **Add** button
  (Enter submits). Implement an `Add`/`Remove` command on each panel VM, calling Core directly:
  - Variable ⇒ parse the value, bind into `workspace.Variables`.
  - Input unit ⇒ parse the definition under a **restricted** workspace (`Restrict(false,false,true,
    false,false)` … `Restore(oldState)`) then `workspace.TryAssignInputUnit(symbol, node)`. (The old
    `InputUnitsViewModel.AddInputUnit` shows this exact restrict/restore pattern — reference only.)
  - Output unit ⇒ `workspace.TryAssignOutputUnit(unitsNode, valueNode)`.
  - Function ⇒ parse `f(x)` + body and register into `workspace.UserFunctions`.
- Each Add subscribes to `workspace.DiagnosticMessagePosted` around the attempt (subscribe/finally),
  exposing a `Diagnostics`/`ErrorMessage` the footer binds to. On success clear the fields; on failure
  show an inline `err` line with the diagnostic and leave the fields intact.
- Row × ⇒ the panel VM's `Remove` command (`TryRemoveInputUnit` / `TryRemoveOutputUnit` / remove from
  `Variables` / `UserFunctions`). Built-ins, constants, and `from sheet` rows have no ×.

**Acceptance:** adding a valid variable/unit/function updates the workspace and the sheet re-highlights/
re-evaluates; an invalid add shows the diagnostic inline and leaves fields intact; × removes a user
item.

---

## Step 10 — Access bar interactions, theme toggle, settings

**Goal:** the access bar and title-bar controls are fully live and preferences persist.

- Write a fresh `SettingsViewModel` owning: the workspace list + selection (creating `Workspace<double>`
  / `Workspace<Complex>` and updating `WorkspaceState`), theme variant, `UnitHue` (enum
  amber/green/violet), and the auto-caption toggle. Persist to `settings.json` in the working directory
  with `System.Text.Json` (a plain serializable settings record — no need to reproduce the old manual
  `Utf8JsonReader` loop).
- **Physics ▾ button** ⇒ workspace switcher (flyout/menu listing the workspaces, calls the select
  command); button label + title caption reflect the active workspace name.
- **Chips** ⇒ open the overlay on the matching section; live count badges bound to the panel VMs'
  item counts (user vars, input units, user funcs — pick the counts shown in the prototype).
- **Theme toggle** ⇒ flips light/dark via the built-in `RequestedThemeVariant` (`ThemeVariant.Light` /
  `ThemeVariant.Dark`); icon swaps ☾/☀; persists.
- Changing `UnitHue` repoints the dynamic `UnitHueBrush` (Step 2) and persists.

**Acceptance:** switching workspace updates title + button + sheet + overlay contents; chip counts are
correct and update on add/remove; theme toggle persists across restart; unit-hue selection persists and
recolors units everywhere.

---

## Step 11 — Sheet persistence & lifecycle

**Goal:** the sheet survives restart; workspaces/settings persist as before.

- Persist the list of line texts (JSON array of strings) in the working directory: save on window
  close, load on startup. Results are always recomputed, so only the raw text is stored.
- Author fresh workspace persistence using Core's `WorkspaceJsonConverter` /
  `WorkspaceJsonConverterFactory` (registered in the Step 1 DI container) to save/load the workspace
  list to `workspace.json`.
- Wire save on `ShellWindow` `Closing`: persist the sheet, the workspaces, and the settings. Load all
  three on startup.

**Acceptance:** add several lines, close, reopen ⇒ the same lines are present and re-evaluated; theme,
unit hue, and workspace selection are restored.

---

## Step 12 — Cutover & cleanup

**Goal:** make the notebook UI the shipping app; retire the old shell.

- Point packaging (icon `maxwell_calc.ico`, `app.manifest`, `ApplicationIcon`) at the new project.
- Manually verify against the prototype `MaxwellCalc Prototype.dc.html` across both themes and all three
  unit hues (fidelity is high-fidelity per the doc).
- Remove the old `MaxwellCalc` project from the solution (or keep it archived on a branch). Since the
  new project shares no UI code with it, this is a clean deletion — only `MaxwellCalc.Core` and
  `MaxwellCalc.Tests` remain alongside `MaxwellCalc.Notebook`.
- Optionally bundle IBM Plex fonts for pixel fidelity (Step 2 left this as a fallback).

**Acceptance:** the new project is the startup app, builds in Release with no `Avalonia.Diagnostics`
reference, matches the prototype, and the old shell is gone or clearly archived.

---

## Risks / things to verify while implementing

- **Scope isolation (Step 4)** is the highest-risk item — confirm `Restrict`/`Restore` (or a per-pass
  scope clone) actually prevents `Assign`/funcdef mutation of the live workspace. Add a regression test.
- **Highlighted editor caret** (Step 6): route 1 (rebuilt inline `Run`s) may fight caret positioning;
  budget time to fall back to a `TextBox` subclass if needed.
- **`oklch` colors** (Step 2): Avalonia's color parser may not accept `oklch(...)`; precompute sRGB hex.
- **Avalonia 12 vs. doc's "11"**: some API names in the doc (e.g. `TextPresenter` internals) may differ;
  trust the repo's version and its existing controls as the pattern of record.
- **Font metrics**: gutter and editor must use the identical mono family or numbers won't align.
