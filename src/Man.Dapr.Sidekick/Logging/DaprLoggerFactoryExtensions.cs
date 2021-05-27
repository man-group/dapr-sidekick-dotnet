// Based on https://github.com/dotnet/extensions/blob/release/2.1/src/Logging/Logging.Abstractions/src/LoggerFactoryExtensions.cs
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See NET_EXTENSIONS_LICENSE in this directory for license information.

using System;

namespace Man.Dapr.Sidekick.Logging
{
    /// <summary>
    /// IDaprLoggerFactory extension methods for common scenarios.
    /// </summary>
    public static class DaprLoggerFactoryExtensions
    {
        /// <summary>
        /// Creates a new IDaprLogger instance using the full name of the given type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns>A <see cref="IDaprLogger{T}"/> instance.</returns>
        public static IDaprLogger<T> CreateLogger<T>(this IDaprLoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return new DaprLogger<T>(factory);
        }

        /// <summary>
        /// Creates a new IDaprLogger instance using the full name of the given type.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="type">The type.</param>
        /// <returns>A <see cref="IDaprLogger"/> instance.</returns>
        public static IDaprLogger CreateLogger(this IDaprLoggerFactory factory, Type type)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return factory.CreateLogger(Internal.TypeNameHelper.GetTypeDisplayName(type));
        }
    }
}
