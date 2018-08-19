using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfCardGenerator
{
    public abstract class AbstractFileProvider
    {

        public abstract Stream Open(string path);

    }
}
