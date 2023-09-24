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
    public LabeledButton(string name) : this(name, name + "Label") { }
    public LabeledButton(string button, string? label)
    {
        Button = new UnturnedButton(button);
        Label = new UnturnedLabel(label ?? (button + "Label"));
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
    public StateButton(string name) : this(name, name + "State") { }
    public StateButton(string button, string? state)
    {
        Button = new UnturnedButton(button);
        State = new UnturnedUIElement(state ?? (button + "State"));
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
    public RightClickableButton(string name) : this(name, name + "RightClickListener") { }
    public RightClickableButton(string button, string? rightClickListenerButton)
    {
        Button = new UnturnedButton(button);
        RightClickListener = new UnturnedButton(rightClickListenerButton ?? (button + "RightClickListener"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;
}

public class LabeledStateButton : LabeledButton, IStateElement
{
    public UnturnedUIElement State { get; }

    public LabeledStateButton(string name) : base(name)
    {
        State = new UnturnedUIElement(name + "State");
    }
    public LabeledStateButton(string button, string? label, string? state) : base(button, label)
    {
        State = new UnturnedUIElement(state ?? (button + "State"));
    }
}

public class RightClickableStateButton : RightClickableButton, IStateElement
{
    public UnturnedUIElement State { get; }
    
    public RightClickableStateButton(string name) : base(name)
    {
        State = new UnturnedUIElement(name + "State");
    }
    public RightClickableStateButton(string button, string? rightClickListenerButton, string? state) : base(button, rightClickListenerButton)
    {
        State = new UnturnedUIElement(state ?? (button + "State"));
    }
}

public class LabeledRightClickableButton : RightClickableButton, ILabeledButton
{
    public UnturnedLabel Label { get; }
    public LabeledRightClickableButton(string name) : base(name)
    {
        Label = new UnturnedLabel(name + "Label");
    }
    public LabeledRightClickableButton(string button, string? label, string? rightClickListenerButton) : base(button, rightClickListenerButton)
    {
        Label = new UnturnedLabel(label ?? (button + "Label"));
    }
}

public class LabeledRightClickableStateButton : LabeledRightClickableButton, IStateElement
{
    public UnturnedUIElement State { get; }
    public LabeledRightClickableStateButton(string name) : base(name)
    {
        State = new UnturnedUIElement(name + "State");
    }
    public LabeledRightClickableStateButton(string button, string? label, string? rightClickListenerButton, string? state) : base(button, label, rightClickListenerButton)
    {
        State = new UnturnedUIElement(state ?? (button + "State"));
    }
}