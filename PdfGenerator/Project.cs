﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Serilizer;

namespace PdfGenerator
{
    public class Project
    {

        public IEnumerable<PageTemplate> Templates { get; set; }

        public static Project Load(System.IO.Stream stream)
        {
            var serializer = new XmlSerializer(typeof(Serilizer.Project));
            var deserilisation = (Serilizer.Project)serializer.Deserialize(stream);

            var templates = deserilisation.Template.Select(original =>
            {
                return new PageTemplate
                {
                    ContextPath = original.Context,
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
                                Paragraphs = textElement.Paragraph.Select(p =>
                                {

                                    var result = new Paragraph
                                    {
                                        AfterParagraph = (XUnit)p.AfterParagraph,
                                        BeforeParagraph = (XUnit)p.BeforeParagraph,
                                        Alignment = p.AlignmentSpecified ? TransformAlignment(p.Alignment) : XLineAlignment.Near,
                                        EmSize = p.EmSize,
                                        IsVisible = GetVisible(p),
                                        Linespacing = p.Linespacing,
                                        FontName = p.FontName,
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
                                                text: textRun.ItemElementName == ItemChoiceType.Text ? textRun.Item : new XPath(textRun.Item));
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
                                ImagePath = GetImageLocation()
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

            });
            return new Project() { Templates = templates };
        }

        public PdfDocument GetDocuments(XDocument xml)
        {
            var document = new PdfDocument
            {
                PageLayout = PdfPageLayout.SinglePage
            };

            foreach (var item in this.Templates)
                foreach (var page in item.GetDocuments(xml).Pages)
                    document.AddPage(page);

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
