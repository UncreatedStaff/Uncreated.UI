using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;

namespace Uncreated.UI.Tests;

public class TestCircularReferences
{
    [SetUp]
    public void Setup()
    {
        TestUIProvider.Setup();
    }

    [Test]
    public void CircularReferencePatternThrows()
    {
        Assert.Throws<ElementPatternCreationException>(() =>
        {
            _ = new TestCircularPatternUI();
        });
    }

    [Test]
    public void CircularReferenceDiscoveryIgnored()
    {
        TestCircularExplicitUI ui = new TestCircularExplicitUI();

        Assert.That(ui.Refs.Test1.Owner, Is.EqualTo(ui));
        Assert.That(ui.Refs.Circ.Test2.Owner, Is.EqualTo(ui));
        Assert.That(ui.Refs.Circ.Circ.Test1, Is.EqualTo(ui.Refs.Test1));
    }
    
    public class TestCircularPatternUI : UnturnedUI
    {
        public CircularReferencePatterns1[] Refs = ElementPatterns.CreateArray<CircularReferencePatterns1>("a/b/c", 0, to: 1);

        public TestCircularPatternUI() : base(12345)
        {
            
        }
    }

    #nullable disable
    public class TestCircularExplicitUI : UnturnedUI
    {
        public CircularReferenceExplicit1 Refs = new CircularReferenceExplicit1();

        public TestCircularExplicitUI() : base(12345)
        {
            
        }
    }

    public class CircularReferenceExplicit1
    {
        public CircularReferenceExplicit2 Circ;
        public UnturnedUIElement Test1 { get; } = new UnturnedUIElement("test1");
        public CircularReferenceExplicit1()
        {
            Circ = new CircularReferenceExplicit2(this);
        }
    }

    public class CircularReferenceExplicit2
    {
        public CircularReferenceExplicit1 Circ;
        public UnturnedUIElement Test2 { get; } = new UnturnedUIElement("test2");
        public CircularReferenceExplicit2(CircularReferenceExplicit1 circ)
        {
            Circ = circ;
        }
    }

    public class CircularReferencePatterns1
    {
        [Pattern("Circ1")]
        public CircularReferencePatterns2 Circ { get; set; }
    }

    public class CircularReferencePatterns2
    {
        [Pattern("Circ2")]
        public CircularReferencePatterns1 Circ { get; set; }
    }
    #nullable restore
}