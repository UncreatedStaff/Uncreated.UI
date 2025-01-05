using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Threading;
using Uncreated.Framework.UI.Presets;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Uncreated.Framework.UI;

/// <summary>
/// Any <see cref="GameObject"/> in a Unity UI.
/// </summary>
public class UnturnedUIElement : IElement
{
    private static int _globalElementId;
    private UnturnedUI? _owner;
    private readonly int _id;
    protected internal ILogger? Logger;
    protected internal ILoggerFactory? LoggerFactory;

    /// <summary>
    /// Get a lazily cached <see cref="ILogger"/> object for this element. Will never be <see langword="null"/>.
    /// </summary>
    protected internal ILogger GetLogger()
    {
        if (Logger != null)
            return Logger;

        if (LoggerFactory == null)
            return NullLogger.Instance;

        Logger = LoggerFactory.CreateLogger(Path);
        return Logger;
    }
    
    /// <summary>
    /// Display name of this element type for <see cref="ToString"/>.
    /// </summary>
    protected virtual string ElementTypeDisplayName => Properties.Resources.DisplayName_UnturnedUIElement;

    /// <summary>
    /// Name in Unity of this UI element.
    /// </summary>
    public ReadOnlyMemory<char> Name { get; }

    /// <summary>
    /// Hierarchical path to this UI element in Unity.
    /// </summary>
    public string Path { get; internal set; }

    /// <summary>
    /// The <see cref="UnturnedUI"/> object this parent belongs to.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the owner has yet to be set.</exception>
    public UnturnedUI Owner
    {
        get
        {
            AssertOwnerSet(false);
            return _owner!;
        }
    }

    /// <summary>
    /// Create a new <see cref="UnturnedUIElement"/> with the given name.
    /// </summary>
    public UnturnedUIElement(string path)
    {
        _id = Interlocked.Increment(ref _globalElementId);
        Path = path;
        Name = UnturnedUIUtility.GetNameFromPathOrName(path.AsMemory());
    }
    

    /// <summary>
    /// Throw an error if the owner of this element isn't set.
    /// </summary>
    /// <param name="checkKey">Also throw an error if the key of the owner is -1.</param>
    /// <exception cref="InvalidOperationException"/>
    protected void AssertOwnerSet(bool checkKey = true)
    {
        if (_owner is null)
            throw new InvalidOperationException(Properties.Resources.Exception_UIOwnerNotSet);
        
        if (_owner.Key == -1 && checkKey)
            throw new InvalidOperationException(Properties.Resources.Exception_UIOwnerM1Key);
    }

    internal void RegisterOwnerIntl(UnturnedUI? owner) => RegisterOwner(owner);

    /// <summary>
    /// Register the owner of this element.
    /// </summary>
    protected virtual void RegisterOwner(UnturnedUI? owner)
    {
        _owner = owner;
        if (owner == null)
        {
            Logger = null;
            LoggerFactory = null;
            return;
        }

        if (owner.LoggerFactory != null)
        {
            LoggerFactory = owner.LoggerFactory;
            Logger = null;
        }
        else
        {
            LoggerFactory = null;
            Logger = owner.Logger;
        }
    }

    internal void DeregisterOwnerIntl() => DeregisterOwner();

    /// <summary>
    /// Deregister the owner of this element.
    /// </summary>
    protected virtual void DeregisterOwner()
    {
        _owner = null;
        Logger = null;
        LoggerFactory = null;
    }

    /// <summary>
    /// Set the visibility of this element.
    /// </summary>
    /// <param name="player">Player to send the image to.</param>
    /// <param name="isEnabled">If the element is enabled/activated or disabled/inactivated.</param>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SetVisibility(SteamPlayer player, bool isEnabled)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.player.isActiveAndEnabled)
            SetVisibilityIntl(player.transportConnection, isEnabled);
    }

    /// <summary>
    /// Set the visibility of this element.
    /// </summary>
    /// <param name="player">Player to send the image to.</param>
    /// <param name="isEnabled">If the element is enabled/activated or disabled/inactivated.</param>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SetVisibility(Player player, bool isEnabled)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.isActiveAndEnabled)
            SetVisibilityIntl(player.channel.owner.transportConnection, isEnabled);
    }

    /// <summary>
    /// Set the visibility of this element.
    /// </summary>
    /// <param name="connection">Connection to send the image to.</param>
    /// <param name="isEnabled">If the element is enabled/activated or disabled/inactivated.</param>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SetVisibility(ITransportConnection connection, bool isEnabled)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        SetVisibilityIntl(connection, isEnabled);
    }

    private void SetVisibilityIntl(ITransportConnection connection, bool isEnabled)
    {
        AssertOwnerSet();

        UnturnedUIProvider.Instance.SetElementVisibility(Owner.Key, connection, Owner.IsReliable, Path, isEnabled);

        if (Owner.DebugLogging)
        {
            GetLogger().LogInformation(Properties.Resources.Log_UnturnedUIElementVisibilityUpdated, Owner.Name, Name, Owner.Key, connection.GetAddressString(true), isEnabled);
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _owner == null
            ? $"{ElementTypeDisplayName} [{Path}]"
            : $"{ElementTypeDisplayName} [{Path}] ({_owner.Name})";
    }

    /// <inheritdoc />
    public override int GetHashCode() => _id;

    /// <inheritdoc />
    public override bool Equals(object? obj) => ReferenceEquals(obj, this);

    [Ignore]
    UnturnedUIElement IElement.Element => this;
}