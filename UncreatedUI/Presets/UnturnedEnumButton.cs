using Microsoft.Extensions.Logging;
using SDG.Unturned;
using System;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// Handles when an enum button updates.
/// </summary>
public delegate void ValueUpdated<TEnum>(UnturnedEnumButtonTracker<TEnum> button, Player player, TEnum value) where TEnum : unmanaged, Enum;

/// <summary>
/// Converts an enum value to text.
/// </summary>
public delegate string TextFormatter<in TEnum>(TEnum value, Player player) where TEnum : unmanaged, Enum;

/// <summary>
/// A button that can switch between values of an enum on click.
/// </summary>
public class UnturnedEnumButton<TEnum> : UnturnedEnumButtonTracker<TEnum> where TEnum : unmanaged, Enum
{
    public UnturnedEnumButton(TEnum defaultValue, string buttonPath, string labelPath) :
        this(GlobalLogger.Instance, defaultValue, buttonPath, labelPath) { }
    public UnturnedEnumButton(ILogger logger, TEnum defaultValue, string buttonPath, string labelPath) :
        base(defaultValue, new UnturnedButton(logger, buttonPath), new UnturnedLabel(logger, labelPath)) { }
    public UnturnedEnumButton(ILoggerFactory factory, TEnum defaultValue, string buttonPath, string labelPath) :
        base(defaultValue, new UnturnedButton(factory, buttonPath), new UnturnedLabel(factory, labelPath)) { }
    public UnturnedEnumButton(TEnum defaultValue, string buttonPath, string labelPath, string? statePath, string? rightClickListenerButtonPath) :
        this(GlobalLogger.Instance, defaultValue, buttonPath, labelPath, statePath, rightClickListenerButtonPath) { }
    public UnturnedEnumButton(ILogger logger, TEnum defaultValue, string buttonPath, string labelPath, string? statePath, string? rightClickListenerButtonPath) :
        base(defaultValue, new UnturnedButton(logger, buttonPath), new UnturnedLabel(logger, labelPath),
            statePath == null ? null : new UnturnedUIElement(logger, statePath),
            rightClickListenerButtonPath == null ? null : new UnturnedButton(logger, rightClickListenerButtonPath)) { }
    public UnturnedEnumButton(ILoggerFactory factory, TEnum defaultValue, string buttonPath, string labelPath, string? statePath, string? rightClickListenerButtonPath) :
        base(defaultValue, new UnturnedButton(factory, buttonPath), new UnturnedLabel(factory, labelPath),
            statePath == null ? null : new UnturnedUIElement(factory, statePath),
            rightClickListenerButtonPath == null ? null : new UnturnedButton(factory, rightClickListenerButtonPath)) { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string buttonPath, string labelPath) :
        this(GlobalLogger.Instance, valueSet, defaultValue, buttonPath, labelPath) { }
    public UnturnedEnumButton(ILogger logger, TEnum[] valueSet, TEnum defaultValue, string buttonPath, string labelPath) :
        base(valueSet, defaultValue, new UnturnedButton(logger, buttonPath), new UnturnedLabel(logger, labelPath)) { }
    public UnturnedEnumButton(ILoggerFactory factory, TEnum[] valueSet, TEnum defaultValue, string buttonPath, string labelPath) :
        base(valueSet, defaultValue, new UnturnedButton(factory, buttonPath), new UnturnedLabel(factory, labelPath)) { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string buttonPath, string labelPath, string? statePath, string? rightClickListenerButtonPath) :
        this(GlobalLogger.Instance, valueSet, defaultValue, buttonPath, labelPath, statePath, rightClickListenerButtonPath) { }
    public UnturnedEnumButton(ILogger logger, TEnum[] valueSet, TEnum defaultValue, string buttonPath, string labelPath, string? statePath, string? rightClickListenerButtonPath) :
        base(valueSet, defaultValue, new UnturnedButton(logger, buttonPath), new UnturnedLabel(logger, labelPath),
            statePath == null ? null : new UnturnedUIElement(logger, statePath),
            rightClickListenerButtonPath == null ? null : new UnturnedButton(logger, rightClickListenerButtonPath)) { }
    public UnturnedEnumButton(ILoggerFactory factory, TEnum[] valueSet, TEnum defaultValue, string buttonPath, string labelPath, string? statePath, string? rightClickListenerButtonPath) :
        base(valueSet, defaultValue, new UnturnedButton(factory, buttonPath), new UnturnedLabel(factory, labelPath),
            statePath == null ? null : new UnturnedUIElement(factory, statePath),
            rightClickListenerButtonPath == null ? null : new UnturnedButton(factory, rightClickListenerButtonPath)) { }
}
