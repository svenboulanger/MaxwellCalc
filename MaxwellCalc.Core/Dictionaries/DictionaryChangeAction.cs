namespace MaxwellCalc.Core.Dictionaries;

/// <summary>
/// An enumeration of possible actions for the <see cref="DictionaryChangedEventArgs{TKey, TValue}"/>.
/// </summary>
public enum DictionaryChangeAction
{
    Add,
    Remove,
    Replace
}
