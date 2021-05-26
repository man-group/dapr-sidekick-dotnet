using System;
using System.ComponentModel;
using System.Globalization;

namespace Man.Dapr.Sidekick.Security
{
    public class SensitiveStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string sensitiveValue)
            {
                return new SensitiveString(sensitiveValue);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
