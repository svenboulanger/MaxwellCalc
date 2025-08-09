using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A generic view model for a collection of other view models.
    /// It supports removing of items as well.
    /// </summary>
    /// <typeparam name="M">The item view model type.</typeparam>
    public abstract partial class FilteredCollectionViewModel<M> : ViewModelBase
    {
        private IWorkspace? _lastWorkspace;

        /// <summary>
        /// Gets the shared model.
        /// </summary>
        public SharedModel Shared { get; }

        [ObservableProperty]
        private M? _selectedItem;

        [ObservableProperty]
        private ObservableCollection<M> _items = [];

        [ObservableProperty]
        private ObservableCollection<M> _filteredItems = [];

        [ObservableProperty]
        private string _filter = string.Empty;

        /// <summary>
        /// Creates a new <see cref="FilteredCollectionViewModel{M}"/>.
        /// </summary>
        protected FilteredCollectionViewModel()
        {
            Shared = new();
            _lastWorkspace = Shared.Workspace;
        }

        /// <summary>
        /// Creates a new <see cref="FilteredCollectionViewModel{M}"/>.
        /// </summary>
        /// <param name="sp"></param>
        protected FilteredCollectionViewModel(IServiceProvider sp)
        {
            Shared = sp.GetRequiredService<SharedModel>();
            _lastWorkspace = Shared.Workspace;

            if (Shared.Workspace is not null)
                BuildModels();
            Shared.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Shared.Workspace))
                    BuildModels();
            };
        }

        /// <summary>
        /// Checks whether the model matches the filter.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Returns <c>true</c> if the model matches the filter; otherwise, <c>false</c>.</returns>
        protected abstract bool MatchesFilter(M model);

        /// <summary>
        /// Compares two models for ordering.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <returns>Should return <c>-1</c> if <paramref name="a"/> comes before <paramref name="b"/>, <c>1</c> if <paramref name="a"/> comes after <paramref name="b"/>, and 0 if they are equal.</returns>
        protected abstract int CompareModels(M a, M b);

        /// <summary>
        /// Gets all the models from a workspace.
        /// </summary>
        /// <param name="oldWorkspace">The old workspace.</param>
        /// <param name="newWorkspace">The new workspace.</param>
        /// <returns>The models.</returns>
        protected abstract IEnumerable<M> ChangeWorkspace(IWorkspace? oldWorkspace, IWorkspace? newWorkspace);

        /// <summary>
        /// Removes an item from the workspace if it was removed from the model collection.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="model">The model</param>
        protected abstract void RemoveModelFromWorkspace(IWorkspace workspace, M model);

        [RelayCommand]
        private void RemoveItem(M model)
        {
            Items.Remove(model);
            FilteredItems.Remove(model);

            if (Shared.Workspace is not null)
                RemoveModelFromWorkspace(Shared.Workspace, model);
        }

        partial void OnFilterChanged(string value)
        {
            OnApplyingNewFilter();
            ApplyFilter();
        }

        protected virtual void OnApplyingNewFilter() { }

        [RelayCommand]
        private void ApplyFilter()
        {
            FilteredItems.Clear();
            foreach (var item in Items.Where(MatchesFilter))
                FilteredItems.Add(item);
        }

        private void BuildModels()
        {
            Items.Clear();
            FilteredItems.Clear();
            if (Shared.Workspace is null)
                return;
            foreach (var model in ChangeWorkspace(_lastWorkspace, Shared.Workspace))
                InsertModel(model);
            _lastWorkspace = Shared.Workspace;
        }

        /// <summary>
        /// Inserts a model.
        /// </summary>
        /// <param name="model">The model.</param>
        protected void InsertModel(M model)
        {
            // Insert into the main list
            int index = 0;
            while (index < Items.Count &&
                CompareModels(model, Items[index]) > 0)
                index++;
            Items.Insert(index, model);

            // Insert into the filtered list
            if (MatchesFilter(model))
            {
                index = 0;
                while (index < FilteredItems.Count &&
                    CompareModels(model, FilteredItems[index]) > 0)
                    index++;
                FilteredItems.Insert(index, model);
            }
        }
    }
}
