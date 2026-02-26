using Microsoft.Extensions.Logging;
using SDG.Unturned;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Uncreated.Framework.UI;

/// <summary>
/// Global logger used by default by all UI elements. The default logger logs to the <see cref="UnturnedLog"/>.
/// </summary>
public class GlobalLogger
{
    /// <summary>
    /// Global logger used by default by all UI elements. Defaults to a logger which logs to the <see cref="UnturnedLog"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Logger factory is <see langword="null"/> in setter.</exception>
    [field: AllowNull]
    public static ILoggerFactory Instance
    {
        get => field ?? UnturnedLoggerFactory.Instance;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// The default logger factory uses the <see cref="UnturnedLog"/> to write log messages.
    /// This method resets <see cref="Instance"/> to log to the <see cref="UnturnedLog"/> in case it's been changed.
    /// </summary>
    public static void ResetToUnturnedLog()
    {
        Instance = UnturnedLoggerFactory.Instance;
    }

    private class UnturnedLoggerFactory : ILoggerFactory
    {
        public static readonly UnturnedLoggerFactory Instance = new UnturnedLoggerFactory();

        static UnturnedLoggerFactory() { }

        private ILoggerProvider[] _providers = Array.Empty<ILoggerProvider>();
        private readonly ConcurrentDictionary<string, UnturnedLogger> _loggerCache = new ConcurrentDictionary<string, UnturnedLogger>();
        private readonly object _sync = new object();
        private int _disposed;

        ~UnturnedLoggerFactory()
        {
            Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_sync)
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                    return;

                while (_loggerCache.Count > 0)
                {
                    foreach (string key in _loggerCache.Keys)
                    {
                        _loggerCache.TryRemove(key, out UnturnedLogger? logger);
                        if (logger == null)
                            continue;

                        ILogger[] loggers = Interlocked.Exchange(ref logger.Loggers, Array.Empty<ILogger>());
                        foreach (ILogger l in loggers)
                        {
                            if (l is IDisposable disp)
                                disp.Dispose();
                        }
                    }
                }

                foreach (ILoggerProvider provider in _providers)
                {
                    provider.Dispose();
                }
            }
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            if (_disposed != 0)
                throw new ObjectDisposedException(nameof(UnturnedLogger));

            return _loggerCache.GetOrAdd(categoryName, static (c, providers) =>
            {
                if (providers.Length == 0)
                {
                    return new UnturnedLogger(c, Array.Empty<ILogger>());
                }

                ILogger[] loggers = new ILogger[providers.Length];
                for (int i = 0; i < providers.Length; ++i)
                {
                    loggers[i] = providers[i].CreateLogger(c);
                }

                return new UnturnedLogger(c, loggers);
            }, _providers);
        }

        /// <inheritdoc />
        public void AddProvider(ILoggerProvider provider)
        {
            lock (_sync)
            {
                if (_disposed != 0)
                    throw new ObjectDisposedException(nameof(UnturnedLogger));

                ILoggerProvider[] newArr = new ILoggerProvider[_providers.Length + 1];
                Array.Copy(_providers, newArr, _providers.Length);
                newArr[^1] = provider;
                _providers = newArr;
                foreach (UnturnedLogger logger in _loggerCache.Values)
                {
                    ILogger[] loggers = new ILogger[logger.Loggers.Length + 1];
                    Array.Copy(logger.Loggers, loggers, logger.Loggers.Length);
                    loggers[^1] = provider.CreateLogger(logger.CategoryName);
                    logger.Loggers = loggers;
                }
            }
        }

        private class UnturnedLogger : ILogger
        {
            internal readonly string CategoryName;
            internal ILogger[] Loggers;

            private static readonly string[] LogLevels =
            [
                "TRC",
                "DBG",
                "INF",
                "WRN",
                "ERR",
                "CRT"
            ];

            public UnturnedLogger(string category, ILogger[] loggers)
            {
                CategoryName = category;
                Loggers = loggers;
            }

            /// <inheritdoc />
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel is < LogLevel.Trace or > LogLevel.Critical)
                    return;

                ILogger[] loggers = Loggers;
                foreach (ILogger logger in loggers)
                {
                    try
                    {
                        logger.Log(logLevel, eventId, state, exception, formatter);
                    }
                    catch { /* ignored */ }
                }

                string log = $"[{LogLevels[(int)logLevel]}] [{CategoryName}] {formatter(state, exception)}";

                if (UnturnedUIProvider.Instance.IsValidThread())
                {
                    Log(logLevel, log, exception);
                    return;
                }

                // copy variables to avoid closure allocation on the common path when on main thread
                string l2 = log;
                Exception? ex2 = exception;
                LogLevel lvl2 = logLevel;
                UnturnedUIProvider.Instance.DispatchToValidThread(() => Log(lvl2, l2, ex2));
            }

            private static void Log(LogLevel lvl, string logMessage, Exception? exception)
            {
                switch (lvl)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                        CommandWindow.Log(logMessage);
                        if (exception != null)
                            CommandWindow.Log(exception);
                        break;
                    case LogLevel.Warning:
                        CommandWindow.LogWarning(logMessage);
                        if (exception != null)
                            CommandWindow.LogWarning(exception);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        CommandWindow.LogError(logMessage);
                        if (exception != null)
                            CommandWindow.LogError(exception);
                        break;
                }
            }

            /// <inheritdoc />
            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel is >= LogLevel.Trace and <= LogLevel.Critical;
            }

            /// <inheritdoc />
            public IDisposable? BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}