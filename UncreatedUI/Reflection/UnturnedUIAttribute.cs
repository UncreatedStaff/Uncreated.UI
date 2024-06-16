using System;

namespace Uncreated.Framework.UI.Reflection;

/// <summary>
/// Add extra metadata to this UI.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public sealed class UnturnedUIAttribute : Attribute
{
    private bool _reliable = true;
    private bool _hasElements = true;
    internal bool HasReliable;
    internal bool HasHasElements;

    /// <summary>
    /// Name displayed in warning messages, etc.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Base path for all elements.
    /// </summary>
    public string? BasePath { get; set; }

    /// <summary>
    /// Should scan the UI for elements.
    /// </summary>
    /// <remarks>Default: <see langword="true"/></remarks>
    public bool HasElements
    {
        get => _hasElements;
        set
        {
            _hasElements = value;
            HasHasElements = true;
        }
    }

    /// <summary>
    /// Should send the UI reliably.
    /// </summary>
    /// <remarks>Default: <see langword="true"/></remarks>
    public bool Reliable
    {
        get => _reliable;
        set
        {
            _reliable = value;
            HasReliable = true;
        }
    }

    public UnturnedUIAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}

/// <summary>
/// Ignore this element if the current object's type is the same as the type this element is defined in.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class IgnoreIfDefinedTypeAttribute : Attribute { }