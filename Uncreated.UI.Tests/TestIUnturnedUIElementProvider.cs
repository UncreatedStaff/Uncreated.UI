using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Presets;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

public class TestIUnturnedUIElementProvider
{
    [SetUp]
    public void Setup()
    {
        TestUIProvider.Setup();
    }

    [Test]
    public void ProvidedElementExists()
    {
        TestUI ui = new TestUI();

        UnturnedUIElement? element = ui.Elements.FirstOrDefault(x => x.Path.Equals("I/Should/Be/Found"));

        Assert.That(element, Is.Not.Null);
        Assert.That(element.Owner, Is.EqualTo(ui));
    }

    [Test]
    public void FieldElementDoesntExist()
    {
        TestUI ui = new TestUI();

        UnturnedUIElement? element = ui.Elements.FirstOrDefault(x => x.Path.Equals("I/Shouldnt/Be/Found"));

        Assert.That(element, Is.Null);
    }

    private class TestUI : UnturnedUI
    {
        public readonly TestUnturnedUIElementProvider Provider = new TestUnturnedUIElementProvider();
        public TestUI() : base(12345) { }
    }

    public class TestUnturnedUIElementProvider : IUnturnedUIElementProvider
    {
        public UnturnedUIElement DontFindElement = new UnturnedUIElement("I/Shouldnt/Be/Found");

        public IEnumerable<UnturnedUIElement> EnumerateElements()
        {
            yield return new UnturnedUIElement("I/Should/Be/Found");
        }
    }
}