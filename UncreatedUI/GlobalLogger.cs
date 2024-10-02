using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Uncreated.Framework.UI;

/// <summary>
/// Global logger used by default by all UI elements.
/// </summary>
public class GlobalLogger
{
    private static ILoggerFactory? _logger;

    /// <summary>
    /// Global logger used by default by all UI elements.
    /// </summary>
    public static ILoggerFactory Instance
    {
        get => _logger ?? NullLoggerFactory.Instance;
        set => _logger = value;
    }
}
