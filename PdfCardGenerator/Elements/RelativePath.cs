using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace PdfCardGenerator.Elements
{
    internal struct RelativePath : IContextValue<Stream>
    {

        public ContextValue<string> Path { get; set; }
        public AbstractFileProvider WorkingDirectory { get; set; }

        public Stream GetValue(XElement context, IXmlNamespaceResolver resolver)
        {
            var relativePath = this.Path.GetValue(context, resolver);
            return this.WorkingDirectory.Open(relativePath);

        }
    }

}