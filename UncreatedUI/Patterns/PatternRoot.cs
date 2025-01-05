using Uncreated.Framework.UI.Data;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI.Patterns;

#nullable disable
/// <summary>
/// Provides a root object for a pattern to derive from. Provides implementations for <see cref="IElement"/>.
/// </summary>
public abstract class PatternRoot : PatternRoot<UnturnedUIElement>;

/// <summary>
/// Provides a root object for a pattern to derive from which has a Text component. Provides implementations for <see cref="IElement"/> and <see cref="ILabel"/>.
/// </summary>
public abstract class PatternLabelRoot : PatternRoot<UnturnedLabel>, ILabel
{
    /// <inheritdoc />
    UnturnedLabel ILabel.Label => Root;
}

/// <summary>
/// Provides a root object for a pattern to derive from which has an Image component. Provides implementations for <see cref="IElement"/> and <see cref="IImage"/>.
/// </summary>
public abstract class PatternImageRoot : PatternRoot<UnturnedImage>, IImage
{
    /// <inheritdoc />
    UnturnedImage IImage.Image => Root;
}

/// <summary>
/// Provides a root object for a pattern to derive from which has a Button component. Provides implementations for <see cref="IElement"/> and <see cref="IButton"/>.
/// </summary>
public abstract class PatternButtonRoot : PatternRoot<UnturnedButton>, IButton
{
    /// <inheritdoc />
    public event ButtonClicked OnClicked
    {
        add => Root.OnClicked += value;
        remove => Root.OnClicked -= value;
    }

    /// <inheritdoc />
    UnturnedButton IButton.Button => Root;
}

/// <summary>
/// Provides a root object for a pattern to derive from which has a TextBox component. Provides implementations for <see cref="IElement"/> and <see cref="ITextBox"/>.
/// </summary>
public abstract class PatternTextBoxRoot : PatternRoot<UnturnedTextBox>, ITextBox
{
    /// <inheritdoc />
    public event TextUpdated OnTextUpdated
    {
        add => Root.OnTextUpdated += value;
        remove => Root.OnTextUpdated -= value;
    }

    /// <inheritdoc />
    bool ITextBox.UseData { get => Root.UseData; set => Root.UseData = value; }

    /// <inheritdoc />
    UnturnedTextBox ITextBox.TextBox => Root;

    /// <inheritdoc />
    UnturnedLabel ILabel.Label => Root;
}

/// <summary>
/// Provides a root object for a pattern to derive from. Provides implementations for <see cref="IElement"/>.
/// </summary>
public abstract class PatternRoot<TUIElement> : IElement where TUIElement : UnturnedUIElement
{
    /// <summary>
    /// The UI element that all objects are under.
    /// </summary>
    [Pattern(Root = true)]
    public TUIElement Root { get; set; }

    /// <inheritdoc />
    UnturnedUIElement IElement.Element => Root;

    /// <inheritdoc />
    public override string ToString() => Root == null ? Properties.Resources.DisplayName_PatternRoot_Uninitialized : Root.ToString();

    // ReSharper disable NonReadonlyMemberInGetHashCode

    /// <inheritdoc />
    public override int GetHashCode() => Root == null ? 0 : Root.GetHashCode();

    // ReSharper restore NonReadonlyMemberInGetHashCode
}
#nullable restore