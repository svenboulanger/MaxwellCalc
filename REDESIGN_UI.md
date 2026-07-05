# Handoff: MaxwellCalc — Notebook Sheet Redesign

## Overview

This is a redesign of **MaxwellCalc**'s UI, moving it from a *single-line expression box + scrolling history* model to a **multi-line editable "notebook sheet"** (Soulver / SpeedCrunch-notebook style). Every line is an independent, always-live calculation: as you type, each line's result is evaluated and shown in a right-hand **result gutter**, with **inline syntax highlighting** in the editor itself. A **bottom access bar** exposes the workspace's units / variables / functions as chips, and a **⌘K command palette overlay** lets you search, add, and remove input units, output units, variables, constants, and functions in one place.

The engine is **not** changing. Keep `MaxwellCalc.Core` exactly as-is. This is a **UI-layer rewrite** (Views + ViewModels) that drives the existing Core API. The scope was confirmed with the designer:

- **Whole redesign** (not piecemeal).
- **Move to the multi-line notebook model** (replace single-line + history).
- **Keep `MaxwellCalc.Core`, change the UI only.**
- Priorities, all in scope: inline syntax highlighting, per-line result gutter with unit formatting, the combined overlay panel (search + add + remove), the bottom access bar with chips and ⌘K, and light/dark theming with an accent + "unit hue".

## About the Design Files

The files in this bundle are **design references created in HTML** — a working prototype that demonstrates the intended *look and behavior*. They are **not** production code to copy. Your task is to **recreate this design in the existing Avalonia / .NET codebase**, using its established patterns: **Avalonia 11**, **Material.Styles** + **Material.Icons.Avalonia**, **CommunityToolkit.Mvvm** (`[ObservableProperty]` / `[RelayCommand]` source generators), DI via `Microsoft.Extensions.DependencyInjection`, and `System.Text.Json` persistence, with the `ViewLocator` mapping `*ViewModel` → `*View`.

The prototype ships its own JavaScript engine, `calc-engine.js`. **Ignore it as an implementation** — it exists only so the HTML prototype can run in a browser. It is a faithful mirror of `MaxwellCalc.Core`'s semantics, so it's useful as a **behavioral spec** (how per-line evaluation, auto output-unit selection, and highlighting should behave). The mapping table below tells you which real Core call replaces each engine function.

## Fidelity

**High-fidelity.** Colors, typography, spacing, and interactions in the prototype are intentional and final. Recreate them faithfully with Avalonia primitives and the Material theme. Where the prototype uses IBM Plex (a web font), see **Typography** below for the desktop substitution and rationale — you do not need to bundle IBM Plex unless you want to.

---

## The engine mapping (prototype JS → MaxwellCalc.Core)

Everything the prototype's `MaxwellEngine.*` does has a Core equivalent. Do not reimplement any of it.

- `makeWorkspace()` → already exists: `SharedModel.Workspace.Key` is the active `IWorkspace<double>` (or `IWorkspace<Complex>`). Workspaces are created/selected in `SettingsViewModel`. **Do not create workspaces in the calculator VM** — read the shared one, exactly as `CalculatorViewModel` does today (`Shared.Workspace?.Key`).
- `evalDoc(ws, texts)` (evaluate every line, top to bottom, threading a running scope) → **no single Core call; you compose it** from: `new Lexer(text)` → `Parser.Parse(lexer, workspace)` → `workspace.TryResolveAndFormat(node, format, CultureInfo.InvariantCulture, out Quantity<string> result)`. See **Sheet evaluation** below for the loop and scope handling.
- `highlight(text, ctx)` (token → CSS class) → build with the existing **`Lexer`** (`MaxwellCalc.Core.Parsers.Lexer` + `TokenTypes`). Classify each token and cross-reference `workspace.InputUnits`, `workspace.Constants`, `workspace.Variables`, `workspace.UserFunctions`, `workspace.BuiltInFunctions` to decide unit / const / var / func / unknown — same logic as the JS `highlight()`.
- `formatQuantity(q, ws)` + auto output-unit selection → `workspace.TryResolveAndFormat(...)` already returns a formatted `Quantity<string>`; the "pick the output unit giving a value just ≥ 1" behavior lives in `TryResolveOutputUnits`. Render the resulting `Quantity<string>` with the **existing `FormattedQuantity` control** (it already draws the scalar + unit symbols with superscript exponents and a separate `UnitForeground` brush).
- `in` operator override (`1 m/s in km/hour`) → already handled by Core's parser (`BinaryOperatorTypes.InUnit`) and formatting. No UI work beyond showing the result.
- `addInputUnit` / `removeInputUnit` → `workspace.TryAssignInputUnit(name, node)` / `workspace.TryRemoveInputUnit(name)`. The node comes from parsing the definition string under a **restricted** workspace — see `InputUnitsViewModel.AddInputUnit` for the exact pattern (`Restrict(false,false,true,false,false)` … `Restore(oldState)`).
- `addOutputUnit` / `removeOutputUnit` → `workspace.TryAssignOutputUnit(unitsNode, valueNode)` / `workspace.TryRemoveOutputUnit(OutputUnitKey)`. See `OutputUnitsViewModel`.
- `addVariable` / `removeVariable`, `addFunction` / `removeFunction` → the existing `UserVariablesViewModel` / `UserFunctionsViewModel` (and `VariablesViewModel` / `FunctionsViewModel`) already implement add/remove against `workspace.Variables` and `workspace.UserFunctions`. Reuse them.
- `listInputUnits` / `listOutputUnits` / `listConstants` / `builtinInfo` → the observable dictionaries `InputUnits`, `OutputUnits`, `Constants`, `BuiltInFunctions`, already surfaced through `FilteredCollectionViewModel<…>` subclasses that also give you **`Filter`** (search) and remove commands for free.
- `serializeWs` / `applyWs` (persistence) → already handled by `WorkspaceJsonConverter` + `SettingsViewModel.Save/LoadWorkspaces`. Sheet contents (the lines) are the new thing to persist — see **State & persistence**.

**Net effect:** the overlay panel is largely a *re-presentation* of view-models you already have; the sheet editor and its gutter are the genuinely new UI.

---

## Screens / Views

The redesign is a **single window** with three stacked regions plus one overlay. Replace the current `SplitView` navigation shell — there are no more left-nav panes (Calculator / Variables / Functions / Units / Settings). Their functionality moves into the access bar, the overlay, and a lightweight settings affordance.

### 1. Title bar (top, fixed, height 44px)

- **Left cluster** (gap 10px): a 15×15 rounded-4px square filled with the **accent** color (app mark), the word **"MaxwellCalc"** (600 weight, 12.5px, ink), and a muted em-dash caption **"— Physics workspace"** (11px, faint). The caption text should reflect the active workspace name when more than one exists.
- **Right cluster** (gap 14px): a **theme toggle** button (☾ in light mode, ☀ in dark), then the platform window controls. In the prototype these are faux glyphs (`– □ ×`); in the real app you already use `ExtendClientAreaToDecorationsHint="True"`, so keep the real OS caption buttons and only add the theme toggle into the extended client area.
- Background `bar`, 1px bottom border `line`. The whole bar is the drag handle for the window.

### 2. Sheet editor (middle, fills remaining height, scrolls)

The core surface. A vertical list of **calculation lines**. Each line is one row:

- **Row layout:** `flex`, `align-items: baseline`, `gap: 24px`, padding `8px 26px`, 1px bottom border `line`. The **focused** row gets a subtle `activeTint` background (a ~5% accent wash in light, ~7% in dark) with a `120ms` background transition.
- **Editor (left, flex:1):** an editable single-line text field showing the raw expression, **with live syntax highlighting**. In the prototype this is done by layering a transparent `<input>` over a colored backdrop that re-renders the highlighted tokens (caret color = accent). In Avalonia, implement highlighting **inside** the text control (see **Inline syntax highlighting** for the recommended approach) rather than the overlay trick.
- **Result gutter (right, shrink:0):** monospace, 600 weight, 15px, right-aligned, `align-items: baseline`. Content depends on the line's evaluation result:
  - *empty line* → nothing.
  - *value* → the formatted quantity (scalar + units), unit symbols in the **unit hue**, exponents as superscripts. Use the `FormattedQuantity` control. Numbers use scientific notation with a superscript exponent when |exp| ≤ −4 or ≥ 6 (`1.23·10⁻⁹`), 5 significant figures otherwise; the prototype's `fmtRealSegs` documents the exact rule, and Core's formatter already matches this.
  - *bare constant referenced alone* (e.g. a line that is just `c`) → prefix a small **`const`** pill (accent-tinted) before the value.
  - *assignment* (`mass = 70 kg`) → show the assigned value in the gutter, same as a value line.
  - *function definition* (`f(x) = x^2 + 1`) → show a small unit-hue **`ƒ defined`** pill instead of a value.
  - *error* → right-aligned, wrapping, `err` color, 12.5px sans (not mono), prefixed with a small ⚠. Text is the Core diagnostic message (collect via `DiagnosticMessagePosted`, as `CalculatorViewModel.Evaluate` does today).
- **Auto-caption (optional, behind a setting):** when the focused line's result had its output unit auto-selected (value ≥ 1 rule), show a tiny right-aligned caption under the row: *"output unit auto-selected (value ≥ 1) — type `in <unit>` to override"*. 10.5px, faint. This is the prototype's `showAutoCaption` toggle → make it a workspace/app setting (default on).

**Keyboard model in the sheet** (this replaces the old up/down history):

- **Enter** → insert a new empty line *below* the current one and move the caret into it.
- **Backspace at column 0** → merge the current line into the previous one (caret lands at the join point).
- **Arrow Up / Down** → move the caret to the previous / next line, preserving column where possible.
- Lines are reorderable/deletable; at minimum support the above. (The old `cls`/`clc`/`clear` command and `history.json` up/down recall are removed by this model — see **Migration notes**.)

### 3. Bottom access bar (fixed, ~flex-wrap row, padding 10px 16px, 1px top border)

Left to right:

- A filled **accent** button **"Physics ▾"** — the active workspace switcher (opens the workspace list; replaces the Settings pane's workspace picker). Label = active workspace name.
- A thin vertical divider.
- Three **chips** (see the Chip component): **Variables** *x* (italic, accent glyph) with a live count badge; **Units** *m* (unit-hue glyph); **Functions** *ƒ* (italic, accent glyph). Clicking a chip opens the overlay on that section.
- A spacer, then a **⌘K** key-hint pill (mono, in a bordered chip) at the far right.

### 4. Command palette overlay (⌘K, or click any chip / the Physics button)

A centered modal, 640px wide (max 92vw), max height 78vh, `card` background, 14px radius, big soft shadow, over a dimmed `overlay` scrim. Opens on **⌘K / Ctrl+K**; closes on **Esc** or scrim click. Structure:

- **Header:** a ⌕ glyph + an autofocused **search input** (mono, 15px) + a right-aligned section title. The search text drives the existing `Filter` property on the collection view-models (live filtering).
- **Body (scrolls):** depends on which section is active:
  - **Variables & constants** — section "Variables — you define these" (each row: name in ink-bold, formatted value on the right, an × remove button). Below it, "Constants — fixed, can't be reassigned" (name + description + value, **no** remove). Empty state: *"None yet — add one below, or assign on a line like `mass = 70 kg`."* Rows sourced from `workspace.Variables` (user vars) and `workspace.Constants`; plus any variables *defined on the sheet* this session, tagged `from sheet` (read-only here).
  - **Units** — a two-tab segmented control (**Input units** / **Output units**). Input-unit rows: symbol (unit hue) `= definition` + × remove; caption "resolve to base SI". Output-unit rows are grouped by physical quantity (Length, Mass, Time, …) with a section label per group; row = unit label `= definition` + × remove; caption "candidates the app auto-picks from". Sourced from `workspace.InputUnits` and `workspace.OutputUnits`.
  - **Functions** — "Your functions" (signature `name(params)` + `= body` + × remove) then "Built-in — always available" (signature + description, no remove). Sourced from `workspace.UserFunctions` and `workspace.BuiltInFunctions`; sheet-defined functions tagged `from sheet`.
- **Footer (add row):** a labeled inline form pinned to the bottom of the panel — a "＋ New …" label, one or two mono input fields, an `=` separator, and an accent **Add** button (Enter also submits). Below the form, an inline **error** line (`err` color) shows the Core diagnostic when an add fails. Wire the fields + Add button to the existing add commands:
  - New variable → `name` + `value` → `AddVariable` (`UserVariablesViewModel` / `VariablesViewModel`).
  - New input unit → `symbol` + `value in base units` → `AddInputUnit` (`InputUnitsViewModel`).
  - New output unit → `symbol` + `value in base units` → `AddOutputUnit` (`OutputUnitsViewModel`).
  - New function → `f(x)` signature + `body` → `AddFunction` (`UserFunctionsViewModel`).

The three chips and the search should be able to open the overlay pre-scoped to a section and pre-cleared. Preserve `Filter` reset when switching sections/tabs (the prototype clears `search` on tab change).

---

## Inline syntax highlighting

The prototype colors these token classes (see `styleFor` and `highlight` in `calc-engine.js`):

| Class | Meaning | Light color | Dark color | Weight |
|---|---|---|---|---|
| `unit` | known input unit | unit hue (see tokens) | unit hue | 500 |
| `kw` | the `in` keyword | accent | accent | 600 |
| `func` | known function (built-in or user), when followed by `(` | ink | ink | 500 |
| `var` | known variable | ink | ink | 500 |
| `const` | known constant | ink | ink | 400 |
| `num` | number literal | ink | ink | 400 |
| `op` / `paren` / `punc` | operators, brackets, commas, `=` | faint | faint | 400 |
| `unknown` | unrecognized identifier / char | err | err | 400 |

Classification rule (mirror exactly): tokenize; for each identifier, if the *next* non-space token is `(` it's a function → `func` if it exists in user or built-in functions else `unknown`; otherwise `var` if in variables, `const` if in constants, `unit` if in input units, else `unknown`. Recompute on every keystroke and whenever the workspace's dictionaries change (adding a unit should recolor the sheet).

**Avalonia approach (recommended):** don't use the transparent-input-over-backdrop hack from the web. Instead, drive highlighting through the text control's inline runs. Two viable routes:

1. **Custom control** wrapping `TextPresenter` (or a `SelectableTextBlock` for display + a hidden `TextBox` for editing) where you rebuild `Inlines` (colored `Run`s) from the lexer output on each `TextChanged`. This matches how `FormattedQuantity` already builds `Run`s, so you have a working pattern in the repo.
2. A **`TextBox` subclass** that overrides text rendering to paint colored token spans. Heavier; only if route 1 can't hold the caret correctly.

Use the existing **`Lexer`** for tokenization (it already produces `TokenTypes` including `Word`, `Scalar`, operators, parentheses, `Assignment`, etc.) so highlighting stays consistent with the parser. Map `TokenTypes` → the class table above, then resolve `Word` tokens against the workspace dictionaries for unit/const/var/func/unknown.

---

## Sheet evaluation (the per-line loop)

Re-evaluate the whole sheet whenever any line changes (cheap; lines are short). Thread a **running scope** top to bottom so later lines see earlier assignments and function definitions — this is what `evalDoc` does. For each line, in order:

1. Trim; empty → result `empty`.
2. `var lexer = new Lexer(text); var node = Parser.Parse(lexer, workspace);`
3. Detect the shape of `node`:
   - **Function definition** (`f(x) = …`, an `Assign` whose left side is a function-form) → register into the running function scope; result `funcdef` (show the `ƒ defined` pill). Do **not** necessarily persist into `workspace.UserFunctions` unless you want sheet-defined functions to outlive the sheet — see the decision note below.
   - **Assignment** (`name = …`, an `Assign` with a bare identifier LHS) → evaluate RHS in the running scope, bind `name`; result = the value (show in gutter). Reject reassigning a constant (Core will post a diagnostic).
   - **Expression** → `workspace.TryResolveAndFormat(node, Shared.Workspace.OutputFormat, CultureInfo.InvariantCulture, out var result)`; result = formatted quantity. Tag `const` badge if the whole line is a single constant identifier.
4. Collect diagnostics via the `DiagnosticMessagePosted` event (subscribe around the parse/resolve, unsubscribe in `finally`) exactly as `CalculatorViewModel.Evaluate` does now; any diagnostic → result `error` with the joined message.

**Scope decision to make (flag for you, the implementer):** the prototype keeps sheet variables/functions in a *transient* scope recomputed each eval, separate from the persistent workspace `Variables`/`UserFunctions` (those are only what the user adds via the panel). This keeps the persistent workspace clean and makes the sheet reproducible. Recommended: **do the same** — evaluate the sheet into a fresh scope seeded from `workspace.Variables` + `workspace.UserFunctions` each pass, rather than mutating the workspace as the user types. Core's `Restrict`/`Restore` and a per-eval scope give you this; verify whether `TryResolve` on an `Assign` node writes to `workspace.Variables` (if it does, evaluate assignments against a scoped copy so typing on the sheet doesn't permanently mutate the workspace). Surface sheet-defined names in the overlay tagged `from sheet` (read-only there).

---

## Interactions & Behavior

- **Global keys:** ⌘K / Ctrl+K opens the overlay (Units section by default in the prototype; either default is fine — chips set the section). Esc closes the overlay.
- **Chip click:** opens the overlay on that section, resets search.
- **Physics button:** opens the workspace switcher.
- **Theme toggle:** flips light/dark. Wire to the same mechanism `SettingsViewModel.OnCurrentThemeChanged` uses (`Application.Current.RequestedThemeVariant` = `Theme.MaterialLight` / `Theme.MaterialDark`). Persist via existing settings save.
- **Add flows:** Enter in either footer field submits; on success the fields clear; on failure the inline error shows the diagnostic. (Matches the existing Add* command behavior.)
- **Remove:** the × on a row calls the existing `RemoveItem` command. Built-ins and constants have no × .
- **Focus:** clicking a row focuses its editor and applies `activeTint`. New-line/merge keyboard actions move focus and set caret position.
- **Transitions:** row background `120ms`. Overlay appears with the scrim; no elaborate animation required.

## State Management

New state (calculator/sheet VM):

- `ObservableCollection<LineViewModel> Lines` — each `LineViewModel` has `Text`, and derived (recomputed) `Result` info: kind (`empty` / `value` / `assign` / `funcdef` / `error`), the `Quantity<string>` to render, badge flag, auto-selected flag, error message. Recompute on any `Text` change (debounce optional).
- `int? FocusedLineIndex` / focus handling for caret movement.
- Overlay state: `bool PaletteOpen`, `enum PaletteSection { Variables, Units, Functions }`, `enum UnitTab { Input, Output }`, and the search string bound to each collection VM's `Filter`.
- Reuse `SharedModel.Workspace` for the active workspace and the existing collection VMs (`InputUnitsViewModel`, `OutputUnitsViewModel`, `UserVariablesViewModel`/`VariablesViewModel`, `ConstantsViewModel`, `UserFunctionsViewModel`/`FunctionsViewModel`, `BuiltInFunctionsViewModel`) for the overlay contents and add/remove.

## State & persistence

- **Sheet lines** are new persistent state. Persist the list of line texts (the prototype uses `localStorage["maxwellcalc.doc"]`). In the app, save alongside the workspace/history JSON in the working directory (mirror `CalculatorViewModel.Save/LoadHistory` — write on close via `MainWindowViewModel.Close`, load on startup). A single JSON array of strings is enough; results are always recomputed.
- **Workspace** (units/vars/funcs/constants) persistence is unchanged (`WorkspaceJsonConverter`, `SettingsViewModel`).
- **Theme + accent + unit hue** persist in settings (extend `SettingsViewModel`).

## Migration notes (what the redesign removes/replaces)

- The **`SplitView` left-nav** and the five pane views (`CalculatorView` shell aside) are replaced by the single-window layout. The *view-models* behind Units/Variables/Functions/Settings are **kept and reused** by the overlay and access bar — only their presentation changes.
- The **single-line input + Evaluate button + scrolling history** is replaced by the editable sheet. The old up/down **history recall** and the `cls`/`clc`/`clear` text commands no longer apply (every line is live and editable, so "history" is just the sheet). If you want to preserve prior sessions, load the last sheet on startup instead of `history.json`.
- **Multiple workspaces** stay; the picker moves from the Settings pane to the "Physics ▾" button.

---

## Design Tokens

Two palettes (light / dark). The prototype defines them in `renderApp()`; reproduce as theme resources / dynamic brushes. Prefer wiring these into the Material theme's resource dictionary so `DynamicResource` works in both variants.

**Light**
- `bg` window backdrop `#eeece7`
- `surf` sheet surface `#faf9f6`
- `bar` title/access bar `#f1efe9` (access bar footer uses `#f4f2ec`)
- `ink` primary text `#1c1c1a`
- `mut` muted text `rgba(0,0,0,.5)`
- `faint` faint text `rgba(0,0,0,.34)`
- `line` hairline borders `rgba(0,0,0,.07)`
- `card` overlay/panel `#ffffff`
- `chip` / `chipBorder` `#ffffff` / `rgba(0,0,0,.08)`
- `activeTint` focused row `rgba(42,120,214,.05)`
- `err` `oklch(0.52 0.16 25)`
- `overlay` scrim `rgba(20,20,18,.16)`
- `accent` `oklch(0.5 0.13 250)` (a calm blue)

**Dark**
- `bg` `#141417`, `surf` `#1b1b20`, `bar` `#191920`
- `ink` `#ececeb`, `mut` `rgba(255,255,255,.44)`, `faint` `rgba(255,255,255,.3)`
- `line` `rgba(255,255,255,.08)`, `card` `#22222a`, `chip` `#26262e`, `chipBorder` `rgba(255,255,255,.1)`
- `activeTint` `rgba(120,170,255,.07)`, `err` `oklch(0.74 0.14 25)`, `overlay` `rgba(0,0,0,.5)`
- `accent` `oklch(0.74 0.13 250)`

**Unit hue** (the color for unit symbols, both in the editor and the gutter). Selectable; three presets. Formula: light `oklch(0.52 0.13 <H>)`, dark `oklch(0.82 0.11 <H>)`, where `<H>` is the hue:
- amber `H = 60` (default) · green `H = 150` · violet `H = 300`

**const pill** (light) accent text on `rgba(42,120,214,.1)`, (dark) on `rgba(120,170,255,.16)`; 10px sans 500, radius 5, padding `2px 7px`.
**ƒ-defined / auto pill** unit-hue text on a faint unit-hue wash; 10.5px sans 500, radius 6, padding `2px 8px`.

**Spacing:** row padding `8px 26px`; editor↔gutter gap `24px`; access bar padding `10px 16px`, chip gap `9px`; overlay padding `6px 8px`, header padding `14px 18px`, add-row padding `11px 16px`.
**Radii:** chips/buttons 7px, inputs 6px, pills 5–6px, overlay card 14px, app mark 4px.
**Shadow:** overlay `0 30px 70px -20px rgba(0,0,0,.5)`.

### Component: Chip (access bar)
`inline-flex`, gap 6, padding `5px 11px`, radius 7, `chip` bg, 1px `chipBorder`, 12px/500 sans, ink text; leading mono glyph in the noted color (accent for x/ƒ, unit-hue for m); optional trailing count in `faint`. Pointer cursor.

### Component: Add button
No border, accent bg, white text, radius 6, padding `6px 12px`, 12px/600 sans.

## Typography

- **UI / labels / captions:** the prototype uses **IBM Plex Sans**. On desktop, either bundle IBM Plex Sans (it's OFL-licensed and would keep the design pixel-accurate) or substitute the platform UI font. The app currently declares `ResultFontFamily = "MS Shell Dlg 2"` — if you're not bundling a font, use the system UI font for chrome.
- **Expressions, results, unit symbols, key-hints:** **monospace** — the prototype uses **IBM Plex Mono**. Bundle IBM Plex Mono or fall back to `Cascadia Code` / `Consolas` / the platform monospace. The result gutter and the editor must share the same mono metrics so numbers line up.
- **Sizes:** editor & gutter `15px` (mono; gutter 600 weight, editor 400, line-height 1.55); title `12.5px/600`; captions `10.5–11px`; chip label `12px/500`; overlay search `15px` mono; section labels `10.5px/600` uppercase, letter-spacing `.08em`, faint.
- **Number formatting** superscripts: exponents render at `0.7em` raised — `FormattedQuantity` already does `0.75 * FontSize` superscript runs, which is the right mechanism; the small difference is cosmetic.

## Assets

- **Icons:** use **Material.Icons.Avalonia** (already referenced). Suggested: `MagnifyClose`/`Magnify` for the search ⌕, `WeatherNight`/`WhiteBalanceSunny` for the theme toggle, `Close` for the × remove, `MenuDown` for the "▾" on the Physics button, `Plus` for "＋". The app mark can stay a solid accent rounded square (no icon needed). The trash/warning/copy `StreamGeometry` resources already in `App.axaml` are available if you prefer them.
- **Fonts:** IBM Plex Sans + IBM Plex Mono if you choose to bundle for fidelity (optional; see Typography). No other assets are required — nothing in this design is a raster image.

## Files in this bundle

- `README.md` — this document (self-sufficient; implementable without the prototype).
- `MaxwellCalc Prototype.dc.html` — the interactive HTML prototype. Open it to see the intended look and every interaction (type in the sheet, press ⌘K, toggle theme, add/remove in the overlay).
- `calc-engine.js` — the prototype's JS engine. **Behavioral reference only** — it mirrors `MaxwellCalc.Core` semantics (per-line eval, auto output-unit selection, highlighting classes, number formatting). Do not port it; use Core.
- `support.js` — runtime needed for the prototype HTML to render locally. Not part of the design.

### Where to implement in the real repo (suggested)

- Replace `Views/MainWindow.axaml` shell: title bar + sheet + access bar + overlay host.
- New `Views/SheetView.axaml` + `ViewModels/SheetViewModel.cs` (evolve `CalculatorViewModel`): the `Lines` collection, evaluation loop, keyboard model, persistence.
- New `Controls/HighlightedExpressionBox` (the inline-highlighting editor) + new `Controls/GutterResult` (or reuse `FormattedQuantity` directly).
- New `Views/CommandPaletteView.axaml` + VM hosting the existing `InputUnitsViewModel` / `OutputUnitsViewModel` / `UserVariablesViewModel` / `ConstantsViewModel` / `UserFunctionsViewModel` / `BuiltInFunctionsViewModel` in tabs, bound search → `Filter`.
- Extend `SettingsViewModel` with `UnitHue` (+ persistence) and keep theme wiring.
- Add sheet-content persistence to `MainWindowViewModel.Close` / startup load.
