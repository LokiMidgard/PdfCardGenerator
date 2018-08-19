using NHyphenator;
using PdfCardGenerator;
using PdfCardGenerator.Elements;
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



namespace PdfCardGenerator
{
    internal class PageTemplate
    {
        public Project Project { get; set; }

        public XUnit Width { get; set; }
        public XUnit Height { get; set; }

        public XPath ContextPath { get; set; }

        public IList<Element> Elements { get; set; } = new List<Element>();
        public RelativePath? XSLT { get; internal set; }


        public PdfDocument GetDocuments(XDocument document, IXmlNamespaceResolver resolver)
        {
            return this.GetDocuments(document.XPathSelectElements(this.ContextPath.Path, resolver), resolver);
        }
        private PdfDocument GetDocuments(IEnumerable<XElement> elements, IXmlNamespaceResolver resolver)
        {
            if (this.XSLT != null)
            {
                elements = elements.Select(e =>
                {
                    var transformer = new XslCompiledTransform();
                    using (var reader = XmlReader.Create(this.XSLT.Value.GetValue(e, resolver)))
                        transformer.Load(reader);

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

                    foreach (var item in this.Elements.Reverse().OrderBy(x => x.ZIndex.GetValue(context, resolver)))
                    {
                        if (!item.IsVisible.GetValue(context, resolver))
                            continue;

                        using (gfx.RotateTransform(item, context, resolver))
                        {
                            var frame = item.Position.GetValue(context, resolver);
                            gfx.IntersectClip(frame);
                            if (item is TextElement textElement)
                            {
                                this.HandleTextElement(resolver, context, gfx, textElement);
                            }
                            else if (item is ImageElement imageElement)
                            {
                                var position = imageElement.Position.GetValue(context, resolver);


                                using (var stream = imageElement.ImagePath.GetValue(context, resolver))
                                using (var image = XImage.FromStream(stream))
                                    gfx.DrawImage(image, position);
                            }
                            else if (item is RectElement rectElement)
                            {
                                var position = rectElement.Position.GetValue(context, resolver);
                                gfx.DrawRectangle(rectElement.BorderColor, rectElement.FillColor, rectElement.Position.GetValue(context, resolver));

                            }
                            else
                                throw new NotSupportedException($"Element of Type {item?.GetType()} is not supported.");
                        }
                    }

                    //// Draw the text
                    //gfx.DrawString("Hello, World!", font, XBrushes.Black,
                    //  new XRect(0, 0, page.Width, page.Height),
                    //  XStringFormats.Center);

                }

            }
            return document;
        }

        private static IEnumerable<Paragraph> TransformParapgraps(IChild<Paragraph> p, IXmlNamespaceResolver resolver, XElement context)
        {
            if (p is ForeEach<Paragraph> forEach)
            {
                foreach (var newContext in context.XPathSelectElements(forEach.Select, resolver))
                    foreach (var child in forEach.Childrean.SelectMany(x => TransformParapgraps(x, resolver, newContext)))
                        yield return child;
            }
            if (p is Paragraph oldP)
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
                foreach (var item in TransformRun(oldP.Runs, currentP, isMarkdown, resolver, context))
                {
                    currentP = item;
                    yield return currentP;
                }

            }
        }

        private static IEnumerable<Paragraph> TransformRun(IEnumerable<IChild<Run>> items, Paragraph currentP, bool isMarkdown, IXmlNamespaceResolver resolver, XElement context)
        {

            foreach (var item in items)
            {
                if (item is LineBreakRun lineBreak)
                {
                    currentP.Runs.Add(new LineBreakRun(currentP)
                    {
                        EmSize = lineBreak.EmSize,
                        FontName = lineBreak.FontName,
                        FontStyle = lineBreak.FontStyle,
                        IsVisible = lineBreak.IsVisible
                    });
                }
                else if (item is ForeEach<Run> forEach)
                {
                    foreach (var newContext in context.XPathSelectElements(forEach.Select, resolver))
                        foreach (var child in TransformRun(forEach.Childrean, currentP, isMarkdown, resolver, newContext))
                        {
                            if (currentP != child)
                            {
                                yield return currentP;
                                currentP = child;
                            }
                        }
                }
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
                                    currentP.Runs.Add(new TextRun(currentP)
                                    {
                                        EmSize = textRun.EmSize,
                                        FontName = textRun.FontName,
                                        IsVisible = textRun.IsVisible,
                                        Color = textRun.Color,
                                        FontStyle = textRun.FontStyle,
                                        Text = builder.ToString()
                                    });
                                    currentP.Runs.Add(new LineBreakRun(currentP)
                                    {
                                        EmSize = textRun.EmSize,
                                        FontName = textRun.FontName,
                                        IsVisible = textRun.IsVisible,
                                        FontStyle = textRun.FontStyle
                                    });

                                    builder.Clear();
                                }
                                else if (lastLinebreaks > 1)
                                {
                                    // we make a new paragraph
                                    currentP.Runs.Add(new TextRun(currentP)
                                    {
                                        EmSize = textRun.EmSize,
                                        FontName = textRun.FontName,
                                        Color = textRun.Color,
                                        IsVisible = textRun.IsVisible,
                                        FontStyle = textRun.FontStyle,
                                        Text = builder.ToString()
                                    });
                                    yield return currentP;
                                    currentP = new Paragraph()
                                    {
                                        AfterParagraph = currentP.AfterParagraph,
                                        Alignment = currentP.Alignment,
                                        BeforeParagraph = currentP.BeforeParagraph,
                                        EmSize = currentP.EmSize,
                                        Color = currentP.Color,
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
                            currentP.Runs.Add(new TextRun(currentP)
                            {
                                EmSize = textRun.EmSize,
                                FontName = textRun.FontName,
                                Color = textRun.Color,
                                IsVisible = textRun.IsVisible,
                                FontStyle = textRun.FontStyle,
                                Text = builder.ToString()
                            });
                    }
                    else
                    {
                        foreach (var c in text)
                        {
                            if (c == '\n')
                            {
                                if (builder.Length > 0)
                                    currentP.Runs.Add(new TextRun(currentP)
                                    {
                                        EmSize = textRun.EmSize,
                                        Color = textRun.Color,
                                        FontName = textRun.FontName,
                                        IsVisible = textRun.IsVisible,
                                        FontStyle = textRun.FontStyle,
                                        Text = builder.ToString()
                                    });
                                currentP.Runs.Add(new LineBreakRun(currentP)
                                {
                                    EmSize = textRun.EmSize,
                                    FontName = textRun.FontName,
                                    IsVisible = textRun.IsVisible,
                                    FontStyle = textRun.FontStyle,
                                });
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
                            currentP.Runs.Add(new TextRun(currentP)
                            {
                                EmSize = textRun.EmSize,
                                FontName = textRun.FontName,
                                Color = textRun.Color,
                                IsVisible = textRun.IsVisible,
                                FontStyle = textRun.FontStyle,
                                Text = builder.ToString()
                            });

                    }

                }
                else
                {
                    throw new NotSupportedException();
                }

            }
            yield return currentP;

        }


        private void HandleTextElement(IXmlNamespaceResolver resolver, XElement context, XGraphics gfx, TextElement textElement)
        {

            var maxScale = textElement.MaxEmSizeScale.GetValue(context, resolver);
            var minScale = textElement.MinEmSizeScale.GetValue(context, resolver);
            const double MIN_CHANGE = 0.01;
            double emScale = 1;
            double? lastScale = null;
            var frame = textElement.Position.GetValue(context, resolver);


            var lastValidParagraphs = this.SimulatePrint(resolver, context, gfx, textElement, emScale);
            bool isLastValidToBig = true;

            if (!lastValidParagraphs.SelectMany(x => x.lines).Any())
                return; // if thre is nothing to print we are finished.

            XRect CalculateDimensions(List<(XLineAlignment horizontalAlignment, List<List<(string textToPrint, XFont font, XBrush brush, XPoint printPosition, XLineAlignment alignment, XSize size)>> lines)> input)
            {
                var left = input.SelectMany(x => x.lines).SelectMany(x => x).Min(x => x.printPosition.X);
                var right = input.SelectMany(x => x.lines).SelectMany(x => x).Max(x => x.printPosition.X + x.size.Width);
                var top = input.SelectMany(x => x.lines).SelectMany(x => x).Min(x => x.printPosition.Y);
                var bottom = input.SelectMany(x => x.lines).SelectMany(x => x).Max(x => x.printPosition.Y + x.size.Height);
                return new XRect(left, top, right - left, bottom - top);
            }
            do
            {
                var p = this.SimulatePrint(resolver, context, gfx, textElement, emScale);
                var dimensions = CalculateDimensions(p);
                double scaleChange;
                if (dimensions.Bottom > frame.Bottom && emScale > minScale)
                {
                    if (isLastValidToBig)
                        lastValidParagraphs = p;
                    if ((lastScale ?? double.MaxValue) > emScale)
                        scaleChange = (minScale - emScale) / 2;
                    else
                        scaleChange = (emScale - lastScale.Value) / 2;
                }
                else if (dimensions.Bottom < frame.Bottom && emScale < maxScale)
                {
                    isLastValidToBig = false;
                    lastValidParagraphs = p;
                    if ((lastScale ?? double.MinValue) < emScale)
                        scaleChange = (maxScale - emScale) / 2;
                    else
                        scaleChange = (emScale - lastScale.Value) / 2;
                }
                else
                {
                    break;
                }

                if (Math.Abs(scaleChange) < MIN_CHANGE)
                    break;
                lastScale = emScale;
                emScale += scaleChange;
            } while (true);

            var paragraphs = lastValidParagraphs;

            var maximumWidth = textElement.Position.GetValue(context, resolver).Width;
            var maximumHeight = textElement.Position.GetValue(context, resolver).Height;

            var printRect = CalculateDimensions(paragraphs);


            double verticalOffset;
            var verticalAlignment = textElement.VerticalAlignment.GetValue(context, resolver);
            switch (verticalAlignment)
            {
                case XLineAlignment.Near:
                    verticalOffset = 0;
                    break;
                case XLineAlignment.Center:
                    verticalOffset = (maximumHeight - printRect.Height) / 2;
                    break;
                case XLineAlignment.Far:
                    verticalOffset = maximumHeight - printRect.Height;
                    break;
                case XLineAlignment.BaseLine:
                    throw new NotSupportedException("Not sure what this means");
                default:
                    throw new NotSupportedException($"The Alignment {verticalAlignment} is not supported.");
            }

            foreach (var (horizontalAlignment, lines) in paragraphs)
            {



                // now we calculate LineAlignment and print 
                foreach (var line in lines.Where(x => x.Count > 0))
                {
                    var leftmost = line.Min(x => x.printPosition.X);
                    var rightmost = line.Max(x => x.printPosition.X + x.size.Width);
                    var width = rightmost - leftmost;

                    double horizontalOffset;
                    switch (horizontalAlignment)
                    {
                        case XLineAlignment.Near:
                            horizontalOffset = 0;
                            break;
                        case XLineAlignment.Center:
                            horizontalOffset = (maximumWidth - width) / 2;
                            break;
                        case XLineAlignment.Far:
                            horizontalOffset = maximumWidth - width;
                            break;
                        case XLineAlignment.BaseLine:
                            throw new NotSupportedException("Not sure what this means");
                        default:
                            throw new NotSupportedException($"The Alignment {horizontalAlignment} is not supported.");
                    }
                    foreach (var print in line)
                        gfx.DrawString(print.textToPrint, print.font, print.brush, new XPoint(print.printPosition.X + horizontalOffset, print.printPosition.Y + verticalOffset), XStringFormats.TopLeft);
                }

            }

        }

        private List<(XLineAlignment horizontalAlignment, List<List<(string textToPrint, XFont font, XBrush brush, XPoint printPosition, XLineAlignment alignment, XSize size)>> lines)> SimulatePrint(IXmlNamespaceResolver resolver, XElement context, XGraphics gfx, TextElement textElement, double emScale)
        {
            var frame = textElement.Position.GetValue(context, resolver);
            var startPosition = frame.Location;
            var currentPosition = startPosition;


            var paragraphs = textElement.Paragraphs.SelectMany(x => TransformParapgraps(x, resolver, context));
            var paragraphsToDraw = new List<(XLineAlignment horizontalAlignment, List<List<(string textToPrint, XFont font, XBrush brush, XPoint printPosition, XLineAlignment alignment, XSize size)>> lines)>();
            foreach (var paragraph in paragraphs)
            {
                if (!paragraph.IsVisible.GetValue(context, resolver))
                    continue;

                var lines = new List<List<(string textToPrint, XFont font, XBrush brush, XPoint printPosition, XLineAlignment alignment, XSize size)>>();
                paragraphsToDraw.Add((paragraph.Alignment.GetValue(context, resolver), lines));
                var currentLine = new List<(string textToPrint, XFont font, XBrush brush, XPoint printPosition, XLineAlignment alignment, XSize size)>();
                currentPosition = new XPoint(currentPosition.X, currentPosition.Y + paragraph.BeforeParagraph.GetValue(context, resolver) * emScale);


                foreach (var run in paragraph.Runs)
                    if (!this.ExpandRuns(run, paragraph, gfx, frame, startPosition, ref currentPosition, lines, currentLine, resolver, context, emScale))
                        break;
                currentPosition = new XPoint(startPosition.X, currentPosition.Y + lines.Where(x => x.Count > 0).LastOrDefault()?.Max(x => x.size.Height) ?? 0 + paragraph.AfterParagraph.GetValue(context, resolver) * emScale);

            }
            return paragraphsToDraw;
        }

        private bool ExpandRuns(IChild<Run> child, Paragraph paragraph, XGraphics gfx, XRect frame, XPoint startPosition, ref XPoint currentPosition, List<List<(string textToPrint, XFont font, XBrush brush, XPoint printPosition, XLineAlignment alignment, XSize size)>> lines, List<(string textToPrint, XFont font, XBrush brush, XPoint printPosition, XLineAlignment alignment, XSize size)> currentLine, IXmlNamespaceResolver resolver, XElement context, double emScale)
        {
            if (child is ForeEach<Run> forEach)
            {
                foreach (var newContext in context.XPathSelectElements(forEach.Select, resolver))
                    foreach (var newChild in forEach.Childrean)
                        if (!this.ExpandRuns(newChild, paragraph, gfx, frame, startPosition, ref currentPosition, lines, currentLine, resolver, newContext, emScale))
                            return false;
                return true;
            }
            else if (child is Run run)
            {

                if (!run.IsVisible.GetValue(context, resolver))
                    return true;

                lines.Add(currentLine);

                var font = new XFont(run.FontName.Value.GetValue(context, resolver), run.EmSize.Value.GetValue(context, resolver) * emScale, run.FontStyle.Value.GetValue(context, resolver));
                var height = font.Height * paragraph.Linespacing.GetValue(context, resolver);

                if (run is LineBreakRun)
                {
                    currentPosition = new XPoint(startPosition.X, currentPosition.Y + height);
                }
                else if (run is TextRun textRun)
                {


                    var textForRun = textRun.Text.GetValue(context, resolver);
                    if (string.IsNullOrEmpty(textForRun))
                        return true;

                    var hypenator = new Hyphenator(new HyphenatePatternsLoader(run.Language.Value.GetValue(context, resolver)), "\u00AD");
                    textForRun = hypenator.HyphenateText(textForRun);


                    IEnumerable<(string text, XSize size, XFont font, LastSplit lastSplit)> CalculateSizes()
                    {
                        int start = 0;
                        var lastSplit = LastSplit.StartOfText;
                        bool isSurrugate = false;
                        for (int i = 0; i < textForRun.Length; i += isSurrugate ? 2 : 1)
                        {
                            isSurrugate = char.IsHighSurrogate(textForRun[i]);

                            if (textForRun[i] == ' ') // possible cut
                            {
                                var text = textForRun.Substring(start, i - start); // cant be surrogate
                                yield return (text, gfx.MeasureString(text, font), font, lastSplit);
                                lastSplit = LastSplit.Space;
                                start = i + 1;
                            }
                            else if (textForRun[i] == '\u00AD')
                            {
                                var text = textForRun.Substring(start, i - start); // cant be surrogate
                                yield return (text, gfx.MeasureString(text, font), font, lastSplit);
                                lastSplit = LastSplit.Hyphon;
                                start = i + 1;
                            }
                            else if (!font.IsCharSupported(textForRun, i))
                            {
                                var text = textForRun.Substring(start, i - start);
                                if (text != "")
                                {

                                    yield return (text, gfx.MeasureString(text, font), font, lastSplit);
                                    lastSplit = LastSplit.NoSplit;
                                }
                                start = i;

                                // now the unsupported character

                                var substituteFont = this.Project.FallbackFonts.Select(x => new XFont(x, font.Size, font.Style)).FirstOrDefault(x => x.IsCharSupported(textForRun, i));

                                if (substituteFont == null)
                                {
                                    if (this.Project.CharacterNotFound == null)
                                        throw new Exception("Character is not supported by any font.");

                                    substituteFont = new XFont(this.Project.CharacterNotFound.FamilyName, font.Size, font.Style);

                                    text = this.Project.CharacterNotFound.SubstituteCharacter;
                                    yield return (text, gfx.MeasureString(text, substituteFont), substituteFont, lastSplit);
                                    lastSplit = LastSplit.NoSplit;
                                    start = i + 1;
                                }
                                else
                                {
                                    text = textForRun.Substring(start, (isSurrugate ? 2 : 1)); // additional -1 for the current char
                                    yield return (text, gfx.MeasureString(text, substituteFont), substituteFont, lastSplit);
                                    lastSplit = LastSplit.NoSplit;
                                    start = i + (isSurrugate ? 2 : 1);
                                }
                            }
                        }
                        {
                            var text = textForRun.Substring(start); // additional -1 for the hyphon itself
                            yield return (text, gfx.MeasureString(text, font), font, lastSplit);
                        }
                    }


                    var wordSizes = CalculateSizes().Where(x => x.text.Length > 0).ToArray();
                    var spaceSize = gfx.MeasureString(" ", font);
                    var hyphonSize = gfx.MeasureString("-", font);



                    int wordsToPrint;
                    for (int i = 0; i < wordSizes.Length; i += wordsToPrint)
                    {
                        bool addHyphon = false;
                        bool addSpace = true;
                        double lineWidth = 0;
                        bool endOfLine = false;
                        for (wordsToPrint = 0; wordsToPrint + i < wordSizes.Length; wordsToPrint++)
                        {
                            var w = wordSizes[i + wordsToPrint];
                            var nextW = (i + wordsToPrint + 1 < wordSizes.Length) ? new (string text, XSize size, XFont font, LastSplit lastSplit)?(wordSizes[i + wordsToPrint + 1]) : null;

                            if (wordsToPrint == 0)
                                addSpace = w.lastSplit == LastSplit.Space;


                            // check if line is to short and we need a line break
                            if (w.size.Width + currentPosition.X + lineWidth > frame.Right && i + wordsToPrint != 0 /*we can't make a linebreka before the first word*/)
                            {
                                var currentW = w;
                                var previousW = (i + wordsToPrint - 1 > 0) ? new (string text, XSize size, XFont font, LastSplit lastSplit)?(wordSizes[i + wordsToPrint - 1]) : null;
                                // we are over the bounding. set current Position to next line
                                while (currentW.lastSplit == LastSplit.Hyphon && currentPosition.X + lineWidth + hyphonSize.Width > frame.Right && previousW != null) // the hyphon alos is to long, so one part lesss
                                {
                                    wordsToPrint--;
                                    lineWidth -= previousW.Value.size.Width;
                                    if (previousW.Value.lastSplit == LastSplit.Space && lineWidth != previousW.Value.size.Width /* the beginning of a line*/)
                                        lineWidth -= spaceSize.Width;

                                    currentW = previousW.Value;
                                    previousW = (i + wordsToPrint - 1 > 0) ? new (string text, XSize size, XFont font, LastSplit lastSplit)?(wordSizes[i + wordsToPrint - 1]) : null;
                                }
                                if (currentW.lastSplit == LastSplit.Hyphon)
                                    addHyphon = true;
                                endOfLine = true;
                                break;
                            }
                            lineWidth += w.size.Width;
                            if (w.lastSplit == LastSplit.Space && lineWidth != 0 /* the beginning of a line*/)
                                lineWidth += spaceSize.Width;

                            //check if font has changed and we need to make an additional draw call
                            if (nextW != null && w.font != nextW?.font && w.size.Width + nextW.Value.size.Width + currentPosition.X + lineWidth <= frame.Right)
                            {
                                wordsToPrint++;
                                //addSpace = false;
                                break;
                            }
                        }
                        wordsToPrint = Math.Max(wordsToPrint, 1); // we want to print at least one word other wise we will not consume it and will print nothing agiain


                        var printFont = wordSizes.Skip(i).Take(wordsToPrint).First().font;

                        var textToPrint = wordSizes.Skip(i).Take(wordsToPrint).Aggregate("", (previousText, x2) =>
                         {
                             switch (x2.lastSplit)
                             {
                                 case LastSplit.NoSplit:
                                 case LastSplit.StartOfText:
                                 case LastSplit.Hyphon:
                                     return previousText + x2.text;
                                 case LastSplit.Space:
                                     if (previousText != "")
                                         return previousText + " " + x2.text;
                                     return x2.text;
                                 default:
                                     throw new NotSupportedException();
                             }
                         });
                        if (addHyphon)
                            textToPrint += "-";

                        var printSize = gfx.MeasureString(textToPrint, printFont);
                        var brush = new XSolidBrush(run.Color.Value.GetValue(context, resolver));
                        currentLine.Add((textToPrint, printFont, brush, currentPosition, paragraph.Alignment.GetValue(context, resolver), printSize));
                        currentPosition = new XPoint(currentPosition.X + (addSpace ? spaceSize.Width : 0) + printSize.Width, currentPosition.Y);

                        if (endOfLine)
                        {
                            currentPosition = new XPoint(startPosition.X, currentPosition.Y + height);
                            currentLine = new List<(string textToPrint, XFont font, XBrush brush, XPoint currentPosition, XLineAlignment alignment, XSize size)>();
                            lines.Add(currentLine);
                        }

                        // we nolonger stop here.

                        //if (wordSizes.Skip(i).Take(wordsToPrint).Max(x => x.size.Height) + currentPosition.Y > frame.Bottom)
                        //    return false; // reached end of box


                    }
                }
                return true;
            }
            else
                throw new NotSupportedException();
        }


        private enum LastSplit
        {
            NoSplit,
            StartOfText,
            Space,
            Hyphon,
        }
    }


}
