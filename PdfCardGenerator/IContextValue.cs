using System.Xml;
using System.Xml.Linq;

namespace PdfGenerator
{
    public interface IContextValue<T>
    {
        T GetValue(XElement context, IXmlNamespaceResolver resolver);
    }
}