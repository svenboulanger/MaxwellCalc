using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.Core.Workspaces
{
    /// <summary>
    /// An implementation of an <see cref="IReadonlyObservableDictionary{TKey, TValue}"/> that will
    /// give a read-only version of an underlying dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class MappedObservableDictionary<TKey, TValue, TOriginalValue>
        : IReadonlyObservableDictionary<TKey, TValue>
    {
        private readonly IObservableDictionary<TKey, TOriginalValue> _dictionary;
        private readonly Func<TOriginalValue, TValue> _mapper;

        /// <inheritdoc />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public TValue this[TKey key] => _mapper(_dictionary[key]);

        /// <inheritdoc />
        public IEnumerable<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc />
        public IEnumerable<TValue> Values => _dictionary.Values.Select(v => _mapper(v));

        /// <inheritdoc />
        public event EventHandler<DictionaryChangedEventArgs<TKey, TValue>>? DictionaryChanged;

        /// <summary>
        /// Creates a new <see cref="MappedObservableDictionary{TKey, TValue, TOriginalValue}"/>.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="mapper">The mapper.</param>
        /// <exception cref="ArgumentNullException">Thrown if either argument is <c>null</c>.</exception>
        public MappedObservableDictionary(IObservableDictionary<TKey, TOriginalValue> dictionary, Func<TOriginalValue, TValue> mapper)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dictionary.DictionaryChanged += OnDictionaryChanged;
        }

        private void OnDictionaryChanged(object? sender, DictionaryChangedEventArgs<TKey, TOriginalValue> args)
        {
            var newArgs = new DictionaryChangedEventArgs<TKey, TValue>(
                args.Action,
                [.. args.Items.Select(item => new KeyValuePair<TKey, TValue>(item.Key, _mapper(item.Value)))]);
            DictionaryChanged?.Invoke(this, newArgs);
        }

        /// <inheritdoc />
        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result = _dictionary.TryGetValue(key, out var originalValue);
            value = _mapper(originalValue);
            return result;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var pair in _dictionary)
                yield return new KeyValuePair<TKey, TValue>(pair.Key, _mapper(pair.Value));
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
