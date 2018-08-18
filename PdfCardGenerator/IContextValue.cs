using System.Xml;
using System.Xml.Linq;

namespace PdfCardGenerator
{
    internal interface IContextValue<T>
    {
        T GetValue(XElement context, IXmlNamespaceResolver resolver);
    }
}