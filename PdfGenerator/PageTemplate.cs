using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace PdfGenerator
{
    public class PageTemplate
    {
        public XUnit Width { get; set; }
        public XUnit Height { get; set; }

        public XPath ContextPath { get; set; }

        public IList<Element> Elements { get; set; } = new List<Element>();
        public RelativePath? XSLT { get; internal set; }

        public PdfDocument GetDocuments(XDocument document, IXmlNamespaceResolver resolver)
        {

            return GetDocuments(document.XPathSelectElements(this.ContextPath.Path, resolver), resolver);
        }
        private PdfDocument GetDocuments(IEnumerable<XElement> elements, IXmlNamespaceResolver resolver)
        {
            if (XSLT != null)
            {
                elements = elements.Select(e =>
                {
                    var transformer = new XslCompiledTransform();
                    transformer.Load(XSLT.Value.GetValue(e, resolver));
                    //transformer.XmlResolver = resolver;
                    var builder = new StringBuilder();
                    using (var writer = XmlWriter.Create(builder))
                    {
                        transformer.Transform(e.CreateNavigator(), writer);
                        return XDocument.Parse(builder.ToString()).Root;
                    }
                }).ToArray();
            }

            var document = new PdfDocument
            {
                PageLayout = PdfPageLayout.SinglePage
            };
            foreach (var context in elements)
            {
                var page = document.AddPage();
                page.Width = this.Width;
                page.Height = this.Height;

                // Get an XGraphics object for drawing
                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    // Create a font

                    foreach (var item in this.Elements.OrderByDescending(x => x.ZIndex.GetValue(context, resolver)))
                    {
                        if (!item.IsVisible.GetValue(context, resolver))
                            continue;

                        if (item is TextElement textElement)
                        {
                            HandleTextElement(resolver, context, gfx, textElement);



                            //gfx.MeasureString(,, XStringFormats.BottomRight)
                        }
                        else if (item is ImageElement imageElement)
                        {
                            var path = imageElement.ImagePath.GetValue(context, resolver);
                            var position = imageElement.Position.GetValue(context, resolver);
                            using (var image = XImage.FromFile(path))
                                gfx.DrawImage(image, position);

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

        private static void HandleTextElement(IXmlNamespaceResolver resolver, XElement context, XGraphics gfx, TextElement textElement)
        {
            var position = textElement.Position.GetValue(context, resolver);

            var frame = textElement.Position.GetValue(context, resolver);
            var startPosition = frame.Location;
            var currentPosition = startPosition;

            IEnumerable<Paragraph> TransformParapgraps(Paragraph oldP )
            {
                var currentP = new Paragraph
                {
                    AfterParagraph = oldP.AfterParagraph,
                    Alignment = oldP.Alignment,
                    BeforeParagraph = oldP.BeforeParagraph,
                    EmSize = oldP.EmSize,
                    FontName = oldP.FontName,
                    FontStyle = oldP.FontStyle,
                    IsVisible = oldP.IsVisible,
                    Linespacing = oldP.Linespacing
                };
                bool isMarkdown = true;
                foreach (var item in oldP.Runs)
                {
                    if (item is LineBreakRun lineBreak)
                        currentP.AddLineBreak(lineBreak.FontStyle, lineBreak.EmSize, lineBreak.FontName, lineBreak.IsVisible);

                    else if (item is TextRun textRun)
                    {
                        var text = textRun.Text.GetValue(context, resolver);
                        var builder = new StringBuilder();
                        if (isMarkdown)
                        {
                            var lastLinebreaks = 0;
                            var lastSpaces = 0;

                            foreach (var c in text)
                            {
                                if (c == '\n')
                                {
                                    lastLinebreaks++;
                                }
                                else if (c == '\r')
                                {
                                    // ignore  \r
                                }
                                else if (c == ' ')
                                {
                                    lastSpaces++;
                                }
                                else
                                {
                                    if (lastLinebreaks == 1 && lastSpaces >= 2)
                                    {
                                        // we make a linebreak and write everything already in our buffer
                                        currentP.AddRun(builder.ToString(), textRun.FontStyle, textRun.EmSize, textRun.FontName, textRun.IsVisible);
                                        currentP.AddLineBreak();
                                        builder.Clear();
                                    }
                                    else if (lastLinebreaks > 1)
                                    {
                                        // we make a new paragraph
                                        currentP.AddRun(builder.ToString(), textRun.FontStyle, textRun.EmSize, textRun.FontName, textRun.IsVisible);
                                        yield return currentP;
                                        currentP = new Paragraph()
                                        {
                                            AfterParagraph = currentP.AfterParagraph,
                                            Alignment = currentP.Alignment,
                                            BeforeParagraph = currentP.BeforeParagraph,
                                            EmSize = currentP.EmSize,
                                            FontName = currentP.FontName,
                                            FontStyle = currentP.FontStyle,
                                            IsVisible = currentP.IsVisible,
                                            Linespacing = currentP.Linespacing
                                        };
                                        builder.Clear();
                                    }
                                    else if (lastSpaces > 0)
                                    {
                                        builder.Append(' ');
                                    }

                                    builder.Append(c);
                                    lastSpaces = 0;
                                    lastLinebreaks = 0;
                                }
                            }
                            if (builder.Length > 0)
                                currentP.AddRun(builder.ToString(), textRun.FontStyle, textRun.EmSize, textRun.FontName, textRun.IsVisible);
                        }
                        else
                        {
                            foreach (var c in text)
                            {
                                if (c == '\n')
                                {
                                    if (builder.Length > 0)
                                        currentP.AddRun(builder.ToString(), textRun.FontStyle, textRun.EmSize, textRun.FontName, textRun.IsVisible);
                                    currentP.AddLineBreak();
                                    builder.Clear();
                                }
                                else if (c == '\r')
                                {
                                    // ignore  \r
                                }
                                else
                                {
                                    builder.Append(c);
                                }
                            }
                            if (builder.Length > 0)
                                currentP.AddRun(builder.ToString(), textRun.FontStyle, textRun.EmSize, textRun.FontName, textRun.IsVisible);

                        }

                    }

                }

                yield return currentP;
            }

            var paragraphs = textElement.Paragraphs.SelectMany(TransformParapgraps);
            foreach (var paragraph in paragraphs)
            {
                if (!paragraph.IsVisible.GetValue(context, resolver))
                    continue;

                var lines = new List<List<(string textToPrint, XFont font, XBrush brush, XPoint printPosition, XLineAlignment alignment, XSize size)>>();
                var currentLine = new List<(string textToPrint, XFont font, XBrush brush, XPoint printPosition, XLineAlignment alignment, XSize size)>();
                currentPosition = new XPoint(currentPosition.X, currentPosition.Y + paragraph.BeforeParagraph.GetValue(context, resolver));
                foreach (var run in paragraph.Runs)
                {
                    if (!run.IsVisible.GetValue(context, resolver))
                        continue;

                    lines.Add(currentLine);

                    var font = new XFont(run.FontName.Value.GetValue(context, resolver), run.EmSize.Value.GetValue(context, resolver), run.FontStyle.Value.GetValue(context, resolver));
                    var height = font.Height * paragraph.Linespacing.GetValue(context, resolver);

                    if (run is LineBreakRun)
                    {
                        currentPosition = new XPoint(startPosition.X, currentPosition.Y + height);
                    }
                    else if (run is TextRun textRun)
                    {


                        var textForRun = textRun.Text.GetValue(context, resolver);
                        if (string.IsNullOrEmpty(textForRun))
                            continue;

                        var wordSizes = textForRun.Split(' ').Select(x => new { Size = gfx.MeasureString(x, font), Word = x }).ToArray();
                        var spaceSize = gfx.MeasureString(" ", font);

                        int wordsToPrint;
                        for (int i = 0; i < wordSizes.Length; i += wordsToPrint)
                        {
                            double lineWidth = 0;
                            bool endOfLine = false;
                            for (wordsToPrint = 0; wordsToPrint + i < wordSizes.Length; wordsToPrint++)
                            {
                                var w = wordSizes[i + wordsToPrint];

                                if (w.Size.Width + currentPosition.X + lineWidth > position.Right && i + wordsToPrint != 0 /*we can't make a linebreka before the first word*/)
                                {
                                    // we are over the bounding. set current Position to next line
                                    endOfLine = true;
                                    break;
                                }
                                lineWidth += w.Size.Width + (wordsToPrint == 0 ? 0 : spaceSize.Width);
                            }
                            wordsToPrint = Math.Max(wordsToPrint, 1); // we want to print at least one word other wise we will not consume it and will print nothing agiain

                            var textToPrint = string.Join(" ", wordSizes.Skip(i).Take(wordsToPrint).Select(x => x.Word));

                            currentLine.Add((textToPrint, font, XBrushes.Black, currentPosition, paragraph.Alignment.GetValue(context, resolver), gfx.MeasureString(textToPrint, font)));
                            currentPosition = new XPoint(currentPosition.X + spaceSize.Width + lineWidth, currentPosition.Y);

                            if (endOfLine)
                            {
                                currentPosition = new XPoint(startPosition.X, currentPosition.Y + height);
                                currentLine = new List<(string textToPrint, XFont font, XBrush brush, XPoint currentPosition, XLineAlignment alignment, XSize size)>();
                                lines.Add(currentLine);
                            }

                            if (wordSizes.Skip(i).Take(wordsToPrint).Max(x => x.Size.Height) + currentPosition.Y > position.Bottom)
                                goto END; // reached end of box


                        }
                    }
                }

                // now we calculate LineAlignment and print 
                END:;
                var maximumWidth = textElement.Position.GetValue(context, resolver).Width;
                foreach (var line in lines.Where(x => x.Count > 0))
                {
                    var leftmost = line.Min(x => x.printPosition.X);
                    var rightmost = line.Max(x => x.printPosition.X + x.size.Width);
                    var width = rightmost - leftmost;

                    double offset;
                    switch (paragraph.Alignment.GetValue(context, resolver))
                    {
                        case XLineAlignment.Near:
                            offset = 0;
                            break;
                        case XLineAlignment.Center:
                            offset = (maximumWidth - width) / 2;
                            break;
                        case XLineAlignment.Far:
                            offset = maximumWidth - width;
                            break;
                        case XLineAlignment.BaseLine:
                            throw new NotSupportedException("Not sure what this means");
                        default:
                            throw new NotSupportedException($"The Alignment {paragraph.Alignment} is not supported.");
                    }
                    foreach (var print in line)
                        gfx.DrawString(print.textToPrint, print.font, print.brush, new XPoint(print.printPosition.X + offset, print.printPosition.Y), XStringFormats.TopLeft);
                }
                currentPosition = new XPoint(startPosition.X, currentPosition.Y + lines.Where(x => x.Count > 0).LastOrDefault()?.Max(x => x.size.Height) ?? 0 + paragraph.AfterParagraph.GetValue(context, resolver));

            }

        }
    }


}
