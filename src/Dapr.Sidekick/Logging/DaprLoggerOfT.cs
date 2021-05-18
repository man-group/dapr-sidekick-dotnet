#pragma warning disable SA1649 // File name should match first type name
// Based on https://github.com/dotnet/extensions/blob/release/2.1/src/Logging/Logging.Abstractions/src/LoggerOfT.cs
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Dapr.Sidekick.Logging
{
    public sealed class DaprLogger<T> : IDaprLogger<T>
    {
        private readonly IDaprLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprLogger{T}"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public DaprLogger(IDaprLoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _logger = factory.CreateLogger(Internal.TypeNameHelper.GetTypeDisplayName(typeof(T)));
        }

        IDisposable IDaprLogger.BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        bool IDaprLogger.IsEnabled(DaprLogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        void IDaprLogger.Log<TState>(DaprLogLevel logLevel, DaprEventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
#pragma warning restore SA1649 // File name should match first type name
