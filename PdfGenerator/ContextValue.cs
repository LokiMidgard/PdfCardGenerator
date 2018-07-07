using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PdfGenerator
{

    public struct ContextValue<T>
    {
        private static ContextValue<T> Empty { get; } = new ContextValue<T>();

        private readonly bool wasConstructed;
        private readonly T value;
        private readonly bool hasConcretValue;
        private readonly string xpath;

        private ContextValue(T value, bool hasConcretValue, string xpath)
        {
            this.wasConstructed = true;
            this.value = value;
            this.hasConcretValue = hasConcretValue;
            this.xpath = xpath;
        }

        public static ContextValue<T> FromValue(T value) => new ContextValue<T>(value, true, null);
        public static ContextValue<T> FromXPath(string xPath) => new ContextValue<T>(default, false, xPath);

        public T GetValue(XElement context)
        {
            if (!this.wasConstructed)
                return default;

            if (this.hasConcretValue)
                return this.value;

            var element = context.XPathEvaluate(this.xpath);

            if (typeof(T) == typeof(bool))
            {
                bool result;

                if (element is null)
                    result = false;
                else if (element is bool b)
                    result = b;
                else if (element is double d)
                    result = d != 0;
                else if (element is string s)
                    result = s != "";
                else if (element is IEnumerable<XElement> xelements)
                    result = xelements.Any();
                else if (element is IEnumerable<XAttribute> xattributes)
                    result = xattributes.Any();
                else throw new NotSupportedException($"The type {element.GetType()}");

                return (T)(object)result;
            }
            else if (typeof(T) == typeof(string))
            {
                string result;

                if (element is null)
                    result = "";
                else if (element is IEnumerable<XElement> xelements)
                    result = xelements.FirstOrDefault()?.Value ?? "";
                else if (element is IEnumerable<XAttribute> xattributes)
                    result = xattributes.FirstOrDefault()?.Value ?? "";
                else
                    result = element.ToString();

                return (T)(object)result;
            }
            else if (typeof(T).IsEnum)
            {
                string text;

                if (element is null)
                    text = "";
                else if (element is IEnumerable<XElement> xelements)
                    text = xelements.FirstOrDefault()?.Value ?? "";
                else if (element is IEnumerable<XAttribute> xattributes)
                    text = xattributes.FirstOrDefault()?.Value ?? "";
                else
                    text = element.ToString();

                return (T)Enum.Parse(typeof(T), text);
            }
            else throw new NotSupportedException($"The type {element.GetType()}");

        }

        public static implicit operator ContextValue<T>(T value) => FromValue(value);
    }
}
