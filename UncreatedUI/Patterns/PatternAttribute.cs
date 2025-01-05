using System;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI.Patterns;

/// <summary>
/// Defines the pattern the element name follows in a nested type.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class PatternAttribute : Attribute
{
    /// <summary>
    /// The pattern the element name follows.
    /// </summary>
    public string? Pattern { get; }

    /// <summary>
    /// Suffixed to the existing path for this object if set.
    /// </summary>
    /// <remarks>Defaults to <see langword="null"/>. This needs to be separate to accomodate format modes like <see cref="FormatMode.Prefix"/>.</remarks>
    public string? AdditionalPath { get; set; }

    /// <summary>
    /// How pattern is formatted.
    /// </summary>
    /// <remarks>Defaults to <see cref="FormatMode.Replace"/>.</remarks>
    public FormatMode Mode { get; set; } = FormatMode.Replace;

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
    /// <see cref="ITextBox"/> objects will be initialized with <see cref="ITextBox.UseData"/> set to this value.
    /// </summary>
    /// <remarks>Defaults to <see langword="false"/>.</remarks>
    public bool TextBoxUseData { get; set; }

    /// <summary>
    /// If there's an object with <see cref="Root"/> = <see langword="true"/>, this object will be under that object, where <see cref="AdditionalPath"/> is relative to that object.
    /// </summary>
    /// <remarks>Defaults to <see langword="true"/>.</remarks>
    public bool UnderRoot { get; set; } = true;

    /// <summary>
    /// Array of other paths to use as the constructor arguments for presets like <see cref="LabeledRightClickableStateButton"/> (in which this array would look like this: <c>[ "./Label", "./RightClickListener", "./State" ]</c>, for example).
    /// </summary>
    /// <remarks>All these paths will be relative if they start with './' or '../' characters.</remarks>
    public string?[]? PresetPaths { get; set; }

    public PatternAttribute() : this(null)
    {
        Mode = FormatMode.None;
    }

    /// <summary>
    /// Defines the pattern the element name follows in a nested type.
    /// </summary>
    public PatternAttribute(string? pattern)
    {
        Pattern = pattern;
    }

    /// <summary>
    /// Defines the pattern the element name follows in a nested type with an additional path.
    /// </summary>
    /// <remarks>This needs to be separate to accomodate format modes like <see cref="FormatMode.Prefix"/>.</remarks>
    public PatternAttribute(string additionalPath, string pattern)
    {
        AdditionalPath = additionalPath;
        Pattern = pattern;
    }
}