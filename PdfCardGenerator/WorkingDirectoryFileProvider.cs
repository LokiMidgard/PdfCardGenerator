using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfCardGenerator
{
    public class WorkingDirectoryFileProvider : AbstractFileProvider
    {
        private readonly DirectoryInfo workingDirectory;

        public WorkingDirectoryFileProvider(DirectoryInfo workingDirectory)
        {
            this.workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
        }

        public override Stream Open(string path)
        {
            if (Path.IsPathRooted(path))
                return File.Open(path, FileMode.Open);

            return File.Open(System.IO.Path.Combine(this.workingDirectory.FullName, path), FileMode.Open);

        }
    }
}
