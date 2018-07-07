using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PdfGenerator
{
    public class PageTemplate
    {
        public XUnit Width { get; set; }
        public XUnit Height { get; set; }

        public ICollection<Element> Elements { get; } = new List<Element>();

        public PdfDocument GetDocuments(IEnumerable<XElement> elements)
        {
            var document = new PdfDocument();
            document.PageLayout = PdfPageLayout.SinglePage;
            foreach (var context in elements)
            {
                var page = document.AddPage();
                page.Width = this.Width;
                page.Height = this.Height;

                // Get an XGraphics object for drawing
                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    // Create a font

                    foreach (var item in this.Elements.OrderByDescending(x => x.ZIndex))
                    {

                        if (item is TextElement textElement)
                        {
                            var position = textElement.Position.GetValue(context);
                            var zIndex = textElement.ZIndex.GetValue(context);

                            var frame = textElement.Position.GetValue(context);
                            var startPosition = frame.Location;
                            foreach (var run in textElement.Paragraphs.SelectMany(x => x.Runs).OfType<TextRun>())
                            {
                                if (!run.IsVisible.GetValue(context))
                                    continue;

                                var textForRun = run.Text.GetValue(context);
                                if (string.IsNullOrEmpty(textForRun))
                                    continue;

                                var font = new XFont(run.FontName.GetValue(context), run.EmSize.GetValue(context), run.FontStyle.GetValue(context));



                                var wordSizes = textForRun.Split(' ').Select(x => new { Size = gfx.MeasureString(x, font), Word = x }).ToArray();
                                var spaceSize = gfx.MeasureString(" ", font);

                                var height = font.Height;
                                var currentPosition = startPosition;

                                for (int i = 0; i < wordSizes.Length; i++)
                                {
                                    var w = wordSizes[i];
                                    if (w.Size.Width + currentPosition.X > position.Right && i != 0 /*we can't make a linebreka before the first word*/)
                                    {
                                        // we are over the bounding. set current Position to next line

                                        //TODO: Using Syllabification
                                        currentPosition = new System.Drawing.PointF(startPosition.X, currentPosition.Y + height);
                                    }
                                    if (w.Size.Height + currentPosition.Y > position.Bottom)
                                        break; // reached end of box

                                    gfx.DrawString(w.Word, font, XBrushes.Black, currentPosition, XStringFormats.TopLeft);
                                    currentPosition = new System.Drawing.PointF((float)(currentPosition.X + spaceSize.Width + w.Size.Width), currentPosition.Y);
                                }
                            }

                            //gfx.MeasureString(,, XStringFormats.BottomRight)
                        }
                        else if (item is ImageElement imageElement)
                        {

                        }
                        else
                            throw new NotSupportedException($"Element of Type {item?.GetType()} is not supported.");


                    }

                    //// Draw the text
                    //gfx.DrawString("Hello, World!", font, XBrushes.Black,
                    //  new XRect(0, 0, page.Width, page.Height),
                    //  XStringFormats.Center);

                }

            }
            return document;
        }
    }
}
