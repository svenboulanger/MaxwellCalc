﻿using System;
using System.Collections.Generic;

namespace MaxwellCalc.Core.Workspaces
{
    /// <summary>
    /// Describes a dictionary that can be registered to when items are added/removed or changed.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public interface IReadOnlyObservableDictionary<TKey, TValue>
        : IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// An event that is called when the dictionary changed.
        /// </summary>
        public event EventHandler<DictionaryChangedEventArgs<TKey, TValue>>? DictionaryChanged;
    }
}
