using System.Collections.Generic;

namespace PdfCardGenerator.Elements
{
    internal class ForeEach<T> : IChild<T>
    {
        public string Select { get; set; }

        public IList<IChild<T>> Childrean { get; set; }

        public ContextValue<bool> IsVisible { get; set; } = true;

    }

}