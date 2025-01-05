using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using SDG.Unturned;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI;

/// <summary>
/// Represents a clickable button in a Unity UI.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {Path}")]
public class UnturnedButton : UnturnedUIElement, IDisposable, IButton
{
    private int _disposed;
    internal UnturnedButton? Duplicate;

    /// <summary>
    /// Called when the button is clicked.
    /// </summary>
    public event ButtonClicked? OnClicked;

    /// <inheritdoc />
    protected override string ElementTypeDisplayName => Properties.Resources.DisplayName_UnturnedButton;

    public UnturnedButton(string path) : base(path)
    {
        EffectManagerListener.RegisterButton(Name.ToString(), this);
    }

    internal void InvokeOnClicked(Player player)
    {
        if (Owner.DebugLogging)
        {
            GetLogger().LogInformation(Properties.Resources.Log_UnturnedButtonClicked, Owner.Name, Name, Owner.Key, player.channel.owner.playerID.steamID.m_SteamID.ToString(CultureInfo.InvariantCulture));
        }

        try
        {
            OnClicked?.Invoke(this, player);
        }
        catch (Exception ex)
        {
            GetLogger().LogError(ex, Properties.Resources.Log_EventError, Owner.Name, Name, nameof(OnClicked));
        }
    }

    protected override void RegisterOwner(UnturnedUI? owner)
    {
        base.RegisterOwner(owner);

        if (Duplicate == null)
            return;

        GetLogger().LogWarning(Properties.Resources.Log_DuplicateButtonName, Owner.Name, Name, Owner.Key, Name, Duplicate.Path, Path);
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