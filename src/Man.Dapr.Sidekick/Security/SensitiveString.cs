using System;
using System.ComponentModel;

namespace Man.Dapr.Sidekick.Security
{
    /// <summary>
    /// Represents a <see cref="string"/> value that contains sensitive data which should not be exposed in logs or reports.
    /// </summary>
    [TypeConverter(typeof(SensitiveStringConverter))]
    public class SensitiveString : ISensitiveValue
    {
        public const string SensitiveValue = "[sensitive]";

        /// <summary>
        /// Converts a <see cref="SensitiveString"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="value">A <see cref="SensitiveString"/>value.</param>
        public static implicit operator string(SensitiveString value) => value?.Value;

        /// <summary>
        /// Converts a <see cref="string"/> to a <see cref="SensitiveString"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/>value.</param>
        public static implicit operator SensitiveString(string value) => value == null ? null : new SensitiveString(value);

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitiveString"/> class to the specified string <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The sensitive <see cref="string"/> value.</param>
        public SensitiveString(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the sensitive value.
        /// </summary>
        public string Value { get; set; }

        object ISensitiveValue.Value => Value;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is string value)
            {
                return value == Value;
            }

            if (obj is SensitiveString sensitiveValue)
            {
                return sensitiveValue.Value == Value;
            }

            return false;
        }

        public override int GetHashCode() => Value?.GetHashCode() ?? base.GetHashCode();

        public static bool operator ==(SensitiveString a, SensitiveString b) => Equals((string)a, (string)b);

        public static bool operator !=(SensitiveString a, SensitiveString b) => !Equals((string)a, (string)b);

        public override string ToString() => Value.IsNullOrWhiteSpaceEx() ? Value : SensitiveValue;
    }
}
