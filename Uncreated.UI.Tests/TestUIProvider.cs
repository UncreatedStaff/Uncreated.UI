using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Uncreated.Framework.UI;
using Action = System.Action;

namespace Uncreated.UI.Tests;
public class TestUIProvider : IUnturnedUIProvider
{
    public static ITransportConnection Connection { get; } = new TestTransportConnection();

    private readonly Dictionary<string, bool> _visibilities = new Dictionary<string, bool>();
    private readonly Dictionary<string, string> _texts = new Dictionary<string, string>();
    private readonly Dictionary<string, ImageData> _images = new Dictionary<string, ImageData>();
    private readonly List<UIData> _uis = new List<UIData>();

    public struct ImageData
    {
        public string? Url;
        public bool ShouldCache;
        public bool ForceRefresh;
    }

    public struct UIData
    {
        public Guid Guid;
        public short Key;
        public bool Reliable;
        public bool Global;
        public string[] Args;

        public UIData(Guid guid, short key, bool reliable, params string[] args)
        {
            Guid = guid;
            Key = key;
            Reliable = reliable;
            Args = args;
        }
    }

    public bool AreAssetsStillLoading => false;

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

    public static string? GetLabelText(string path)
    {
        ((TestUIProvider)UnturnedUIProvider.Instance)._texts.TryGetValue(path, out string? text);
        return text;
    }

    public static ImageData? GetImageData(string path)
    {
        return ((TestUIProvider)UnturnedUIProvider.Instance)._images.TryGetValue(path, out ImageData data)
            ? data
            : null;
    }

    public static bool? GetElementVisibility(string path)
    {
        return ((TestUIProvider)UnturnedUIProvider.Instance)._visibilities.TryGetValue(path, out bool vis)
            ? vis
            : null;
    }

    public static UIData? GetUIData(Guid guid)
    {
        List<UIData> uis = ((TestUIProvider)UnturnedUIProvider.Instance)._uis;
        int index = uis.FindIndex(x => x.Guid == guid);
        if (index == -1)
            return null;

        return uis[index];
    }

    public static UIData? GetUIData(short key)
    {
        List<UIData> uis = ((TestUIProvider)UnturnedUIProvider.Instance)._uis;
        int index = uis.FindIndex(x => x.Key == key);
        if (index == -1)
            return null;

        return uis[index];
    }

    public event EffectManager.EffectButtonClickedHandler? OnButtonClicked;
    public event EffectManager.EffectTextCommittedHandler? OnTextCommitted;
    public event Provider.ServerDisconnected OnDisconnect;

    public static void Setup()
    {
        GlobalLogger.Instance = LoggerFactory.Create(bldr => bldr.AddConsole());
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

    public static void DisconnectPlayer(CSteamID steam64)
    {
        ((TestUIProvider)UnturnedUIProvider.Instance).OnDisconnect?.Invoke(steam64);
    }

    public EffectAsset? GetEffectAsset(ushort id) => null;

    public EffectAsset? GetEffectAsset(Guid guid) => null;

    public void ClearGlobal(EffectAsset asset)
    {
        Console.WriteLine($"~~~ CLEAR ID {asset.FriendlyName} FOR ALL PLAYERS ~~~");
        _uis.RemoveAll(x => x.Guid == asset.GUID);
    }

    public void SendUIGlobal(EffectAsset asset, short key, bool isReliable)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable) { Global = true });
    }

    public void SendUIGlobal(EffectAsset asset, short key, bool isReliable, string arg0)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS {{0}} = {arg0} ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable, arg0) { Global = true });
    }

    public void SendUIGlobal(EffectAsset asset, short key, bool isReliable, string arg0, string arg1)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS {{0}} = {arg0} {{1}} = {arg1} ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable, arg0, arg1) { Global = true });
    }

    public void SendUIGlobal(EffectAsset asset, short key, bool isReliable, string arg0, string arg1, string arg2)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS {{0}} = {arg0} {{1}} = {arg1} {{2}} = {arg2} ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable, arg0, arg1, arg2) { Global = true });
    }

    public void SendUIGlobal(EffectAsset asset, short key, bool isReliable, string arg0, string arg1, string arg2, string arg3)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR ALL PLAYERS {{0}} = {arg0} {{1}} = {arg1} {{2}} = {arg2} {{3}} = {arg3} ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable, arg0, arg1, arg2, arg3) { Global = true });
    }

    public void SendUI(EffectAsset asset, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1, string arg2, string arg3)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} {{0}} = {arg0} {{1}} = {arg1} {{2}} = {arg2} {{3}} = {arg3} ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable, arg0, arg1, arg2, arg3));
    }

    public void SendUI(EffectAsset asset, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1, string arg2)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} {{0}} = {arg0} {{1}} = {arg1} {{2}} = {arg2} ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable, arg0, arg1, arg2));
    }

    public void SendUI(EffectAsset asset, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} {{0}} = {arg0} {{1}} = {arg1} ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable, arg0, arg1));
    }

    public void SendUI(EffectAsset asset, short key, ITransportConnection connection, bool isReliable, string arg0)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} {{0}} = {arg0} ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable, arg0));
    }

    public void SendUI(EffectAsset asset, short key, ITransportConnection connection, bool isReliable)
    {
        Console.WriteLine($"~~~ SEND ID {asset.FriendlyName} (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} ~~~");
        _uis.RemoveAll(x => x.Key == key || x.Guid == asset.GUID);
        _uis.Add(new UIData(asset.GUID, key, isReliable));
    }

    public void SetElementImageUrl(short key, ITransportConnection connection, bool isReliable, string path, string imageUrl, bool shouldCache, bool forceRefresh)
    {
        Console.WriteLine($"~~~ SET IMG '{imageUrl}' FOR '{path}' (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} (SHOULD CACHE {shouldCache}, FORCE REFRESH {forceRefresh}) ~~~");
        if (_uis.Exists(x => x.Key == key))
        {
            _images[path] = new ImageData { ForceRefresh = forceRefresh, ShouldCache = shouldCache, Url = imageUrl };
        }
    }

    public void SetElementText(short key, ITransportConnection connection, bool isReliable, string path, string text)
    {
        Console.WriteLine($"~~~ SET TXT '{text}' FOR '{path}' (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} ~~~");
        if (_uis.Exists(x => x.Key == key))
        {
            _texts[path] = text;
        }
    }

    public void SetElementVisibility(short key, ITransportConnection connection, bool isReliable, string path, bool state)
    {
        Console.WriteLine($"~~~ SET VIS '{state}' FOR '{path}' (KEY {key}, RELIABLE: {isReliable}) FOR {connection.GetAddressString(true)} ~~~");
        if (_uis.Exists(x => x.Key == key))
        {
            _visibilities[path] = state;
        }
    }

    public void Clear(EffectAsset asset, ITransportConnection connection)
    {
        Console.WriteLine($"~~~ CLEAR ID {asset.FriendlyName} FOR {connection.GetAddressString(true)} ~~~");
        _uis.RemoveAll(x => x.Guid == asset.GUID);
    }

    private class TestTransportConnection : ITransportConnection
    {
        public bool Equals(ITransportConnection? other) => other is TestTransportConnection;
        public bool TryGetIPv4Address([UnscopedRef] out uint address)
        {
            address = 0;
            return false;
        }
        public bool TryGetPort([UnscopedRef] out ushort port)
        {
            port = 0;
            return false;
        }
        public bool TryGetSteamId([UnscopedRef] out ulong steamId)
        {
            steamId = 0;
            return false;
        }
        public IPAddress GetAddress()
        {
            return IPAddress.Loopback;
        }
        public string GetAddressString(bool withPort)
        {
            return IPAddress.Loopback + ":00000";
        }
        public void CloseConnection() { }
        public void Send(byte[] buffer, long size, ENetReliability reliability) { }
    }
}