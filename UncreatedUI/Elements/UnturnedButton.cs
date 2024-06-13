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

    /// <summary>
    /// Called when the button is clicked.
    /// </summary>
    public event ButtonClicked? OnClicked;

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public UnturnedButton(string name) : this(GlobalLogger.Instance, name) { }
    public UnturnedButton(ILogger logger, string name) : base(logger, name)
    {
        EffectManagerListener.RegisterButton(name, this);
    }
    public UnturnedButton(ILoggerFactory factory, string name) : base(factory, name)
    {
        EffectManagerListener.RegisterButton(name, this);
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

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        EffectManagerListener.DeregisterButton(Name);
    }
    UnturnedButton IButton.Button => this;
}

/// <summary>
/// Handles a <see cref="UnturnedButton"/> being clicked.
/// </summary>
public delegate void ButtonClicked(UnturnedButton button, Player player);