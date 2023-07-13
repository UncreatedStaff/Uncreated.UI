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
}
