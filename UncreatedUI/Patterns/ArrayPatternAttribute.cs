using JetBrains.Annotations;
using System;

namespace Uncreated.Framework.UI.Patterns;

/// <summary>
/// Defines how many times an array pattern is repeated in a nested type.
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ArrayPatternAttribute(int start, int length) : Attribute
{
    /// <summary>
    /// First number.
    /// </summary>
    public int Start { get; } = start;

    /// <summary>
    /// Optional length of the array.
    /// </summary>
    /// <remarks>Must either supply this or <see cref="To"/>.</remarks>
    public int Length { get; set; } = length;

    /// <summary>
    /// Optional end number.
    /// </summary>
    /// <remarks>Must either supply this or <see cref="Length"/>.</remarks>
    public int To
    {
        get => Length + Start - 1;
        set => Length = value - Start + 1;
    }
    public ArrayPatternAttribute(int start) : this(start, 0) { }
}
