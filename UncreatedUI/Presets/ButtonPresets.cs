using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using System;

namespace Uncreated.Framework.UI.Presets;
public class LabeledButton : ILabeledButton
{
    public UnturnedButton Button { get; }
    public UnturnedLabel Label { get; }

    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledButton(string name) : this(GlobalLogger.Instance, name, name + "Label") { }
    public LabeledButton(ILoggerFactory factory, string name) : this(factory, name, name + "Label") { }
    public LabeledButton(ILogger logger, string name) : this(logger, name, name + "Label") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledButton(string button, string? label) : this(GlobalLogger.Instance, button, label) { }
    public LabeledButton(ILoggerFactory factory, string button, string? label)
    {
        Button = new UnturnedButton(factory, button);
        Label = new UnturnedLabel(factory, label ?? (button + "Label"));
    }
    public LabeledButton(ILogger logger, string button, string? label)
    {
        Button = new UnturnedButton(logger, button);
        Label = new UnturnedLabel(logger, label ?? (button + "Label"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;
}

public class StateButton : IButton, IStateElement
{
    public UnturnedButton Button { get; }
    public UnturnedUIElement State { get; }

    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StateButton(string name) : this(GlobalLogger.Instance, name, name + "State") { }
    public StateButton(ILogger logger, string name) : this(logger, name, name + "State") { }
    public StateButton(ILoggerFactory factory, string name) : this(factory, name, name + "State") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StateButton(string button, string? state) : this(GlobalLogger.Instance, button, state) { }
    public StateButton(ILogger logger, string button, string? state)
    {
        Button = new UnturnedButton(logger, button);
        State = new UnturnedUIElement(logger, state ?? (button + "State"));
    }
    public StateButton(ILoggerFactory factory, string button, string? state)
    {
        Button = new UnturnedButton(factory, button);
        State = new UnturnedUIElement(factory, state ?? (button + "State"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;
}

public class RightClickableButton : IRightClickableButton
{
    public UnturnedButton Button { get; }
    public UnturnedButton RightClickListener { get; }

    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }
    public event ButtonClicked OnRightClicked
    {
        add => RightClickListener.OnClicked += value;
        remove => RightClickListener.OnClicked -= value;
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public RightClickableButton(string name) : this(GlobalLogger.Instance, name, name + "RightClickListener") { }
    public RightClickableButton(ILogger logger, string name) : this(logger, name, name + "RightClickListener") { }
    public RightClickableButton(ILoggerFactory factory, string name) : this(factory, name, name + "RightClickListener") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public RightClickableButton(string button, string? rightClickListenerButton) : this(GlobalLogger.Instance, button, rightClickListenerButton) { }
    public RightClickableButton(ILogger logger, string button, string? rightClickListenerButton)
    {
        Button = new UnturnedButton(logger, button);
        RightClickListener = new UnturnedButton(logger, rightClickListenerButton ?? (button + "RightClickListener"));
    }
    public RightClickableButton(ILoggerFactory factory, string button, string? rightClickListenerButton)
    {
        Button = new UnturnedButton(factory, button);
        RightClickListener = new UnturnedButton(factory, rightClickListenerButton ?? (button + "RightClickListener"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;
}

public class LabeledStateButton : LabeledButton, IStateElement
{
    public UnturnedUIElement State { get; }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledStateButton(string name) : this(GlobalLogger.Instance, name) { }
    public LabeledStateButton(ILogger logger, string name) : base(logger, name)
    {
        State = new UnturnedUIElement(logger, name + "State");
    }
    public LabeledStateButton(ILoggerFactory factory, string name) : base(factory, name)
    {
        State = new UnturnedUIElement(factory, name + "State");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledStateButton(string button, string? label, string? state) : this(GlobalLogger.Instance, button, label, state) { }
    public LabeledStateButton(ILogger logger, string button, string? label, string? state) : base(logger, button, label)
    {
        State = new UnturnedUIElement(logger, state ?? (button + "State"));
    }
    public LabeledStateButton(ILoggerFactory factory, string button, string? label, string? state) : base(factory, button, label)
    {
        State = new UnturnedUIElement(factory, state ?? (button + "State"));
    }
}

public class RightClickableStateButton : RightClickableButton, IStateElement
{
    public UnturnedUIElement State { get; }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public RightClickableStateButton(string name) : this(GlobalLogger.Instance, name) { }
    public RightClickableStateButton(ILogger logger, string name) : base(logger, name)
    {
        State = new UnturnedUIElement(logger, name + "State");
    }
    public RightClickableStateButton(ILoggerFactory factory, string name) : base(factory, name)
    {
        State = new UnturnedUIElement(factory, name + "State");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public RightClickableStateButton(string button, string? rightClickListenerButton, string? state) : this(GlobalLogger.Instance, button, rightClickListenerButton, state) { }
    public RightClickableStateButton(ILogger logger, string button, string? rightClickListenerButton, string? state) : base(logger, button, rightClickListenerButton)
    {
        State = new UnturnedUIElement(logger, state ?? (button + "State"));
    }
    public RightClickableStateButton(ILoggerFactory factory, string button, string? rightClickListenerButton, string? state) : base(factory, button, rightClickListenerButton)
    {
        State = new UnturnedUIElement(factory, state ?? (button + "State"));
    }
}

public class LabeledRightClickableButton : RightClickableButton, ILabeledButton
{
    public UnturnedLabel Label { get; }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledRightClickableButton(string name) : this(GlobalLogger.Instance, name) { }
    public LabeledRightClickableButton(ILogger logger, string name) : base(logger, name)
    {
        Label = new UnturnedLabel(logger, name + "Label");
    }
    public LabeledRightClickableButton(ILoggerFactory factory, string name) : base(factory, name)
    {
        Label = new UnturnedLabel(factory, name + "Label");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledRightClickableButton(string button, string? label, string? rightClickListenerButton) : this(GlobalLogger.Instance, button, label, rightClickListenerButton) { }
    public LabeledRightClickableButton(ILogger logger, string button, string? label, string? rightClickListenerButton) : base(logger, button, rightClickListenerButton)
    {
        Label = new UnturnedLabel(logger, label ?? (button + "Label"));
    }
    public LabeledRightClickableButton(ILoggerFactory factory, string button, string? label, string? rightClickListenerButton) : base(factory, button, rightClickListenerButton)
    {
        Label = new UnturnedLabel(factory, label ?? (button + "Label"));
    }
}

public class LabeledRightClickableStateButton : LabeledRightClickableButton, IStateElement
{
    public UnturnedUIElement State { get; }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledRightClickableStateButton(string name) : this(GlobalLogger.Instance, name) { }
    public LabeledRightClickableStateButton(ILogger logger, string name) : base(logger, name)
    {
        State = new UnturnedUIElement(logger, name + "State");
    }
    public LabeledRightClickableStateButton(ILoggerFactory factory, string name) : base(factory, name)
    {
        State = new UnturnedUIElement(factory, name + "State");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public LabeledRightClickableStateButton(string button, string? label, string? rightClickListenerButton, string? state) : this(GlobalLogger.Instance, button, label, rightClickListenerButton, state) { }
    public LabeledRightClickableStateButton(ILogger logger, string button, string? label, string? rightClickListenerButton, string? state) : base(logger, button, label, rightClickListenerButton)
    {
        State = new UnturnedUIElement(logger, state ?? (button + "State"));
    }
    public LabeledRightClickableStateButton(ILoggerFactory factory, string button, string? label, string? rightClickListenerButton, string? state) : base(factory, button, label, rightClickListenerButton)
    {
        State = new UnturnedUIElement(factory, state ?? (button + "State"));
    }
}