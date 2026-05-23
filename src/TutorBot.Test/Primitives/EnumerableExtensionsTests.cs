using Shouldly;

namespace System.Collections.Generic.Tests;

public class EnumerableExtensionsTests
{
    [Fact]
    public void IsMultiple_MoreThanOne_ReturnsTrue()
    {
        new[] { 1, 2, 3 }.IsMultiple().ShouldBeTrue();
    }

    [Fact]
    public void IsMultiple_SingleItem_ReturnsFalse()
    {
        new[] { 1 }.IsMultiple().ShouldBeFalse();
    }

    [Fact]
    public void IsMultiple_Empty_ReturnsFalse()
    {
        Array.Empty<int>().IsMultiple().ShouldBeFalse();
    }

    [Fact]
    public void PeekCount_Sufficient_ReturnsTrue()
    {
        new[] { 1, 2, 3 }.PeekCount(2).ShouldBeTrue();
    }

    [Fact]
    public void PeekCount_Insufficient_ReturnsFalse()
    {
        new[] { 1, 2 }.PeekCount(5).ShouldBeFalse();
    }

    [Fact]
    public void PeekCount_NullSource_ReturnsFalse()
    {
        ((IEnumerable<int>)null!).PeekCount(1).ShouldBeFalse();
    }

    [Fact]
    public void PeekCount_WithOutput_ReturnsTrue()
    {
        new[] { 1, 2, 3 }.PeekCount(2, out var count).ShouldBeTrue();
        count.ShouldBe(2);
    }

    [Fact]
    public void RecursiveSelect_SingleItem_ReturnsItem()
    {
        var result = 5.RecursiveSelect(x => x > 0 ? [x - 1] : []);
        result.ShouldBe([5, 4, 3, 2, 1, 0]);
    }

    [Fact]
    public void RecursiveSelect_MultipleItems_TraversesTree()
    {
        var tree = new TestNode("root", [
            new TestNode("child1", [new TestNode("grandchild")]),
            new TestNode("child2")
        ]);

        var result = new[] { tree }.RecursiveSelect(x => x.Children!);

        result.Select(x => x.Name).ShouldBe(["root", "child1", "grandchild", "child2"]);
    }

    private record TestNode(string Name, List<TestNode>? Children = null);

    [Fact]
    public void ChunkBy_EmptySource_ReturnsEmpty()
    {
        Array.Empty<int>().ChunkBy(3).ShouldBeEmpty();
    }

    [Fact]
    public void ChunkBy_ExactSize_ReturnsChunks()
    {
        var result = new[] { 1, 2, 3, 4, 5, 6 }.ChunkBy(3).Select(x => x.ToArray()).ToArray();
        result.ShouldBe([[1, 2, 3], [4, 5, 6]]);
    }

    [Fact]
    public void ChunkBy_WithRemainder_ReturnsPartialLast()
    {
        var result = new[] { 1, 2, 3, 4, 5 }.ChunkBy(3).Select(x => x.ToArray()).ToArray();
        result.ShouldBe([[1, 2, 3], [4, 5]]);
    }

    [Fact]
    public void ChunkBy_NullSource_Throws() =>
        Should.Throw<ArgumentNullException>(() => ((IEnumerable<int>)null!).ChunkBy(3).ToList());

    [Fact]
    public void ChunkBy_InvalidSize_Throws() =>
        Should.Throw<ArgumentException>(() => new[] { 1 }.ChunkBy(0).ToList());

    [Fact]
    public void ChunkByList_ReturnsLists()
    {
        var result = new[] { 1, 2, 3 }.ChunkByList(2);
        result.ShouldBe([[1, 2], [3]]);
        result.First().ShouldBeOfType<List<int>>();
    }

    [Fact]
    public void ChunkByArray_ReturnsArrays()
    {
        var result = new[] { 1, 2, 3 }.ChunkByArray(2);
        result.ShouldBe([[1, 2], [3]]);
        result.First().ShouldBeOfType<int[]>();
    }

    [Fact]
    public void RecursiveSelect_WithCycle_StopsByCycleDetection()
    {
        var item = new CyclicNode("a");
        var item2 = new CyclicNode("b");
        item.Next = item2;
        item2.Next = item;

        var result = item.RecursiveSelect(x => x.Next!);

        result.Select(x => x.Name).ShouldBe(["a", "b"]);
    }

    private class CyclicNode(string Name)
    {
        public CyclicNode? Next { get; set; }
        public string Name { get; } = Name;
    }

    [Fact]
    public void RecursiveSelect_WithRecordCycle_StopsByCycleDetection()
    {
        var a = new CyclicRecord("a");
        var b = new CyclicRecord("b");
        a.Next = b;
        b.Next = a;

        var result = a.RecursiveSelect(x => x.Next!);

        result.Select(x => x.Name).ShouldBe(["a", "b"]);
    }

    [Fact]
    public void RecursiveSelect_WithRecordCycle_MultipleChildren_StopsByCycleDetection()
    {
        var a = new CyclicRecord("a");
        var b = new CyclicRecord("b");
        a.Next = b;
        b.Next = a;

        var result = a.RecursiveSelect(x => x.Next is not null ? [x.Next] : []);

        result.Select(x => x.Name).ShouldBe(["a", "b"]);
    }

    [Fact]
    public void RecursiveSelect_WithRecordCycle_FromEnumerable_StopsByCycleDetection()
    {
        var a = new CyclicRecord("a");
        var b = new CyclicRecord("b");
        var c = new CyclicRecord("c");
        a.Next = b;
        b.Next = a;
        c.Next = c;

        var result = new[] { a, c }.RecursiveSelect(x => x.Next!);

        result.Select(x => x.Name).ShouldBe(["a", "b", "c"]);
    }

    private record CyclicRecord(string Name)
    {
        public CyclicRecord? Next { get; set; }
    }
}
