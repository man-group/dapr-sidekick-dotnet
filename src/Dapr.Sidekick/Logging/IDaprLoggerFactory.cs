// Based on https://github.com/dotnet/extensions/blob/release/2.1/src/Logging/Logging.Abstractions/src/ILoggerFactory.cs
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See NET_EXTENSIONS_LICENSE in this directory for license information.

namespace Dapr.Sidekick.Logging
{
    /// <summary>
    /// Represents a type used to configure the logging system and create instances of <see cref="IDaprLogger"/>.
    /// </summary>
    public interface IDaprLoggerFactory
    {
        /// <summary>
        /// Creates a new <see cref="IDaprLogger"/> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>The <see cref="IDaprLogger"/>.</returns>
        IDaprLogger CreateLogger(string categoryName);
    }
}
