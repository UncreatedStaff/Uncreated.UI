using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;
using Uncreated.Framework.UI.Presets;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

public class TestNestedPatterns
{
    private static readonly Guid TestGuid = new Guid("b3f526c6-dfd9-427f-ba91-5b76925d4236");

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
            Assert.That(pattern1.Root.Path, Is.EqualTo($"Base/Path/P1s/Pattern1_{p1}"));
            Assert.That(pattern1.Pattern2s.Length, Is.EqualTo(50));
            for (int p2 = 1; p2 <= pattern1.Pattern2s.Length; ++p2)
            {
                TestUI.Pattern2 pattern2 = pattern1.Pattern2s[p2 - 1];
                Assert.That(pattern2.Root.Path, Is.EqualTo($"Base/Path/P1s/Pattern1_{p1}/P2s/Pattern2_{p2}"));
                Assert.That(pattern2.Labels.Length, Is.EqualTo(6));
                for (int l = 1; l <= pattern2.Labels.Length; ++l)
                {
                    UnturnedLabel label = pattern2.Labels[l - 1];
                    Assert.That(label.Owner, Is.EqualTo(ui));
                    Assert.That(label.Path, Is.EqualTo($"Base/Path/P1s/Pattern1_{p1}/P2s/Pattern2_{p2}/Pattern3_{l}/Value"));
                }

                Assert.That(pattern2.Buttons.Length, Is.EqualTo(6));
                for (int b = 1; b <= pattern2.Buttons.Length; ++b)
                {
                    UnturnedButton button = pattern2.Buttons[b - 1];
                    Assert.That(button.Owner, Is.EqualTo(ui));
                    Assert.That(button.Path, Is.EqualTo($"Base/Path/P1s/Pattern1_{p1}/P2s/Pattern2_{p2}/Button_{p1}/Button_{p2}_{b}"));
                }

                Assert.That(pattern2.LabeledButtons.Length, Is.EqualTo(4));
                for (int b = 1; b <= pattern2.LabeledButtons.Length; ++b)
                {
                    LabeledButton button = pattern2.LabeledButtons[b - 1];
                    Assert.That(button.Button.Owner, Is.EqualTo(ui));
                    Assert.That(button.Button.Path, Is.EqualTo($"Base/Path/P1s/Pattern1_{p1}/P2s/Pattern2_{p2}/LabeledButton_{p2}_{b}"));
                    Assert.That(button.Label.Owner, Is.EqualTo(ui));
                    Assert.That(button.Label.Path, Is.EqualTo($"Base/Path/P1s/Pattern1_{p1}/P2s/Pattern2_{p2}/LabeledButton_{p2}_{b}/Label_{p2}_{b}"));
                }
            }

            Assert.That(pattern1.Buttons.Length, Is.EqualTo(6));
            for (int b = 1; b <= pattern1.Buttons.Length; ++b)
            {
                UnturnedButton button = pattern1.Buttons[b - 1];
                Assert.That(button.Owner, Is.EqualTo(ui));
                Assert.That(button.Path, Is.EqualTo($"Base/Path/P1s/Pattern1_{p1}/Button_{p1}/Button_{p1}_{b}"));
            }
        }
    }

    [Test]
    public void PathsCorrectForSingularIndexInjection()
    {
        ServerListUI ui = new ServerListUI();

        Assert.That(ui.ServerLists.Length, Is.EqualTo(10));
        for (int sl = 1; sl <= ui.ServerLists.Length; ++sl)
        {
            ServerListUI.ServerList list = ui.ServerLists[sl - 1];
            for (int si = 1; si <= list.Servers.Length; ++si)
            {
                ServerListUI.ServerInfo info = list.Servers[si - 1];
                Assert.That(info.JoinButton.Path, Is.EqualTo($"ServerList_{sl}/Server_{si}/SL_{sl}_Server_{si}_JoinButton"));
            }
        }
    }

    #nullable disable
    public class ServerListUI : UnturnedUI
    {
        public readonly ServerList[] ServerLists = ElementPatterns.CreateArray<ServerList>("ServerList_{0}/", 1, to: 10);
        
        public ServerListUI() : base(TestGuid) { }

        public class ServerList : PatternRoot
        {
            [ArrayPattern(1, To = 4)]
            [Pattern("Server_{0}")]
            public ServerInfo[] Servers { get; set; }
        }

        public class ServerInfo : PatternRoot
        {
            [Pattern("SL_{1}_Server_{0}_JoinButton")]
            public UnturnedButton JoinButton { get; set; }
        }
    }


    [UnturnedUI(BasePath = "Base/Path")]
    public class TestUI : UnturnedUI
    {
        public readonly Pattern1[] Pattern1s = ElementPatterns.CreateArray<Pattern1>("P1s/Pattern1_{0}", 1, to: 2);

        public TestUI() : base(TestGuid) { }

        public class Pattern1 : PatternRoot
        {
            [ArrayPattern(1, To = 6)]
            [Pattern("Button_{1}_{0}", AdditionalPath = "Button_{1}")]
            public UnturnedButton[] Buttons { get; set; }

            [ArrayPattern(1, To = 50)]
            [Pattern("P2s/Pattern2_{0}")]
            public Pattern2[] Pattern2s { get; set; }
        }

        public class Pattern2 : PatternRoot
        {
            [ArrayPattern(1, To = 6)]
            [Pattern("Button_{1}_{0}", AdditionalPath = "Button_{2}")]
            public UnturnedButton[] Buttons { get; set; }

            [ArrayPattern(1, To = 6)]
            [Pattern("Pattern3_{0}/Value")]
            public UnturnedLabel[] Labels { get; set; }

            [ArrayPattern(1, To = 4)]
            [Pattern("LabeledButton_{1}_{0}", PresetPaths = [ "./Label_{1}_{0}" ])]
            public LabeledButton[] LabeledButtons { get; set; }
        }
#nullable restore
    }
}