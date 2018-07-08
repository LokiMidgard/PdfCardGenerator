using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Drawing;
using System.Xml.Linq;
using System.Linq;
using System.Xml.XPath;

namespace PdfGenerator
{
    public class Class1
    {
        public static void Test()
        {

            const string xml = @"<Elements>
    <Element Header='Test 1'>
        Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.
    </Element>
    <Element Header='Test 2'>
        Dies ist ein zweiter test
    </Element>
</Elements>";

            var doc = XDocument.Parse(xml);

            var template = new PageTemplate()
            {
                Height = new XUnit(88, XGraphicsUnit.Millimeter),
                Width = new XUnit(60, XGraphicsUnit.Millimeter)
            };
            var header = new ParaGraph()
            {
                EmSize = 20,
                FontStyle = XFontStyle.Bold,
                Alignment = XLineAlignment.Center,
                AfterParagraph = new XUnit(4, XGraphicsUnit.Millimeter)
            };
            header.AddRun(new XPath("@Header"));
            var body = new ParaGraph()
            {
                FontStyle = XFontStyle.Italic
            };

            body.AddRun(new XPath("normalize-space(text())"));


            var textelement = new TextElement()
            {
                Position = new XRect(
                    x: new XUnit(3, XGraphicsUnit.Millimeter).Point,
                    y: new XUnit(3, XGraphicsUnit.Millimeter).Point,
                    width: new XUnit(54, XGraphicsUnit.Millimeter).Point,
                    height: new XUnit(82, XGraphicsUnit.Millimeter).Point),
                Paragraphs = new[]
                {
                    header,
                    body
                }
            };

            template.Elements.Add(textelement);

            
            var document = template.GetDocuments(doc.XPathSelectElements("//Elements/Element"));
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
            //System.Diagnostics.Process.Start(filename);
        }
    }
}
