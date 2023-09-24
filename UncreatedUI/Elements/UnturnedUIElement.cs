using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using Uncreated.Networking;

namespace Uncreated.Framework.UI;
/// <summary>
/// Any GameObject in a Unity UI.
/// </summary>
public class UnturnedUIElement : ICloneable
{
    protected readonly string _name;
    protected UnturnedUI? _owner;
    protected UnturnedUIElement? _parent;
    public string Name => _name;

    /// <exception cref="InvalidOperationException">Thrown when the owner has yet to be set.</exception>
    public UnturnedUI Owner
    {
        get
        {
            AssertOwnerSet(false);
            return _owner!;
        }
    }
    public UnturnedUIElement? Parent { get; private set; }
    public UnturnedUIElement(string name)
    {
        _name = name;
    }
    protected UnturnedUIElement(UnturnedUIElement original)
    {
        _name = original._name;
    }
    protected void AssertOwnerSet(bool checkKey = true)
    {
        if (_owner is null)
            throw new InvalidOperationException("UI owner has not yet been set. Make sure the element is a field in it's owner's class.");
        if (_owner.Key == -1 && checkKey)
            throw new InvalidOperationException("Owner's key is set to -1.");
    }
    internal void RegisterOwnerInternal(UnturnedUI? owner) => RegisterOwner(owner);
    internal void AddIncludedElementsInternal(List<UnturnedUIElement> elements)
    {
        int ct = elements.Count;
        AddIncludedElements(elements);
        for (int i = ct; i < elements.Count; ++i)
            elements[i].Parent = this;
    }

    protected virtual void RegisterOwner(UnturnedUI? owner)
    {
        bool debug = owner?.DebugLogging ?? _owner is { DebugLogging: true };
        this._owner = owner;
        if (debug)
        {
            if (owner == null)
                Logging.LogInfo($"[{Name.ToUpperInvariant()}] Deregistered.");
            else
                Logging.LogInfo($"[{Owner.Name.ToUpperInvariant()}] [{Name.ToUpperInvariant()}] {{{Owner.Key}}} Registered.");
        }
    }

    public virtual void SetVisibility(SteamPlayer player, bool isEnabled)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.player.isActiveAndEnabled)
            SetVisibilityIntl(player.transportConnection, isEnabled);
    }

    public virtual void SetVisibility(Player player, bool isEnabled)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.isActiveAndEnabled)
            SetVisibilityIntl(player.channel.owner.transportConnection, isEnabled);
    }

    public virtual void SetVisibility(ITransportConnection connection, bool isEnabled)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));
        SetVisibilityIntl(connection, isEnabled);
    }

    private void SetVisibilityIntl(ITransportConnection connection, bool isEnabled)
    {
        AssertOwnerSet();

        if (ThreadQueue.Queue.IsMainThread)
            EffectManager.sendUIEffectVisibility(_owner!.Key, connection, _owner.IsReliable, _name, isEnabled);
        else
            ThreadQueue.Queue.RunOnMainThread(() => EffectManager.sendUIEffectVisibility(_owner!.Key, connection, _owner.IsReliable, _name, isEnabled));

        if (Owner.DebugLogging)
        {
            Logging.LogInfo($"[{Owner.Name.ToUpperInvariant()}] [{Name.ToUpperInvariant()}] {{{Owner.Key}}} Set visibility for {connection.GetAddressString(true)}, visibility: {isEnabled}.");
        }
    }
    protected virtual void AddIncludedElements(List<UnturnedUIElement> elements) { }
    public virtual object Clone()
    {
        return new UnturnedUIElement(this);
    }
}