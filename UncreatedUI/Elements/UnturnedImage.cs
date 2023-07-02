using SDG.NetTransport;
using SDG.Unturned;
using System.Globalization;

namespace Uncreated.Framework.UI;
public class UnturnedImage : UnturnedUIElement
{
    public UnturnedImage(string name) : base(name) { }
    internal UnturnedImage(UnturnedImage original) : base(original) { }
    public void SetImage(ITransportConnection connection, string url, bool forceRefresh = false)
    {
        AssertOwnerSet();
        EffectManager.sendUIEffectImageURL(_owner!.Key, connection, _owner.IsReliable, _name, url, true, forceRefresh);
    }
    public override object Clone()
    {
        return new UnturnedImage(this);
    }
    public new static UnturnedImage[] GetPattern(string name, int length, int start = 1)
    {
        UnturnedImage[] elems = new UnturnedImage[length];
        for (int i = 0; i < length; ++i)
        {
            elems[i] = new UnturnedImage(Util.QuickFormat(name, (i + start).ToString(CultureInfo.InvariantCulture)));
        }

        return elems;
    }
}