using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;
using Uncreated.Framework.UI.Presets;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

public class TestPatternElementsWithoutRoot
{
    [SetUp]
    public void Setup()
    {
        TestUIProvider.Setup();
    }

    [Test]
    public void TestBasicPatternSuffix()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.Suffix.Path, Is.EqualTo("a/b/_c_{1}_d_Suffix"));
    }

    [Test]
    public void TestBasicPatternPrefix()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.Prefix.Path, Is.EqualTo("a/b/Prefix_c_{1}_d_"));
    }

    [Test]
    public void TestBasicPatternReplace()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.Replace.Path, Is.EqualTo("a/b/Replace"));
    }

    [Test]
    public void TestBasicPatternFormat()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.Format.Path, Is.EqualTo("a/b/_c_Format_d_"));
    }

    [Test]
    public void TestBasicPatternFormatCleanJoin()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.FormatCleanJoin.Path, Is.EqualTo("a/b/_c_Format_d_"));
    }

    [Test]
    public void TestBasicPatternFormatCleanJoinEmpty()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.FormatCleanJoinEmpty.Path, Is.EqualTo("a/b/_c_d_"));
    }

    [Test]
    public void TestBasicPatternNone()
    {
        TestUI ui = new TestUI();

        TestPattern pattern = ui.Pattern;

        Assert.That(pattern.None.Path, Is.EqualTo("a/b/_c_{1}_d_"));
    }


    [Test]
    public void TestBasicPatternArraySuffix()
    {
        TestUI ui = new TestUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.Suffix.Path, Is.EqualTo($"a/b/_{i + 1}_{{1}}_Suffix"));
        }
    }

    [Test]
    public void TestBasicPatternArrayPrefix()
    {
        TestUI ui = new TestUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.Prefix.Path, Is.EqualTo($"a/b/Prefix_{i + 1}_{{1}}_"));
        }
    }

    [Test]
    public void TestBasicPatternArrayReplace()
    {
        TestUI ui = new TestUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.Replace.Path, Is.EqualTo("a/b/Replace"));
        }
    }

    [Test]
    public void TestBasicPatternArrayFormat()
    {
        TestUI ui = new TestUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.Format.Path, Is.EqualTo($"a/b/_{i + 1}_Format_"));
        }
    }

    [Test]
    public void TestBasicPatternArrayFormatCleanJoin()
    {
        TestUI ui = new TestUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.FormatCleanJoin.Path, Is.EqualTo($"a/b/_{i + 1}_Format_"));
        }
    }

    [Test]
    public void TestBasicPatternArrayFormatCleanJoinEmpty()
    {
        TestUI ui = new TestUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.FormatCleanJoinEmpty.Path, Is.EqualTo($"a/b/_{i + 1}_"));
        }
    }

    [Test]
    public void TestBasicPatternArrayNone()
    {
        TestUI ui = new TestUI();

        for (int i = 0; i < 10; ++i)
        {
            TestPattern pattern = ui.Patterns[i];

            Assert.That(pattern.None.Path, Is.EqualTo($"a/b/_{i + 1}_{{1}}_"));
        }
    }

    public class TestPattern
    {
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
        public readonly TestPattern Pattern = ElementPatterns.Create<TestPattern>("a/b/_c_{1}_d_");
        public readonly TestPattern[] Patterns = ElementPatterns.CreateArray<TestPattern>("a/b/_{0}_{1}_", 1, to: 10);

        public TestUI() : base(12345, debugLogging: true) { }
    }
}