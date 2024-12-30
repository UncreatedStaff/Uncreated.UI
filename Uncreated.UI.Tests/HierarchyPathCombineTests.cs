using Uncreated.Framework.UI;

namespace Uncreated.UI.Tests;

internal class HierarchyPathCombineTests
{
    [Test]
    public void CombineTwoEmpty()
    {
        Assert.That(UnturnedUIUtility.CombinePath(ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty), Is.Empty);
    }

    [Test]
    public void CombineOneEmpty()
    {
        Assert.That(UnturnedUIUtility.CombinePath("test", ReadOnlySpan<char>.Empty), Is.EqualTo("test/"));
        Assert.That(UnturnedUIUtility.CombinePath(ReadOnlySpan<char>.Empty, "test"), Is.EqualTo("test"));
    }

    [Test]
    public void CombineTwoSingles()
    {
        Assert.That(UnturnedUIUtility.CombinePath("a", "b"), Is.EqualTo("a/b"));
    }

    [Test]
    public void CombineSingleAndMultiple()
    {
        Assert.That(UnturnedUIUtility.CombinePath("a/b", "c"), Is.EqualTo("a/b/c"));
    }

    [Test]
    public void CombineMultiple()
    {
        Assert.That(UnturnedUIUtility.CombinePath("a/b", "c/d"), Is.EqualTo("a/b/c/d"));
    }

    [Test]
    public void CombineRelative()
    {
        Assert.That(UnturnedUIUtility.CombinePath("a/b", "./c/d"), Is.EqualTo("a/b/./c/d"));
    }

    [Test]
    public void CombineRelativeMultipleDots()
    {
        Assert.That(UnturnedUIUtility.CombinePath("a/b", "../c/d"), Is.EqualTo("a/b/../c/d"));
    }
}