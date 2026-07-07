using MaxwellCalc.Notebook.ViewModels.Overlay;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MaxwellCalc.Tests;

public class CollectionReconcilerTests
{
    // A value-equal record whose instances are still distinguishable by reference, so we can assert that
    // unchanged rows keep their original instance (and therefore their realized UI container).
    private sealed record Row(int Id);

    private static (ObservableCollection<Row> target, List<NotifyCollectionChangedAction> actions) Track(params int[] ids)
    {
        var target = new ObservableCollection<Row>();
        foreach (int id in ids)
            target.Add(new Row(id));
        var actions = new List<NotifyCollectionChangedAction>();
        target.CollectionChanged += (_, e) => actions.Add(e.Action);
        return (target, actions);
    }

    [Fact]
    public void When_Reconciling_Expect_FinalSequenceMatchesDesired()
    {
        var (target, _) = Track(1, 2, 3);
        var desired = new List<Row> { new(3), new(4), new(1) };

        CollectionReconciler.Reconcile(target, desired);

        Assert.Equal(new[] { 3, 4, 1 }, target.Select(r => r.Id));
    }

    [Fact]
    public void When_AddingOneRow_Expect_NoResetAndOriginalInstancesKept()
    {
        var (target, actions) = Track(1, 2, 3);
        var original = target.ToArray();
        // Same rows plus a new one appended (value-equal projections of the unchanged rows).
        var desired = new List<Row> { new(1), new(2), new(3), new(4) };

        CollectionReconciler.Reconcile(target, desired);

        Assert.Equal(new[] { 1, 2, 3, 4 }, target.Select(r => r.Id));
        // The three unchanged rows are the very same instances — not re-created.
        Assert.Same(original[0], target[0]);
        Assert.Same(original[1], target[1]);
        Assert.Same(original[2], target[2]);
        // A single Add, and crucially never a Reset (which would tear down every container).
        Assert.DoesNotContain(NotifyCollectionChangedAction.Reset, actions);
        Assert.Single(actions, NotifyCollectionChangedAction.Add);
    }

    [Fact]
    public void When_RemovingOneRow_Expect_NoResetAndOtherInstancesKept()
    {
        var (target, actions) = Track(1, 2, 3);
        var original = target.ToArray();
        var desired = new List<Row> { new(1), new(3) };

        CollectionReconciler.Reconcile(target, desired);

        Assert.Equal(new[] { 1, 3 }, target.Select(r => r.Id));
        Assert.Same(original[0], target[0]);
        Assert.Same(original[2], target[1]);
        Assert.DoesNotContain(NotifyCollectionChangedAction.Reset, actions);
        Assert.Single(actions, NotifyCollectionChangedAction.Remove);
    }

    [Fact]
    public void When_Reordering_Expect_NoResetAndInstancesReused()
    {
        var (target, actions) = Track(1, 2, 3);
        var original = target.ToArray();
        var desired = new List<Row> { new(2), new(3), new(1) };

        CollectionReconciler.Reconcile(target, desired);

        Assert.Equal(new[] { 2, 3, 1 }, target.Select(r => r.Id));
        // Every retained id maps back to its original instance.
        Assert.Same(original[1], target[0]);
        Assert.Same(original[2], target[1]);
        Assert.Same(original[0], target[2]);
        Assert.DoesNotContain(NotifyCollectionChangedAction.Reset, actions);
    }

    [Fact]
    public void When_NothingChanges_Expect_NoNotifications()
    {
        var (target, actions) = Track(1, 2, 3);
        var desired = new List<Row> { new(1), new(2), new(3) };

        CollectionReconciler.Reconcile(target, desired);

        Assert.Equal(new[] { 1, 2, 3 }, target.Select(r => r.Id));
        Assert.Empty(actions);
    }

    [Fact]
    public void When_ClearingToEmpty_Expect_AllRemovedWithoutReset()
    {
        var (target, actions) = Track(1, 2);

        CollectionReconciler.Reconcile(target, new List<Row>());

        Assert.Empty(target);
        Assert.DoesNotContain(NotifyCollectionChangedAction.Reset, actions);
    }

    [Fact]
    public void When_StartingFromEmpty_Expect_ItemsInserted()
    {
        var (target, _) = Track();

        CollectionReconciler.Reconcile(target, new List<Row> { new(1), new(2) });

        Assert.Equal(new[] { 1, 2 }, target.Select(r => r.Id));
    }
}
