using SDG.NetTransport;
using SDG.Unturned;
using System;
using Uncreated.Networking;

namespace Uncreated.Framework.UI;
/// <summary>
/// Represents a text component in a Unity UI.
/// </summary>
public class UnturnedLabel : UnturnedUIElement
{
    public UnturnedLabel(string name) : base(name) { }
    internal UnturnedLabel(UnturnedLabel original) : base(original) { }
    public virtual void SetText(SteamPlayer player, string text)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        text ??= string.Empty;
        if (player.player.isActiveAndEnabled)
            SetTextIntl(player.transportConnection, text);
    }

    public virtual void SetText(Player player, string? text)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        text ??= string.Empty;
        if (player.isActiveAndEnabled)
            SetTextIntl(player.channel.owner.transportConnection, text);
    }
    public virtual void SetText(ITransportConnection connection, string text)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));
        text ??= string.Empty;

        SetTextIntl(connection, text);
    }
    private void SetTextIntl(ITransportConnection connection, string text)
    {
        AssertOwnerSet();

        if (ThreadQueue.Queue.IsMainThread)
            EffectManager.sendUIEffectText(_owner!.Key, connection, _owner.IsReliable, _name, text);
        else
            ThreadQueue.Queue.RunOnMainThread(() => EffectManager.sendUIEffectText(_owner!.Key, connection, _owner.IsReliable, _name, text));

        if (Owner.DebugLogging)
        {
            Logging.LogInfo($"[{Owner.Name.ToUpperInvariant()}] [{Name.ToUpperInvariant()}] {{{Owner.Key}}} Set label text, text: {text}.");
        }
    }
    public override object Clone()
    {
        return new UnturnedLabel(this);
    }
}