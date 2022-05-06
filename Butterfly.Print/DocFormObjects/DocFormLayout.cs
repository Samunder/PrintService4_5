namespace Butterfly.Print.DocFormObjects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Butterfly.Print.PrintJobObjects;

    public class DocFormLayout
    {
        //[JsonIgnore]
        //private bool isValid = false;

        public double ScaleFactor { get; set; }

        public DocFormLayout()
        {
            this.Name = "Layout";
            this.Title = string.Empty;
            this.Author = string.Empty;
            this.Version = string.Empty;
            this.Duplex = "None";
            this.Page1Front = null;
            this.Page1Back = null;
            this.Page2Front = null;
            this.Page2Back = null;
            this.PagePairs = new List<DocFormPagePair>();
            this.Font = null;

            PossiblePageData = new List<string>();
            PossibleBlockDataRecursive = new List<string>();
            DynamicTargets = new List<DocFormDynamicTargetItem>();
        }

        public string Author { get; set; }

        public string Duplex { get; set; }

        [DefaultValue(null)]
        public DocFormFont Font { get; set; }

        public string Name { get; set; }

        public DocFormPage Page1Back { get; set; }

        public DocFormPage Page1Front { get; set; }

        public DocFormPage Page2Back { get; set; }

        public DocFormPage Page2Front { get; set; }

        public List<DocFormPagePair> PagePairs { get; set; }

        public string Title { get; set; }

        public string Version { get; set; }

        //[JsonIgnore]
        internal List<string> PossiblePageData { get; set; }

        //[JsonIgnore]
        internal List<string> PossibleBlockDataRecursive { get; set; }

        //[JsonIgnore]
        internal List<DocFormDynamicTargetItem> DynamicTargets { get; set; }

        public void InitializeLayout()
        {
            try
            {
                
                if (Page1Front != null)
                {
                    Page1Front.InitializePage(this);

                    PossiblePageData.AddRange(Page1Front.PossiblePageData);
                    PossibleBlockDataRecursive.AddRange(Page1Front.PossibleBlockDataRecursive);
                }

                if (Page1Back != null)
                {
                    Page1Back.InitializePage(this);

                    PossiblePageData.AddRange(Page1Back.PossiblePageData);
                    PossibleBlockDataRecursive.AddRange(Page1Back.PossibleBlockDataRecursive);
                }

                if (Page2Front != null)
                {
                    Page2Front.InitializePage(this);

                    PossiblePageData.AddRange(Page2Front.PossiblePageData);
                    PossibleBlockDataRecursive.AddRange(Page2Front.PossibleBlockDataRecursive);
                }

                if (Page2Back != null)
                {
                    Page2Back.InitializePage(this);

                    PossiblePageData.AddRange(Page2Back.PossiblePageData);
                    PossibleBlockDataRecursive.AddRange(Page2Back.PossibleBlockDataRecursive);
                }


                foreach (var pagePair in PagePairs)
                {
                    if (pagePair.Front != null)
                    {
                        pagePair.Front.InitializePage(this);

                        PossiblePageData.AddRange(pagePair.Front.PossiblePageData);
                        PossibleBlockDataRecursive.AddRange(pagePair.Front.PossibleBlockDataRecursive);
                    }
                    if (pagePair.Back != null)
                    {
                        pagePair.Back.InitializePage(this);

                        PossiblePageData.AddRange(pagePair.Back.PossiblePageData);
                        PossibleBlockDataRecursive.AddRange(pagePair.Back.PossibleBlockDataRecursive);
                    }
                }

                // Remove duplicates
                PossiblePageData = PossiblePageData.Distinct().ToList();

                // Create temp list of pageDynamicTargets
                List<DocFormDynamicTargetItem> pageDynamicTargets = new List<DocFormDynamicTargetItem>();
                PossiblePageData.ForEach(name => pageDynamicTargets.Add(new DocFormDynamicTargetItem(name, "")));

                // Add pageDynamicTargets at start of DynamicTargets
                DynamicTargets.InsertRange(0, pageDynamicTargets);
            }
            catch (Exception ex)
            {
                throw new Exception("Print.DocFormLayout.PopulatePossibleDataCollections - Failed", ex);
            }
        }

        public void ApplyScalingFactor(double scalingFactor)
        {
            //Apply scaling on pages to get correct DocFormPage size
            this.ScaleFactor = scalingFactor;
            if (Page1Front != null) Page1Front.ApplyScaling();
            if (Page1Back != null) Page1Back.ApplyScaling();
            if (Page2Front != null) Page2Front.ApplyScaling();
            if (Page2Back != null) Page2Back.ApplyScaling();

            foreach (var pagePair in PagePairs)
            {
                if (pagePair.Front != null)
                {
                    pagePair.Front.ApplyScaling();
                }
                if (pagePair.Back != null)
                {
                    pagePair.Back.ApplyScaling();
                }
            }
        }

        internal bool IsBlockDataMatchingPage2(List<PrintJobDataItem> blockData)
        {
            try
            {
                if (Page2Front == null) return false;

                List<string> keys = new List<string>();

                foreach (PrintJobDataItem oPJDI in blockData)
                {
                    if (keys.IndexOf(oPJDI.Key) == -1)
                    {
                        keys.Add(oPJDI.Key);
                    }
                }

                foreach (string strPossSource in Page2Front.PossibleBlockDataRecursive)
                {
                    if (keys.IndexOf(strPossSource) != -1) return true;
                }

                if (Page2Back == null) return false;

                foreach (string strPossSource in Page2Back.PossibleBlockDataRecursive)
                {
                    if (keys.IndexOf(strPossSource) != -1) return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Print.DocFormLayout.IsBlockDataMatchingPage2 - Failed", ex);
            }

            return false;
        }

        internal bool IsBlockDataMatchingPage(List<PrintJobDataItem> blockData, DocFormPagePair pagePair)
        {
            try
            {
                if (pagePair.Front == null) return false;

                List<string> keys = new List<string>();

                foreach (PrintJobDataItem oPJDI in blockData)
                {
                    if (keys.IndexOf(oPJDI.Key) == -1)
                    {
                        keys.Add(oPJDI.Key);
                    }
                }

                foreach (string strPossSource in pagePair.Front.PossibleBlockDataRecursive)
                {
                    if (keys.IndexOf(strPossSource) != -1) return true;
                }

                if (pagePair.Back == null) return false;

                foreach (string strPossSource in pagePair.Back.PossibleBlockDataRecursive)
                {
                    if (keys.IndexOf(strPossSource) != -1) return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Print.DocFormLayout.IsBlockDataMatchingPage - Failed", ex);
            }

            return false;
        }
    }
}