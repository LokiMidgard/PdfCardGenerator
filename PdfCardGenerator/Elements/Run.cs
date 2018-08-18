using PdfSharp.Drawing;
using PdfCardGenerator.Serilizer;

namespace PdfCardGenerator.Elements
{
    internal abstract class Run : IChild<Run>
    {
        private ContextValue<string>? _fontName;
        private ContextValue<double>? _emSize;
        private ContextValue<XFontStyle>? _fontStyle;
        private ContextValue<XColor>? _color;
        private ContextValue<Language>? _language;

        public ContextValue<Language>? Language { get => this._language ?? this.Paragraph.Language; internal set => this._language = value; }
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

        public ContextValue<XColor>? Color { get => this._color ?? this.Paragraph.Color; set => this._color = value; }

        public ContextValue<bool> IsVisible { get; set; } = true;
        public Paragraph Paragraph { get; }

        internal Run(Paragraph paragraph)
        {
            this.Paragraph = paragraph;
        }

    }

}