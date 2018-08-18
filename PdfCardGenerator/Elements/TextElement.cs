using PdfSharp.Drawing;
using System.Collections.Generic;

namespace PdfCardGenerator.Elements
{
    internal class TextElement : Element
    {
        public IList<IChild<Paragraph>> Paragraphs { get; set; }
        public ContextValue<XLineAlignment> VerticalAlignment { get; set; }
        public ContextValue<double> MinEmSizeScale { get; set; }
        public ContextValue<double> MaxEmSizeScale { get; set; }

    }

}