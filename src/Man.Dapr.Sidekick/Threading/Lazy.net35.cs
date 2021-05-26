#if NET35
// This implementation taken from : https://stackoverflow.com/questions/3207580/implementation-of-lazyt-for-net-3-5

namespace System
{
    /// <summary>
    /// Provides support for lazy initialization.
    /// </summary>
    /// <typeparam name="T">Specifies the type of object that is being lazily initialized.</typeparam>
    internal sealed class Lazy<T>
    {
        private readonly object _padlock = new object();
        private readonly Func<T> _createValue;
        private bool _isValueCreated;
        private T _value;

        /// <summary>
        /// Gets the lazily initialized value of the current Lazy{T} instance.
        /// </summary>
        public T Value
        {
            get
            {
                if (!_isValueCreated)
                {
                    lock (_padlock)
                    {
                        if (!_isValueCreated)
                        {
                            _value = _createValue();
                            _isValueCreated = true;
                        }
                    }
                }

                return _value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a value has been created for this Lazy{T} instance.
        /// </summary>
        public bool IsValueCreated
        {
            get
            {
                lock (_padlock)
                {
                    return _isValueCreated;
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Lazy{T}"/> class.
        /// </summary>
        /// <param name="createValue">The delegate that produces the value when it is needed.</param>
        public Lazy(Func<T> createValue)
        {
            _createValue = createValue ?? throw new ArgumentNullException(nameof(createValue));
        }


        /// <summary>
        /// Creates and returns a string representation of the Lazy{T}.Value.
        /// </summary>
        /// <returns>The string representation of the Lazy{T}.Value property.</returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
#endif
