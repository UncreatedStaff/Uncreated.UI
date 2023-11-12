using DanielWillett.ReflectionTools;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// Represents a text box with a placeholder.
/// </summary>
public class PlaceholderTextBox : IPlaceholderTextBox
{
    [UIPattern("", Mode = FormatMode.Format)]
    public UnturnedTextBox TextBox { get; }

    [UIPattern("Placeholder", Mode = FormatMode.Format)]
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
    public PlaceholderTextBox(string textBox) : this(textBox, textBox + "Placeholder") { }
    public PlaceholderTextBox(string textBox, string? placeholder)
    {
        TextBox = new UnturnedTextBox(textBox);
        Placeholder = new UnturnedLabel(placeholder ?? (textBox + "Placeholder"));
    }

    [Ignore]
    UnturnedLabel ILabel.Label => TextBox;

    [Ignore]
    UnturnedUIElement IElement.Element => TextBox;
}

/// <summary>
/// Represents a text box with a state.
/// </summary>
public class StateTextBox : IStateElement, ITextBox
{
    [UIPattern("", Mode = FormatMode.Format)]
    public UnturnedTextBox TextBox { get; }

    [UIPattern("State", Mode = FormatMode.Format)]
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
    public StateTextBox(string textBox) : this(textBox, textBox + "State") { }
    public StateTextBox(string textBox, string? state)
    {
        TextBox = new UnturnedTextBox(textBox);
        State = new UnturnedUIElement(state ?? (textBox + "State"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => TextBox;

    [Ignore]
    UnturnedLabel ILabel.Label => TextBox;
}

/// <summary>
/// Represents a text box with a state and placeholder.
/// </summary>
public class StatePlaceholderTextBox : PlaceholderTextBox, IStateElement
{

    [UIPattern("State", Mode = FormatMode.Format)]
    public UnturnedUIElement State { get; }

    public StatePlaceholderTextBox(string textBox) : base(textBox)
    {
        State = new UnturnedLabel(textBox + "State");
    }
    public StatePlaceholderTextBox(string textBox, string? placeholder, string? state) : base(textBox, placeholder ?? (textBox + "Placeholder"))
    {
        State = new UnturnedLabel(state ?? (textBox + "State"));
    }
}
