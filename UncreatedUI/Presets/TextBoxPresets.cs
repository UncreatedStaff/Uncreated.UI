using DanielWillett.ReflectionTools;
using Uncreated.Framework.UI.Patterns;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// Represents a text box with a placeholder.
/// </summary>
public class PlaceholderTextBox : IPlaceholderTextBox
{
    /// <inheritdoc />
    [Pattern("", Mode = FormatMode.Format, Root = true)]
    public UnturnedTextBox TextBox { get; }

    /// <inheritdoc />
    [Pattern("Placeholder", Mode = FormatMode.Format)]
    public UnturnedLabel Placeholder { get; }

    /// <inheritdoc />
    public event TextUpdated OnTextUpdated
    {
        add => TextBox.OnTextUpdated += value;
        remove => TextBox.OnTextUpdated -= value;
    }

    /// <inheritdoc />
    public bool UseData
    {
        get => TextBox.UseData;
        set => TextBox.UseData = value;
    }

    public PlaceholderTextBox(string path) : this(path, path + "Placeholder") { }

    public PlaceholderTextBox(string path, string? placeholderPath)
    {
        TextBox = new UnturnedTextBox(path);
        Placeholder = new UnturnedLabel(UnturnedUIUtility.GetPresetValue(path, placeholderPath, "Placeholder"));
    }

    [Ignore]
    UnturnedLabel ILabel.Label => TextBox;

    [Ignore]
    UnturnedUIElement IElement.Element => TextBox;

    /// <inheritdoc />
    public override string ToString()
    {
        return "Placeholder Text Box [" + TextBox.Path + "] (" + TextBox.Owner.Name + ")";
    }
}

/// <summary>
/// Represents a text box with a state.
/// </summary>
public class StateTextBox : IStateElement, ITextBox
{
    /// <inheritdoc />
    [Pattern("", Mode = FormatMode.Format, Root = true)]
    public UnturnedTextBox TextBox { get; }

    /// <inheritdoc />
    [Pattern("State", Mode = FormatMode.Format)]
    public UnturnedUIElement State { get; }

    /// <inheritdoc />
    public event TextUpdated OnTextUpdated
    {
        add => TextBox.OnTextUpdated += value;
        remove => TextBox.OnTextUpdated -= value;
    }

    /// <inheritdoc />
    public bool UseData
    {
        get => TextBox.UseData;
        set => TextBox.UseData = value;
    }

    public StateTextBox(string path) : this(path, path + "State") { }

    public StateTextBox(string path, string? statePath)
    {
        TextBox = new UnturnedTextBox(path);
        State = new UnturnedUIElement(UnturnedUIUtility.GetPresetValue(path, statePath, "State"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => TextBox;

    [Ignore]
    UnturnedLabel ILabel.Label => TextBox;

    /// <inheritdoc />
    public override string ToString()
    {
        return "State Text Box [" + TextBox.Path + "] (" + TextBox.Owner.Name + ")";
    }
}

/// <summary>
/// Represents a text box with a state and placeholder.
/// </summary>
public class StatePlaceholderTextBox : PlaceholderTextBox, IStateElement
{
    /// <inheritdoc />
    [Pattern("State", Mode = FormatMode.Format)]
    public UnturnedUIElement State { get; }

    public StatePlaceholderTextBox(string path) : base(path)
    {
        State = new UnturnedLabel(path + "State");
    }

    public StatePlaceholderTextBox(string path, string? placeholderPath, string? statePath)
        : base(path, UnturnedUIUtility.GetPresetValue(path, placeholderPath, "Placeholder"))
    {
        State = new UnturnedLabel(UnturnedUIUtility.GetPresetValue(path, statePath, "State"));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "State Placeholder Text Box [" + TextBox.Path + "] (" + TextBox.Owner.Name + ")";
    }
}