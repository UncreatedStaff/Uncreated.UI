using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using Uncreated.Framework.UI.Data;

namespace Uncreated.Framework.UI.Presets;
public class UnturnedEnumButton<TEnum> where TEnum : unmanaged, Enum
{
    private static readonly TEnum[] ValuesIntl = (TEnum[])typeof(TEnum).GetEnumValues();
    private readonly int _defaultIndex;

    public UnturnedButton Button { get; }
    public UnturnedLabel Label { get; }
    public UnturnedUIElement? DisableState { get; }
    public TEnum Default { get; set; }

    public static IReadOnlyList<TEnum> Values { get; } = new ReadOnlyCollection<TEnum>(ValuesIntl);

    public UnturnedEnumButton(TEnum defaultValue, string button, string label) : this(defaultValue, button, label, null) { }
    public UnturnedEnumButton(TEnum defaultValue, string button, string label, string? disableState)
    {
        _defaultIndex = -1;
        TEnum[] values = ValuesIntl;
        for (int i = 0; i < values.Length; ++i)
        {
            if (values[i].CompareTo(defaultValue) == 0)
            {
                _defaultIndex = i;
                break;
            }
        }
        Button = new UnturnedButton(button);
        Label = new UnturnedLabel(label);
        DisableState = disableState == null ? null : new UnturnedUIElement(disableState);
    }

    private class UnturnedEnumButtonData : IUnturnedUIData
    {
        public CSteamID Player { get; }
        public UnturnedUI Owner { get; }
        public UnturnedUIElement Element { get; }
        public UnturnedEnumButtonData(CSteamID player, UnturnedUI owner, UnturnedUIElement element, int selectionValueIndex)
        {
            Player = player;
            Owner = owner;
            Element = element;
            SelectionValueIndex = selectionValueIndex;
        }
        public int SelectionValueIndex { get; }
        public TEnum? Selection => SelectionValueIndex >= 0 && SelectionValueIndex < ValuesIntl.Length ? new TEnum?(ValuesIntl[SelectionValueIndex]) : null;
    }
}
