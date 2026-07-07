using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// Brings an <see cref="ObservableCollection{T}"/> into line with a desired ordered list using the
/// smallest set of add / remove / move notifications, rather than <see cref="ObservableCollection{T}.Clear"/>
/// followed by a full re-add.
/// <para>
/// The command-palette panels re-project their rows wholesale on every workspace change (and on every
/// search keystroke), but the rows are value-equal records: an unchanged entry projects to a new
/// instance that <see cref="EqualityComparer{T}.Default"/> considers equal to the one already shown. This
/// reconciler keeps those equal instances in place, so a single add/remove touches only the one changed
/// row instead of raising a <c>Reset</c> that makes the (potentially non-virtualized) items host tear
/// down and re-realize every row's visual tree.
/// </para>
/// <para>
/// Assumes the desired sequence has no duplicate (value-equal) elements, which holds for the palette's
/// rows. The result is always exactly <paramref name="desired"/>; the move count is greedy, not minimal.
/// </para>
/// </summary>
public static class CollectionReconciler
{
    /// <summary>Mutates <paramref name="target"/> in place until it equals <paramref name="desired"/>.</summary>
    /// <typeparam name="T">The element type; reconciliation uses its default equality.</typeparam>
    /// <param name="target">The bound collection to update.</param>
    /// <param name="desired">The rows the collection should end up holding, in order.</param>
    public static void Reconcile<T>(ObservableCollection<T> target, IReadOnlyList<T> desired)
    {
        var comparer = EqualityComparer<T>.Default;

        // 1. Drop rows that are no longer wanted (walk from the end so earlier indices stay valid).
        var wanted = new HashSet<T>(desired, comparer);
        for (int i = target.Count - 1; i >= 0; i--)
        {
            if (!wanted.Contains(target[i]))
                target.RemoveAt(i);
        }

        // 2. Walk the desired order, putting the right row at each slot: skip it if it's already there,
        //    move an existing equal row up, or insert the new one.
        for (int i = 0; i < desired.Count; i++)
        {
            if (i < target.Count && comparer.Equals(target[i], desired[i]))
                continue;

            int existing = -1;
            for (int j = i + 1; j < target.Count; j++)
            {
                if (comparer.Equals(target[j], desired[i]))
                {
                    existing = j;
                    break;
                }
            }

            if (existing >= 0)
                target.Move(existing, i);
            else
                target.Insert(i, desired[i]);
        }

        // 3. Trim anything left past the desired length (defensive; step 1 usually handles it).
        while (target.Count > desired.Count)
            target.RemoveAt(target.Count - 1);
    }
}
