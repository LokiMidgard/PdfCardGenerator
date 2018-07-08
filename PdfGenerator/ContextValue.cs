using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PdfGenerator
{

    [System.Diagnostics.DebuggerDisplay("Value: {value} / XPath:{xpath}")]
    public struct ContextValue<T> : IContextValue<T>
    {
        private static ContextValue<T> Empty { get; } = new ContextValue<T>();

        private readonly bool wasConstructed;
        private readonly T value;
        private readonly bool hasConcretValue;
        private readonly string xpath;
        internal bool IsXPath => !this.hasConcretValue;

        private ContextValue(T value, bool hasConcretValue, string xpath)
        {
            this.wasConstructed = true;
            this.value = value;
            this.hasConcretValue = hasConcretValue;
            this.xpath = xpath;
        }

        public static ContextValue<T> FromValue(T value) => new ContextValue<T>(value, true, null);
        public static ContextValue<T> FromXPath(string xPath) => new ContextValue<T>(default, false, xPath);

        public T GetValue(XElement context, IXmlNamespaceResolver resolver)
        {
            if (!this.wasConstructed)
                return default;

            if (this.hasConcretValue)
                return this.value;

            var element = context.XPathEvaluate(this.xpath, resolver);

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
                else if (element is System.Collections.IEnumerable objects)
                    result = objects.OfType<object>().Any();
                else throw new NotSupportedException($"The type {element.GetType()}");

                return (T)(object)result;
            }
            else if (typeof(T) == typeof(string))
            {
                string result;

                if (element is null)
                    result = "";
                else if (element is string s) // string implements IEnumberable, so we don't want just to get the first character
                    result = s;
                else if (element is System.Collections.IEnumerable objects)
                {
                    result = string.Join(" ", objects.OfType<object>().Select(firstValue =>
                    {
                        if (firstValue == null)
                            return null;
                        else if (firstValue is XElement xElement)
                            return xElement.Value;
                        else if (firstValue is XAttribute xAttribute)
                            return xAttribute.Value;
                        else
                            return firstValue.ToString();
                    }).Where(x => x != null));
                }
                else
                    result = element.ToString();

                return (T)(object)result;
            }
            else if (typeof(T).IsEnum)
            {
                string text;

                if (element is null)
                    text = "";
                else if (element is System.Collections.IEnumerable objects)
                {
                    var firstValue = objects.OfType<object>().FirstOrDefault();
                    if (firstValue == null)
                        text = "";
                    else if (firstValue is XElement xElement)
                        text = xElement.Value;
                    else if (firstValue is XAttribute xAttribute)
                        text = xAttribute.Value;
                    else
                        text = firstValue.ToString();
                }
                else
                    text = element.ToString();

                return (T)Enum.Parse(typeof(T), text);
            }
            else throw new NotSupportedException($"The type {element.GetType()}");

        }

        public static implicit operator ContextValue<T>(T value) => FromValue(value);
        public static implicit operator ContextValue<T>(XPath value) => FromXPath(value.Path);
    }

    public struct XPath
    {
        public readonly string Path;
        public XPath(string path)
        {
            this.Path = path;
        }

        public static explicit operator XPath(string value) => new XPath(value);

    }
}
