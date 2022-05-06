namespace Butterfly.Print
{
    using System;
    using System.Collections.Generic;

    using DocFormObjects;
    using Interfaces;
    using Objects;

    public class LayoutCache
    {
        private Dictionary<string, Layout> cachedLayouts = new Dictionary<string, Layout>();
        private ILogService logService;

        public LayoutCache(ILogService logService)
        {
            this.logService = logService;
        }

        public bool isLayoutCachingOn { get; set; }


        internal DocFormLayout GetLayout(string layoutName)
        {
            Layout layout = null;

            try
            {
               this.logService.Info("Print.LayoutCache.GetLayout - Entering GetLayout().");
               this.logService.Info("Print.LayoutCache.GetLayout - layoutName=" + layoutName);

                // Do we cache
                if (isLayoutCachingOn)
                {
                    //Check if in cache
                    if (cachedLayouts.ContainsKey(layoutName))
                    {
                        // Get from cache
                       this.logService.Info("Print.LayoutCache.GetLayout - Found in cache");
                        layout = cachedLayouts[layoutName];
                    }
                    else
                    {
                        // Not in cache
                       this.logService.Info("Print.LayoutCache.GetLayout - Not found in cache");
                    }
                }

                if (layout == null)
                {
                    // Load Layout
                   this.logService.Info("Print.LayoutCache.GetLayout - Loading from database");

                    layout = LoadLayoutFromDatabase(layoutName);
                    if (layout != null)
                    {
                        if (isLayoutCachingOn)
                        {
                           this.logService.Info("Print.LayoutCache.GetLayout - Load Success, Adding to cache.");
                            cachedLayouts[layoutName] = layout;
                        }
                        else
                        {
                           this.logService.Info("Print.LayoutCache.GetLayout - Load Success");
                        }
                    }
                    else
                    {
                       this.logService.Error("Print.LayoutCache.GetLayout - Load Layout Failed");
                    }
                }

               this.logService.Info("Print.LayoutCache.GetLayout - Exiting GetLayout()");
            }
            catch (Exception ex)
            {
               this.logService.Error("Print.LayoutCache.GetLayout - Failed", ex);
                layout = null;
            }

            return layout.DocFormLayout;
        }

        private Layout LoadLayoutFromDatabase(string layoutName)
        {
            Layout layout = null;
            try
            {
               this.logService.Info("Print.LayoutCache.LoadLayoutFromDatabase - Query database");
                if (layout != null)
                {
                    layout.InitializeDocFormLayout();
                }
                else
                {
                   this.logService.Error("Print.LayoutCache.LoadLayoutFromDatabase - Query database failed");
                }
            }
            catch (Exception ex)
            {
               this.logService.Error("Print.LayoutCache.LoadLayoutFromDatabase - Failed", ex);
                layout = null;
            }

            return layout;
        }
    }
}