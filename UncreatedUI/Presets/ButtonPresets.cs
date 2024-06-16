using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using System;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// A button with a label.
/// </summary>
public class LabeledButton : ILabeledButton
{
    /// <inheritdoc />
    public UnturnedButton Button { get; }

    /// <inheritdoc />
    public UnturnedLabel Label { get; }

    /// <inheritdoc />
    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledButton(string path) : this(GlobalLogger.Instance, path, path + "Label") { }
    public LabeledButton(ILoggerFactory factory, string path) : this(factory, path, path + "Label") { }
    public LabeledButton(ILogger logger, string path) : this(logger, path, path + "Label") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledButton(string buttonPath, string? labelPath) : this(GlobalLogger.Instance, buttonPath, labelPath) { }
    public LabeledButton(ILoggerFactory factory, string buttonPath, string? labelPath)
    {
        Button = new UnturnedButton(factory, buttonPath);
        Label = new UnturnedLabel(factory, UnturnedUIUtility.GetPresetValue(buttonPath, labelPath, "Label"));
    }
    public LabeledButton(ILogger logger, string buttonPath, string? labelPath)
    {
        Button = new UnturnedButton(logger, buttonPath);
        Label = new UnturnedLabel(logger, UnturnedUIUtility.GetPresetValue(buttonPath, labelPath, "Label"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;
}

/// <summary>
/// A button with a enabled/disabled state.
/// </summary>
public class StateButton : IButton, IStateElement
{
    /// <inheritdoc />
    public UnturnedButton Button { get; }

    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    /// <inheritdoc />
    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StateButton(string path) : this(GlobalLogger.Instance, path, path + "State") { }
    public StateButton(ILogger logger, string path) : this(logger, path, path + "State") { }
    public StateButton(ILoggerFactory factory, string path) : this(factory, path, path + "State") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StateButton(string buttonPath, string? statePath) : this(GlobalLogger.Instance, buttonPath, statePath) { }
    public StateButton(ILogger logger, string buttonPath, string? statePath)
    {
        Button = new UnturnedButton(logger, buttonPath);
        State = new UnturnedUIElement(logger, UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
    public StateButton(ILoggerFactory factory, string buttonPath, string? statePath)
    {
        Button = new UnturnedButton(factory, buttonPath);
        State = new UnturnedUIElement(factory, UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;
}

/// <summary>
/// A button with a right click listener.
/// </summary>
public class RightClickableButton : IRightClickableButton
{
    /// <inheritdoc />
    public UnturnedButton Button { get; }

    /// <inheritdoc />
    public UnturnedButton RightClickListener { get; }

    /// <inheritdoc />
    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }

    /// <inheritdoc />
    public event ButtonClicked OnRightClicked
    {
        add => RightClickListener.OnClicked += value;
        remove => RightClickListener.OnClicked -= value;
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public RightClickableButton(string path) : this(GlobalLogger.Instance, path, path + "RightClickListener") { }
    public RightClickableButton(ILogger logger, string path) : this(logger, path, path + "RightClickListener") { }
    public RightClickableButton(ILoggerFactory factory, string path) : this(factory, path, path + "RightClickListener") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public RightClickableButton(string buttonPath, string? rightClickListenerButtonPath)
        : this(GlobalLogger.Instance, buttonPath, rightClickListenerButtonPath) { }
    public RightClickableButton(ILogger logger, string buttonPath, string? rightClickListenerButtonPath)
    {
        Button = new UnturnedButton(logger, buttonPath);
        RightClickListener = new UnturnedButton(logger, UnturnedUIUtility.GetPresetValue(buttonPath, rightClickListenerButtonPath, "RightClickListener"));
    }
    public RightClickableButton(ILoggerFactory factory, string buttonPath, string? rightClickListenerButtonPath)
    {
        Button = new UnturnedButton(factory, buttonPath);
        RightClickListener = new UnturnedButton(factory, UnturnedUIUtility.GetPresetValue(buttonPath, rightClickListenerButtonPath, "RightClickListener"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;
}

/// <summary>
/// Labeled button who's intractability can be enabled and disabled.
/// </summary>
public class LabeledStateButton : LabeledButton, IStateElement
{
    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledStateButton(string path) : this(GlobalLogger.Instance, path) { }
    public LabeledStateButton(ILogger logger, string path) : base(logger, path)
    {
        State = new UnturnedUIElement(logger, path + "State");
    }
    public LabeledStateButton(ILoggerFactory factory, string path) : base(factory, path)
    {
        State = new UnturnedUIElement(factory, path + "State");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledStateButton(string buttonPath, string? labelPath, string? statePath)
        : this(GlobalLogger.Instance, buttonPath, labelPath, statePath) { }
    public LabeledStateButton(ILogger logger, string buttonPath, string? labelPath, string? statePath)
        : base(logger, buttonPath, labelPath)
    {
        State = new UnturnedUIElement(logger, UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
    public LabeledStateButton(ILoggerFactory factory, string buttonPath, string? labelPath, string? statePath)
        : base(factory, buttonPath, labelPath)
    {
        State = new UnturnedUIElement(factory, UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
}

/// <summary>
/// Right-clickable button who's intractability can be enabled and disabled.
/// </summary>
public class RightClickableStateButton : RightClickableButton, IStateElement
{
    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public RightClickableStateButton(string path) : this(GlobalLogger.Instance, path) { }
    public RightClickableStateButton(ILogger logger, string path) : base(logger, path)
    {
        State = new UnturnedUIElement(logger, path + "State");
    }
    public RightClickableStateButton(ILoggerFactory factory, string path) : base(factory, path)
    {
        State = new UnturnedUIElement(factory, path + "State");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public RightClickableStateButton(string buttonPath, string? rightClickListenerButtonPath, string? statePath)
        : this(GlobalLogger.Instance, buttonPath, rightClickListenerButtonPath, statePath) { }
    public RightClickableStateButton(ILogger logger, string buttonPath, string? rightClickListenerButtonPath, string? statePath)
        : base(logger, buttonPath, rightClickListenerButtonPath)
    {
        State = new UnturnedUIElement(logger, UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
    public RightClickableStateButton(ILoggerFactory factory, string buttonPath, string? rightClickListenerButtonPath, string? statePath)
        : base(factory, buttonPath, rightClickListenerButtonPath)
    {
        State = new UnturnedUIElement(factory, UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
}

/// <summary>
/// A button with a label and right click listener.
/// </summary>
public class LabeledRightClickableButton : RightClickableButton, ILabeledButton
{
    /// <inheritdoc />
    public UnturnedLabel Label { get; }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledRightClickableButton(string path) : this(GlobalLogger.Instance, path) { }
    public LabeledRightClickableButton(ILogger logger, string path) : base(logger, path)
    {
        Label = new UnturnedLabel(logger, path + "Label");
    }
    public LabeledRightClickableButton(ILoggerFactory factory, string path) : base(factory, path)
    {
        Label = new UnturnedLabel(factory, path + "Label");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledRightClickableButton(string buttonPath, string? labelPath, string? rightClickListenerButtonPath)
        : this(GlobalLogger.Instance, buttonPath, labelPath, rightClickListenerButtonPath) { }
    public LabeledRightClickableButton(ILogger logger, string buttonPath, string? labelPath, string? rightClickListenerButtonPath)
        : base(logger, buttonPath, rightClickListenerButtonPath)
    {
        Label = new UnturnedLabel(logger, UnturnedUIUtility.GetPresetValue(buttonPath, labelPath, "Label"));
    }
    public LabeledRightClickableButton(ILoggerFactory factory, string buttonPath, string? labelPath, string? rightClickListenerButtonPath)
        : base(factory, buttonPath, rightClickListenerButtonPath)
    {
        Label = new UnturnedLabel(factory, UnturnedUIUtility.GetPresetValue(buttonPath, labelPath, "Label"));
    }
}

/// <summary>
/// A button with a label and right click listener, who's intractability can be enabled and disabled.
/// </summary>
public class LabeledRightClickableStateButton : LabeledRightClickableButton, IStateElement
{
    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledRightClickableStateButton(string path) : this(GlobalLogger.Instance, path) { }
    public LabeledRightClickableStateButton(ILogger logger, string path) : base(logger, path)
    {
        State = new UnturnedUIElement(logger, path + "State");
    }
    public LabeledRightClickableStateButton(ILoggerFactory factory, string path) : base(factory, path)
    {
        State = new UnturnedUIElement(factory, path + "State");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledRightClickableStateButton(string buttonPath, string? labelPath, string? rightClickListenerButtonPath, string? statePath)
        : this(GlobalLogger.Instance, buttonPath, labelPath, rightClickListenerButtonPath, statePath) { }
    public LabeledRightClickableStateButton(ILogger logger, string buttonPath, string? labelPath, string? rightClickListenerButtonPath, string? statePath)
        : base(logger, buttonPath, labelPath, rightClickListenerButtonPath)
    {
        State = new UnturnedUIElement(logger, UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
    public LabeledRightClickableStateButton(ILoggerFactory factory, string buttonPath, string? labelPath, string? rightClickListenerButtonPath, string? statePath)
        : base(factory, buttonPath, labelPath, rightClickListenerButtonPath)
    {
        State = new UnturnedUIElement(factory, UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
}