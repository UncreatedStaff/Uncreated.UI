using Microsoft.Extensions.Logging;
using System;

namespace Uncreated.Framework.UI;

/// <summary>
/// Global logger used by default by all UI elements.
/// </summary>
public class GlobalLogger
{
    private static ILogger? _logger;

    /// <summary>
    /// Global logger used by default by all UI elements.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="Instance"/> not initialized.</exception>
    public static ILogger Instance
    {
        get => _logger ?? throw new InvalidOperationException("Global logger has not been set.");
        set => _logger = value;
    }
}
