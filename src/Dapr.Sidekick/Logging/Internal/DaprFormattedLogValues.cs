// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Dapr.Sidekick.Logging.Internal
{
    /// <summary>
    /// LogValues to enable formatting options supported by <see cref="M:string.Format"/>.
    /// This also enables using {NamedformatItem} in the format string.
    /// </summary>
    internal readonly struct DaprFormattedLogValues : IEnumerable<KeyValuePair<string, object>>
    {
        internal const int MaxCachedFormatters = 1024;
        private const string NullFormat = "[null]";
        private static int _count;
#if NET35
        private static Dictionary<string, DaprLogValuesFormatter> _formatters = new Dictionary<string, DaprLogValuesFormatter>();
#else
        private static System.Collections.Concurrent.ConcurrentDictionary<string, DaprLogValuesFormatter> _formatters = new System.Collections.Concurrent.ConcurrentDictionary<string, DaprLogValuesFormatter>();
#endif
        private readonly DaprLogValuesFormatter _formatter;
        private readonly object[] _values;
        private readonly string _originalMessage;

        // for testing purposes
        internal DaprLogValuesFormatter Formatter => _formatter;

        public DaprFormattedLogValues(string format, params object[] values)
        {
            if (values != null && values.Length != 0 && format != null)
            {
                if (_count >= MaxCachedFormatters)
                {
                    if (!_formatters.TryGetValue(format, out _formatter))
                    {
                        _formatter = new DaprLogValuesFormatter(format);
                    }
                }
                else
                {
#if NET35
                    if (!_formatters.TryGetValue(format, out _formatter))
                    {
                        Interlocked.Increment(ref _count);
                        _formatter = new DaprLogValuesFormatter(format);
                        _formatters.Add(format, _formatter);
                    }
#else
                    _formatter = _formatters.GetOrAdd(format, f =>
                    {
                        Interlocked.Increment(ref _count);
                        return new DaprLogValuesFormatter(f);
                    });
#endif
                }
            }
            else
            {
                _formatter = null;
            }

            _originalMessage = format ?? NullFormat;
            _values = values;
        }

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException(nameof(index));
                }

                if (index == Count - 1)
                {
                    return new KeyValuePair<string, object> ("{OriginalFormat}", _originalMessage);
                }

                return _formatter.GetValue(_values, index);
            }
        }

        public int Count
        {
            get
            {
                if (_formatter == null)
                {
                    return 1;
                }

                return _formatter.ValueNames.Count + 1;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        public override string ToString()
        {
            if (_formatter == null)
            {
                return _originalMessage;
            }

            return _formatter.Format(_values);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
