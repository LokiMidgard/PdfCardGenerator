using PdfSharp.Drawing;

namespace PdfCardGenerator.Elements
{
    internal class RectElement : Element
    {
        public XPen BorderColor { get; set; }
        public XBrush FillColor { get; set; }

    }

}