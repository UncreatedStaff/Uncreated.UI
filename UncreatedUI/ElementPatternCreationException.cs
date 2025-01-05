using System;
using Uncreated.Framework.UI.Patterns;

namespace Uncreated.Framework.UI;

/// <summary>
/// Thrown when <see cref="ElementPatterns"/> is unable to create a pattern.
/// </summary>
public class ElementPatternCreationException : Exception
{
    public ElementPatternCreationException() { }

    public ElementPatternCreationException(string message) : base(message) { }

    public ElementPatternCreationException(string message, Exception inner) : base(message, inner) { }
}
