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
        public ContextValue<XRect> Position { get; set; }

        public ContextValue<double> ZIndex { get; set; }

        public ContextValue<bool> IsVisible { get; set; } = true;


        internal Element()
        {
        }
    }

    public class TextElement : Element
    {
        public IList<Paragraph> Paragraphs { get; set; }

    }

    public class Paragraph
    {
        public IReadOnlyList<Run> Runs { get; }

        private readonly List<Run> runs;
        /// <summary>
        /// Linespace relativ to normal linespacing.
        /// </summary>
        public ContextValue<double> Linespacing { get; set; } = 1.0;

        public ContextValue<XUnit> BeforeParagraph { get; set; }
        public ContextValue<XUnit> AfterParagraph { get; set; }

        public ContextValue<XFontStyle> FontStyle { get; set; } = XFontStyle.Regular;
        public ContextValue<double> EmSize { get; set; } = 12.0;
        public ContextValue<string> FontName { get; set; } = "Verdana";
        public ContextValue<bool> IsVisible { get; set; } = true;
        public ContextValue<XLineAlignment> Alignment { get; set; }

        public Paragraph()
        {
            this.runs = new List<Run>();
            this.Runs = this.runs.AsReadOnly();
        }

        public TextRun AddRun(ContextValue<string>? text = null, ContextValue<XFontStyle>? fontStyle = null, ContextValue<double>? emSize = null, ContextValue<string>? fontName = null, ContextValue<bool>? isVisible = null) => this.InsertRun(this.runs.Count, text, fontStyle, emSize, fontName, isVisible);

        public TextRun InsertRun(int index, ContextValue<string>? text = null, ContextValue<XFontStyle>? fontStyle = null, ContextValue<double>? emSize = null, ContextValue<string>? fontName = null, ContextValue<bool>? isVisible = null)
        {
            var newRun = new TextRun(this)
            {
                EmSize = emSize,
                FontName = fontName,
                FontStyle = fontStyle,
                Text = text ?? default,
                IsVisible = isVisible ?? true
            };
            this.runs.Insert(index, newRun);
            return newRun;
        }

        public LineBreakRun AddLineBreak(ContextValue<XFontStyle>? fontStyle = null, ContextValue<double>? emSize = null, ContextValue<string>? fontName = null, ContextValue<bool>? isVisible = null) => this.InsertLineBreak(this.runs.Count, fontStyle, emSize, fontName, isVisible);
        public LineBreakRun InsertLineBreak(int index, ContextValue<XFontStyle>? fontStyle = null, ContextValue<double>? emSize = null, ContextValue<string>? fontName = null, ContextValue<bool>? isVisible = null)
        {
            var newRun = new LineBreakRun(this)
            {
                EmSize = emSize,
                FontName = fontName,
                FontStyle = fontStyle,
                IsVisible = isVisible ?? true
            };
            this.runs.Insert(index, newRun);
            return newRun;
        }

    }

    public abstract class Run
    {
        private ContextValue<string>? _fontName;
        private ContextValue<double>? _emSize;
        private ContextValue<XFontStyle>? _fontStyle;

        public ContextValue<XFontStyle>? FontStyle { get => this._fontStyle ?? this.Paragraph.FontStyle; set => this._fontStyle = value; }
        public ContextValue<double>? EmSize { get => this._emSize ?? this.Paragraph.EmSize; set => this._emSize = value; }
        public ContextValue<string>? FontName { get => this._fontName ?? this.Paragraph.FontName; set => this._fontName = value; }

        public ContextValue<bool> IsVisible { get; set; } = true;
        public Paragraph Paragraph { get; }

        internal Run(Paragraph paragraph)
        {
            this.Paragraph = paragraph;
        }

    }
    public sealed class LineBreakRun : Run
    {
        internal LineBreakRun(Paragraph paragraph) : base(paragraph)
        {
        }
    }
    public sealed class TextRun : Run
    {
        internal TextRun(Paragraph paragraph) : base(paragraph)
        {
        }

        public ContextValue<string> Text { get; set; }
    }


    public class ImageElement : Element
    {
        public ContextValue<string> ImagePath { get; set; }
    }
}