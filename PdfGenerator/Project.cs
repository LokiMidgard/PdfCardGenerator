using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Serialization;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Serilizer;
using System.Xml;
using PdfSharp.Pdf.IO;
using System.IO;

namespace PdfGenerator
{
    public class Project
    {

        public IEnumerable<PageTemplate> Templates { get; set; }
        public XDocument Document { get; private set; }


        public static Project Load(string path)
        {
            var file = new FileInfo(path);
            using (var stream = file.OpenRead())
                return Load(stream, file.Directory);
        }
        public static Project Load(System.IO.Stream stream, DirectoryInfo workingDirectory)
        {

            var doc = XDocument.Load(stream);

            Serilizer.Project deserilisation;
            var serializer = new XmlSerializer(typeof(Serilizer.Project));
            using (var reader = doc.CreateReader())
                deserilisation = (Serilizer.Project)serializer.Deserialize(reader);

            var templates = deserilisation.Template.Select(original =>
            {
                return new PageTemplate
                {
                    XSLT = original.Xslt != null ? (RelativePath?)(new RelativePath() { Path = original.Xslt, WorkingDirectory = workingDirectory } ): null,
                    ContextPath = (XPath)original.Context,
                    Height = original.Height,
                    Width = original.Width,
                    Elements = original.Items.Select<object, Element>(x =>
                    {
                        if (x is Serilizer.TextElement textElement)
                        {
                            return new TextElement
                            {
                                IsVisible = GetVisible(textElement),
                                Position = GetPosition(textElement),
                                ZIndex = textElement.ZPosition,
                                Paragraphs = textElement.Items.Select(p =>
                                {

                                    var result = new Paragraph
                                    {
                                        AfterParagraph = (XUnit)(p.AfterParagraph ?? XUnit.Zero),
                                        BeforeParagraph = (XUnit)(p.BeforeParagraph ?? XUnit.Zero),
                                        Alignment = p.AlignmentSpecified ? TransformAlignment(p.Alignment) : XLineAlignment.Near,
                                        EmSize = p.EmSizeSpecified ? p.EmSize : Paragraph.DEFAULT_EM_SIZE,
                                        IsVisible = GetVisible(p),
                                        Linespacing = p.Linespacing,
                                        FontName = p.FontName ?? Paragraph.DEFAULT_FONT_NAME,
                                        FontStyle = TransformFontStyle(p.FontStyle)
                                    };

                                    foreach (var run in p.Items)
                                    {
                                        if (run is LineBreak lineBreak)
                                        {
                                            result.AddLineBreak(
                                                fontStyle: lineBreak.FontStyleSpecified ? (ContextValue<XFontStyle>?)ContextValue<XFontStyle>.FromValue(TransformFontStyle(lineBreak.FontStyle)) : null,
                                                emSize: lineBreak.EmSizeSpecified ? (ContextValue<double>?)ContextValue<double>.FromValue(lineBreak.EmSize) : null,
                                                fontName: lineBreak.FontName,
                                                isVisible: GetVisible(lineBreak));
                                        }
                                        else if (run is Serilizer.TextRun textRun)
                                        {
                                            result.AddRun(
                                                fontStyle: textRun.FontStyleSpecified ? (ContextValue<XFontStyle>?)ContextValue<XFontStyle>.FromValue(TransformFontStyle(textRun.FontStyle)) : null,
                                                emSize: textRun.EmSizeSpecified ? (ContextValue<double>?)ContextValue<double>.FromValue(textRun.EmSize) : null,
                                                fontName: textRun.FontName,
                                                isVisible: GetVisible(textRun),
                                                text: textRun.ItemElementName == ItemChoiceType.Text ? textRun.Item : ContextValue<string>.FromXPath(textRun.Item));
                                        }
                                    }

                                    return result;

                                }).ToArray()
                            };




                        }
                        else if (x is Serilizer.ImageElement imageElement)
                        {
                            var result = new ImageElement
                            {
                                IsVisible = GetVisible(imageElement),
                                Position = GetPosition(imageElement),
                                ZIndex = imageElement.ZPosition,
                                ImagePath = new RelativePath()
                                {
                                    Path = GetImageLocation(),
                                    WorkingDirectory = workingDirectory
                                }
                            };

                            ContextValue<string> GetImageLocation()
                            {
                                if (imageElement.ImageLocationPath != null)
                                    return new XPath(imageElement.ImageLocationPath);
                                else
                                    return imageElement.ImageLocation;
                            }

                            return result;
                        }
                        else
                            throw new NotSupportedException();
                    }).ToArray()
                };

            }).ToArray();
            return new Project()
            {
                Templates = templates,
                Document = doc
            };
        }

        public PdfDocument GetDocuments(XDocument xml)
        {
            var document = new PdfDocument
            {
                PageLayout = PdfPageLayout.SinglePage
            };
            var navigator = this.Document.Root.CreateNavigator();

            foreach (var item in this.Templates)
            {
                using (var doc2 = item.GetDocuments(xml, navigator))
                using (var memory = new MemoryStream())
                {
                    doc2.Save(memory);
                    memory.Seek(0, SeekOrigin.Begin);
                    using (var doc3 = PdfReader.Open(memory, PdfDocumentOpenMode.Import))
                        foreach (var page in doc3.Pages)
                            document.AddPage(page);
                }

            }
            return document;
        }

        private static XFontStyle TransformFontStyle(FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case FontStyle.Regular:
                    return XFontStyle.Regular;
                case FontStyle.Bold:
                    return XFontStyle.Bold;
                case FontStyle.Italic:
                    return XFontStyle.Italic;
                case FontStyle.BoldItalic:
                    return XFontStyle.BoldItalic;
                case FontStyle.Underline:
                    return XFontStyle.Underline;
                case FontStyle.Strikeout:
                    return XFontStyle.Strikeout;
                default:
                    throw new NotSupportedException($"The Value {fontStyle} is not supported");

            }
        }

        private static XLineAlignment TransformAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Near:
                    return XLineAlignment.Near;
                case Alignment.Center:
                    return XLineAlignment.Center;
                case Alignment.Far:
                    return XLineAlignment.Far;
                default:
                    throw new NotSupportedException($"The Value {alignment} is not supported");
            }
        }

        private static XRect GetPosition(Serilizer.IHavePosition imageElement)
        {
            return new PdfSharp.Drawing.XRect(GetPointFromUnitString(imageElement.left), GetPointFromUnitString(imageElement.top), GetPointFromUnitString(imageElement.width), GetPointFromUnitString(imageElement.height));
            double GetPointFromUnitString(string left) => ((XUnit)left).Point;
        }


        private static ContextValue<bool> GetVisible(Serilizer.IVisible imageElement)
        {
            if (imageElement.IsVisiblePath != null)
                return new XPath(imageElement.IsVisiblePath);
            else
                return imageElement.IsVisibleSpecified ? imageElement.IsVisible : true;
        }
    }
}
