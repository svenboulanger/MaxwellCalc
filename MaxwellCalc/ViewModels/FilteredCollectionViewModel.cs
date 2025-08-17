using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A generic view model for a collection of other view models.
    /// It supports removing of items as well.
    /// </summary>
    /// <typeparam name="M">The item view model type.</typeparam>
    public abstract partial class FilteredCollectionViewModel<M, TKey, TValue> : ViewModelBase
        where M : SelectableViewModelBase<TKey>, new()
    {
        private IWorkspace? _lastWorkspace;
        private IReadOnlyObservableDictionary<TKey, TValue>? _dictionary;
        private bool _holdOffHeaderChecked = false;

        /// <summary>
        /// Gets the shared model.
        /// </summary>
        public SharedModel Shared { get; }

        [ObservableProperty]
        private M? _selectedItem;

        [ObservableProperty]
        private ObservableCollection<M> _items = [];

        [ObservableProperty]
        private string _filter = string.Empty;

        [ObservableProperty]
        private bool _isHeaderChecked = false;

        /// <summary>
        /// Creates a new <see cref="FilteredCollectionViewModel{M}"/>.
        /// </summary>
        protected FilteredCollectionViewModel()
        {
            Shared = new();
            Shared.PropertyChanged += SharedPropertyChanged;

            _lastWorkspace = Shared.Workspace;
            if (_lastWorkspace is not null)
            {
                _dictionary = GetCollection(_lastWorkspace);
                _dictionary.DictionaryChanged += DictionaryChanged;
            }
            BuildModels();
        }

        /// <summary>
        /// Creates a new <see cref="FilteredCollectionViewModel{M}"/>.
        /// </summary>
        /// <param name="sp"></param>
        protected FilteredCollectionViewModel(IServiceProvider sp)
        {
            Shared = sp.GetRequiredService<SharedModel>();
            Shared.PropertyChanged += SharedPropertyChanged;

            _lastWorkspace = Shared.Workspace;
            if (_lastWorkspace is not null)
            {
                _dictionary = GetCollection(_lastWorkspace);
                _dictionary.DictionaryChanged += DictionaryChanged;
            }
            BuildModels();
        }

        private void SharedPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Shared.Workspace))
            {
                if (_dictionary is not null)
                    _dictionary.DictionaryChanged -= DictionaryChanged;
                _lastWorkspace = Shared.Workspace;
                if (_lastWorkspace is not null)
                {
                    _dictionary = GetCollection(_lastWorkspace);
                    _dictionary.DictionaryChanged += DictionaryChanged;
                }
                BuildModels();
            }
        }

        /// <summary>
        /// Gets the dictionary that needs to be treated.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <returns></returns>
        protected abstract IReadOnlyObservableDictionary<TKey, TValue> GetCollection(IWorkspace workspace);

        /// <summary>
        /// Updates a model with values from the original collection.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="key">The original dictionary key.</param>
        /// <param name="value">The original dictionary value.</param>
        protected abstract void UpdateModel(M model, TKey key, TValue value);

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

        partial void OnFilterChanged(string value)
        {
            if (IsHeaderChecked)
            {
                // Reset the header check boolean
                _holdOffHeaderChecked = true;
                IsHeaderChecked = false;
            }
            ApplyFilter();
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            foreach (var item in Items)
                item.Visible = MatchesFilter(item);
        }

        private void BuildModels()
        {
            Items.Clear();
            if (Shared.Workspace is null)
                return;
            var dictionary = GetCollection(Shared.Workspace);
            foreach (var pair in dictionary)
            {
                var model = new M();
                UpdateModel(model, pair.Key, pair.Value);
                model.Visible = MatchesFilter(model);
                InsertModel(model);
            }
        }

        private void DictionaryChanged(object? sender, DictionaryChangedEventArgs<TKey, TValue> e)
        {
            switch (e.Action)
            {
                case DictionaryChangeAction.Add:
                    foreach (var item in e.Items)
                    {
                        var model = new M() { Key = item.Key };
                        UpdateModel(model, item.Key, item.Value);
                        model.Visible = MatchesFilter(model);
                        InsertModel(model);
                    }
                    break;

                case DictionaryChangeAction.Replace:
                    foreach (var item in e.Items)
                    {
                        var model = Items.First(m => m.Key?.Equals(item.Key) ?? false);
                        UpdateModel(model, item.Key, item.Value);
                        model.Visible = MatchesFilter(model);
                    }
                    break;

                case DictionaryChangeAction.Remove:
                    foreach (var item in e.Items)
                    {
                        var model = Items.FirstOrDefault(m => m.Key?.Equals(item.Key) ?? false);
                        if (model is not null)
                            Items.Remove(model);
                    }
                    break;
            }
        }

        /// <summary>
        /// Inserts a model.
        /// </summary>
        /// <param name="model">The model.</param>
        private void InsertModel(M model)
        {
            // Insert into the main list
            int index = 0;
            while (index < Items.Count &&
                CompareModels(model, Items[index]) > 0)
                index++;
            Items.Insert(index, model);
        }

        protected virtual void RemoveItem(TKey key)
            => throw new NotImplementedException();

        /// <summary>
        /// Removes an item.
        /// </summary>
        /// <param name="model">The model.</param>
        [RelayCommand]
        private void RemoveItem(M model)
        {
            if (model is not null && model.Key is not null)
                RemoveItem(model.Key);
        }

        /// <summary>
        /// Removes any selected items.
        /// </summary>
        [RelayCommand]
        private void RemoveSelectedItems()
        {
            var toRemove = Items.Where(item => item.Selected).Select(item => item.Key).ToList();
            foreach (var key in toRemove)
            {
                if (key is not null)
                    RemoveItem(key);
            }
        }

        /// <summary>
        /// Removes all selected items.
        /// </summary>
        [RelayCommand]
        private void RemoveAllItems()
        {
            var toRemove = Items.Select(item => item.Key).ToList();
            foreach (var key in toRemove)
            {
                if (key is not null)
                    RemoveItem(key);
            }
        }

        partial void OnIsHeaderCheckedChanged(bool value)
        {
            if (_holdOffHeaderChecked)
                _holdOffHeaderChecked = false;
            else
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Visible)
                        Items[i].Selected = value;
                }
            }
        }
    }
}
