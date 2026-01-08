using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.Core.Dictionaries;

/// <summary>
/// An implementation of <see cref="IObservableDictionary{TKey, TValue}"/>.
/// </summary>
public class ObservableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _dictionary;

    /// <inheritdoc />
    public event EventHandler<DictionaryChangedEventArgs<TKey, TValue>>? DictionaryChanged;

    /// <inheritdoc />
    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            if (_dictionary.TryGetValue(key, out var existing) && existing is not null)
            {
                if (existing.Equals(value))
                    return;

                // Replace value
                _dictionary[key] = value;
                OnDictionaryChanged(new(
                    DictionaryChangeAction.Replace,
                    [new KeyValuePair<TKey, TValue>(key, value)]));
            }
            else
            {
                // Add value
                _dictionary[key] = value;
                OnDictionaryChanged(new(
                    DictionaryChangeAction.Add,
                    [new KeyValuePair<TKey, TValue>(key, value)]));
            }
        }
    }

    /// <inheritdoc />
    public ICollection<TKey> Keys => _dictionary.Keys;

    /// <inheritdoc />
    public ICollection<TValue> Values => _dictionary.Values;

    /// <inheritdoc />
    public int Count => _dictionary.Count;

    /// <inheritdoc />
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    /// <summary>
    /// Creates a new <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    public ObservableDictionary(IEqualityComparer<TKey>? comparer = null)
    {
        _dictionary = new Dictionary<TKey, TValue>(comparer);
    }

    /// <summary>
    /// A method that is called when the dictionary changed.
    /// </summary>
    /// <param name="args">The arguments.</param>
    protected virtual void OnDictionaryChanged(DictionaryChangedEventArgs<TKey, TValue> args)
        => DictionaryChanged?.Invoke(this, args);

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value); // Will throw an exception if an item with the name already exists
        OnDictionaryChanged(new(
            DictionaryChangeAction.Add,
            [new KeyValuePair<TKey, TValue>(key, value)]));
    }

    /// <inheritdoc />
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if (_dictionary.TryGetValue(key, out var value))
        {
            _dictionary.Remove(key);
            OnDictionaryChanged(new(
                DictionaryChangeAction.Remove,
                [new KeyValuePair<TKey, TValue>(key, value)]));
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

    /// <inheritdoc />
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        => Add(item.Key, item.Value);

    /// <inheritdoc />
    public void Clear()
    {
        // Clear the whole dictionary
        var list = _dictionary.ToList();
        _dictionary.Clear();
        OnDictionaryChanged(new(
            DictionaryChangeAction.Remove,
            list));
    }

    /// <inheritdoc />
    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);

    /// <inheritdoc />
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        => Remove(item.Key);

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
