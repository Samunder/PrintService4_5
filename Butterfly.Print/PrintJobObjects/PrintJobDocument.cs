namespace Butterfly.Print.PrintJobObjects
{
    using System;
    using System.Collections.Generic;
    using Butterfly.Print.PageObjects;

    [Serializable]
    public class PrintJobDocument : IDisposable
    {
        private bool disposed = false;

        public PrintJobDocument()
        {
            PrintJobLayouts = new List<PrintJobLayout>();
            Pages = new List<Page>();

            Copies = 1;
            PaperSource = -1;
            XOffset = 0;
            YOffset = 0;
        }

        public List<PrintJobLayout> PrintJobLayouts { get; set; }

        public List<Page> Pages { get; set; }

        public int Copies { get; set; }

        public int PaperSource { get; set; }

        public int XOffset { get; set; }

        public int YOffset { get; set; }

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
                if (Pages != null)
                {
                    DisposeHelper.DisposePages(Pages);
                }
            }

            disposed = true;
        }
    }
}