namespace Butterfly.Print.PageObjects
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    using C1.C1Pdf;

    public class Page: IDisposable
    {
        private bool disposed = false;

        public Page()
        {
            PageObjects = new List<PageObject>();

            this.PageSize = 9;
            this.Orientation = "Portrait";
            this.Width = 2100;
            this.Height = 2970;
            this.PaperSource = 15;
            this.IsStatic = true;
        }

        public List<PageObject> PageObjects { get; set; }

        public int Height { get; set; }

        public bool IsStatic { get; set; }

        public string Orientation { get; set; }

        public int PageSize { get; set; }

        public int PaperSource { get; set; }

        public double PageScale { get; set; }

        public int Width { get; set; }

        public void DrawPage(Graphics gfx)
        {
            foreach (PageObject pageObject in PageObjects)
            {
                pageObject.Draw(gfx);
            }
        }

        public void DrawPage(C1PdfDocument docPdf)
        {
            foreach (PageObject pageObject in PageObjects)
            {
                pageObject.Draw(docPdf);
            }
        }

        public void Offset(int xOffset, int yOffset)
        {
            foreach (PageObject pageObject in PageObjects)
            {
                pageObject.Offset(xOffset, yOffset, true);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (PageObjects != null)
                {
                    foreach (var pageObject in PageObjects)
                    {
                        (pageObject as IDisposable)
                            ?.Dispose();
                    }
                }
            }

            disposed = true;
        }
    }
}