using SDG.NetTransport;

namespace Uncreated.Framework.UI.Presets;
public readonly struct ChangeableTextBox
{
    public UnturnedTextBox TextBox { get; }
    public UnturnedLabel Text { get; }
    public UnturnedLabel Placeholder { get; }

    public event TextUpdated OnTextUpdated
    {
        add => TextBox.OnTextUpdated += value;
        remove => TextBox.OnTextUpdated -= value;
    }
    public ChangeableTextBox(string textBox)
    {
        TextBox = new UnturnedTextBox(textBox);
        Text = new UnturnedLabel(textBox + "Text");
        Placeholder = new UnturnedLabel(textBox + "Placeholder");
    }
    public ChangeableTextBox(string textBox, string? text = null, string? placeholder = null)
    {
        TextBox = new UnturnedTextBox(textBox);
        Text = new UnturnedLabel(text ?? (textBox + "Text"));
        Placeholder = new UnturnedLabel(placeholder ?? (textBox + "Placeholder"));
    }
    public void SetText(ITransportConnection connection, string text) => Text.SetText(connection, text);
    public void SetPlaceholder(ITransportConnection connection, string placeholder) => Placeholder.SetText(connection, placeholder);
}
public readonly struct ChangeableStateTextBox
{
    public UnturnedTextBox TextBox { get; }
    public UnturnedLabel Text { get; }
    public UnturnedLabel Placeholder { get; }
    public UnturnedUIElement State { get; }

    public event TextUpdated OnTextUpdated
    {
        add => TextBox.OnTextUpdated += value;
        remove => TextBox.OnTextUpdated -= value;
    }

    public ChangeableStateTextBox(string textBox)
    {
        TextBox = new UnturnedTextBox(textBox);
        Text = new UnturnedLabel(textBox + "Text");
        Placeholder = new UnturnedLabel(textBox + "Placeholder");
        State = new UnturnedLabel(textBox + "State");
    }
    public ChangeableStateTextBox(string textBox, string? text = null, string? placeholder = null, string? state = null)
    {
        TextBox = new UnturnedTextBox(textBox);
        Text = new UnturnedLabel(text ?? (textBox + "Text"));
        Placeholder = new UnturnedLabel(placeholder ?? (textBox + "Placeholder"));
        State = new UnturnedLabel(state ?? (textBox + "State"));
    }

    public void Disable(ITransportConnection connection) => State.SetVisibility(connection, false);
    public void Enable(ITransportConnection connection) => State.SetVisibility(connection, true);
    public void SetText(ITransportConnection connection, string text) => Text.SetText(connection, text);
    public void SetPlaceholder(ITransportConnection connection, string placeholder) => Placeholder.SetText(connection, placeholder);
}
