namespace Dapr.Sidekick.Security
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Gets the sensitive value for an object if it implements <see cref="ISensitiveValue"/>, else the original value.
        /// </summary>
        /// <param name="value">The object value.</param>
        /// <returns>The sensitive value, or the original value if not sensitive.</returns>
        public static object SensitiveValue(this object value) => value is ISensitiveValue sensitiveValue ? sensitiveValue.Value : value;
    }
}
