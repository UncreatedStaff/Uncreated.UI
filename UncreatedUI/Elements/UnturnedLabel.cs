using SDG.NetTransport;
using SDG.Unturned;
using System.Globalization;
using Uncreated.Networking;

namespace Uncreated.Framework.UI;
/// <summary>
/// Represents a text component in a Unity UI.
/// </summary>
public class UnturnedLabel : UnturnedUIElement
{
    public UnturnedLabel(string name) : base(name)
    {
    }
    internal UnturnedLabel(UnturnedLabel original) : base(original) { }
    public void SetText(ITransportConnection connection, string text)
    {
        AssertOwnerSet();
        EffectManager.sendUIEffectText(_owner!.Key, connection, _owner.IsReliable, _name, text);
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