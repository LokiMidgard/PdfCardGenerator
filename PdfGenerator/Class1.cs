using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Drawing;
using System.Xml.Linq;

namespace PdfGenerator
{
    public class Class1
    {
        public static void Test()
        {

            var template = new PageTemplate()
            {
                Height = new XUnit(88, XGraphicsUnit.Millimeter),
                Width = new XUnit(60, XGraphicsUnit.Millimeter)
            };

            var textelement = new TextElement()
            {
                Position = new RectangleF(
                    x: (float)new XUnit(3, XGraphicsUnit.Millimeter).Point,
                    y: (float)new XUnit(3, XGraphicsUnit.Millimeter).Point,
                    width: (float)new XUnit(54, XGraphicsUnit.Millimeter).Point,
                    height: (float)new XUnit(82, XGraphicsUnit.Millimeter).Point),
                Paragraphs = new[] 
                {
                    new ParaGraph()
                    {
                        Runs = new[]
                        {
                            new TextRun()
                            {
                                Text = "Hallo Welt un allen toll fadf asdfasfasdfasfdasfdaasdf adf asdfa",
                                EmSize = 20,
                                FontName = "Verdana",
                                FontStyle = XFontStyle.BoldItalic,
                                IsVisible = true
                            }
                        }
                    },
                }
            };

            template.Elements.Add(textelement);

            var document = template.GetDocuments(new[] { new XElement("Test") });
            document.Info.Title = "Created with PDFsharp";

            //// Create an empty page
            //PdfPage page = document.AddPage();


            //// Get an XGraphics object for drawing
            //XGraphics gfx = XGraphics.FromPdfPage(page);

            //// Create a font
            //XFont font = new XFont("Verdana", 20, XFontStyle.BoldItalic);

            //// Draw the text
            //gfx.DrawString("Hello, World!", font, XBrushes.Black,
            //  new XRect(0, 0, page.Width, page.Height),
            //  XStringFormats.Center);

            // Save the document...
            const string filename = "HelloWorld.pdf";
            document.Save(filename);
            // ...and start a viewer.
            System.Diagnostics.Process.Start(filename);
        }
    }
}
