namespace PdfCardGenerator.Elements
{
    internal sealed class TextRun : Run
    {
        public TextRun(Paragraph paragraph) : base(paragraph)
        {
        }

        public ContextValue<string> Text { get; set; }
    }

}