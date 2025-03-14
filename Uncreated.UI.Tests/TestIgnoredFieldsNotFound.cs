using Uncreated.Framework.UI;

namespace Uncreated.UI.Tests;

public class TestIgnoredFieldsNotFound
{
    private static readonly Guid TestGuid = new Guid("b3f526c6-dfd9-427f-ba91-5b76925d4236");

    [SetUp]
    public void Setup()
    {
        TestUIProvider.Setup();
    }

    [Test]
    public void IgnoredFieldNotFound()
    {
        TestUI ui = new TestUI();

        UnturnedUIElement element = ui.IgnoredField;

        Assert.Throws<InvalidOperationException>(() => _ = element.Owner);
    }

    [Test]
    public void IgnoredPropertyNotFound()
    {
        TestUI ui = new TestUI();

        UnturnedUIElement element = ui.IgnoredProperty;

        Assert.Throws<InvalidOperationException>(() => _ = element.Owner);
    }

    [Test]
    public void IgnoredEnumerableNotFound()
    {
        TestUI ui = new TestUI();

        UnturnedUIElement element = ui.IgnoredEnumerable[0];

        Assert.Throws<InvalidOperationException>(() => _ = element.Owner);
    }

    private class TestUI : UnturnedUI
    {
        [DanielWillett.ReflectionTools.Ignore]
        public readonly UnturnedUIElement IgnoredField = new UnturnedUIElement("a/b/c");

        [DanielWillett.ReflectionTools.Ignore]
        public UnturnedUIElement IgnoredProperty { get; } = new UnturnedUIElement("a/b/c");

        [DanielWillett.ReflectionTools.Ignore]
        public readonly UnturnedUIElement[] IgnoredEnumerable = [ new UnturnedUIElement("a/b/c") ];

        public TestUI() : base(TestGuid) { }
    }
}