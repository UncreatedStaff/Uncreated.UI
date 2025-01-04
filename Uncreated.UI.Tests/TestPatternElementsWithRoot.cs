using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;
using Uncreated.Framework.UI.Presets;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

public class TestPatternElementsWithRoot
{
    [SetUp]
    public void Setup()
    {
        TestUIProvider.Setup();
    }

    [Test]
    public void TestBasicPatternRoot()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.Root.Path, Is.EqualTo("a/b/c"));
    }

    [Test]
    public void TestBasicPatternSuffix()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.Suffix.Path, Is.EqualTo("a/b/c/Suffix"));
    }

    [Test]
    public void TestBasicPatternPrefix()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.Prefix.Path, Is.EqualTo("a/b/c/Prefix"));
    }

    [Test]
    public void TestBasicPatternReplace()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.Replace.Path, Is.EqualTo("a/b/c/Replace"));
    }

    [Test]
    public void TestBasicPatternFormat()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.Format.Path, Is.EqualTo("a/b/c"));
    }

    [Test]
    public void TestBasicPatternFormatCleanJoin()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.FormatCleanJoin.Path, Is.EqualTo("a/b/c"));
    }

    [Test]
    public void TestBasicPatternFormatCleanJoinEmpty()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.FormatCleanJoinEmpty.Path, Is.EqualTo("a/b/c"));
    }

    [Test]
    public void TestBasicPatternNone()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.None.Path, Is.EqualTo("a/b/c"));
    }


    [Test]
    public void TestBasicPatternArrayRoot()
    {
        TestArrayUI ui = new TestArrayUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.Root.Path, Is.EqualTo($"a/b/c_{i + 1}"));
        }
    }

    [Test]
    public void TestBasicPatternArraySuffix()
    {
        TestArrayUI ui = new TestArrayUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.Suffix.Path, Is.EqualTo($"a/b/c_{i + 1}/Suffix"));
        }
    }

    [Test]
    public void TestBasicPatternArrayPrefix()
    {
        TestArrayUI ui = new TestArrayUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.Prefix.Path, Is.EqualTo($"a/b/c_{i + 1}/Prefix"));
        }
    }

    [Test]
    public void TestBasicPatternArrayReplace()
    {
        TestArrayUI ui = new TestArrayUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.Replace.Path, Is.EqualTo($"a/b/c_{i + 1}/Replace"));
        }
    }

    [Test]
    public void TestBasicPatternArrayFormat()
    {
        TestArrayUI ui = new TestArrayUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.Format.Path, Is.EqualTo($"a/b/c_{i + 1}"));
        }
    }

    [Test]
    public void TestBasicPatternArrayFormatCleanJoin()
    {
        TestArrayUI ui = new TestArrayUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.FormatCleanJoin.Path, Is.EqualTo($"a/b/c_{i + 1}"));
        }
    }

    [Test]
    public void TestBasicPatternArrayFormatCleanJoinEmpty()
    {
        TestArrayUI ui = new TestArrayUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.FormatCleanJoinEmpty.Path, Is.EqualTo($"a/b/c_{i + 1}"));
        }
    }

    [Test]
    public void TestBasicPatternArrayNone()
    {
        TestArrayUI ui = new TestArrayUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.None.Path, Is.EqualTo($"a/b/c_{i + 1}"));
        }
    }

    public class TestPattern
    {
        [Pattern("", Root = true)]
        public required UnturnedUIElement Root { get; init; }

        [Pattern("Suffix", Mode = FormatMode.Suffix)]
        public required UnturnedUIElement Suffix { get; init; }

        [Pattern("Prefix", Mode = FormatMode.Prefix)]
        public required UnturnedUIElement Prefix { get; init; }

        [Pattern("Replace", Mode = FormatMode.Replace)]
        public required UnturnedUIElement Replace { get; init; }

        [Pattern("Format", Mode = FormatMode.Format)]
        public required UnturnedUIElement Format { get; init; }

        [Pattern("Format", Mode = FormatMode.Format, CleanJoin = '_')]
        public required UnturnedUIElement FormatCleanJoin { get; init; }

        [Pattern("", Mode = FormatMode.Format, CleanJoin = '_')]
        public required UnturnedUIElement FormatCleanJoinEmpty { get; init; }

        [Pattern("None", Mode = FormatMode.None)]
        public required UnturnedUIElement None { get; init; }
    }

    private class TestUI : UnturnedUI
    {
        public readonly TestPattern Pattern = ElementPatterns.Create<TestPattern>("a/b/c/");

        public TestUI() : base(12345, debugLogging: true) { }
    }
    private class TestArrayUI : UnturnedUI
    {
        public readonly TestPattern[] Patterns = ElementPatterns.CreateArray<TestPattern>("a/b/c_{0}/", 1, to: 10);

        public TestArrayUI() : base(12345, debugLogging: true) { }
    }
}