using System;
using System.Collections.Generic;

namespace Man.Dapr.Sidekick.Process
{
    public class CommandLineArgumentBuilder
    {
        private const string DefaultArgumentSeparator = " ";
        private const string DefaultArgumentPrefix = "--";

        private readonly List<string> _tokens = new List<string>();
        private string _argumentSeparator;

        public CommandLineArgumentBuilder()
            : this(null)
        {
        }

        public CommandLineArgumentBuilder(string argumentPrefix)
        {
            ArgumemtPrefix = string.IsNullOrEmpty(argumentPrefix) ? DefaultArgumentPrefix : argumentPrefix;
            ArgumentSeparator = DefaultArgumentSeparator;
        }

        public string ArgumemtPrefix { get; }

        public string ArgumentSeparator
        {
            get { return _argumentSeparator; }
            set { _argumentSeparator = string.IsNullOrEmpty(value) ? DefaultArgumentSeparator : value; }
        }

        public CommandLineArgumentBuilder Add(
            string name,
            object value = null,
            bool requiresValue = true,
            bool forceQuotes = false,
            Func<bool> predicate = null)
        {
            // Precondition checks
            if (name.IsNullOrWhiteSpaceEx() || predicate?.Invoke() == false)
            {
                return this;
            }

            // If a boolean value is passed, only need to add the name and only if value is true
            var fullName = !name.StartsWith(ArgumemtPrefix) ? ArgumemtPrefix + name : name;
            if (value is bool?)
            {
                if ((bool?)value == true)
                {
                    _tokens.Add(fullName);
                }
            }
            else if (value != null)
            {
                _tokens.Add(fullName);
                var valueString = value.ToString();
                if (valueString.Contains(ArgumentSeparator) || forceQuotes)
                {
                    // Quote value
                    _tokens.Add($"\"{valueString}\"");
                }
                else
                {
                    _tokens.Add(valueString);
                }
            }
            else if (!requiresValue)
            {
                // Just add the name (no value)
                _tokens.Add(fullName);
            }

            return this;
        }

#if NET35
        public override string ToString() => string.Join(ArgumentSeparator, _tokens.ToArray());
#else
        public override string ToString() => string.Join(ArgumentSeparator, _tokens);
#endif
    }
}
