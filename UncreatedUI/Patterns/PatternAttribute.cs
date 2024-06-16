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
    /// <remarks>Defaults to <see langword="null"/>.</remarks>
    public string? AdditionalPath { get; set; }

    /// <summary>
    /// How pattern is formatted.
    /// </summary>
    /// <remarks>Defaults to <see cref="FormatMode.Suffix"/>.</remarks>
    public FormatMode Mode { get; set; } = FormatMode.Suffix;

    /// <summary>
    /// Formatting index of the placeholder when <see cref="Mode"/> is <see cref="FormatMode.Format"/>.
    /// </summary>
    /// <remarks>Defaults to 1.</remarks>
    public int FormatIndex { get; set; } = 1;

    /// <summary>
    /// Join these characters if they're left sequentially when <see cref="Pattern"/> is an empty string and <see cref="Mode"/> is <see cref="FormatMode.Format"/>.
    /// </summary>
    public char CleanJoin { get; set; }

    /// <summary>
    /// All other objects not marked with <see cref="UnderRoot"/> = <see langword="false"/> will be put under this object, where <see cref="AdditionalPath"/> is relative to this object.
    /// </summary>
    /// <remarks>Defaults to <see langword="false"/>.</remarks>
    public bool Root { get; set; }

    /// <summary>
    /// If there's an object with <see cref="Root"/> = <see langword="true"/>, this object will be under that object, where <see cref="AdditionalPath"/> is relative to that object.
    /// </summary>
    /// <remarks>Defaults to <see langword="true"/>.</remarks>
    public bool UnderRoot { get; set; } = true;

    public PatternAttribute() : this(null)
    {
        Mode = FormatMode.None;
    }
}