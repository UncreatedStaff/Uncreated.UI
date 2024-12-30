using SDG.NetTransport;
using SDG.Unturned;
using Uncreated.Framework.UI;
using Action = System.Action;

namespace Uncreated.UI.Tests;
public class TestUIProvider : IUnturnedUIProvider
{
    public bool IsValidThread()
    {
        return true;
    }

    public void DispatchToValidThread(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public event EffectManager.EffectButtonClickedHandler? OnButtonClicked;
    public event EffectManager.EffectTextCommittedHandler? OnTextCommitted;

    public static void Setup()
    {
        UnturnedUIProvider.Instance = new TestUIProvider();
    }

    public static void PressButton(Player player, string name)
    {
        ((TestUIProvider)UnturnedUIProvider.Instance).OnButtonClicked?.Invoke(player, name);
    }

    public static void CommitText(Player player, string name, string text)
    {
        ((TestUIProvider)UnturnedUIProvider.Instance).OnTextCommitted?.Invoke(player, name, text);
    }

    public void ClearByIdGlobal(ushort id)
    {
        Console.WriteLine($"~~~ CLEAR ID {id} FOR ALL PLAYERS ~~~");
    }

    public void SendUIGlobal(ushort id, short key, bool isReliable)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS ~~~");
    }

    public void SendUIGlobal(ushort id, short key, bool isReliable, string arg0)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS {{0}} = {arg0} ~~~");
    }

    public void SendUIGlobal(ushort id, short key, bool isReliable, string arg0, string arg1)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS {{0}} = {arg0} {{1}} = {arg1} ~~~");
    }

    public void SendUIGlobal(ushort id, short key, bool isReliable, string arg0, string arg1, string arg2)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS {{0}} = {arg0} {{1}} = {arg1} {{2}} = {arg2} ~~~");
    }

    public void SendUIGlobal(ushort id, short key, bool isReliable, string arg0, string arg1, string arg2, string arg3)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS {{0}} = {arg0} {{1}} = {arg1} {{2}} = {arg2} {{3}} = {arg3} ~~~");
    }

    public void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1, string arg2, string arg3)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} {{0}} = {arg0} {{1}} = {arg1} {{2}} = {arg2} {{3}} = {arg3} ~~~");
    }

    public void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1, string arg2)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} {{0}} = {arg0} {{1}} = {arg1} {{2}} = {arg2} ~~~");
    }

    public void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} {{0}} = {arg0} {{1}} = {arg1} ~~~");
    }

    public void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} {{0}} = {arg0} ~~~");
    }

    public void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable)
    {
        Console.WriteLine($"~~~ SEND ID {id} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} ~~~");
    }

    public void SetElementImageUrl(short key, ITransportConnection connection, bool isReliable, string path, string imageUrl, bool shouldCache, bool forceRefresh)
    {
        Console.WriteLine($"~~~ SET IMG '{imageUrl}' FOR '{path}' (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} (SHOULD CACHE {shouldCache}, FORCE REFRESH {forceRefresh}) ~~~");
    }

    public void SetElementText(short key, ITransportConnection connection, bool isReliable, string path, string text)
    {
        Console.WriteLine($"~~~ SET TXT '{text}' FOR '{path}' (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} ~~~");
    }

    public void SetElementVisibility(short key, ITransportConnection connection, bool isReliable, string path, bool state)
    {
        Console.WriteLine($"~~~ SET VIS '{state}' FOR '{path}' (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} ~~~");
    }

    public void ClearById(ushort id, ITransportConnection connection)
    {
        Console.WriteLine($"~~~ CLEAR ID {id} FOR {connection.GetAddressString(true)} ~~~");
    }
}