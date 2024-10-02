using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using SDG.Unturned;
using System;
using System.Globalization;
using System.Threading;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI;

/// <summary>
/// Represents a clickable button in a Unity UI.
/// </summary>
public class UnturnedButton : UnturnedUIElement, IDisposable, IButton
{
    private int _disposed;
    internal UnturnedButton? Duplicate;

    /// <summary>
    /// Called when the button is clicked.
    /// </summary>
    public event ButtonClicked? OnClicked;

    /// <inheritdoc />
    protected override string ElementTypeDisplayName => "Button";

    public UnturnedButton(string path) : base(path)
    {
        EffectManagerListener.RegisterButton(Name.ToString(), this);
    }

    internal void InvokeOnClicked(Player player)
    {
        if (Owner.DebugLogging)
        {
            Logger.LogInformation("[{0}] [{1}] {{{2}}} Clicked by {3}.", Owner.Name, Name, Owner.Key, player.channel.owner.playerID.steamID.m_SteamID.ToString(CultureInfo.InvariantCulture));
        }

        try
        {
            OnClicked?.Invoke(this, player);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[{0}] [{1}] Error invoking {2}.", Owner.Name, Name, nameof(OnClicked));
        }
    }

    protected override void RegisterOwner(UnturnedUI? owner, ILoggerFactory? loggerFactory)
    {
        base.RegisterOwner(owner, loggerFactory);

        if (Duplicate == null)
            return;

        Logger?.LogWarning("[{0}] [{1}] {{{2}}} There is already a button with name \"{3}\", replacing. Multiple buttons can not listen to events with the same name. Old button: \"{4}\". This button: \"{5}\".", Owner.Name, Name, Owner.Key, Name, Duplicate.Path, Path);
        Duplicate = null;
    }

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        EffectManagerListener.DeregisterButton(Name.ToString());
    }

    [Ignore]
    UnturnedButton IButton.Button => this;
}

/// <summary>
/// Handles a <see cref="UnturnedButton"/> being clicked.
/// </summary>
public delegate void ButtonClicked(UnturnedButton button, Player player);