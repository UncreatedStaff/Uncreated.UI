namespace Uncreated.Framework.UI.Presets;
public interface IElement
{
    UnturnedUIElement Element { get; }
}
public interface IStateElement : IElement
{
    UnturnedUIElement? State { get; }
}
public interface ILabel : IElement
{
    UnturnedLabel Label { get; }
}
public interface IButton : IElement
{
    event ButtonClicked OnClicked;
    UnturnedButton Button { get; }
}
public interface ILabeledButton : IButton, ILabel { }
public interface ILabeledRightClickableButton : IRightClickableButton, ILabeledButton { }
public interface IRightClickableButton : IButton
{
    event ButtonClicked OnRightClicked;
    UnturnedButton RightClickListener { get; }
}
public interface IImage : IElement
{
    UnturnedImage Image { get; }
}
public interface ITextBox : ILabel
{
    event TextUpdated OnTextUpdated;
    UnturnedTextBox TextBox { get; }
    bool UseData { get; set; }
}

public interface IPlaceholderTextBox : ITextBox
{
    UnturnedLabel Placeholder { get; }
}