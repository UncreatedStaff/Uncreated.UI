using DanielWillett.ReflectionTools;
using Uncreated.Framework.UI.Patterns;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// A button with a label.
/// </summary>
public class LabeledButton : ILabeledButton
{
    /// <inheritdoc />
    [Pattern("", Mode = FormatMode.Format, Root = true)]
    public UnturnedButton Button { get; }

    /// <inheritdoc />
    [Pattern("Label", Mode = FormatMode.Format)]
    public UnturnedLabel Label { get; }

    /// <inheritdoc />
    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }

    public LabeledButton(string path) : this(path, path + "Label") { }

    public LabeledButton(string buttonPath, string? labelPath)
    {
        Button = new UnturnedButton(buttonPath);
        Label = new UnturnedLabel(UnturnedUIUtility.GetPresetValue(buttonPath, labelPath, "Label"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;


    /// <inheritdoc />
    public override string ToString()
    {
        return "Labeled Button [" + Button.Path + "] (" + Button.Owner.Name + ")";
    }
}

/// <summary>
/// A button with a enabled/disabled state.
/// </summary>
public class StateButton : IButton, IStateElement
{
    /// <inheritdoc />
    [Pattern("", Mode = FormatMode.Format, Root = true)]
    public UnturnedButton Button { get; }

    /// <inheritdoc />
    [Pattern("State", Mode = FormatMode.Format)]
    public UnturnedUIElement State { get; }

    /// <inheritdoc />
    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }

    public StateButton(string path) : this(path, path + "State") { }

    public StateButton(string buttonPath, string? statePath)
    {
        Button = new UnturnedButton(buttonPath);
        State = new UnturnedUIElement(UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;

    /// <inheritdoc />
    public override string ToString()
    {
        return "State Button [" + Button.Path + "] (" + Button.Owner.Name + ")";
    }
}

/// <summary>
/// A button with a right click listener.
/// </summary>
public class RightClickableButton : IRightClickableButton
{
    /// <inheritdoc />
    [Pattern("", Mode = FormatMode.Format, Root = true)]
    public UnturnedButton Button { get; }

    /// <inheritdoc />
    [Pattern("RightClickListener", Mode = FormatMode.Format)]
    public UnturnedButton RightClickListener { get; }

    /// <inheritdoc />
    public event ButtonClicked OnClicked
    {
        add => Button.OnClicked += value;
        remove => Button.OnClicked -= value;
    }

    /// <inheritdoc />
    public event ButtonClicked OnRightClicked
    {
        add => RightClickListener.OnClicked += value;
        remove => RightClickListener.OnClicked -= value;
    }

    public RightClickableButton(string path) : this(path, path + "RightClickListener") { }

    public RightClickableButton(string buttonPath, string? rightClickListenerButtonPath)
    {
        Button = new UnturnedButton(buttonPath);
        RightClickListener = new UnturnedButton(UnturnedUIUtility.GetPresetValue(buttonPath, rightClickListenerButtonPath, "RightClickListener"));
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;


    /// <inheritdoc />
    public override string ToString()
    {
        return "Right-Clickable Button [" + Button.Path + "] (" + Button.Owner.Name + ")";
    }
}

/// <summary>
/// Labeled button who's intractability can be enabled and disabled.
/// </summary>
public class LabeledStateButton : LabeledButton, IStateElement
{
    /// <inheritdoc />
    [Pattern("State", Mode = FormatMode.Format)]
    public UnturnedUIElement State { get; }

    public LabeledStateButton(string path) : base(path)
    {
        State = new UnturnedUIElement(path + "State");
    }

    public LabeledStateButton(string buttonPath, string? labelPath, string? statePath)
        : base(buttonPath, labelPath)
    {
        State = new UnturnedUIElement(UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "Labeled State Button [" + Button.Path + "] (" + Button.Owner.Name + ")";
    }
}

/// <summary>
/// Right-clickable button who's intractability can be enabled and disabled.
/// </summary>
public class RightClickableStateButton : RightClickableButton, IStateElement
{
    /// <inheritdoc />
    [Pattern("State", Mode = FormatMode.Format)]
    public UnturnedUIElement State { get; }

    public RightClickableStateButton(string path) : base(path)
    {
        State = new UnturnedUIElement(path + "State");
    }

    public RightClickableStateButton(string buttonPath, string? rightClickListenerButtonPath, string? statePath) : base(buttonPath, rightClickListenerButtonPath)
    {
        State = new UnturnedUIElement(UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "Right-Clickable State Button [" + Button.Path + "] (" + Button.Owner.Name + ")";
    }
}

/// <summary>
/// A button with a label and right click listener.
/// </summary>
public class LabeledRightClickableButton : RightClickableButton, ILabeledButton
{
    /// <inheritdoc />
    [Pattern("Label", Mode = FormatMode.Format)]
    public UnturnedLabel Label { get; }

    public LabeledRightClickableButton(string path) : base(path)
    {
        Label = new UnturnedLabel(path + "Label");
    }

    public LabeledRightClickableButton(string buttonPath, string? labelPath, string? rightClickListenerButtonPath) : base(buttonPath, rightClickListenerButtonPath)
    {
        Label = new UnturnedLabel(UnturnedUIUtility.GetPresetValue(buttonPath, labelPath, "Label"));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "Labeled Right-Clickable Button [" + Button.Path + "] (" + Button.Owner.Name + ")";
    }
}

/// <summary>
/// A button with a label and right click listener, who's intractability can be enabled and disabled.
/// </summary>
public class LabeledRightClickableStateButton : LabeledRightClickableButton, IStateElement
{
    /// <inheritdoc />
    [Pattern("State", Mode = FormatMode.Format)]
    public UnturnedUIElement State { get; }

    public LabeledRightClickableStateButton(string path) : base(path)
    {
        State = new UnturnedUIElement(path + "State");
    }

    public LabeledRightClickableStateButton(string buttonPath, string? labelPath, string? rightClickListenerButtonPath, string? statePath)
        : base(buttonPath, labelPath, rightClickListenerButtonPath)
    {
        State = new UnturnedUIElement(UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "Labeled Right-Clickable State Button [" + Button.Path + "] (" + Button.Owner.Name + ")";
    }
}