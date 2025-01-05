using Uncreated.Framework.UI.Data;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// An element with a child element.
/// </summary>
public interface IElement
{
    /// <summary>
    /// The base element.
    /// </summary>
    UnturnedUIElement Element { get; }
}

/// <summary>
/// An element with an enabled/disabled state.
/// </summary>
public interface IStateElement : IElement
{
    /// <summary>
    /// The element with an activation hook to set enabled or disabled.
    /// </summary>
    UnturnedUIElement? State { get; }
}

/// <summary>
/// An element with a label.
/// </summary>
public interface ILabel : IElement
{
    /// <summary>
    /// The base label element.
    /// </summary>
    UnturnedLabel Label { get; }
}

/// <summary>
/// An element with a button.
/// </summary>
public interface IButton : IElement
{
    /// <summary>
    /// Called when the button is clicked.
    /// </summary>
    event ButtonClicked OnClicked;

    /// <summary>
    /// The base button element.
    /// </summary>
    UnturnedButton Button { get; }
}

/// <summary>
/// A button with a text component.
/// </summary>
public interface ILabeledButton : IButton, ILabel;

/// <summary>
/// A button with a text component and right click listener.
/// </summary>
public interface ILabeledRightClickableButton : IRightClickableButton, ILabeledButton;

/// <summary>
/// A button with a right click listener.
/// </summary>
public interface IRightClickableButton : IButton
{
    /// <summary>
    /// Called when the button is right clicked.
    /// </summary>
    event ButtonClicked OnRightClicked;

    /// <summary>
    /// The base button right click element, which is 'clicked' when the user right clicks <see cref="IButton.Button"/>.
    /// </summary>
    UnturnedButton RightClickListener { get; }
}

/// <summary>
/// An element with an image.
/// </summary>
public interface IImage : IElement
{
    /// <summary>
    /// The base image element.
    /// </summary>
    UnturnedImage Image { get; }
}

/// <summary>
/// An element with a text box.
/// </summary>
public interface ITextBox : ILabel
{
    /// <summary>
    /// Called when text is commited to the text box.
    /// </summary>
    event TextUpdated OnTextUpdated;

    /// <summary>
    /// The base text box element.
    /// </summary>
    UnturnedTextBox TextBox { get; }

    /// <summary>
    /// If <see cref="UnturnedTextBoxData"/> is used to track text box content.
    /// </summary>
    bool UseData { get; set; }
}

/// <summary>
/// A text box with a placeholder label.
/// </summary>
public interface IPlaceholderTextBox : ITextBox
{
    /// <summary>
    /// The placeholder element inside the text box.
    /// </summary>
    UnturnedLabel Placeholder { get; }
}