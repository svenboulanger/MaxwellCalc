using System;
using System.Collections.Generic;

namespace MaxwellCalc.Core.Workspaces
{
    /// <summary>
    /// Event arguments that are used when a <see cref="IObservableDictionary{TKey, TValue}"/> changed.
    /// </summary>
    /// <typeparam name="TKey">The key.</typeparam>
    /// <typeparam name="TValue">The value.</typeparam>
    /// <param name="action">The action.</param>
    /// <param name="items">The changed items.</param>
    public class DictionaryChangedEventArgs<TKey, TValue>(DictionaryChangeAction action, IReadOnlyList<KeyValuePair<TKey, TValue>> items) : EventArgs
    {
        /// <summary>
        /// Gets the action.
        /// </summary>
        public DictionaryChangeAction Action { get; } = action;

        /// <summary>
        /// Gets the items that changed.
        /// </summary>
        public IReadOnlyList<KeyValuePair<TKey, TValue>> Items { get; } = items ?? throw new ArgumentNullException(nameof(items));
    }
}
