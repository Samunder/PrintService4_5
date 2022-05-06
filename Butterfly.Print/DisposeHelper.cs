namespace Butterfly.Print
{
    using System;
    using System.Collections.Generic;

    using Butterfly.Print.Interfaces;
    using Butterfly.Print.PageObjects;

    public class DisposeHelper
    {
        public static void DisposePageObjects(List<PageObject> pageObjects, ILogService log)
        {
            foreach (var pageObject in pageObjects)
            {
                try
                {
                    (pageObject as IDisposable)
                        ?.Dispose();
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                }
            }
        }

        public static void DisposePages(List<Page> pages, ILogService log = null)
        {
            foreach (var page in pages)
            {
                try
                {
                    (page as IDisposable)
                        ?.Dispose();
                }
                catch (Exception exception)
                {
                    log?.Error(exception);
                }
            }
        }
    }
}