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
    public void SetImage(ITransportConnection connection, string url, bool forceRefresh = false)
    {
        AssertOwnerSet();
        EffectManager.sendUIEffectImageURL(_owner!.Key, connection, _owner.IsReliable, _name, url, true, forceRefresh);
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