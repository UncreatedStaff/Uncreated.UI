using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using System;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// Represents a text box with a placeholder.
/// </summary>
public class PlaceholderTextBox : IPlaceholderTextBox
{
    /// <inheritdoc />
    [UIPattern("", Mode = FormatMode.Format)]
    public UnturnedTextBox TextBox { get; }

    /// <inheritdoc />
    [UIPattern("Placeholder", Mode = FormatMode.Format)]
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

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public PlaceholderTextBox(string path) : this(GlobalLogger.Instance, path, path + "Placeholder") { }
    public PlaceholderTextBox(ILogger logger, string path) : this(logger, path, path + "Placeholder") { }
    public PlaceholderTextBox(ILoggerFactory factory, string path) : this(factory, path, path + "Placeholder") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public PlaceholderTextBox(string path, string? placeholderPath) : this(GlobalLogger.Instance, path, placeholderPath) { }
    public PlaceholderTextBox(ILogger logger, string path, string? placeholderPath)
    {
        TextBox = new UnturnedTextBox(logger, path);
        Placeholder = new UnturnedLabel(logger, placeholderPath ?? (path + "Placeholder"));
    }
    public PlaceholderTextBox(ILoggerFactory factory, string path, string? placeholderPath)
    {
        TextBox = new UnturnedTextBox(factory, path);
        Placeholder = new UnturnedLabel(factory, placeholderPath ?? (path + "Placeholder"));
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
    /// <inheritdoc />
    [UIPattern("", Mode = FormatMode.Format)]
    public UnturnedTextBox TextBox { get; }

    /// <inheritdoc />
    [UIPattern("State", Mode = FormatMode.Format)]
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

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StateTextBox(string path) : this(GlobalLogger.Instance, path, path + "State") { }
    public StateTextBox(ILogger logger, string path) : this(logger, path, path + "State") { }
    public StateTextBox(ILoggerFactory factory, string path) : this(factory, path, path + "State") { }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StateTextBox(string path, string? statePath) : this(GlobalLogger.Instance, path, statePath) { }
    public StateTextBox(ILogger logger, string path, string? statePath)
    {
        TextBox = new UnturnedTextBox(logger, path);
        State = new UnturnedUIElement(logger, statePath ?? (path + "State"));
    }
    public StateTextBox(ILoggerFactory factory, string path, string? statePath)
    {
        TextBox = new UnturnedTextBox(factory, path);
        State = new UnturnedUIElement(factory, statePath ?? (path + "State"));
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
    /// <inheritdoc />
    [UIPattern("State", Mode = FormatMode.Format)]
    public UnturnedUIElement State { get; }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StatePlaceholderTextBox(string path) : this(GlobalLogger.Instance, path) { }
    public StatePlaceholderTextBox(ILogger logger, string path) : base(logger, path)
    {
        State = new UnturnedLabel(logger, path + "State");
    }
    public StatePlaceholderTextBox(ILoggerFactory factory, string path) : base(factory, path)
    {
        State = new UnturnedLabel(factory, path + "State");
    }

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public StatePlaceholderTextBox(string path, string? placeholderPath, string? statePath) : this(GlobalLogger.Instance, path, placeholderPath, statePath) { }
    public StatePlaceholderTextBox(ILogger logger, string path, string? placeholderPath, string? statePath) : base(logger, path, placeholderPath ?? (path + "Placeholder"))
    {
        State = new UnturnedLabel(logger, statePath ?? (path + "State"));
    }
    public StatePlaceholderTextBox(ILoggerFactory factory, string path, string? placeholderPath, string? statePath) : base(factory, path, placeholderPath ?? (path + "Placeholder"))
    {
        State = new UnturnedLabel(factory, statePath ?? (path + "State"));
    }
}
