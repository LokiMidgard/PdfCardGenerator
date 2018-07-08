using Fclp;
using System;
using System.Xml.Linq;

namespace PDFGenerator.Client
{
    class Program
    {
        public class ApplicationArguments
        {
            public string ProjectPath { get; set; }
            public string DataPath { get; set; }
            public string OutputPath { get; set; }
            public bool Override { get; set; }
        }


        static void Main(string[] args)
        {

            var p = new FluentCommandLineParser<ApplicationArguments>();

            p.Setup(arg => arg.ProjectPath)
                .As('p', "ProjectPath")
                .Required()
                .WithDescription("The Path to the Project File");

            p.Setup(arg => arg.DataPath)
                .As('i', "InputPath")
                .Required()
                .WithDescription("The Path to the input xml file");

            p.Setup(arg => arg.OutputPath)
                .As('o', "OutputPath")
                .Required()
                .WithDescription("The Path to the Generated PDF");

            p.Setup(arg => arg.OutputPath)
                .As('f', "Force")
                .WithDescription("Overrides an already existing PDF");

            var result = p.Parse(args);


            if (result.HelpCalled || result.HasErrors)
            {
                if (result.HasErrors)
                    Console.WriteLine(result.ErrorText);
                p.SetupHelp("?", "help")
                    .Callback(text => Console.WriteLine(text));
                p.HelpOption.ShowHelp(p.Options);
            }
            else
            {
                var options = p.Object;

                using (var projectStream = System.IO.File.OpenRead(options.ProjectPath))
                using (var xmlStream = System.IO.File.OpenRead(options.DataPath))
                {
                    var project = PdfGenerator.Project.Load(projectStream);

                    var xml = XDocument.Load(xmlStream);

                    var pdf = project.GetDocuments(xml);
                    pdf.Save(options.OutputPath);

                }
            }
        }
    }
}
