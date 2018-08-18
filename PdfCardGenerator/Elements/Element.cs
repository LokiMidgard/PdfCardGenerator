using PdfSharp.Drawing;
using System;
using System.Drawing;
using System.Linq;
using System.Xml.XPath;

namespace PdfCardGenerator.Elements
{
    internal abstract class Element
    {
        public ContextValue<XRect> Position { get; set; }
        public ContextValue<double> ZIndex { get; set; }


        public ContextValue<double> Rotation { get; set; }

        public ContextValue<XPoint> RotationOrigin { get; set; }

        public ContextValue<bool> IsVisible { get; set; } = true;


        internal Element()
        {
        }
    }

}