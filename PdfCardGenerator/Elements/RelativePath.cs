using System.Xml;
using System.Xml.Linq;

namespace PdfCardGenerator.Elements
{
    internal struct RelativePath : IContextValue<string>
    {

        public ContextValue<string> Path { get; set; }
        public System.IO.DirectoryInfo WorkingDirectory { get; set; }

        public string GetValue(XElement context, IXmlNamespaceResolver resolver)
        {
            var relativePath = this.Path.GetValue(context, resolver);
            return System.IO.Path.Combine(this.WorkingDirectory.FullName, relativePath);

        }
    }

}