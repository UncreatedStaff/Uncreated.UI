using SDG.NetTransport;

namespace Uncreated.Framework.UI.Presets;
public readonly struct LabeledButton
{
    public UnturnedButton Button { get; }
    public UnturnedLabel Label { get; }

    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }
    public LabeledButton(string name)
    {
        Button = new UnturnedButton(name);
        Label = new UnturnedLabel(name + "Label");
    }
    public LabeledButton(string button, string label)
    {
        Button = new UnturnedButton(button);
        Label = new UnturnedLabel(label);
    }
    public void SetText(ITransportConnection connection, string text) => Label.SetText(connection, text);
    public void Show(ITransportConnection player) => Button.SetVisibility(player, true);
    public void Hide(ITransportConnection player) => Button.SetVisibility(player, false);
}

public readonly struct RightClickableButton
{
    public UnturnedButton Button { get; }
    public UnturnedButton RightClickListener { get; }
    public UnturnedLabel Label { get; }

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
    public RightClickableButton(string name)
    {
        Button = new UnturnedButton(name);
        RightClickListener = new UnturnedButton(name + "RightClickListener");
        Label = new UnturnedLabel(name + "Label");
    }
    public RightClickableButton(string button, string? rightClickListenerButton = null, string? label = null)
    {
        Button = new UnturnedButton(button);
        RightClickListener = new UnturnedButton(rightClickListenerButton ?? (button + "RightClickListener"));
        Label = new UnturnedLabel(label ?? (button + "Label"));
    }
    public void SetText(ITransportConnection connection, string text) => Label.SetText(connection, text);
    public void Show(ITransportConnection player) => Button.SetVisibility(player, true);
    public void Hide(ITransportConnection player) => Button.SetVisibility(player, false);
}

public readonly struct LabeledStateButton
{
    public UnturnedButton Button { get; }
    public UnturnedLabel Label { get; }
    public UnturnedUIElement State { get; }

    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }
    public LabeledStateButton(string name)
    {
        Button = new UnturnedButton(name);
        Label = new UnturnedLabel(name + "Label");
        State = new UnturnedUIElement(name + "State");
    }
    public LabeledStateButton(string button, string? label = null, string? state = null)
    {
        Button = new UnturnedButton(button);
        Label = new UnturnedLabel(label ?? (button + "Label"));
        State = new UnturnedUIElement(state ?? (button + "State"));
    }

    public void SetText(ITransportConnection connection, string text) => Label.SetText(connection, text);
    public void Disable(ITransportConnection connection) => State.SetVisibility(connection, false);
    public void Enable(ITransportConnection connection) => State.SetVisibility(connection, true);
    public void Show(ITransportConnection player) => Button.SetVisibility(player, true);
    public void Hide(ITransportConnection player) => Button.SetVisibility(player, false);
}
public readonly struct RightClickableStateButton
{
    public UnturnedButton Button { get; }
    public UnturnedButton RightClickListener { get; }
    public UnturnedLabel Label { get; }
    public UnturnedUIElement State { get; }

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
    public RightClickableStateButton(string name)
    {
        Button = new UnturnedButton(name);
        RightClickListener = new UnturnedButton(name + "RightClickListener");
        Label = new UnturnedLabel(name + "Label");
        State = new UnturnedUIElement(name + "State");
    }
    public RightClickableStateButton(string button, string? rightClickListenerButton = null, string? label = null, string? state = null)
    {
        Button = new UnturnedButton(button);
        RightClickListener = new UnturnedButton(rightClickListenerButton ?? (button + "RightClickListener"));
        Label = new UnturnedLabel(label ?? (button + "Label"));
        State = new UnturnedUIElement(state ?? (button + "State"));
    }

    public void SetText(ITransportConnection connection, string text) => Label.SetText(connection, text);
    public void Disable(ITransportConnection connection) => State.SetVisibility(connection, false);
    public void Enable(ITransportConnection connection) => State.SetVisibility(connection, true);
    public void Show(ITransportConnection player) => Button.SetVisibility(player, true);
    public void Hide(ITransportConnection player) => Button.SetVisibility(player, false);
}