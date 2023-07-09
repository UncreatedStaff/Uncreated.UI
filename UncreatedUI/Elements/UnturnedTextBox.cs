using SDG.Unturned;
using System;
using System.Globalization;
using Uncreated.Networking;

namespace Uncreated.Framework.UI;
public delegate void TextUpdated(UnturnedTextBox button, Player player, string text);
/// <summary>
/// Represents an input component in a Unity UI.
/// </summary>
public class UnturnedTextBox : UnturnedUIElement
{
    private bool _disposed;
    /// <summary>
    /// Called when text is committed by the user.
    /// </summary>
    public event TextUpdated? OnTextUpdated;
    public UnturnedTextBox(string name) : base(name)
    {
        EffectManagerListener.RegisterInputBox(Name, this);
    }
    internal UnturnedTextBox(UnturnedTextBox original) : base(original)
    {
        EffectManagerListener.RegisterInputBox(Name, this);
    }
    ~UnturnedTextBox()
    {
        if (_disposed) return;
        Dispose(false);
    }
    private void Dispose(bool disposing)
    {
        _disposed = true;
        EffectManagerListener.DeregisterInputBox(Name);

        if (disposing)
            GC.SuppressFinalize(this);
    }
    public void Dispose()
    {
        if (_disposed) return;
        Dispose(true);
    }
    internal void InvokeOnTextCommitted(Player player, string text)
    {
        if (Owner.DebugLogging)
        {
            Logging.LogInfo($"[{Owner.Name.ToUpperInvariant()}] [{Name.ToUpperInvariant()}] {{{Owner.Key}}} Text committed by {player.channel.owner.playerID.steamID.m_SteamID.ToString(CultureInfo.InvariantCulture)}, text: {text}.");
        }
        OnTextUpdated?.Invoke(this, player, text);
    }

    public override object Clone()
    {
        return new UnturnedTextBox(this);
    }
}