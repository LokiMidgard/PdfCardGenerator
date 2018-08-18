using PdfCardGenerator;
using PdfSharp.Fonts;
using Serilizer;
using System;
using System.IO;

namespace PdfCardGenerator
{
    internal class FontResolver : IFontResolver
    {

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, (FontResolverInfo resolver, byte[] data)> fontFaceLookup = new System.Collections.Concurrent.ConcurrentDictionary<string, (FontResolverInfo facename, byte[] data)>();

        private readonly bool systemFontsFallback;

        public FontResolver()
        {

        }

        public FontResolver(bool systemFontsFallback)
        {
            this.systemFontsFallback = systemFontsFallback;
        }

        public void AddFont(string familyName, BoldStyle boldStyle, ItalicStyle italicStyle, Stream stream)
        {
            var faceName = familyName.ToLower()
                + (boldStyle != BoldStyle.None ? "|b" : "")
                + (italicStyle != ItalicStyle.None ? "|i" : "");

            var resolver = new FontResolverInfo(faceName, boldStyle == BoldStyle.Simulate, italicStyle == ItalicStyle.Simulate);
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                var data = memoryStream.ToArray();
                if (!this.fontFaceLookup.TryAdd(faceName, (resolver, data)))
                    throw new ArgumentException($"FontFace <{faceName}> already used");
            }
        }

        public byte[] GetFont(string faceName)
        {
            if (this.fontFaceLookup.TryGetValue(faceName, out var item))
                return item.data;
            return null;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string faceName = familyName.ToLower() +
                (isBold ? "|b" : "") +
                (isItalic ? "|i" : "");
            if (this.fontFaceLookup.TryGetValue(faceName, out var item))
                return item.resolver;
            if (this.systemFontsFallback)
                return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
            return null;
        }

        public enum BoldStyle
        {
            None,
            Applyed,
            Simulate,
        }

        public enum ItalicStyle
        {
            None,
            Applyed,
            Simulate
        }

    }

}

