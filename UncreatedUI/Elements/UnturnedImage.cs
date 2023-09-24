using System;
using SDG.NetTransport;
using SDG.Unturned;
using Uncreated.Networking;

namespace Uncreated.Framework.UI;
/// <summary>
/// Represents a web image in a Unity UI.
/// </summary>
public class UnturnedImage : UnturnedUIElement
{
    public UnturnedImage(string name) : base(name) { }
    internal UnturnedImage(UnturnedImage original) : base(original) { }

    public void SetImage(SteamPlayer player, string? url, bool forceRefresh = false)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        url ??= string.Empty;
        if (player.player.isActiveAndEnabled)
            SetImageIntl(player.transportConnection, url, forceRefresh);
    }
    public virtual void SetImage(Player player, string? url, bool forceRefresh = false)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        url ??= string.Empty;
        if (player.isActiveAndEnabled)
            SetImageIntl(player.channel.owner.transportConnection, url, forceRefresh);
    }

    public virtual void SetImage(ITransportConnection connection, string? url, bool forceRefresh = false)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));
        url ??= string.Empty;
        SetImageIntl(connection, url, forceRefresh);
    }
    private void SetImageIntl(ITransportConnection connection, string url, bool forceRefresh)
    {
        AssertOwnerSet();

        if (ThreadQueue.Queue.IsMainThread)
            EffectManager.sendUIEffectImageURL(_owner!.Key, connection, _owner.IsReliable, _name, url, true, forceRefresh);
        else
            ThreadQueue.Queue.RunOnMainThread(() => EffectManager.sendUIEffectImageURL(_owner!.Key, connection, _owner.IsReliable, _name, url, true, forceRefresh));

        if (Owner.DebugLogging)
        {
            Logging.LogInfo($"[{Owner.Name.ToUpperInvariant()}] [{Name.ToUpperInvariant()}] {{{Owner.Key}}} Set image URL, link: {url}, force refresh: {forceRefresh}.");
        }
    }
    public override object Clone()
    {
        return new UnturnedImage(this);
    }
}