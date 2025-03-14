using System.Diagnostics;
using DanielWillett.ReflectionTools;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// A button with a label.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {Button.Path}")]
public class LabeledButton : ILabeledButton
{
    /// <inheritdoc />
    public UnturnedButton Button { get; }

    /// <inheritdoc />
    public UnturnedLabel Label { get; }

    [Ignore]
    protected virtual string ElementTypeDisplayName => Properties.Resources.DisplayName_LabeledButton;

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
        return $"{ElementTypeDisplayName} [{Button.Path}] ({Button.Owner.Name})";
    }

    /// <inheritdoc />
    public override int GetHashCode() => Button.GetHashCode();
}

/// <summary>
/// A button with a enabled/disabled state.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {Button.Path}")]
public class StateButton : IButton, IStateElement
{
    /// <inheritdoc />
    public UnturnedButton Button { get; }

    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    [Ignore]
    protected virtual string ElementTypeDisplayName => Properties.Resources.DisplayName_StateButton;

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
        return $"{ElementTypeDisplayName} [{Button.Path}] ({Button.Owner.Name})";
    }

    /// <inheritdoc />
    public override int GetHashCode() => Button.GetHashCode();
}

/// <summary>
/// A button with a right click listener.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {Button.Path}")]
public class RightClickableButton : IRightClickableButton
{
    /// <inheritdoc />
    public UnturnedButton Button { get; }

    /// <inheritdoc />
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

    [Ignore]
    protected virtual string ElementTypeDisplayName => Properties.Resources.DisplayName_RightClickableButton;

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
        return $"{ElementTypeDisplayName} [{Button.Path}] ({Button.Owner.Name})";
    }

    /// <inheritdoc />
    public override int GetHashCode() => Button.GetHashCode();
}

/// <summary>
/// A labeled button who's intractability can be enabled and disabled.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {Button.Path}")]
public class LabeledStateButton : LabeledButton, IStateElement
{
    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    [Ignore]
    protected override string ElementTypeDisplayName => Properties.Resources.DisplayName_LabeledStateButton;

    public LabeledStateButton(string path) : base(path)
    {
        State = new UnturnedUIElement(path + "State");
    }

    public LabeledStateButton(string buttonPath, string? labelPath, string? statePath)
        : base(buttonPath, labelPath)
    {
        State = new UnturnedUIElement(UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
}

/// <summary>
/// A right-clickable button who's intractability can be enabled and disabled.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {Button.Path}")]
public class RightClickableStateButton : RightClickableButton, IStateElement
{
    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    [Ignore]
    protected override string ElementTypeDisplayName => Properties.Resources.DisplayName_RightClickableStateButton;

    public RightClickableStateButton(string path) : base(path)
    {
        State = new UnturnedUIElement(path + "State");
    }

    public RightClickableStateButton(string buttonPath, string? rightClickListenerButtonPath, string? statePath) : base(buttonPath, rightClickListenerButtonPath)
    {
        State = new UnturnedUIElement(UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
}

/// <summary>
/// A button with a label and right click listener.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {Button.Path}")]
public class LabeledRightClickableButton : RightClickableButton, ILabeledButton
{
    /// <inheritdoc />
    public UnturnedLabel Label { get; }

    [Ignore]
    protected override string ElementTypeDisplayName => Properties.Resources.DisplayName_LabeledRightClickableButton;

    public LabeledRightClickableButton(string path) : base(path)
    {
        Label = new UnturnedLabel(path + "Label");
    }

    public LabeledRightClickableButton(string buttonPath, string? labelPath, string? rightClickListenerButtonPath) : base(buttonPath, rightClickListenerButtonPath)
    {
        Label = new UnturnedLabel(UnturnedUIUtility.GetPresetValue(buttonPath, labelPath, "Label"));
    }
}

/// <summary>
/// A button with a label and right click listener, who's intractability can be enabled and disabled.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {Button.Path}")]
public class LabeledRightClickableStateButton : LabeledRightClickableButton, IStateElement
{
    /// <inheritdoc />
    public UnturnedUIElement State { get; }

    [Ignore]
    protected override string ElementTypeDisplayName => Properties.Resources.DisplayName_LabeledRightClickableStateButton;

    public LabeledRightClickableStateButton(string path) : base(path)
    {
        State = new UnturnedUIElement(path + "State");
    }

    public LabeledRightClickableStateButton(string buttonPath, string? labelPath, string? rightClickListenerButtonPath, string? statePath)
        : base(buttonPath, labelPath, rightClickListenerButtonPath)
    {
        State = new UnturnedUIElement(UnturnedUIUtility.GetPresetValue(buttonPath, statePath, "State"));
    }
}