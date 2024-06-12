using Microsoft.Extensions.Logging;
using SDG.Unturned;
using System;

namespace Uncreated.Framework.UI.Presets;

public delegate void ValueUpdated<TEnum>(UnturnedEnumButtonTracker<TEnum> button, Player player, TEnum value) where TEnum : unmanaged, Enum;
public delegate string TextFormatter<in TEnum>(TEnum value, Player player) where TEnum : unmanaged, Enum;
public class UnturnedEnumButton<TEnum> : UnturnedEnumButtonTracker<TEnum> where TEnum : unmanaged, Enum
{
    public UnturnedEnumButton(TEnum defaultValue, string button, string label) :
        this(GlobalLogger.Instance, defaultValue, button, label) { }
    public UnturnedEnumButton(ILogger logger, TEnum defaultValue, string button, string label) :
        base(defaultValue, new UnturnedButton(logger, button), new UnturnedLabel(logger, label)) { }
    public UnturnedEnumButton(ILoggerFactory factory, TEnum defaultValue, string button, string label) :
        base(defaultValue, new UnturnedButton(factory, button), new UnturnedLabel(factory, label)) { }
    public UnturnedEnumButton(TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) :
        this(GlobalLogger.Instance, defaultValue, button, label, disableState, rightClickListener) { }
    public UnturnedEnumButton(ILogger logger, TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) :
        base(defaultValue, new UnturnedButton(logger, button), new UnturnedLabel(logger, label),
            disableState == null ? null : new UnturnedUIElement(logger, disableState),
            rightClickListener == null ? null : new UnturnedButton(logger, rightClickListener)) { }
    public UnturnedEnumButton(ILoggerFactory factory, TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) :
        base(defaultValue, new UnturnedButton(factory, button), new UnturnedLabel(factory, label),
            disableState == null ? null : new UnturnedUIElement(factory, disableState),
            rightClickListener == null ? null : new UnturnedButton(factory, rightClickListener)) { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string button, string label) :
        this(GlobalLogger.Instance, valueSet, defaultValue, button, label) { }
    public UnturnedEnumButton(ILogger logger, TEnum[] valueSet, TEnum defaultValue, string button, string label) :
        base(valueSet, defaultValue, new UnturnedButton(logger, button), new UnturnedLabel(logger, label)) { }
    public UnturnedEnumButton(ILoggerFactory factory, TEnum[] valueSet, TEnum defaultValue, string button, string label) :
        base(valueSet, defaultValue, new UnturnedButton(factory, button), new UnturnedLabel(factory, label)) { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) :
        this(GlobalLogger.Instance, valueSet, defaultValue, button, label, disableState, rightClickListener) { }
    public UnturnedEnumButton(ILogger logger, TEnum[] valueSet, TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) :
        base(valueSet, defaultValue, new UnturnedButton(logger, button), new UnturnedLabel(logger, label),
            disableState == null ? null : new UnturnedUIElement(logger, disableState),
            rightClickListener == null ? null : new UnturnedButton(logger, rightClickListener)) { }
    public UnturnedEnumButton(ILoggerFactory factory, TEnum[] valueSet, TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) :
        base(valueSet, defaultValue, new UnturnedButton(factory, button), new UnturnedLabel(factory, label),
            disableState == null ? null : new UnturnedUIElement(factory, disableState),
            rightClickListener == null ? null : new UnturnedButton(factory, rightClickListener)) { }
}
