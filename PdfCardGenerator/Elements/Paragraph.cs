using PdfSharp.Drawing;
using Serilizer;
using System.Collections.Generic;

namespace PdfCardGenerator.Elements
{
    internal class Paragraph : IChild<Paragraph>
    {


        private IList<IChild<Run>> _runs;

        public IList<IChild<Run>> Runs { get => this._runs ?? (this._runs = new List<IChild<Run>>()); set => this._runs = value; }
        /// <summary>
        /// Linespace relativ to normal linespacing.
        /// </summary>
        public ContextValue<double> Linespacing { get; set; }

        public ContextValue<XUnit> BeforeParagraph { get; set; }
        public ContextValue<XUnit> AfterParagraph { get; set; }

        public ContextValue<XFontStyle> FontStyle { get; set; }
        public ContextValue<double> EmSize { get; set; }
        public ContextValue<string> FontName { get; set; }
        public ContextValue<XColor> Color { get; set; }
        public ContextValue<bool> IsVisible { get; set; }
        public ContextValue<XLineAlignment> Alignment { get; set; }
        public Language Language { get; internal set; }

        public Paragraph()
        {
        }
    }

}