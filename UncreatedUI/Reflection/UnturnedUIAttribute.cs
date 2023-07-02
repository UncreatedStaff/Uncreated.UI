using System;

namespace Uncreated.Framework.UI.Reflection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public sealed class UnturnedUIAttribute : Attribute
{
    /// <summary>
    /// Name displayed in warning messages, etc.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Should scan the UI for elements.
    /// </summary>
    /// <remarks>Default: <see langword="true"/></remarks>
    public bool HasElements { get; set; } = true;

    /// <summary>
    /// Should send the UI reliably.
    /// </summary>
    /// <remarks>Default: <see langword="true"/></remarks>
    public bool Reliable { get; set; } = true;
    public UnturnedUIAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}