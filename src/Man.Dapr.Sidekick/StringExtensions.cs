namespace System
{
    /// <summary>
    /// Extension methods for the <see cref="string"/> type for methods missing in .NET3.5.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///  Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><c>true</c> if the value parameter is null or <see cref="string.Empty"/>, or if <paramref name="value"/> consists exclusively of white-space characters.</returns>
        public static bool IsNullOrWhiteSpaceEx(this string value)
        {
#if NET35
            if (value == null)
            {
                return true;
            }

            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
#else
            return string.IsNullOrWhiteSpace(value);
#endif
        }
    }
}
