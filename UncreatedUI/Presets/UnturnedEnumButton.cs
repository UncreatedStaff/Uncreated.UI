using SDG.Unturned;
using System;

namespace Uncreated.Framework.UI.Presets;

public delegate void ValueUpdated<TEnum>(UnturnedEnumButtonTracker<TEnum> button, Player player, TEnum value) where TEnum : unmanaged, Enum;
public delegate string TextFormatter<in TEnum>(TEnum value, Player player) where TEnum : unmanaged, Enum;
public class UnturnedEnumButton<TEnum> : UnturnedEnumButtonTracker<TEnum> where TEnum : unmanaged, Enum
{
    public UnturnedEnumButton(TEnum defaultValue, string button, string label) :
        base(defaultValue, new UnturnedButton(button), new UnturnedLabel(label)) { }
    public UnturnedEnumButton(TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) :
        base(defaultValue, new UnturnedButton(button), new UnturnedLabel(label),
            disableState == null ? null : new UnturnedUIElement(disableState),
            rightClickListener == null ? null : new UnturnedButton(rightClickListener)) { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string button, string label) :
        base(valueSet, defaultValue, new UnturnedButton(button), new UnturnedLabel(label)) { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) :
        base(valueSet, defaultValue, new UnturnedButton(button), new UnturnedLabel(label),
            disableState == null ? null : new UnturnedUIElement(disableState),
            rightClickListener == null ? null : new UnturnedButton(rightClickListener)) { }
}
