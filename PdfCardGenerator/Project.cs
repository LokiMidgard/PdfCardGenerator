using PdfCardGenerator.Elements;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using Alignment = PdfCardGenerator.Serilizer.Alignment;
using FontStyle = PdfCardGenerator.Serilizer.FontStyle;

namespace PdfCardGenerator
{
    public class Project
    {

        internal IEnumerable<PageTemplate> Templates { get; set; }
        public XDocument Document { get; private set; }

        internal IFontResolver FontResolver { get; set; }
        internal Serilizer.Language DefaultLanguage { get; set; }
        internal string[] FallbackFonts { get; set; }
        internal Serilizer.ProjectFallbackFontsNotFoundCharacter CharacterNotFound { get; set; }
        public AbstractFileProvider FileProvider { get; private set; }

        public static Project Load(string path)
        {
            var file = new FileInfo(path);
            using (var stream = file.OpenRead())
                return Load(stream, new WorkingDirectoryFileProvider(file.Directory));
        }
        public static Project Load(System.IO.Stream stream, AbstractFileProvider fileProvider)
        {

            var doc = XDocument.Load(stream);

            Serilizer.Project deserilisation;
            var serializer = new XmlSerializer(typeof(Serilizer.Project));
            using (var reader = doc.CreateReader())
                deserilisation = (Serilizer.Project)serializer.Deserialize(reader);

            var templates = deserilisation.Templates.Select(original =>
            {
                return new PageTemplate
                {
                    XSLT = original.Xslt != null ? (RelativePath?)(new RelativePath() { Path = original.Xslt, WorkingDirectory = fileProvider }) : null,
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
                                MinEmSizeScale = textElement.MinEmSizeScale,
                                MaxEmSizeScale = textElement.MaxEmSizeScale,
                                VerticalAlignment = TransformAlignment(textElement.VerticalAlignment),
                                Rotation = textElement.rotation,
                                RotationOrigin = new XPoint(textElement.rotationOriginX, textElement.rotationOriginY),
                                ZIndex = textElement.ZPosition,
                                Paragraphs = textElement.Items.Select(child =>
                                {
                                    if (child is Serilizer.ForeEachParagraph foreEach)
                                        return GetParagraps(foreEach);
                                    else if (child is Serilizer.Paragraph p)
                                        return GetParagrap(p);
                                    else
                                        throw new NotImplementedException($"Type {child?.GetType()} is not supporteted");
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
                                Rotation = imageElement.rotation,
                                RotationOrigin = new XPoint(imageElement.rotationOriginX, imageElement.rotationOriginY),
                                ImagePath = new RelativePath()
                                {
                                    Path = GetImageLocation(),
                                    WorkingDirectory = fileProvider
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
                        else if (x is Serilizer.RectElement rectElement)
                        {
                            var frame = GetPosition(rectElement);
                            XBrush brush;
                            XPen pen;

                            if (rectElement.Fill == null)
                            {
                                brush = null;
                            }
                            else
                            {
                                if (rectElement.Fill.Item is Serilizer.SolidFill solid)
                                {
                                    var color = GetColorFromString(solid.Color);

                                    brush = new XSolidBrush(color);
                                }
                                else if (rectElement.Fill.Item is Serilizer.GradientFill gradiant)
                                {
                                    var color1 = GetColorFromString(gradiant.Color1);
                                    var color2 = GetColorFromString(gradiant.Color2);

                                    var point1 = new XPoint(frame.Left + gradiant.Point[0].X * frame.Width, frame.Top + gradiant.Point[0].Y * frame.Height);
                                    var point2 = new XPoint(frame.Left + gradiant.Point[1].X * frame.Width, frame.Top + gradiant.Point[1].Y * frame.Height);

                                    brush = new XLinearGradientBrush(point1, point2, color1, color2);
                                    //brush = new XLinearGradientBrush(frame, color1, color2, XLinearGradientMode.Vertical);

                                }
                                else throw new NotSupportedException($"The Type {rectElement.Fill.Item} is not Supported.");
                            }

                            if (rectElement.Border == null)
                            {
                                pen = null;
                            }
                            else
                            {
                                var color = GetColorFromString(rectElement.Border.Color);
                                pen = new XPen(color);
                            }
                            var result = new RectElement
                            {
                                IsVisible = GetVisible(rectElement),
                                Position = frame,
                                ZIndex = rectElement.ZPosition,
                                Rotation = rectElement.rotation,
                                RotationOrigin = new XPoint(rectElement.rotationOriginX, rectElement.rotationOriginY),
                                FillColor = brush,
                                BorderColor = pen

                            };



                            return result;
                        }
                        else
                            throw new NotSupportedException();
                    }).ToArray()
                };

            }).ToArray();
            var fontResolver = new FontResolver(deserilisation.Fonts.UseSystemFallback);

            foreach (var f in deserilisation.Fonts.Font)
            {
              
                using (var fontStream = fileProvider.Open(f.Regular.Path))
                    fontResolver.AddFont(
                        familyName: f.FamilyName,
                        boldStyle: PdfCardGenerator.FontResolver.BoldStyle.None,
                        italicStyle: PdfCardGenerator.FontResolver.ItalicStyle.None,
                        stream: fontStream);

                using (var fontStream = fileProvider.Open(f.Italic?.Path ?? f.Regular.Path))
                    fontResolver.AddFont(
                        familyName: f.FamilyName,
                        boldStyle: PdfCardGenerator.FontResolver.BoldStyle.None,
                        italicStyle: f.Italic != null ? PdfCardGenerator.FontResolver.ItalicStyle.Applyed : PdfCardGenerator.FontResolver.ItalicStyle.Simulate,
                        stream: fontStream);

                using (var fontStream = fileProvider.Open(f.Bold?.Path ?? f.Regular.Path))
                    fontResolver.AddFont(
                        familyName: f.FamilyName,
                        boldStyle: f.Bold != null ? PdfCardGenerator.FontResolver.BoldStyle.Applyed : PdfCardGenerator.FontResolver.BoldStyle.Simulate,
                        italicStyle: PdfCardGenerator.FontResolver.ItalicStyle.None,
                        stream: fontStream);

                using (var fontStream = fileProvider.Open(f.BoldItalic?.Path ?? f.Regular.Path))
                    fontResolver.AddFont(
                        familyName: f.FamilyName,
                        boldStyle: f.Bold != null ? PdfCardGenerator.FontResolver.BoldStyle.Applyed : PdfCardGenerator.FontResolver.BoldStyle.Simulate,
                        italicStyle: f.Italic != null ? PdfCardGenerator.FontResolver.ItalicStyle.Applyed : PdfCardGenerator.FontResolver.ItalicStyle.Simulate,
                        stream: fontStream);

            }

            var project = new Project()
            {
                CharacterNotFound = deserilisation.FallbackFonts.NotFoundCharacter,
                FallbackFonts = deserilisation.FallbackFonts.Font.Select(x => x.FamilyName).ToArray(),
                DefaultLanguage = deserilisation.DefaultLanguage,
                FontResolver = fontResolver,
                Templates = templates,
                FileProvider = fileProvider,
                Document = doc
            };
            foreach (var tempalte in project.Templates)
                tempalte.Project = project;
            return project;



            IChild<Paragraph> GetParagraps(Serilizer.ForeEachParagraph p)
            {
                return new ForeEach<Paragraph>()
                {
                    IsVisible = GetVisible(p),
                    Select = p.Select,
                    Childrean = p.Items.Select(x =>
                    {
                        if (x is Serilizer.ForeEachParagraph newForEach)
                            return GetParagraps(newForEach);
                        else if (x is Serilizer.Paragraph newP)
                            return GetParagrap(newP);
                        else
                            throw new NotSupportedException();
                    }).ToArray()
                };

            }

            IChild<Paragraph> GetParagrap(Serilizer.Paragraph p)
            {
                var result = new Paragraph
                {
                    AfterParagraph = p.AfterParagraph ?? XUnit.Zero,
                    BeforeParagraph = p.BeforeParagraph ?? XUnit.Zero,
                    Alignment = TransformAlignment(p.Alignment),
                    EmSize = p.EmSize,
                    IsVisible = GetVisible(p),
                    Color = GetColorFromString(p.Color),
                    Linespacing = p.Linespacing,
                    Language = p.LanguageSpecified ? p.Language : deserilisation.DefaultLanguage,
                    FontName = p.FontName ?? deserilisation.Fonts.DefaultFont,
                    FontStyle = TransformFontStyle(p.FontStyle)
                };

                IChild<Run> GetRun(Serilizer.Run run)
                {
                    if (run is Serilizer.LineBreak lineBreak)
                    {
                        return new LineBreakRun(result)
                        {

                            FontStyle = lineBreak.FontStyleSpecified ? (ContextValue<XFontStyle>?)ContextValue<XFontStyle>.FromValue(TransformFontStyle(lineBreak.FontStyle)) : null,
                            EmSize = lineBreak.EmSizeSpecified ? (ContextValue<double>?)ContextValue<double>.FromValue(lineBreak.EmSize) : null,
                            FontName = lineBreak.FontName,
                            IsVisible = GetVisible(lineBreak)
                        };
                    }
                    else if (run is Serilizer.TextRun textRun)
                    {
                        return new TextRun(result)
                        {
                            FontStyle = textRun.FontStyleSpecified ? (ContextValue<XFontStyle>?)ContextValue<XFontStyle>.FromValue(TransformFontStyle(textRun.FontStyle)) : null,
                            EmSize = textRun.EmSizeSpecified ? (ContextValue<double>?)ContextValue<double>.FromValue(textRun.EmSize) : null,
                            Language = textRun.LanguageSpecified ? (ContextValue<Serilizer.Language>?)ContextValue<Serilizer.Language>.FromValue(textRun.Language) : null,
                            FontName = textRun.FontName,
                            Color = textRun.Color == null ? (XColor?)null : GetColorFromString(textRun.Color),
                            IsVisible = GetVisible(textRun),
                            Text = textRun.ItemElementName == Serilizer.ItemChoiceType.Text ? textRun.Item : ContextValue<string>.FromXPath(textRun.Item)
                        };
                    }
                    else if (run is Serilizer.ForEachRun forEach)
                    {
                        return new ForeEach<Run>()
                        {
                            IsVisible = GetVisible(forEach),
                            Select = forEach.Select,
                            Childrean = forEach.Items.Select(GetRun).ToArray()
                        };
                    }
                    else
                        throw new NotSupportedException($"THe type {run?.GetType()} is not supported");
                }
                result.Runs = p.Items.Select(GetRun).ToArray();
                return result;
            }

        }

        private static XColor GetColorFromString(string colorString)
        {
            if (colorString == null)
                throw new ArgumentNullException(nameof(colorString));
            if (colorString.StartsWith("#"))
            {

                var a = int.Parse(colorString.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                var r = int.Parse(colorString.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                var g = int.Parse(colorString.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                var b = int.Parse(colorString.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                return XColor.FromArgb(a, r, g, b);
            }
            else
            {
                return XColor.FromName(colorString);
            }
        }

        public PdfDocument GetDocuments(XDocument xml)
        {
            var document = new PdfDocument
            {
                PageLayout = PdfPageLayout.SinglePage
            };
            var navigator = this.Document.Root.CreateNavigator();
            GlobalFontSettings.FontResolver = this.FontResolver;


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
                return imageElement.IsVisible;
        }
    }

}
