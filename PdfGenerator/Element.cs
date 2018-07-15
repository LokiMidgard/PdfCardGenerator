using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
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

    public class RectElement : Element
    {
        public XPen BorderColor { get; set; }
        public XBrush FillColor { get; set; }

    }
    public class TextElement : Element
    {
        public IList<IChild<Paragraph>> Paragraphs { get; set; }

    }

    public class Paragraph : IChild<Paragraph>
    {

        public const string DEFAULT_FONT_NAME = "Verdana";
        public const double DEFAULT_EM_SIZE = 12.0;
        private IList<IChild<Run>> _runs;

        public IList<IChild<Run>> Runs { get => _runs ?? (_runs = new List<IChild<Run>>()); set => _runs = value; }
        /// <summary>
        /// Linespace relativ to normal linespacing.
        /// </summary>
        public ContextValue<double> Linespacing { get; set; } = 1.0;

        public ContextValue<XUnit> BeforeParagraph { get; set; }
        public ContextValue<XUnit> AfterParagraph { get; set; }

        public ContextValue<XFontStyle> FontStyle { get; set; } = XFontStyle.Regular;
        public ContextValue<double> EmSize { get; set; } = DEFAULT_EM_SIZE;
        public ContextValue<string> FontName { get; set; } = DEFAULT_FONT_NAME;
        public ContextValue<bool> IsVisible { get; set; } = true;
        public ContextValue<XLineAlignment> Alignment { get; set; }

        public Paragraph()
        {
        }

        //public TextRun AddRun(ContextValue<string>? text = null, ContextValue<XFontStyle>? fontStyle = null, ContextValue<double>? emSize = null, ContextValue<string>? fontName = null, ContextValue<bool>? isVisible = null) => this.InsertRun(this.runs.Count, text, fontStyle, emSize, fontName, isVisible);

        //public TextRun InsertRun(int index, ContextValue<string>? text = null, ContextValue<XFontStyle>? fontStyle = null, ContextValue<double>? emSize = null, ContextValue<string>? fontName = null, ContextValue<bool>? isVisible = null)
        //{
        //    var newRun = new TextRun(this)
        //    {
        //        EmSize = emSize,
        //        FontName = fontName,
        //        FontStyle = fontStyle,
        //        Text = text ?? default,
        //        IsVisible = isVisible ?? true
        //    };
        //    this.runs.Insert(index, newRun);
        //    return newRun;
        //}

        //public LineBreakRun AddLineBreak(ContextValue<XFontStyle>? fontStyle = null, ContextValue<double>? emSize = null, ContextValue<string>? fontName = null, ContextValue<bool>? isVisible = null) => this.InsertLineBreak(this.runs.Count, fontStyle, emSize, fontName, isVisible);
        //public LineBreakRun InsertLineBreak(int index, ContextValue<XFontStyle>? fontStyle = null, ContextValue<double>? emSize = null, ContextValue<string>? fontName = null, ContextValue<bool>? isVisible = null)
        //{
        //    var newRun = new LineBreakRun(this)
        //    {
        //        EmSize = emSize,
        //        FontName = fontName,
        //        FontStyle = fontStyle,
        //        IsVisible = isVisible ?? true
        //    };
        //    this.runs.Insert(index, newRun);
        //    return newRun;
        //}

        //public LineBreakRun InsertForeach(int index, ContextValue<XFontStyle>? fontStyle = null, ContextValue<double>? emSize = null, ContextValue<string>? fontName = null, ContextValue<bool>? isVisible = null)
        //{
        //    var newRun = new LineBreakRun(this)
        //    {
        //        EmSize = emSize,
        //        FontName = fontName,
        //        FontStyle = fontStyle,
        //        IsVisible = isVisible ?? true
        //    };
        //    this.runs.Insert(index, newRun);
        //    return newRun;
        //}
    }

    public abstract class Run : IChild<Run>
    {
        private ContextValue<string>? _fontName;
        private ContextValue<double>? _emSize;
        private ContextValue<XFontStyle>? _fontStyle;

        public ContextValue<XFontStyle>? FontStyle { get => this._fontStyle ?? this.Paragraph.FontStyle; set => this._fontStyle = value; }
        public ContextValue<double>? EmSize { get => this._emSize ?? this.Paragraph.EmSize; set => this._emSize = value; }
        public ContextValue<string>? FontName
        {
            get => this._fontName ?? this.Paragraph.FontName; set
            {
                if (!value.HasValue)
                    this._fontName = null;
                else if (!value.Value.IsXPath && value.Value.GetValue(null, null) == null)
                    this._fontName = null;
                else
                    this._fontName = value.Value;
            }
        }

        public ContextValue<bool> IsVisible { get; set; } = true;
        public Paragraph Paragraph { get; }

        internal Run(Paragraph paragraph)
        {
            this.Paragraph = paragraph;
        }

    }
    public sealed class LineBreakRun : Run
    {
        public LineBreakRun(Paragraph paragraph) : base(paragraph)
        {
        }
    }
    public sealed class TextRun : Run
    {
        public TextRun(Paragraph paragraph) : base(paragraph)
        {
        }

        public ContextValue<string> Text { get; set; }
    }


    public class ImageElement : Element
    {

        public RelativePath ImagePath { get; set; }
    }

    public struct RelativePath : IContextValue<string>
    {

        public ContextValue<string> Path { get; set; }
        public System.IO.DirectoryInfo WorkingDirectory { get; set; }

        public string GetValue(XElement context, IXmlNamespaceResolver resolver)
        {
            var relativePath = Path.GetValue(context, resolver);
            return System.IO.Path.Combine(this.WorkingDirectory.FullName, relativePath);

        }
    }

    public interface IChild<T> { }

    public class ForeEach<T> : IChild<T>
    {
        public string Select { get; set; }

        public IList<IChild<T>> Childrean { get; set; }

        public ContextValue<bool> IsVisible { get; set; } = true;

    }

}