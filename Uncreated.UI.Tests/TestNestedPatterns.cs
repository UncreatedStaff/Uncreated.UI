using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

public class TestNestedPatterns
{
    [SetUp]
    public void Setup()
    {
        TestUIProvider.Setup();
    }

    [Test]
    public void PathsCorrect()
    {
        TestUI ui = new TestUI();
        
        Assert.That(ui.Pattern1s.Length, Is.EqualTo(2));
        for (int p1 = 1; p1 <= ui.Pattern1s.Length; ++p1)
        {
            TestUI.Pattern1 pattern1 = ui.Pattern1s[p1 - 1];
            Assert.That(pattern1.Pattern2s.Length, Is.EqualTo(50));
            for (int p2 = 1; p2 <= pattern1.Pattern2s.Length; ++p2)
            {
                TestUI.Pattern2 pattern2 = pattern1.Pattern2s[p2 - 1];
                Assert.That(pattern2.Labels.Length, Is.EqualTo(6));
                for (int l = 1; l <= pattern2.Labels.Length; ++l)
                {
                    UnturnedLabel label = pattern2.Labels[l - 1];
                    Assert.That(label.Owner, Is.EqualTo(ui));
                    Assert.That(label.Path, Is.EqualTo($"Base/Path/P1s/Pattern1_{p1}/P2s/Pattern2_{p2}/Pattern3_{l}/Value"));
                }
            }
        }
    }

    [UnturnedUI(BasePath = "Base/Path")]
    public class TestUI : UnturnedUI
    {
        public readonly Pattern1[] Pattern1s = ElementPatterns.CreateArray<Pattern1>("P1s/Pattern1_{0}", 1, to: 2);

        public TestUI() : base(0) { }

#nullable disable
        public class Pattern1 : PatternRoot
        {
            [ArrayPattern(1, To = 50)]
            [Pattern("P2s/Pattern2_{0}")]
            public Pattern2[] Pattern2s { get; set; }
        }

        public class Pattern2 : PatternRoot
        {
            [ArrayPattern(1, To = 6)]
            [Pattern("Pattern3_{0}/Value")]
            public UnturnedLabel[] Labels { get; set; }
        }
#nullable restore
    }
}