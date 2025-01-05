using System.Collections.Generic;

namespace Uncreated.Framework.UI.Reflection;

/// <summary>
/// Values implementing this interface will act similar to <see cref="IEnumerable{T}"/>'s of elements during element discovery.
/// </summary>
/// <remarks>Fields and properties of objects implementing this interface will not be searched for elements.</remarks>
public interface IUnturnedUIElementProvider
{
    /// <summary>
    /// Yields all elements that should be considered for registration from this object during element discovery.
    /// </summary>
    IEnumerable<UnturnedUIElement> EnumerateElements();
}