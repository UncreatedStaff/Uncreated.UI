using Uncreated.Framework.UI;

namespace Uncreated.UI.Tests;

internal class HierarchyPathIsRootedTests
{
    [Test]
    public void IsRootedEmptyPath()
    {
        Assert.That(UnturnedUIUtility.IsRooted(string.Empty), Is.True);
    }

    [Test]
    public void IsRootedWhitespace()
    {
        Assert.That(UnturnedUIUtility.IsRooted("    "), Is.True);
    }

    [Test]
    public void IsRootedStartsWithDotButRooted()
    {
        Assert.That(UnturnedUIUtility.IsRooted(".test/test/test"), Is.True);
    }

    [Test]
    public void IsRootedStandardRootedPathNoSlash()
    {
        Assert.That(UnturnedUIUtility.IsRooted("test/test/test"), Is.True);
    }

    [Test]
    public void IsRootedStandardRootedPathSlash()
    {
        Assert.That(UnturnedUIUtility.IsRooted("/test/test/test"), Is.True);
    }

    [Test]
    public void IsRootedUnrootedPathOneDotLong()
    {
        Assert.That(UnturnedUIUtility.IsRooted("./test/test/test"), Is.False);
    }

    [Test]
    public void IsRootedUnrootedPathTwoDotsLong()
    {
        Assert.That(UnturnedUIUtility.IsRooted("../test/test/test"), Is.False);
    }

    [Test]
    public void IsRootedUnrootedPathOneDotShort()
    {
        Assert.That(UnturnedUIUtility.IsRooted("."), Is.False);
    }

    [Test]
    public void IsRootedUnrootedPathTwoDotsShort()
    {
        Assert.That(UnturnedUIUtility.IsRooted(".."), Is.False);
    }
}
