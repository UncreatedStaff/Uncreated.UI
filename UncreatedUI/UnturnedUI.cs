using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Uncreated.Framework.UI.Reflection;
using Uncreated.Networking;

namespace Uncreated.Framework.UI;
public class UnturnedUI : IDisposable
{
    [Ignore]
    private string _name;
    [Ignore]
    private bool _isValid;
    [Ignore]
    private bool _isDefaultName = true;
    [Ignore]
    private bool _disposed;
    [Ignore]
    private bool _hasDebugLogged;

    [Ignore]
    public bool IsReliable { get; set; }
    [Ignore]
    public bool IsSendReliable { get; set; }
    [Ignore]
    public bool IsValid => _isValid;
    [Ignore]
    public bool HasElements { get; }
    [Ignore]
    public bool DebugLogging { get; }
    [Ignore]
    public IReadOnlyList<UnturnedUIElement> Elements { get; }
    [Ignore]
    public JsonAssetReference<EffectAsset> Asset { get; private set; } = null!;
    [Ignore]
    public bool HasDefaultName => _isDefaultName;
    [Ignore]
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            _isDefaultName = false;
        }
    }
    [Ignore]
    public short Key { get; set; }
    private UnturnedUI(bool hasElements, bool keyless, bool reliable, bool debugLogging)
    {
        Type type = GetType();
        DebugLogging = debugLogging;
        HasElements = hasElements;
        List<UnturnedUIElement> elements = new List<UnturnedUIElement>(hasElements ? 16 : 0);
        Elements = elements.AsReadOnly();
        Key = keyless ? (short)-1 : UnturnedUIKeyPool.Claim();
        IsReliable = reliable;
        IsSendReliable = reliable;

        _name = type.Name;

        if (Attribute.GetCustomAttribute(type, typeof(UnturnedUIAttribute)) is UnturnedUIAttribute attr)
        {
            Name = attr.DisplayName;
            if (attr.Reliable.HasValue)
                IsReliable = attr.Reliable.Value;
            if (attr.HasElements.HasValue)
                HasElements = attr.HasElements.Value;
        }

        if (!hasElements)
            return;

        UIElementDiscovery.LinkAllElements(this, elements);

        for (int i = 0; i < elements.Count; ++i)
        {
            UnturnedUIElement element = elements[i];
            element.RegisterOwnerInternal(this);
        }
    }
    public UnturnedUI(ushort defaultId, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false) : this(hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(defaultId);
    }
    public UnturnedUI(Guid defaultGuid, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false) : this(hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(defaultGuid);
    }
    public UnturnedUI(JsonAssetReference<EffectAsset>? asset, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false) : this(hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(asset);
    }
    ~UnturnedUI()
    {
        if (_disposed) return;
        Dispose(false);
    }
    private void Dispose(bool disposing)
    {
        _disposed = true;
        IUnturnedUIDataSource? src = UnturnedUIDataSource.Instance;

        if (src != null)
        {
            if (src.RequiresMainThread && ThreadQueue.Queue is { IsMainThread: false })
                ThreadQueue.Queue.RunOnMainThread(() => IntlDispose(Elements.ToList(), src));
            else
                IntlDispose(Elements, src);
        }

        if (disposing)
            GC.SuppressFinalize(this);
    }
    private void IntlDispose(IReadOnlyList<UnturnedUIElement> elements, IUnturnedUIDataSource src)
    {
        if (DebugLogging)
            Logging.LogInfo($"[{Name.ToUpperInvariant()}] Deregistering {elements.Count} elements.");

        src.RemoveOwner(this);
        for (int i = 0; i < elements.Count; ++i)
        {
            UnturnedUIElement element = elements[i];
            src.RemoveElement(element);
            element.RegisterOwnerInternal(null);
            if (element is IDisposable disp)
                disp.Dispose();
        }
    }
    public void Dispose()
    {
        if (_disposed) return;
        Dispose(true);
    }

    public void LoadFromConfig(JsonAssetReference<EffectAsset>? asset)
    {
        if (asset is null)
        {
            Asset = JsonAssetReference<EffectAsset>.Nil;
            return;
        }
        if (asset.Exists)
        {
            this.Asset = asset;
            _isValid = true;
            if (_isDefaultName)
                _name = asset.Asset!.FriendlyName;
        }
        else if (asset.Id != 0 || Asset is null)
        {
            this.Asset = asset;
            if (_isDefaultName)
                _name = asset.Id.ToString();
        }
        else Asset = asset;
    }
    public void LoadFromConfig(Guid guid)
    {
        LoadFromConfig(new JsonAssetReference<EffectAsset>(guid));
    }
    public void LoadFromConfig(ushort id)
    {
        LoadFromConfig(new JsonAssetReference<EffectAsset>(id));
    }
    public void SendToPlayer(ITransportConnection connection)
    {
        if (Asset is null || !Asset.Exists)
            Logging.LogWarning("Invalid UI: " + _name);
        else
            EffectManager.sendUIEffect(this.Asset.Id, Key, connection, IsReliable || IsSendReliable);
        if (DebugLogging)
        {
            Logging.LogInfo($"[{Name.ToUpperInvariant()}] Sent to {connection.GetAddressString(true)}.");
        }
    }
    public void SendToPlayer(ITransportConnection connection, string arg1)
    {
        if (Asset is null || !Asset.Exists)
            Logging.LogWarning("Invalid UI: " + _name);
        else
            EffectManager.sendUIEffect(this.Asset.Id, Key, connection, IsReliable || IsSendReliable, arg1);
        if (DebugLogging)
        {
            Logging.LogInfo($"[{Name.ToUpperInvariant()}] Sent to {connection.GetAddressString(true)}, args: {arg1}.");
        }
    }
    public void SendToPlayer(ITransportConnection connection, string arg1, string arg2)
    {
        if (Asset is null || !Asset.Exists)
            Logging.LogWarning("Invalid UI: " + _name);
        else
            EffectManager.sendUIEffect(this.Asset.Id, Key, connection, IsReliable || IsSendReliable, arg1, arg2);
        if (DebugLogging)
        {
            Logging.LogInfo($"[{Name.ToUpperInvariant()}] Sent to {connection.GetAddressString(true)}, args: {arg1}, {arg2}.");
        }
    }
    public void SendToPlayer(ITransportConnection connection, string arg1, string arg2, string arg3)
    {
        if (Asset is null || !Asset.Exists)
            Logging.LogWarning("Invalid UI: " + this.GetType().Name);
        else
            EffectManager.sendUIEffect(this.Asset.Id, Key, connection, IsReliable || IsSendReliable, arg1, arg2, arg3);
        if (DebugLogging)
        {
            Logging.LogInfo($"[{Name.ToUpperInvariant()}] Sent to {connection.GetAddressString(true)}, args: {arg1}, {arg2}, {arg3}.");
        }
    }
    public void SendToPlayer(ITransportConnection connection, string arg1, string arg2, string arg3, string arg4)
    {
        if (Asset is null || !Asset.Exists)
            Logging.LogWarning("Invalid UI: " + this.GetType().Name);
        else
            EffectManager.sendUIEffect(this.Asset.Id, Key, connection, IsReliable || IsSendReliable, arg1, arg2, arg3, arg4);
        if (DebugLogging)
        {
            Logging.LogInfo($"[{Name.ToUpperInvariant()}] Sent to {connection.GetAddressString(true)}, args: {arg1}, {arg2}, {arg3}, {arg4}.");
        }
    }
    public void ClearFromPlayer(ITransportConnection connection)
    {
        if (Asset is null || !Asset.Exists)
            Logging.LogWarning("Invalid UI: " + this.GetType().Name);
        else
            EffectManager.askEffectClearByID(this.Asset.Id, connection);
        if (DebugLogging)
        {
            Logging.LogInfo($"[{Name.ToUpperInvariant()}] Cleared from {connection.GetAddressString(true)}.");
        }
    }
    public void SendToAllPlayers()
    {
        if (Asset is null || !Asset.Exists)
            Logging.LogWarning("Invalid UI: " + this.GetType().Name);
        else
            EffectManager.sendUIEffect(this.Asset.Id, Key, IsReliable || IsSendReliable);
        if (DebugLogging)
        {
            Logging.LogInfo($"[{Name.ToUpperInvariant()}] Sent to all players.");
        }
    }
    public void ClearFromAllPlayers()
    {
        if (Asset is null || !Asset.Exists)
            Logging.LogWarning("Invalid UI: " + this.GetType().Name);
        else
            EffectManager.ClearEffectByID_AllPlayers(this.Asset.Id);
        if (DebugLogging)
        {
            Logging.LogInfo($"[{Name.ToUpperInvariant()}] Cleared from all players.");
        }
    }
}