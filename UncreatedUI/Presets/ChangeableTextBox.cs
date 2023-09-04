using SDG.NetTransport;

namespace Uncreated.Framework.UI.Presets;
public readonly struct ChangeableTextBox
{
    public UnturnedTextBox TextBox { get; }
    public UnturnedLabel Placeholder { get; }

    public event TextUpdated OnTextUpdated
    {
        add => TextBox.OnTextUpdated += value;
        remove => TextBox.OnTextUpdated -= value;
    }
    public bool UseData
    {
        get => TextBox.UseData;
        set => TextBox.UseData = value;
    }
    public ChangeableTextBox(string textBox) : this(textBox, textBox + "Placeholder") { }
    public ChangeableTextBox(string textBox, string placeholder)
    {
        TextBox = new UnturnedTextBox(textBox);
        Placeholder = new UnturnedLabel(placeholder ?? (textBox + "Placeholder"));
    }
    public void Hide(ITransportConnection connection) => TextBox.SetVisibility(connection, false);
    public void Show(ITransportConnection connection) => TextBox.SetVisibility(connection, true);
    public void SetText(ITransportConnection connection, string text) => TextBox.SetText(connection, text);
    public void SetPlaceholder(ITransportConnection connection, string placeholder) => Placeholder.SetText(connection, placeholder);
}
public readonly struct ChangeableStateTextBox
{
    public UnturnedTextBox TextBox { get; }
    public UnturnedLabel Placeholder { get; }
    public UnturnedUIElement State { get; }

    public event TextUpdated OnTextUpdated
    {
        add => TextBox.OnTextUpdated += value;
        remove => TextBox.OnTextUpdated -= value;
    }
    public bool UseData
    {
        get => TextBox.UseData;
        set => TextBox.UseData = value;
    }

    public ChangeableStateTextBox(string textBox)
    {
        TextBox = new UnturnedTextBox(textBox);
        Placeholder = new UnturnedLabel(textBox + "Placeholder");
        State = new UnturnedLabel(textBox + "State");
    }
    public ChangeableStateTextBox(string textBox, string? placeholder = null, string? state = null)
    {
        TextBox = new UnturnedTextBox(textBox);
        Placeholder = new UnturnedLabel(placeholder ?? (textBox + "Placeholder"));
        State = new UnturnedLabel(state ?? (textBox + "State"));
    }

    public void Disable(ITransportConnection connection) => State.SetVisibility(connection, false);
    public void Enable(ITransportConnection connection) => State.SetVisibility(connection, true);
    public void Hide(ITransportConnection connection) => TextBox.SetVisibility(connection, false);
    public void Show(ITransportConnection connection) => TextBox.SetVisibility(connection, true);
    public void SetText(ITransportConnection connection, string text) => TextBox.SetText(connection, text);
    public void SetPlaceholder(ITransportConnection connection, string placeholder) => Placeholder.SetText(connection, placeholder);
}
