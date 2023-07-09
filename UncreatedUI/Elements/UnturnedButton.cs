using SDG.Unturned;
using System;
using System.Globalization;
using Uncreated.Networking;

namespace Uncreated.Framework.UI;
public delegate void ButtonClicked(UnturnedButton button, Player player);
/// <summary>
/// Represents a clickable button in a Unity UI.
/// </summary>
public class UnturnedButton : UnturnedUIElement, IDisposable
{
    private bool _disposed;
    /// <summary>
    /// Called when the button is clicked.
    /// </summary>
    public event ButtonClicked? OnClicked;
    public UnturnedButton(string name) : base(name)
    {
        EffectManagerListener.RegisterButton(name, this);
    }
    internal UnturnedButton(UnturnedButton original) : base(original)
    {
        EffectManagerListener.RegisterButton(original.Name, this);
    }
    ~UnturnedButton()
    {
        if (_disposed) return;
        Dispose(false);
    }
    private void Dispose(bool disposing)
    {
        _disposed = true;
        EffectManagerListener.DeregisterButton(Name);

        if (disposing)
            GC.SuppressFinalize(this);
    }
    public void Dispose()
    {
        if (_disposed) return;
        Dispose(true);
    }

    internal void InvokeOnClicked(Player player)
    {
        if (Owner.DebugLogging)
        {
            Logging.LogInfo($"[{Owner.Name.ToUpperInvariant()}] [{Name.ToUpperInvariant()}] {{{Owner.Key}}} Clicked by {player.channel.owner.playerID.steamID.m_SteamID.ToString(CultureInfo.InvariantCulture)}.");
        }
        OnClicked?.Invoke(this, player);
    }
    public override object Clone()
    {
        return new UnturnedButton(this);
    }
}