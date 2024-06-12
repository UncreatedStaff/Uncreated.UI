using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using System;

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

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public PlaceholderTextBox(string textBox) : this(GlobalLogger.Instance, textBox, textBox + "Placeholder") { }
    public PlaceholderTextBox(ILogger logger, string textBox) : this(logger, textBox, textBox + "Placeholder") { }
    public PlaceholderTextBox(ILoggerFactory factory, string textBox) : this(factory, textBox, textBox + "Placeholder") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public PlaceholderTextBox(string textBox, string? placeholder) : this(GlobalLogger.Instance, textBox, placeholder) { }
    public PlaceholderTextBox(ILogger logger, string textBox, string? placeholder)
    {
        TextBox = new UnturnedTextBox(logger, textBox);
        Placeholder = new UnturnedLabel(logger, placeholder ?? (textBox + "Placeholder"));
    }
    public PlaceholderTextBox(ILoggerFactory factory, string textBox, string? placeholder)
    {
        TextBox = new UnturnedTextBox(factory, textBox);
        Placeholder = new UnturnedLabel(factory, placeholder ?? (textBox + "Placeholder"));
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

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StateTextBox(string textBox) : this(GlobalLogger.Instance, textBox, textBox + "State") { }
    public StateTextBox(ILogger logger, string textBox) : this(logger, textBox, textBox + "State") { }
    public StateTextBox(ILoggerFactory factory, string textBox) : this(factory, textBox, textBox + "State") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StateTextBox(string textBox, string? state) : this(GlobalLogger.Instance, textBox, state) { }
    public StateTextBox(ILogger logger, string textBox, string? state)
    {
        TextBox = new UnturnedTextBox(logger, textBox);
        State = new UnturnedUIElement(logger, state ?? (textBox + "State"));
    }
    public StateTextBox(ILoggerFactory factory, string textBox, string? state)
    {
        TextBox = new UnturnedTextBox(factory, textBox);
        State = new UnturnedUIElement(factory, state ?? (textBox + "State"));
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

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StatePlaceholderTextBox(string textBox) : this(GlobalLogger.Instance, textBox) { }
    public StatePlaceholderTextBox(ILogger logger, string textBox) : base(logger, textBox)
    {
        State = new UnturnedLabel(logger, textBox + "State");
    }
    public StatePlaceholderTextBox(ILoggerFactory factory, string textBox) : base(factory, textBox)
    {
        State = new UnturnedLabel(factory, textBox + "State");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StatePlaceholderTextBox(string textBox, string? placeholder, string? state) : this(GlobalLogger.Instance, textBox, placeholder, state) { }
    public StatePlaceholderTextBox(ILogger logger, string textBox, string? placeholder, string? state) : base(logger, textBox, placeholder ?? (textBox + "Placeholder"))
    {
        State = new UnturnedLabel(logger, state ?? (textBox + "State"));
    }
    public StatePlaceholderTextBox(ILoggerFactory factory, string textBox, string? placeholder, string? state) : base(factory, textBox, placeholder ?? (textBox + "Placeholder"))
    {
        State = new UnturnedLabel(factory, state ?? (textBox + "State"));
    }
}
