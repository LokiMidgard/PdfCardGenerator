using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PdfGenerator
{
    public abstract class Element
    {
        public ContextValue<RectangleF> Position { get; set; }

        public ContextValue<double> ZIndex { get; set; }



        internal Element()
        {
        }
    }

    public class TextElement : Element
    {
        public IEnumerable<ParaGraph> Paragraphs { get; set; }

    }

    public class ParaGraph
    {
        public IEnumerable<Run> Runs { get; set; }

        /// <summary>
        /// Linespace relativ to normal linespacing.
        /// </summary>
        public ContextValue<double> Linespacing { get; set; } = 1.0;

        public ContextValue<XUnit> BeforeParagraph { get; set; }
        public ContextValue<XUnit> AfterParagraph { get; set; }

    }

    public abstract class Run { internal Run() { } }
    public class LineBreakRun : Run { }
    public class TextRun : Run
    {
        public ContextValue<bool> IsVisible { get; set; } = true;

        public ContextValue<string> Text { get; set; }

        public ContextValue<XFontStyle> FontStyle { get; set; } = XFontStyle.Regular;
        public ContextValue<double> EmSize { get; set; } = 12.0;
        public ContextValue<string> FontName { get; set; } = "Verdana";

    }


    public class ImageElement : Element
    {

    }
}