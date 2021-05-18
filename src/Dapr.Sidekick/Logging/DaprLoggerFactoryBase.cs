// Based on https://github.com/dotnet/extensions/blob/release/2.1/src/Logging/Logging/src/LoggerFactory.cs
// See LICENSE in Dapr.Sidekick/Logging

using System;
using System.Collections.Generic;

namespace Dapr.Sidekick.Logging
{
    /// <summary>
    /// Base class for <see cref="IDaprLoggerFactory"/> implementations.
    /// </summary>
    public abstract class DaprLoggerFactoryBase : DaprDisposable, IDaprLoggerFactory
    {
        private readonly Dictionary<string, IDaprLogger> _loggers = new Dictionary<string, IDaprLogger>(StringComparer.Ordinal);
        private readonly object _sync = new object();

        public IDaprLogger CreateLogger(string categoryName)
        {
            EnsureNotDisposed();

            // Cannot use ConcurrentDictionary as need to support net35.
            // Standard check-lock-check approach
            if (!_loggers.TryGetValue(categoryName, out var logger))
            {
                lock (_sync)
                {
                    if (!_loggers.TryGetValue(categoryName, out logger))
                    {
                        logger = CreateLoggerImpl(categoryName);
                        _loggers[categoryName] = logger;
                    }
                }
            }

            return logger;
        }

        protected abstract IDaprLogger CreateLoggerImpl(string categoryName);
    }
}
