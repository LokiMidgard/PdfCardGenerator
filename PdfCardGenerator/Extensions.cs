using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PdfCardGenerator
{
    public static class Extensions
    {
        public static IDisposable RotateTransform(this XGraphics gfx, PdfGenerator.Element element, XElement context, IXmlNamespaceResolver resolver)
        {
            //rotation = Math.PI * 2 * rotation / 360.0;
            var state = gfx.SaveState();
            var rotation = element.Rotation.GetValue(context, resolver);
            var origin = element.RotationOrigin.GetValue(context, resolver);

            var frame = element.Position.GetValue(context, resolver);

            origin = new XPoint(origin.X * frame.Width + frame.X, origin.Y * frame.Height + frame.Y);

            gfx.RotateAtTransform(rotation, origin);
            return state;
        }

        public static IDisposable SaveState(this XGraphics gfx)
        {
            var state = gfx.Save();
            return new DisposeDeleagte(() => gfx.Restore(state));
        }

        private class DisposeDeleagte : IDisposable
        {
            private readonly Action onDispose;

            public DisposeDeleagte(Action onDispose)
            {
                this.onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
            }


            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls


            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                        onDispose();

                    disposedValue = true;
                }
            }


            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
            }
            #endregion

        }
    }
}
