using SDG.NetTransport;
using SDG.Unturned;
using System.Globalization;

namespace Uncreated.Framework.UI;
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
    }
    public override object Clone()
    {
        return new UnturnedLabel(this);
    }
    public new static UnturnedLabel[] GetPattern(string name, int length, int start = 1)
    {
        UnturnedLabel[] elems = new UnturnedLabel[length];
        for (int i = 0; i < length; ++i)
        {
            elems[i] = new UnturnedLabel(Util.QuickFormat(name, (i + start).ToString(CultureInfo.InvariantCulture)));
        }

        return elems;
    }
}