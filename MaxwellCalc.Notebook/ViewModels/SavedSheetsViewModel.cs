using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Notebook.ViewModels.Overlay;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MaxwellCalc.Notebook.ViewModels;

/// <summary>
/// One saved sheet: a named snapshot of just the calculation text lines — a reusable template that can be
/// recalled into any workspace. Distinct from a <see cref="WorkspaceEntry"/> (which carries units,
/// variables, functions and settings); a sheet is only the raw line texts.
/// </summary>
public partial class SavedSheetItem : ObservableObject
{
    /// <summary>Gets the stable id (unique within the library).</summary>
    public string Id { get; }

    /// <summary>Gets the raw text of each calculation line, in order.</summary>
    public IReadOnlyList<string> Doc { get; private set; }

    /// <summary>Gets the display name (unique by case-insensitive comparison).</summary>
    [ObservableProperty]
    private string _name;

    /// <summary>Gets the epoch-millisecond timestamp the sheet was last saved.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Meta))]
    private long _savedAt;

    /// <summary>
    /// Gets or sets whether this row is showing the transient "saved" flash (an <c>activeTint</c> background
    /// and a "saved" meta line) for ~1.6s after a save.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Meta))]
    private bool _isFlashing;

    /// <summary>Gets or sets whether this row is being inline-renamed (swaps in a text field).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRenaming))]
    private bool _isRenaming;

    /// <summary>Gets or sets the inline-rename draft text.</summary>
    [ObservableProperty]
    private string _renameDraft = string.Empty;

    /// <summary>Gets whether the row is showing its normal (non-rename) content.</summary>
    public bool IsNotRenaming => !IsRenaming;

    /// <summary>
    /// Gets the meta line under the name: the count of non-empty lines and the saved date (e.g.
    /// <c>3 lines · Jul 6</c>), or the word "saved" while the row is flashing after a save.
    /// </summary>
    public string Meta
    {
        get
        {
            if (IsFlashing)
                return "saved";
            int count = Doc.Count(text => !string.IsNullOrWhiteSpace(text));
            string date = DateTimeOffset.FromUnixTimeMilliseconds(SavedAt).LocalDateTime
                .ToString("MMM d", CultureInfo.CurrentCulture);
            return $"{count} {(count == 1 ? "line" : "lines")} · {date}";
        }
    }

    /// <summary>Creates a new <see cref="SavedSheetItem"/>.</summary>
    /// <param name="id">The stable id.</param>
    /// <param name="name">The display name.</param>
    /// <param name="doc">The raw line texts.</param>
    /// <param name="savedAt">The epoch-millisecond save timestamp.</param>
    public SavedSheetItem(string id, string name, IReadOnlyList<string> doc, long savedAt)
    {
        Id = id;
        _name = name;
        Doc = doc;
        _savedAt = savedAt;
    }

    /// <summary>Replaces the stored lines and bumps the timestamp (an overwrite).</summary>
    /// <param name="doc">The new line texts.</param>
    /// <param name="savedAt">The new save timestamp.</param>
    public void Update(IReadOnlyList<string> doc, long savedAt)
    {
        Doc = doc;
        SavedAt = savedAt;   // notifies Meta
    }
}

/// <summary>
/// Hosts the Saved Sheets command palette (⌘O / the "Sheets" access-bar chip): a lightweight library of
/// named line-snapshots kept separate from the workspace store. Owns the open/closed state, the live
/// search filter, the save-name draft with its overwrite confirm, and the inline-rename state. Recalling a
/// sheet replaces only the current sheet's editor lines — the active workspace and its units / variables /
/// functions / settings stay intact. Persists to <c>sheets.json</c> in the working directory.
/// </summary>
public partial class SavedSheetsViewModel : ViewModelBase
{
    private readonly SheetViewModel _sheet;
    private readonly string _sheetsFile;
    private readonly DispatcherTimer _flashTimer;
    private SavedSheetItem? _flashItem;

    private static readonly JsonSerializerOptions SaveOptions = new() { WriteIndented = true };

    /// <summary>Gets the full library, most-recent-first.</summary>
    public ObservableCollection<SavedSheetItem> Library { get; } = [];

    /// <summary>Gets the currently visible rows (the library filtered by <see cref="Search"/>).</summary>
    public ObservableCollection<SavedSheetItem> Filtered { get; } = [];

    /// <summary>Gets whether the palette overlay is open.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CloseCommand))]
    private bool _paletteOpen;

    /// <summary>Gets the live search/filter query (matches sheet names, case-insensitive substring).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoMatchText))]
    private string _search = string.Empty;

    /// <summary>Gets the save-name draft (the footer "name…" field), independent of the search box.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PrimarySaveCommand))]
    [NotifyPropertyChangedFor(nameof(IsExistingName))]
    [NotifyPropertyChangedFor(nameof(PrimarySaveLabel))]
    [NotifyPropertyChangedFor(nameof(ShowOverwriteHint))]
    [NotifyPropertyChangedFor(nameof(ShowArmedControls))]
    [NotifyPropertyChangedFor(nameof(ShowPrimaryButton))]
    [NotifyPropertyChangedFor(nameof(OverwriteHintText))]
    private string _nameDraft = string.Empty;

    /// <summary>Gets whether the overwrite confirm is armed (Cancel + red Overwrite showing).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOverwriteHint))]
    [NotifyPropertyChangedFor(nameof(ShowArmedControls))]
    [NotifyPropertyChangedFor(nameof(ShowPrimaryButton))]
    private bool _overwriteArm;

    /// <summary>Gets the trailing count shown on the "Sheets" chip (empty when the library is empty).</summary>
    public string CountLabel => Library.Count > 0 ? Library.Count.ToString(CultureInfo.CurrentCulture) : string.Empty;

    /// <summary>Gets whether the library is empty (drives the "No saved sheets yet…" empty state).</summary>
    public bool IsEmpty => Library.Count == 0;

    /// <summary>Gets whether the library is non-empty but the search filtered everything out.</summary>
    public bool HasNoMatches => Library.Count > 0 && Filtered.Count == 0;

    /// <summary>Gets the "No sheets match …" text for the no-match empty state.</summary>
    public string NoMatchText => $"No sheets match \"{Search}\".";

    /// <summary>Gets the existing sheet whose name matches the draft (case-insensitive), or <c>null</c>.</summary>
    public SavedSheetItem? ExistingMatch
    {
        get
        {
            string name = NameDraft.Trim();
            if (name.Length == 0)
                return null;
            return Library.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>Gets whether the draft names an existing sheet (so saving would overwrite it).</summary>
    public bool IsExistingName => ExistingMatch is not null;

    /// <summary>Gets the primary save button label: "Overwrite" for an existing name, else "Save".</summary>
    public string PrimarySaveLabel => IsExistingName ? "Overwrite" : "Save";

    /// <summary>Gets whether the "already exists — will overwrite" hint is shown (existing name, not yet armed).</summary>
    public bool ShowOverwriteHint => IsExistingName && !OverwriteArm;

    /// <summary>Gets whether the armed confirm controls (Cancel + red Overwrite) are shown.</summary>
    public bool ShowArmedControls => IsExistingName && OverwriteArm;

    /// <summary>Gets whether the single primary Save/Overwrite button is shown (i.e. not armed).</summary>
    public bool ShowPrimaryButton => !ShowArmedControls;

    /// <summary>Gets the "A sheet named … already exists" hint text.</summary>
    public string OverwriteHintText =>
        $"A sheet named \"{ExistingMatch?.Name}\" already exists — saving will overwrite it.";

    /// <summary>Creates a new <see cref="SavedSheetsViewModel"/> and loads the library from storage.</summary>
    /// <param name="sheet">The notebook sheet, read on save and replaced on recall.</param>
    public SavedSheetsViewModel(SheetViewModel sheet)
    {
        _sheet = sheet;
        _sheetsFile = Path.Combine(Directory.GetCurrentDirectory(), "sheets.json");
        _flashTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1600) };
        _flashTimer.Tick += OnFlashTick;

        Library.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CountLabel));
            OnPropertyChanged(nameof(IsEmpty));
        };

        Load();
        ApplyFilter();
    }

    /// <summary>Opens the palette, resetting the search, name draft, overwrite-arm and rename state.</summary>
    [RelayCommand]
    private void Open()
    {
        Search = string.Empty;
        NameDraft = string.Empty;
        OverwriteArm = false;
        ClearRenames();
        ApplyFilter();
        PaletteOpen = true;
    }

    /// <summary>Closes the palette (Esc or a scrim click), clearing the overwrite-arm and rename state.</summary>
    [RelayCommand(CanExecute = nameof(PaletteOpen))]
    private void Close()
    {
        OverwriteArm = false;
        ClearRenames();
        PaletteOpen = false;
    }

    /// <summary>
    /// Recalls a saved sheet: replaces the current sheet's editor lines with its snapshot and closes the
    /// palette. Only the lines change — the active workspace and its units / variables / functions /
    /// settings are untouched. The editor is persisted immediately after.
    /// </summary>
    /// <param name="item">The sheet to recall.</param>
    [RelayCommand]
    private void Recall(SavedSheetItem? item)
    {
        if (item is null)
            return;
        _sheet.SetLines(item.Doc);
        _sheet.Save();
        OverwriteArm = false;
        ClearRenames();
        PaletteOpen = false;
    }

    /// <summary>
    /// The footer's primary control. Empty name: no-op. New name: saves. Existing name, not yet armed: arms
    /// the overwrite confirm. Existing name, armed (the red Overwrite / Enter again): overwrites.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void PrimarySave()
    {
        if (!CanSave())
            return;
        if (IsExistingName && !OverwriteArm)
        {
            OverwriteArm = true;
            return;
        }
        SaveCurrentSheet();
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(NameDraft);

    /// <summary>Disarms the overwrite confirm without saving (the Cancel button).</summary>
    [RelayCommand]
    private void CancelOverwrite() => OverwriteArm = false;

    /// <summary>Starts inline-renaming a row (the ✎ button).</summary>
    /// <param name="item">The sheet to rename.</param>
    [RelayCommand]
    private void StartRename(SavedSheetItem? item)
    {
        if (item is null)
            return;
        ClearRenames();
        item.RenameDraft = item.Name;
        item.IsRenaming = true;
    }

    /// <summary>Commits an inline rename (Enter or blur); an empty name keeps the old one.</summary>
    /// <param name="item">The sheet being renamed.</param>
    [RelayCommand]
    private void CommitRename(SavedSheetItem? item)
    {
        if (item is null || !item.IsRenaming)
            return;
        string name = item.RenameDraft.Trim();
        if (name.Length > 0)
        {
            item.Name = name;
            SaveStore();
        }
        item.IsRenaming = false;
    }

    /// <summary>Cancels an inline rename without applying it (Escape).</summary>
    /// <param name="item">The sheet being renamed.</param>
    [RelayCommand]
    private void CancelRename(SavedSheetItem? item)
    {
        if (item is not null)
            item.IsRenaming = false;
    }

    /// <summary>Deletes a sheet immediately (the × button, single click).</summary>
    /// <param name="item">The sheet to delete.</param>
    [RelayCommand]
    private void Delete(SavedSheetItem? item)
    {
        if (item is null)
            return;
        if (ReferenceEquals(item, _flashItem))
        {
            _flashTimer.Stop();
            _flashItem = null;
        }
        Library.Remove(item);
        SaveStore();
        ApplyFilter();
    }

    // Upserts the current sheet under the draft name: an existing (case-insensitive) name is updated in
    // place (keeping its id and list position); a new name is unshifted to the top. The saved row flashes.
    private void SaveCurrentSheet()
    {
        string name = NameDraft.Trim();
        if (name.Length == 0)
            return;

        var doc = _sheet.Lines.Select(line => line.Text ?? string.Empty).ToList();
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        SavedSheetItem entry;
        if (ExistingMatch is { } existing)
        {
            existing.Update(doc, now);
            entry = existing;
        }
        else
        {
            entry = new SavedSheetItem("s" + now, name, doc, now);
            Library.Insert(0, entry);
        }

        NameDraft = string.Empty;
        OverwriteArm = false;
        SaveStore();
        ApplyFilter();
        Flash(entry);
    }

    // Starts the transient "saved" flash on a row, cancelling any prior flash first.
    private void Flash(SavedSheetItem item)
    {
        _flashTimer.Stop();
        if (_flashItem is not null)
            _flashItem.IsFlashing = false;
        _flashItem = item;
        item.IsFlashing = true;
        _flashTimer.Start();
    }

    private void OnFlashTick(object? sender, EventArgs e)
    {
        _flashTimer.Stop();
        if (_flashItem is not null)
            _flashItem.IsFlashing = false;
        _flashItem = null;
    }

    private void ClearRenames()
    {
        foreach (var item in Library)
            item.IsRenaming = false;
    }

    partial void OnNameDraftChanged(string value) => OverwriteArm = false;

    partial void OnSearchChanged(string value) => ApplyFilter();

    // Rebuilds Filtered from Library (name substring match), reconciling in place so unchanged rows keep
    // their containers (and any in-flight flash / rename visual state).
    private void ApplyFilter()
    {
        string query = Search.Trim();
        var target = query.Length == 0
            ? Library.ToList()
            : Library.Where(s => s.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        CollectionReconciler.Reconcile(Filtered, target);
        OnPropertyChanged(nameof(HasNoMatches));
    }

    // ---- Persistence -------------------------------------------------------------------------

    private void Load()
    {
        try
        {
            if (!File.Exists(_sheetsFile))
                return;
            var data = JsonSerializer.Deserialize<List<SavedSheetData>>(File.ReadAllText(_sheetsFile));
            if (data is null)
                return;
            foreach (var item in data)
            {
                if (string.IsNullOrEmpty(item.Id) || item.Name is null)
                    continue;
                Library.Add(new SavedSheetItem(item.Id, item.Name, item.Doc ?? [], item.SavedAt));
            }
        }
        catch
        {
            // A missing, empty, or malformed file leaves the library empty.
            Library.Clear();
        }
    }

    private void SaveStore()
    {
        try
        {
            var data = Library
                .Select(item => new SavedSheetData
                {
                    Id = item.Id,
                    Name = item.Name,
                    Doc = item.Doc.ToList(),
                    SavedAt = item.SavedAt,
                })
                .ToList();
            File.WriteAllText(_sheetsFile, JsonSerializer.Serialize(data, SaveOptions));
        }
        catch
        {
            // Persistence is best-effort; a failed write must not break the UI.
        }
    }

    // The serializable shape of one sheets.json entry.
    private sealed class SavedSheetData
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<string>? Doc { get; set; }

        public long SavedAt { get; set; }
    }
}
