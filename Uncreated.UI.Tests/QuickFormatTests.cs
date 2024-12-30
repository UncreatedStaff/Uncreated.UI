using Uncreated.Framework.UI;

namespace Uncreated.UI.Tests;

internal class QuickFormatTests
{
    [Test]
    public void EmptyFormatter()
    {
        Assert.That(UnturnedUIUtility.QuickFormat("test {0} test", ReadOnlySpan<char>.Empty, 0), Is.EqualTo("test  test"));
    }

    [Test]
    public void EmptyFormatterCleanJoin()
    {
        Assert.That(UnturnedUIUtility.QuickFormat("test {0} test", ReadOnlySpan<char>.Empty, 0, ' '), Is.EqualTo("test test"));
    }

    [Test]
    public void BasicFormatter()
    {
        Assert.That(UnturnedUIUtility.QuickFormat("test {0} test", "t", 0), Is.EqualTo("test t test"));
    }

    [Test]
    public void NegativeFormatArgument()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UnturnedUIUtility.QuickFormat("test {-1} test", "t", -1));
    }

    [Test]
    public void PositiveFormatArgument()
    {
        Assert.That(UnturnedUIUtility.QuickFormat("test {1} test", "t", 1), Is.EqualTo("test t test"));
    }

    [Test]
    public void FormatToEmptyString()
    {
        Assert.That(UnturnedUIUtility.QuickFormat("{0}", "t", 0), Is.EqualTo("t"));
    }

    [Test]
    public void FormatEmptyToEmptyString()
    {
        Assert.That(UnturnedUIUtility.QuickFormat("{0}", string.Empty, 0), Is.Empty);
    }
}