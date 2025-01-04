using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Presets;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

public class TestBasicElements
{
    [SetUp]
    public void Setup()
    {
        TestUIProvider.Setup();
    }

    [Test]
    public void OwnerSet()
    {
        TestUI ui = new TestUI();

        Assert.That(ui.Element.Owner, Is.EqualTo(ui));
    }

    [Test]
    public void NameAndPathSet()
    {
        TestUI ui = new TestUI();

        Assert.That(ui.Element.Path, Is.EqualTo("a/b/c"));
        Assert.That(ui.Element.Name.ToString(), Is.EqualTo("c"));
    }

    [Test]
    public void NameAndPathSetBasePath()
    {
        TestUIWithBasePath ui = new TestUIWithBasePath();

        Assert.That(ui.Element.Path, Is.EqualTo("base/path/a/b/c"));
        Assert.That(ui.Element.Name.ToString(), Is.EqualTo("c"));
    }

    [Test]
    public void NameAndPathSetBasePathWithRoot()
    {
        TestUIWithBasePath ui = new TestUIWithBasePath();

        Assert.That(ui.RootElement.Path, Is.EqualTo("a/b/c"));
        Assert.That(ui.RootElement.Name.ToString(), Is.EqualTo("c"));
    }

    [Test]
    public void NameAndPathSetBasePathWithDots()
    {
        TestUIWithBasePath ui = new TestUIWithBasePath();

        Assert.That(ui.UpElement.Path, Is.EqualTo("base/a/b/c"));
        Assert.That(ui.UpElement.Name.ToString(), Is.EqualTo("c"));
    }

    [Test]
    public void NameAndPathSetCompoundElement()
    {
        TestUI ui = new TestUI();

        Assert.That(ui.CompoundElement.Button.Path, Is.EqualTo("a/d"));
        Assert.That(ui.CompoundElement.Button.Name.ToString(), Is.EqualTo("d"));
        Assert.That(ui.CompoundElement.Label.Path, Is.EqualTo("a/d/lbl"));
        Assert.That(ui.CompoundElement.Label.Name.ToString(), Is.EqualTo("lbl"));
    }

    [Test]
    public void NameAndPathSetCompoundElementNoLabelPath()
    {
        TestUI ui = new TestUI();

        Assert.That(ui.CompoundElementNoLblPath.Button.Path, Is.EqualTo("a/d"));
        Assert.That(ui.CompoundElementNoLblPath.Button.Name.ToString(), Is.EqualTo("d"));
        Assert.That(ui.CompoundElementNoLblPath.Label.Path, Is.EqualTo("a/dLabel"));
        Assert.That(ui.CompoundElementNoLblPath.Label.Name.ToString(), Is.EqualTo("dLabel"));
    }

    private class TestUI : UnturnedUI
    {
        public readonly UnturnedUIElement Element = new UnturnedUIElement("a/b/c");

        public readonly LabeledButton CompoundElement = new LabeledButton("a/d", "./lbl");
        public readonly LabeledButton CompoundElementNoLblPath = new LabeledButton("a/d");

        public TestUI() : base(12345) { }
    }

    [UnturnedUI(BasePath = "base/path")]
    private class TestUIWithBasePath : UnturnedUI
    {
        public readonly UnturnedUIElement Element = new UnturnedUIElement("a/b/c");
        public readonly UnturnedUIElement RootElement = new UnturnedUIElement("~a/b/c");
        public readonly UnturnedUIElement UpElement = new UnturnedUIElement("../a/b/c");

        public TestUIWithBasePath() : base(12345) { }
    }
}