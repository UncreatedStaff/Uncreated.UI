using Uncreated.Framework.UI;

namespace Uncreated.UI.Tests;

internal class HierarchyPathResolveRelativeTests
{
    [Test]
    public void RelativePathDontAssumeRelative()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", "./c/d");

        Assert.That(relative, Is.EqualTo("a/b/c/d"));
    }

    [Test]
    public void RelativePathAssumeRelative()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", "./c/d", assumeRelative: true);

        Assert.That(relative, Is.EqualTo("a/b/c/d"));
    }

    [Test]
    public void RelativePathEmptyDontAssumeRelative()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", ReadOnlySpan<char>.Empty);

        Assert.That(relative, Is.Empty);
    }

    [Test]
    public void RelativePathEmptyAssumeRelative()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", ReadOnlySpan<char>.Empty, assumeRelative: true);

        Assert.That(relative, Is.EqualTo("a/b"));
    }

    [Test]
    public void RelativePathLeadingSlashDontAssumeRelative()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", "/b/c");

        Assert.That(relative, Is.EqualTo("b/c"));
    }

    [Test]
    public void RelativePathLeadingSlashAssumeRelative()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", "/b/c", assumeRelative: true);

        Assert.That(relative, Is.EqualTo("a/b/b/c"));
    }

    [Test]
    public void RelativePathBacktrack()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", "../c");

        Assert.That(relative, Is.EqualTo("a/c"));
    }

    [Test]
    public void RelativePathBacktrackMultipleTimes()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", "../../c");

        Assert.That(relative, Is.EqualTo("c"));
    }

    [Test]
    public void RelativePathBacktrackToRoot()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", "../../c/..");

        Assert.That(relative, Is.Empty);
    }

    [Test]
    public void RelativePathRoot()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", "~/c");

        Assert.That(relative, Is.EqualTo("c"));
    }

    [Test]
    public void RelativePathBacktrackTooManyTimes()
    {
        Assert.Throws<ArgumentException>(() => UnturnedUIUtility.ResolveRelativePath("a/b", "../../.."));
    }

    [Test]
    public void RelativePathSpaceEscape()
    {
        string relative = UnturnedUIUtility.ResolveRelativePath("a/b", "\\ test/test2", assumeRelative: true);

        Assert.That(relative, Is.EqualTo("a/b/ test/test2"));
    }
}