using System.Diagnostics;
using DanielWillett.ReflectionTools;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// Represents a text box with a placeholder.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {TextBox.Path}")]
public class PlaceholderTextBox : IPlaceholderTextBox
{
    /// <inheritdoc />
    public UnturnedTextBox TextBox { get; }

    /// <inheritdoc />
    public UnturnedLabel Placeholder { get; }

    /// <inheritdoc />
    public event TextUpdated OnTextUpdated
    {
        add => TextBox.OnTextUpdated += value;
        remove => TextBox.OnTextUpdated -= value;
    }

    /// <inheritdoc />
    [Ignore]
    public bool UseData
    {
        get => TextBox.UseData;
        set => TextBox.UseData = value;
    }

    [Ignore]
    protected virtual string ElementTypeDisplayName => Properties.Resources.DisplayName_PlaceholderTextBox;

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
        return $"{ElementTypeDisplayName} [{TextBox.Path}] ({TextBox.Owner.Name})";
    }

    /// <inheritdoc />
    public override int GetHashCode() => TextBox.GetHashCode();
}

/// <summary>
/// Represents a text box with a state.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {TextBox.Path}")]
public class StateTextBox : IStateElement, ITextBox
{
    /// <inheritdoc />
    public UnturnedTextBox TextBox { get; }

    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    /// <inheritdoc />
    public event TextUpdated OnTextUpdated
    {
        add => TextBox.OnTextUpdated += value;
        remove => TextBox.OnTextUpdated -= value;
    }

    /// <inheritdoc />
    [Ignore]
    public bool UseData
    {
        get => TextBox.UseData;
        set => TextBox.UseData = value;
    }

    [Ignore]
    protected virtual string ElementTypeDisplayName => Properties.Resources.DisplayName_StateTextBox;

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
        return $"{ElementTypeDisplayName} [{TextBox.Path}] ({TextBox.Owner.Name})";
    }

    /// <inheritdoc />
    public override int GetHashCode() => TextBox.GetHashCode();
}

/// <summary>
/// Represents a text box with a state and placeholder.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {TextBox.Path}")]
public class StatePlaceholderTextBox : PlaceholderTextBox, IStateElement
{
    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    [Ignore]
    protected override string ElementTypeDisplayName => Properties.Resources.DisplayName_StatePlaceholderTextBox;

    public StatePlaceholderTextBox(string path) : base(path)
    {
        State = new UnturnedLabel(path + "State");
    }

    public StatePlaceholderTextBox(string path, string? placeholderPath, string? statePath)
        : base(path, UnturnedUIUtility.GetPresetValue(path, placeholderPath, "Placeholder"))
    {
        State = new UnturnedLabel(UnturnedUIUtility.GetPresetValue(path, statePath, "State"));
    }
}