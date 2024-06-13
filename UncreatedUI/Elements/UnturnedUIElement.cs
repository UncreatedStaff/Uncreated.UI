using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    private UnturnedUI? _owner;

    /// <summary>
    /// Logger used for debug or error logging for this element.
    /// </summary>
    protected internal ILogger Logger;

    /// <summary>
    /// Name in Unity of this UI element.
    /// </summary>
    public ReadOnlyMemory<char> Name { get; }

    /// <summary>
    /// Hierarchical path to this UI element in Unity.
    /// </summary>
    public string Path { get; }

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
    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public UnturnedUIElement(string path) : this(GlobalLogger.Instance, path) { }
    
    /// <summary>
    /// Create a new <see cref="UnturnedUIElement"/> with the given display name and logger factory.
    /// </summary>
    public UnturnedUIElement(ILoggerFactory logFactory, string path)
    {
        Path = path;
        Name = UnturnedUIUtility.GetNameFromPathOrName(path.AsMemory());
        Logger = logFactory.CreateLogger(path);
    }

    /// <summary>
    /// Create a new <see cref="UnturnedUIElement"/> with the given display name and logger.
    /// </summary>
    public UnturnedUIElement(ILogger logger, string path)
    {
        Path = path;
        Name = UnturnedUIUtility.GetNameFromPathOrName(path.AsMemory());
        Logger = logger;
    }

    /// <summary>
    /// Throw an error if the owner of this element isn't set.
    /// </summary>
    /// <param name="checkKey">Also throw an error if the key of the owner is -1.</param>
    /// <exception cref="InvalidOperationException"/>
    protected void AssertOwnerSet(bool checkKey = true)
    {
        if (_owner is null)
            throw new InvalidOperationException("UI owner has not yet been set. Make sure the element is a field in it's owner's class.");
        if (_owner.Key == -1 && checkKey)
            throw new InvalidOperationException("Owner's key is set to -1.");
    }

    /// <summary>
    /// Register the owner of this element.
    /// </summary>
    protected internal virtual void RegisterOwner(UnturnedUI? owner)
    {
        _owner = owner;
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

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.sendUIEffectVisibility(Owner.Key, connection, Owner.IsReliable, Path, isEnabled);
        }
        else
        {
            ITransportConnection c2 = connection;
            bool state2 = isEnabled;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffectVisibility(Owner.Key, c2, Owner.IsReliable, Path, state2);
            });
        }

        if (Owner.DebugLogging)
        {
            Logger.LogInformation("[{0}] [{1}] {{{2}}} Set visibility for {3}, visibility: {4}.", Owner.Name, Name, Owner.Key, connection.GetAddressString(true), isEnabled);
        }
    }

    UnturnedUIElement IElement.Element => this;
}