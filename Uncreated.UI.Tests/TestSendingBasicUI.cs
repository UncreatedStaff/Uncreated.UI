using Uncreated.Framework.UI;

namespace Uncreated.UI.Tests;

public class TestSendingBasicUI
{
    [SetUp]
    public void Setup()
    {
        TestUIProvider.Setup();
    }

    [Test]
    public void TestSendUI([Values(true, false)] bool isReliable)
    {
        UnturnedUI ui = new UnturnedUI(12345, hasElements: false, reliable: isReliable, debugLogging: true);

        ui.SendToPlayer(TestUIProvider.Connection);

        TestUIProvider.UIData? data = TestUIProvider.GetUIData((ushort)12345);

        Assert.That(data.HasValue, "UI exists");

        Assert.That(data.Value.Id, Is.EqualTo(12345));
        Assert.That(data.Value.Key, Is.EqualTo(ui.Key));
        Assert.That(data.Value.Global, Is.False);
        Assert.That(data.Value.Args, Is.Empty);
        Assert.That(data.Value.Reliable, Is.EqualTo(isReliable));
    }

    [Test]
    public void TestSendGlobalUI([Values(true, false)] bool isReliable)
    {
        UnturnedUI ui = new UnturnedUI(12345, hasElements: false, reliable: isReliable, debugLogging: true);

        ui.SendToAllPlayers();

        TestUIProvider.UIData? data = TestUIProvider.GetUIData((ushort)12345);

        Assert.That(data.HasValue, "UI exists");

        Assert.That(data.Value.Id, Is.EqualTo(12345));
        Assert.That(data.Value.Key, Is.EqualTo(ui.Key));
        Assert.That(data.Value.Global, Is.True);
        Assert.That(data.Value.Args, Is.Empty);
        Assert.That(data.Value.Reliable, Is.EqualTo(isReliable));
    }

    [Test]
    public void TestClearGlobalUI([Values(true, false)] bool isReliable)
    {
        UnturnedUI ui = new UnturnedUI(12345, hasElements: false, reliable: isReliable, debugLogging: true);

        ui.SendToPlayer(TestUIProvider.Connection);
        ui.ClearFromPlayer(TestUIProvider.Connection);

        TestUIProvider.UIData? data = TestUIProvider.GetUIData((ushort)12345);

        Assert.That(!data.HasValue, "UI was cleared");
    }

    [Test]
    public void TestClearUI([Values(true, false)] bool isReliable)
    {
        UnturnedUI ui = new UnturnedUI(12345, hasElements: false, reliable: isReliable, debugLogging: true);

        ui.SendToAllPlayers();
        ui.ClearFromAllPlayers();

        TestUIProvider.UIData? data = TestUIProvider.GetUIData((ushort)12345);

        Assert.That(!data.HasValue, "UI was cleared");
    }

    [Test]
    public void TestSendUIArgs_1([Values(true, false)] bool isReliable, [Values(true, false)] bool isGlobal)
    {
        UnturnedUI ui = new UnturnedUI(12345, hasElements: false, reliable: isReliable, debugLogging: true);

        if (isGlobal)
            ui.SendToAllPlayers("arg0");
        else
            ui.SendToPlayer(TestUIProvider.Connection, "arg0");

        TestUIProvider.UIData? data = TestUIProvider.GetUIData((ushort)12345);

        Assert.That(data.HasValue, "UI exists");

        Assert.That(data.Value.Id, Is.EqualTo(12345));
        Assert.That(data.Value.Key, Is.EqualTo(ui.Key));
        Assert.That(data.Value.Global, Is.EqualTo(isGlobal));
        Assert.That(data.Value.Args, Is.EquivalentTo(new string[] { "arg0" }));
        Assert.That(data.Value.Reliable, Is.EqualTo(isReliable));
    }

    [Test]
    public void TestSendUIArgs_2([Values(true, false)] bool isReliable, [Values(true, false)] bool isGlobal)
    {
        UnturnedUI ui = new UnturnedUI(12345, hasElements: false, reliable: isReliable, debugLogging: true);

        if (isGlobal)
            ui.SendToAllPlayers("arg0", "arg1");
        else
            ui.SendToPlayer(TestUIProvider.Connection, "arg0", "arg1");

        TestUIProvider.UIData? data = TestUIProvider.GetUIData((ushort)12345);

        Assert.That(data.HasValue, "UI exists");

        Assert.That(data.Value.Id, Is.EqualTo(12345));
        Assert.That(data.Value.Key, Is.EqualTo(ui.Key));
        Assert.That(data.Value.Global, Is.EqualTo(isGlobal));
        Assert.That(data.Value.Args, Is.EquivalentTo(new string[] { "arg0", "arg1" }));
        Assert.That(data.Value.Reliable, Is.EqualTo(isReliable));
    }

    [Test]
    public void TestSendUIArgs_3([Values(true, false)] bool isReliable, [Values(true, false)] bool isGlobal)
    {
        UnturnedUI ui = new UnturnedUI(12345, hasElements: false, reliable: isReliable, debugLogging: true);

        if (isGlobal)
            ui.SendToAllPlayers("arg0", "arg1", "arg2");
        else
            ui.SendToPlayer(TestUIProvider.Connection, "arg0", "arg1", "arg2");

        TestUIProvider.UIData? data = TestUIProvider.GetUIData((ushort)12345);

        Assert.That(data.HasValue, "UI exists");

        Assert.That(data.Value.Id, Is.EqualTo(12345));
        Assert.That(data.Value.Key, Is.EqualTo(ui.Key));
        Assert.That(data.Value.Global, Is.EqualTo(isGlobal));
        Assert.That(data.Value.Args, Is.EquivalentTo(new string[] { "arg0", "arg1", "arg2" }));
        Assert.That(data.Value.Reliable, Is.EqualTo(isReliable));
    }

    [Test]
    public void TestSendUIArgs_4([Values(true, false)] bool isReliable, [Values(true, false)] bool isGlobal)
    {
        UnturnedUI ui = new UnturnedUI(12345, hasElements: false, reliable: isReliable, debugLogging: true);

        if (isGlobal)
            ui.SendToAllPlayers("arg0", "arg1", "arg2", "arg3");
        else
            ui.SendToPlayer(TestUIProvider.Connection, "arg0", "arg1", "arg2", "arg3");

        TestUIProvider.UIData? data = TestUIProvider.GetUIData((ushort)12345);

        Assert.That(data.HasValue, "UI exists");

        Assert.That(data.Value.Id, Is.EqualTo(12345));
        Assert.That(data.Value.Key, Is.EqualTo(ui.Key));
        Assert.That(data.Value.Global, Is.EqualTo(isGlobal));
        Assert.That(data.Value.Args, Is.EquivalentTo(new string[] { "arg0", "arg1", "arg2", "arg3" }));
        Assert.That(data.Value.Reliable, Is.EqualTo(isReliable));
    }
}