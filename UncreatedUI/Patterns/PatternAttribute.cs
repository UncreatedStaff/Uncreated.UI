using System;

namespace Uncreated.Framework.UI.Patterns;

/// <summary>
/// Defines the pattern the element name follows in a nested type.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class PatternAttribute(string? pattern) : Attribute
{
    /// <summary>
    /// The pattern the element name follows.
    /// </summary>
    public string? Pattern { get; } = pattern;

    /// <summary>
    /// Suffixed to the existing path for this object if set.
    /// </summary>
    public string? AdditionalPath { get; set; }

    /// <summary>
    /// How pattern is formatted. Defaults to <see cref="FormatMode.Suffix"/>.
    /// </summary>
    public FormatMode Mode { get; set; } = FormatMode.Suffix;

    /// <summary>
    /// Formatting index of the placeholder when <see cref="Mode"/> is <see cref="FormatMode.Format"/>.
    /// </summary>
    public int FormatIndex { get; set; } = 1;

    /// <summary>
    /// Join these characters if they're left sequentially when <see cref="Pattern"/> is an empty string and <see cref="Mode"/> is <see cref="FormatMode.Format"/>.
    /// </summary>
    public char CleanJoin { get; set; }
}