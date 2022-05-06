namespace Butterfly.Print
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using DocFormObjects;
    using Interfaces;
    using Objects;
    using PageObjects;
    using PrintJobObjects;

    /// <summary>
    /// Summary description for DocumentBuilder.
    /// </summary>
    public class DocumentBuilder
    {
        private ImageCache imageCache = null;
        private ILogService logService;

        public DocumentBuilder(ILogService logService)
        {
            this.logService = logService;
        }

        public static string _layoutName;

        public bool BuildDocument(PrintJobDocument printJobDocument, Layout layout, ImageCache imgCache, double scalingFactor = 1)
        {
            var firstPages = new List<Page>();
            var secondPages = new List<Page>();

            try
            {
                this.imageCache = imgCache;

                // Process PrintJobLayouts
                foreach (var printJobLayout in printJobDocument.PrintJobLayouts)
                {
                    _layoutName = printJobLayout.LayoutName;

                    DocFormLayout docFormLayout = null;
                    var pageData = new Dictionary<string, string>();
                    var blockData = new List<PrintJobDataItem>();

                    // Get Layout
                    if (printJobLayout.LayoutName == string.Empty)
                    {
                        this.logService.Error("Print.DocumentBuilder.BuildDocument - LayoutName empty");
                        return false;
                    }

                    docFormLayout = layout.DocFormLayout;

                    if (docFormLayout == null)
                    {
                        this.logService.Error("Print.DocumentBuilder.BuildDocument - GetLayout failed");
                        return false;
                    }

                    //this is first time we are setting scale factor on docFormLayout
                    // set scaling factor on layout, which is different for Graphics(1) and ComponentOne(.28.... something)
                    docFormLayout.ApplyScalingFactor(scalingFactor);

                    // Separate Data
                    foreach (var printJobDataItem in printJobLayout.PrintJobDataItems)
                    {
                        if (docFormLayout.PossiblePageData.IndexOf(printJobDataItem.Key) != -1)
                        {
                            pageData[printJobDataItem.Key] = printJobDataItem.Value;
                        }
                        else if (docFormLayout.PossibleBlockDataRecursive.IndexOf(printJobDataItem.Key) != -1)
                        {
                            blockData.Add(printJobDataItem);
                        }
                    }


                    Page page = null;

                    //    // Build 1 Page 1 Front + (back)
                    if (docFormLayout.Page1Front != null)
                    {
                        // Front
                        page = this.BuildPage(docFormLayout.Page1Front, pageData, blockData);
                        if (page != null)
                        {
                            firstPages.Add(page);

                            // Back
                            if (docFormLayout.Page1Back != null)
                            {
                                page = this.BuildPage(docFormLayout.Page1Back, pageData, blockData);
                                if (page != null)
                                {
                                    firstPages.Add(page);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.logService.Error("Print.DocumentBuilder.BuildDocument - Page 1 Front is null, that's bad");
                        return false;
                    }

                    // Build Page 2s Front + (back)
                    do
                    {
                        if (docFormLayout.Page2Front != null)
                        {
                            // Front
                            page = this.BuildPage(docFormLayout.Page2Front, pageData, blockData);
                            if (page != null)
                            {
                                secondPages.Add(page);
                                
                                // Back
                                if (docFormLayout.Page2Back != null)
                                {
                                    page = this.BuildPage(docFormLayout.Page2Back, pageData, blockData);
                                    if (page != null)
                                    {
                                        secondPages.Add(page);
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    while ((blockData.Count > 0) && docFormLayout.IsBlockDataMatchingPage2(blockData));

                    foreach (var pagePair in docFormLayout.PagePairs)
                    {
                        do
                        {
                            if (pagePair.Front != null)
                            {
                                if (!blockData.Exists(x => x.Key.StartsWith(pagePair.Front.Name)))//page is not needed as no blockdata does not contains any data for specific page.
                                {
                                    break;
                                }
                                page = this.BuildPage(pagePair.Front, pageData, blockData);
                                if (page != null)
                                {
                                    secondPages.Add(page);

                                    // Back
                                    if (pagePair.Back != null)
                                    {
                                        page = this.BuildPage(pagePair.Back, pageData, blockData);
                                        if (page != null)
                                        {
                                            secondPages.Add(page);
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        while ((blockData.Count > 0) && docFormLayout.IsBlockDataMatchingPage(blockData, pagePair));
                    }

                    // Build Page 1s Front + (back)
                    while (blockData.Count > 0)
                    {
                        // Front
                        page = this.BuildPage(docFormLayout.Page1Front, pageData, blockData);
                        if (page != null && page.IsStatic == false)
                        {
                            firstPages.Add(page);

                            // Back
                            if (docFormLayout.Page1Back != null)
                            {
                                page = this.BuildPage(docFormLayout.Page1Back, pageData, blockData);
                                if (page != null)
                                {
                                    firstPages.Add(page);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Add page2s to alPages
                    firstPages.AddRange(secondPages);

                    // Apply PaperSource, XOffset, YOffset
                    foreach (var firstPage in firstPages)
                    {
                        firstPage.PaperSource = printJobLayout.PaperSource;

                        firstPage.Offset(printJobLayout.XOffset, printJobLayout.YOffset);
                    }

                    // Add to Document with copies
                    for (var i = 0; i < printJobLayout.Copies; i++)
                    {
                        printJobDocument.Pages.AddRange(firstPages);
                    }
                }

                // Run Macro
                this.RunMacro(printJobDocument);
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildDocument - Failed", ex);

                DisposeHelper.DisposePages(firstPages, logService);
                DisposeHelper.DisposePages(secondPages, logService);

                throw;

                // return false;
            }

            return true;
        }

        private Page BuildPage(DocFormPage docFormPage, Dictionary<string, string> pageData, List<PrintJobDataItem> blockData)
        {
            Page page = null;

            try
            {
                // Check if DiscardIfStatic and no BlockData matching page
                if (docFormPage.DiscardIfStatic)
                {
                    if (docFormPage.IsBlockDataMatching(blockData) == false)
                    {
                        return null;
                    }
                }

                // Add page
                page = new Page
                {
                    PageScale = docFormPage.PageScale,
                    PageSize = docFormPage.PageSize,
                    Orientation = docFormPage.Orientation,
                    Height = docFormPage.PageHeight,
                    Width = docFormPage.PageWidth
                };


                // Build objects not in blocks
                this.BuildNonBlockObjects(docFormPage, pageData, page);

                var isStatic = true;

                this.BuildBlocks(page, docFormPage, pageData, blockData, ref isStatic);

                page.IsStatic = isStatic;

                if (docFormPage.DiscardIfStatic && page.IsStatic)
                {
                    page = null;
                }
                
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildPage - Failed", ex);

                page?.Dispose();

                return null;
            }

            return page;
        }

        private void BuildNonBlockObjects(DocFormPage docFormPage, Dictionary<string, string> pageData, Page page)
        {
            foreach (var docFormPageObject in docFormPage.Objects)
            {
                if (docFormPageObject is DocFormText)
                {
                    var pageObjectText = this.BuildPageObjectText((DocFormText)docFormPageObject, pageData, null, page.PageScale);
                    if (pageObjectText != null)
                    {
                        page.PageObjects.Add(pageObjectText);
                    }
                }
                else if (docFormPageObject is DocFormImage)
                {
                    var pageObjectImage = this.BuildPageObjectImage((DocFormImage)docFormPageObject, pageData, null, page.PageScale);
                    if (pageObjectImage != null)
                    {
                        page.PageObjects.Add(pageObjectImage);
                    }
                }
                else if (docFormPageObject is DocFormBarCode)
                {
                    var pageObjectBarCode = this.BuildPageObjectBarCode(
                        (DocFormBarCode)docFormPageObject,
                        pageData,
                        null, 
                        page.PageScale);
                    if (pageObjectBarCode != null)
                    {
                        page.PageObjects.Add(pageObjectBarCode);
                    }
                }
                else if (docFormPageObject is DocFormLine)
                {
                    var pageObjectLine = this.BuildPageObjectLine((DocFormLine)docFormPageObject, page.PageScale);
                    if (pageObjectLine != null)
                    {
                        page.PageObjects.Add(pageObjectLine);
                    }
                }
                else if (docFormPageObject is DocFormRectangle)
                {
                    var pageObjectRectangle = this.BuildPageObjectRectangle((DocFormRectangle)docFormPageObject, page.PageScale);
                    if (pageObjectRectangle != null)
                    {
                        page.PageObjects.Add(pageObjectRectangle);
                    }
                }
                else if (docFormPageObject is DocFormEllipse)
                {
                    var pageObjectEllipse = this.BuildPageObjectEllipse((DocFormEllipse)docFormPageObject, page.PageScale);
                    if (pageObjectEllipse != null)
                    {
                        page.PageObjects.Add(pageObjectEllipse);
                    }
                }
            }
        }

        private void BuildBlocks(Page page, DocFormPage docFormPage, Dictionary<string, string> pageData, List<PrintJobDataItem> blockData, ref bool isStatic)
        {
            try
            {
                // Move Dynamic objects in block with page data to static objects
                foreach (var docFormBlock in docFormPage.Blocks)
                {
                    this.MoveBlockObjsWithPageDataInDynObjs(docFormBlock, pageData);
                }
                 
                var currentPageHeight = 0;
                // Build Blocks
                foreach (var docFormBlock in docFormPage.Blocks)
                {
                    if (docFormBlock.DockToBlock == string.Empty)
                    {
                        foreach (var source in docFormPage.Blocks.Where(b => b.DockToBlock == docFormBlock.Name))
                        {
                            docFormBlock.PossibleBlockDataRecursive.AddRange(source.PossibleBlockData);
                            docFormBlock.PossibleBlockDataRecursive.AddRange(source.PossibleBlockDataRecursive);
                        }
                        var firstIndex = 0;


                        var dynamicData = new List<PrintJobDataItem>();

                        // Sort out dynamic data Find first pos in blockData
                        SortOutBlockData(blockData, firstIndex, docFormBlock, dynamicData);

                        // Get startY
                        var maxHeight = GetMaxHeight(page, docFormBlock, 0);

                        var heightInfo = new HeightInfo(maxHeight);

                        var removedBlockData = new List<PrintJobDataItem>();
                        var pageObjects = new List<PageObject>();
                        var isOverflowed = false;
                        currentPageHeight = this.BuildBlock(docFormBlock, pageData, dynamicData, pageObjects, removedBlockData, maxHeight, true, ref isOverflowed, ref isStatic, heightInfo);

                        if (docFormBlock.Anchor == "Top")
                        {
                            this.OffsetObjects(pageObjects, docFormBlock.Top);
                        }
                        else
                        {
                            this.OffsetObjects(pageObjects, docFormBlock.Bottom - currentPageHeight);
                        }
                        // Put back unused dynamic datab
                        blockData.InsertRange(0, dynamicData);
                        //Put alBlockObjects on the Page
                        page.PageObjects.AddRange(pageObjects);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildBlocks - Failed", ex);
                throw;
            }
        }

        private static void SortOutBlockData(List<PrintJobDataItem> blockData, int firstIndex, DocFormBlock docFormBlock, List<PrintJobDataItem> dynamicData)
        {
            while (firstIndex < blockData.Count)
            {
                if (docFormBlock.PossibleBlockDataRecursive.IndexOf((blockData[firstIndex]).Key) != -1)
                {
                    break;
                }
                firstIndex++;
            }

            // Add to new array
            while (firstIndex < blockData.Count)
            {
                if (docFormBlock.PossibleBlockDataRecursive.IndexOf((blockData[firstIndex]).Key) != -1)
                {
                    dynamicData.Add(blockData[firstIndex]);
                    blockData.RemoveAt(firstIndex);
                }
                else
                {
                    break;
                }
            }
        }

        private static int GetMaxHeight(Page page, DocFormBlock docFormBlock, int startY)
        {
            var maxHeight = 0;
            if (docFormBlock.Anchor == "Top")
            {
                if (docFormBlock.RepeatsTo != 0)
                {
                    maxHeight = docFormBlock.RepeatsTo - startY;
                }
                else
                {
                    maxHeight = page.Height - startY;
                }
            }
            else
            {
                startY = docFormBlock.Bottom;

                if (docFormBlock.RepeatsTo != 0)
                {
                    maxHeight = startY - docFormBlock.RepeatsTo;
                }
                else
                {
                    maxHeight = docFormBlock.Bottom;
                }
            }
            return maxHeight;
        }

        public static int counter23 = 0;
        public static int block15Counter = 0;
        private int BuildBlock(DocFormBlock docFormBlock, Dictionary<string, string> pageData, 
            List<PrintJobDataItem> blockData, List<PageObject> newPageObjects, 
            List<PrintJobDataItem> removedBlockData, int maxHeight, bool isSplitAllowed, 
            ref bool isOverflowed, ref bool isStatic, HeightInfo heightInfoOriginal)
        {
            int currentHeight = 0;
            int iteration = 0;
            bool isFirstTime = true;
            bool isSpaceLeftForOneMoreBlock = true;

            var heightInfoWorkingCopy = heightInfoOriginal.Copy();

            try
            {
                if (docFormBlock.Name == "Block16")
                {
                    counter23++;
                    Console.Out.WriteLine("Counter: " + counter23);
                }

                if (docFormBlock.Name == "Block15")
                {
                    block15Counter++;
                }

                // bAnoteroneCouldFit
                //if ((currentHeight + (docFormBlock.Bottom - docFormBlock.Top)) <= maxHeight) isSpaceLeftForOneMoreBlock = true; else isSpaceLeftForOneMoreBlock = false;

                if (!isSplitAllowed)
                {
                    isSpaceLeftForOneMoreBlock = (currentHeight + (docFormBlock.Bottom - docFormBlock.Top)) <= heightInfoWorkingCopy.MaxHeightBeforeUnsplittablePartMustBeSplitted;
                }
                else
                {
                    isSpaceLeftForOneMoreBlock = (currentHeight + (docFormBlock.Bottom - docFormBlock.Top)) <= maxHeight;
                }

                var lastBlockDataCount = blockData.Count;

                // Start iterate
                while (isSpaceLeftForOneMoreBlock && (blockData.Count > 0 || isFirstTime))
                {
                    iteration++;

                    if (IterationForRepeatableBlock(
                        docFormBlock, pageData, blockData, newPageObjects, 
                        removedBlockData, maxHeight, isSplitAllowed, ref isOverflowed, 
                        ref isStatic, iteration, 
                        ref isFirstTime, ref currentHeight, ref isSpaceLeftForOneMoreBlock, heightInfoWorkingCopy)) break;

                    if (lastBlockDataCount == blockData.Count && lastBlockDataCount != 0)
                    {
                        break;
                    }

                    lastBlockDataCount = blockData.Count;
                }

                heightInfoOriginal = heightInfoWorkingCopy;

                // Build docked block
                if (docFormBlock.DockedBlock != null)
                {
                    int dockedBlockHeight = 0;
                    List<PageObject> dockedBlockNewObjects = new List<PageObject>();
                    List<PrintJobDataItem> dockedBlockRemovedBlockData = new List<PrintJobDataItem>();

                    var heightInfoForDockedBlock = heightInfoWorkingCopy.Copy();

                    try
                    {
                        dockedBlockHeight = BuildDockedBlock(
                            docFormBlock, pageData, blockData, dockedBlockNewObjects,
                            dockedBlockRemovedBlockData, maxHeight - currentHeight,
                            isSplitAllowed, ref isOverflowed, ref isStatic, heightInfoForDockedBlock);

                        // Check if docked block fits
                        if (isSplitAllowed == false || ((currentHeight + dockedBlockHeight) <= maxHeight))
                        {
                            // Add objects Offset objects and add to alPageObjects
                            OffsetObjects(dockedBlockNewObjects, currentHeight);
                            newPageObjects.AddRange(dockedBlockNewObjects);

                            currentHeight += dockedBlockHeight;
                            removedBlockData.AddRange(dockedBlockRemovedBlockData);

                            heightInfoOriginal.UnSplittableHeight = heightInfoForDockedBlock.UnSplittableHeight;
                        }
                        else
                        {
                            // Overflow
                            isOverflowed = true;
                            blockData.InsertRange(0, dockedBlockRemovedBlockData);
                        }
                    }
                    catch (Exception)
                    {
                        DisposeHelper.DisposePageObjects(dockedBlockNewObjects, logService);

                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildBlock - Failed", ex);
                throw;
            }

            return currentHeight;
        }

        private bool IterationForRepeatableBlock(DocFormBlock docFormBlock, Dictionary<string, string> pageData, List<PrintJobDataItem> blockData,
            List<PageObject> newPageObjects, List<PrintJobDataItem> removedBlockData, int maxHeight, bool isSplitAllowed, ref bool isOverflowed,
            ref bool isStatic, int iteration, ref bool isFirstTime, ref int currentHeight, ref bool isSpaceLeftForOneMoreBlock,
            HeightInfo heightInfo)
        {
// BUG 9312: Added temporary workaround to guard against loops that won't end
            // TODO: Fix properly
            if (iteration > 10000)
            {
                return true;
            }

            isFirstTime = false;

            var thisNewPageObjects = new List<PageObject>();
            var thisRemovedBlockData = new List<PrintJobDataItem>();
            var alreadyAddedKeys = new List<string>();
            var height = docFormBlock.Bottom - docFormBlock.Top;

            // Add static objects
            this.AddStaticObjects(docFormBlock, pageData, thisNewPageObjects);

            // Add dynamic objects
            PrintJobDataItem printJobDataItem;
            DocFormPageObject docFormPageObject;
            bool dataMatched = false;
            bool isContinuingBlock = false;

            while (blockData.Count > 0)
            {
                printJobDataItem = blockData[0];

                if (alreadyAddedKeys.IndexOf(printJobDataItem.Key) != -1) break;
                if (docFormBlock.PossibleBlockData.IndexOf(printJobDataItem.Key) == -1) break;

                docFormPageObject = docFormBlock.GetDynamicObjectByKey(printJobDataItem.Key);

                if (docFormPageObject is DocFormText)
                {
                    PageObjectText pageObjectText =
                        BuildPageObjectText((DocFormText) docFormPageObject, pageData, printJobDataItem.Value, docFormBlock.ScalingFactor);
                    if (pageObjectText != null) thisNewPageObjects.Add(pageObjectText);
                }
                else if (docFormPageObject is DocFormImage)
                {
                    PageObjectImage pageObjectImage =
                        BuildPageObjectImage((DocFormImage) docFormPageObject, pageData, printJobDataItem.Value, docFormBlock.ScalingFactor);
                    if (pageObjectImage != null) thisNewPageObjects.Add(pageObjectImage);
                }
                else if (docFormPageObject is DocFormBarCode)
                {
                    PageObjectBarCode pageObjectBarCode =
                        BuildPageObjectBarCode((DocFormBarCode) docFormPageObject, pageData, printJobDataItem.Value, docFormBlock.ScalingFactor);
                    if (pageObjectBarCode != null) thisNewPageObjects.Add(pageObjectBarCode);
                }

                dataMatched = true;
                if (printJobDataItem.Value != string.Empty) isStatic = false;
                thisRemovedBlockData.Add(printJobDataItem);
                blockData.RemoveAt(0);

                alreadyAddedKeys.Add(printJobDataItem.Key);
            }

            if ((docFormBlock.PossibleBlockData.Count > 0) && !dataMatched)
            {
                // Continuing block, remove items
                isContinuingBlock = true;

                //thisNewPageObjects.Clear();
            }

            // Build blocks in block
            List<PageObject> newBlockObjects = new List<PageObject>();

            height = BuildBlocksInBlock(docFormBlock, pageData, blockData, newBlockObjects, thisRemovedBlockData,
                maxHeight - currentHeight, isSplitAllowed, ref isOverflowed, ref isStatic, heightInfo);

            // Check static
            if (docFormBlock.DiscardIfStatic && isStatic)
            {
                return true;
            }

            // Continuing block, Cut off to skip outer block
            int cutOff = 0;
            if (isContinuingBlock)
            {
                // Calculate cutOff
                cutOff = docFormBlock.Bottom - docFormBlock.Top;
                foreach (PageObject pageObject in newBlockObjects)
                {
                    if (pageObject.Top < cutOff) cutOff = pageObject.Top;
                }

                // Offset objects in block
                foreach (PageObject pageObject in newBlockObjects)
                {
                    pageObject.Offset(0, -cutOff);
                }

                // Set new height
                height -= cutOff;
            }

            // Resize due to Multiline TextBoxes
            foreach (PageObject pageObject in thisNewPageObjects)
            {
                if (pageObject is PageObjectText)
                {
                    PageObjectText pageObjectText = (PageObjectText) pageObject;
                    if (pageObjectText.MultiLine && !pageObjectText.Clip)
                    {
                        int newHeight = 0;

                        switch (pageObjectText.Anchor)
                        {
                            case "TopBottom":
                                newHeight = pageObjectText.PaddingTop + (pageObjectText.Bottom - pageObjectText.Top) + pageObjectText.PaddingBottom;
                                break;

                            case "Top":
                                newHeight = pageObjectText.PaddingTop + (pageObjectText.Bottom - pageObjectText.Top);
                                break;

                            case "Bottom":
                                newHeight = (pageObjectText.Bottom - pageObjectText.Top) + pageObjectText.PaddingBottom;
                                break;
                        }

                        if (newHeight > height) height = newHeight;
                    }
                }
            }

            // Size to content
            if (docFormBlock.ShrinkToContent)
            {
                int newHeight = 0;

                foreach (PageObject pageObject in newBlockObjects)
                {
                    if (pageObject.Bottom > newHeight) newHeight = pageObject.Bottom;
                }

                height = newHeight;
            }

            // Check if Block changed size
            if (docFormBlock.Bottom - docFormBlock.Top != height)
            {
                // Resize this block objects
                for (int i = 0; i < thisNewPageObjects.Count;)
                {
                    PageObject pageObject = thisNewPageObjects[i];

                    if (pageObject.Anchor == "TopBottom")
                    {
                        pageObject.SetTop(pageObject.PaddingTop - cutOff);
                        pageObject.SetBottom(height - pageObject.PaddingBottom);
                    }
                    else if (pageObject.Anchor == "Top")
                    {
                        pageObject.Offset(0, -cutOff);
                    }
                    else if (pageObject.Anchor == "Bottom")
                    {
                        pageObject.Offset(0, ((height - pageObject.PaddingBottom) - pageObject.Bottom) - cutOff);
                    }

                    if (pageObject.Bottom <= 0 && pageObject.Top < 0)
                    {
                        // Remove if object is outside visible part of block
                        thisNewPageObjects.RemoveAt(i);
                        continue;
                    }

                    if (pageObject.Top < 0) pageObject.Top = 0;

                    i++;
                }
            }

            // Add block objects
            thisNewPageObjects.AddRange(newBlockObjects);

            var forcedSplitBecauseOfPageHeightNeeded = heightInfo.IsForcedSplitNeeded(height);

            // Check if block still fits
            if ((isSplitAllowed == false && !forcedSplitBecauseOfPageHeightNeeded ) || ((currentHeight + height) <= maxHeight))
            {
                // Add objects Offset objects and add to alPageObjects
                OffsetObjects(thisNewPageObjects, currentHeight);
                newPageObjects.AddRange(thisNewPageObjects);

                currentHeight += height;
                if (!isSplitAllowed)
                {
                    heightInfo.IncreaseUnsplittableHeight(height);
                }

                removedBlockData.AddRange(thisRemovedBlockData);
            }
            else
            {
                // Overflow
                isOverflowed = true;
                blockData.InsertRange(0, thisRemovedBlockData);
                return true;
            }

            // bAnoteroneCouldFit
            //var forcedSplitBecauseOfPageHeightNeededForNextBlock =
            //    heightInfo.IsForcedSplitNeeded(docFormBlock.Bottom - docFormBlock.Top);

            if ((isSplitAllowed == false /*&& !forcedSplitBecauseOfPageHeightNeededForNextBlock*/) || ((currentHeight + (docFormBlock.Bottom - docFormBlock.Top)) <= maxHeight))
            {
                isSpaceLeftForOneMoreBlock = true;
            }
            else
            {
                isSpaceLeftForOneMoreBlock = false;
            }

            // If there still is data and that data dont fit this block break (should be
            // docked block data)
            if (blockData.Count > 0)
            {
                PrintJobDataItem nextPrintJobDataItem = blockData[0];
                if (docFormBlock.PossibleBlockData.IndexOf(nextPrintJobDataItem.Key) == -1)
                {
                    if (docFormBlock.PossibleBlockInBlockData.IndexOf(nextPrintJobDataItem.Key) == -1) return true;
                }
            }

            return false;
        }

        private void AddStaticObjects(DocFormBlock docFormBlock, Dictionary<string, string> pageData, List<PageObject> thisNewPageObjects)
         {
            foreach (DocFormPageObject staticDocFormPageObject in docFormBlock.StaticObjects)
            {
                if (staticDocFormPageObject is DocFormText)
                {
                    var pageObjectText = this.BuildPageObjectText((DocFormText)staticDocFormPageObject, pageData, null, docFormBlock.ScalingFactor);
                    if (pageObjectText != null)
                    {
                        thisNewPageObjects.Add(pageObjectText);
                    }
                }
                else if (staticDocFormPageObject is DocFormImage)
                {
                    var pageObjectImage = this.BuildPageObjectImage((DocFormImage)staticDocFormPageObject, pageData, null, docFormBlock.ScalingFactor);
                    if (pageObjectImage != null)
                    {
                        thisNewPageObjects.Add(pageObjectImage);
                    }
                }
                else if (staticDocFormPageObject is DocFormBarCode)
                {
                    var pageObjectBarCode = this.BuildPageObjectBarCode((DocFormBarCode)staticDocFormPageObject, pageData, null, docFormBlock.ScalingFactor);
                    if (pageObjectBarCode != null)
                    {
                        thisNewPageObjects.Add(pageObjectBarCode);
                    }
                }
                else if (staticDocFormPageObject is DocFormLine)
                {
                    var pageObjectLine = this.BuildPageObjectLine((DocFormLine)staticDocFormPageObject, docFormBlock.ScalingFactor);
                    if (pageObjectLine != null)
                    {
                        thisNewPageObjects.Add(pageObjectLine);
                    }
                }
                else if (staticDocFormPageObject is DocFormRectangle)
                {
                    var pageObjectRectangle = this.BuildPageObjectRectangle((DocFormRectangle)staticDocFormPageObject, docFormBlock.ScalingFactor);
                    if (pageObjectRectangle != null)
                    {
                        thisNewPageObjects.Add(pageObjectRectangle);
                    }
                }
                else if (staticDocFormPageObject is DocFormEllipse)
                {
                    var pageObjectEllipse = this.BuildPageObjectEllipse((DocFormEllipse)staticDocFormPageObject, docFormBlock.ScalingFactor);
                    if (pageObjectEllipse != null)
                    {
                        thisNewPageObjects.Add(pageObjectEllipse);
                    }
                }
            }
        }

        private int BuildBlocksInBlock(DocFormBlock block, Dictionary<string, string> pageData, 
            List<PrintJobDataItem> blockData, List<PageObject> newObjects, List<PrintJobDataItem> removedBlockData, 
            int maxHeight, bool isSplitAllowed, ref bool isOverflowed, ref bool isParentStatic, HeightInfo heightInfo)
        {
            try
            {
                int iNewBlockHeight = block.Bottom - block.Top;
                if (block.Blocks.Count == 0) return iNewBlockHeight;

                // Get AvailableBlockData for all blocks in block
                List<PrintJobDataItem> availableBlockData = new List<PrintJobDataItem>();
                while (blockData.Count > 0)
                {
                    PrintJobDataItem oPJDI = blockData[0];

                    if (block.PossibleBlockInBlockData.IndexOf(oPJDI.Key) == -1) break;

                    availableBlockData.Add(oPJDI);
                    blockData.RemoveAt(0);
                }

                foreach (DocFormBlock blockInBlock in block.Blocks)
                {
                    if (blockInBlock.DockToBlock == string.Empty)
                    {
                        // Sort out BlockData for this block
                        int iIndex = 0;
                        List<PrintJobDataItem> thisBlockData = new List<PrintJobDataItem>();
                        while (iIndex < availableBlockData.Count)
                        {
                            PrintJobDataItem oPJDI = availableBlockData[iIndex];

                            if (blockInBlock.PossibleBlockDataRecursive.IndexOf(oPJDI.Key) != -1)
                            {
                                thisBlockData.Add(oPJDI);
                                availableBlockData.RemoveAt(iIndex);
                            }
                            else
                            {
                                iIndex++;
                            }
                        }

                        List<PageObject> thisNewObjects = new List<PageObject>();
                        int thisMaxHeight = 0;
                        bool thisAllowSplit = isSplitAllowed & block.AllowSplit;
                        bool isStatic = true;

                        var heighInfoForBlockInBlock = isSplitAllowed ? heightInfo.Copy() : heightInfo;

                        if (blockInBlock.Anchor == "Top") thisMaxHeight = maxHeight - blockInBlock.PaddingTop; else thisMaxHeight = maxHeight - (blockInBlock.PaddingTop + blockInBlock.PaddingBottom);

                        int iBlockHeight = BuildBlock(blockInBlock, pageData, thisBlockData, thisNewObjects, 
                            removedBlockData, thisMaxHeight, thisAllowSplit, ref isOverflowed, 
                            ref isStatic, heighInfoForBlockInBlock);

                        if (isStatic == false) isParentStatic = false;

                        if (blockInBlock.Anchor == "Top")
                        {
                            if ((blockInBlock.PaddingTop + iBlockHeight) > iNewBlockHeight) iNewBlockHeight = (blockInBlock.PaddingTop + iBlockHeight);
                        }
                        else
                        {
                            if ((blockInBlock.PaddingTop + iBlockHeight + blockInBlock.PaddingBottom) > iNewBlockHeight) iNewBlockHeight = (blockInBlock.PaddingTop + iBlockHeight + blockInBlock.PaddingBottom);
                        }
                        //TODO anders here - check for overflow???
                        OffsetObjects(thisNewObjects, blockInBlock.Top);
                        newObjects.AddRange(thisNewObjects);

                        // Put back unused data
                        availableBlockData.InsertRange(0, thisBlockData);
                    }
                }

                // Put back unused data
                blockData.InsertRange(0, availableBlockData);

                return iNewBlockHeight;
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildBlocksInBlock - Failed", ex);
                throw;
            }
        }

        private int BuildDockedBlock(DocFormBlock block, Dictionary<string, string> pageData, List<PrintJobDataItem> blockData, 
            List<PageObject> dockedBlockNewObjects, List<PrintJobDataItem> dockedBlockRemovedData, int maxHeight, 
            bool isSplitAllowed, ref bool isOverflowed, ref bool isParentStatic, HeightInfo heightInfo)
        {
            try
            {
                int iDockedBlockHeight = 0;
                bool isStatic = true;

                // If data sitll exists and data is fitting last block, end of space, return.
                if (blockData.Count > 0)
                {
                    PrintJobDataItem printJobDataItem = blockData[0];

                    if (block.PossibleBlockData.IndexOf(printJobDataItem.Key) != -1) return 0;
                    if (block.PossibleBlockInBlockData.IndexOf(printJobDataItem.Key) != -1) return 0;
                }

                iDockedBlockHeight = BuildBlock(block.DockedBlock, pageData, blockData, dockedBlockNewObjects, dockedBlockRemovedData, maxHeight, isSplitAllowed, ref isOverflowed, ref isStatic, heightInfo);

                if (isStatic == false) isParentStatic = false;

                return iDockedBlockHeight;
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildDockedBlock - Failed", ex);
                throw;
            }
        }

        private void MoveBlockObjsWithPageDataInDynObjs(DocFormBlock docFormBlock, Dictionary<string, string> pageData)
        {
            try
            {
                // Fix this Block
                foreach (DocFormPageObject docFormPageObject in docFormBlock.DynamicObjects)
                {
                    string strKey = string.Empty;

                    if (docFormPageObject is DocFormText)
                    {
                        strKey = ((DocFormText)docFormPageObject).Source;
                    }
                    else if (docFormPageObject is DocFormImage)
                    {
                        strKey = ((DocFormImage)docFormPageObject).Source;
                    }
                    else if (docFormPageObject is DocFormBarCode)
                    {
                        strKey = ((DocFormBarCode)docFormPageObject).Source;
                    }

                    if (pageData.ContainsKey(strKey))
                    {
                        docFormBlock.StaticObjects.Add(docFormPageObject);
                        docFormBlock.DynamicObjects.Remove(docFormPageObject);

                        docFormBlock.PossibleBlockData.Remove(strKey);
                        docFormBlock.PossibleBlockDataRecursive.Remove(strKey);
                    }
                }

                // Fix Block in Block
                foreach (DocFormBlock innerDocFormBlock in docFormBlock.Blocks)
                {
                    MoveBlockObjsWithPageDataInDynObjs(innerDocFormBlock, pageData);
                }
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.MoveBlockObjsWithPageDataInDynObjs - Failed", ex);
            }
        }

        private void OffsetObjects(List<PageObject> pageObjects, int offset)
        {
            try
            {
                foreach (PageObject pageObject in pageObjects)
                {
                    pageObject.Offset(0, offset);
                }
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.OffsetObjects - Failed", ex);
            }
        }

        private PageObjectText BuildPageObjectText(DocFormText docFormText, Dictionary<string, string> pageData, string strData, double scalingFactor)
        {
            PageObjectText pageObjectText;

            try
            {
                pageObjectText = new PageObjectText(scalingFactor);

                pageObjectText.Name = docFormText.Name;

                pageObjectText.Top = docFormText.Top;
                pageObjectText.Bottom = docFormText.Bottom;
                pageObjectText.Left = docFormText.Left;
                pageObjectText.Right = docFormText.Right;

                pageObjectText.PaddingTop = docFormText.PaddingTop;
                pageObjectText.PaddingBottom = docFormText.PaddingBottom;

                pageObjectText.Anchor = docFormText.Anchor;

                pageObjectText.XAlign = ConvertToAligns(docFormText.XAlign);
                pageObjectText.YAlign = ConvertToAligns(docFormText.YAlign);

                pageObjectText.Rotation = docFormText.Rotation;
                pageObjectText.MultiLine = docFormText.MultiLine;
                pageObjectText.Clip = docFormText.Clip;

                pageObjectText.Font = CreateFont(docFormText.Font); 
                pageObjectText.FontColor = docFormText.Font.Color;
                pageObjectText.FontBgColor = docFormText.Font.BgColor;

                if (docFormText.SourceType == "Data")
                {
                    pageObjectText.Text = docFormText.Source;
                }
                else if (docFormText.SourceType == "Variable")
                {
                    if (strData != null)
                    {
                        pageObjectText.Text = strData;
                    }
                    else
                    {
                        if (pageData.ContainsKey(docFormText.Source))
                        {
                            pageObjectText.Text = pageData[docFormText.Source];
                        }

                        if (pageObjectText.Text == null) pageObjectText.Text = string.Empty;
                    }
                }
                else if (docFormText.SourceType == "Macro")
                {
                    pageObjectText.Text = docFormText.Source;
                    pageObjectText.IsMacro = true;
                }

                if (pageObjectText.MultiLine == true && pageObjectText.Clip == false && (pageObjectText.Rotation == 0 || pageObjectText.Rotation == 180))
                {
                    pageObjectText.CalculateRequiredHeight();
                }
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildPageObjectText - Failed", ex);
                pageObjectText = null;
            }

            return pageObjectText;
        }

        private PageObjectImage BuildPageObjectImage(DocFormImage docFormImage, Dictionary<string, string> pageData, string strData, double scalingFactor)
        {
            PageObjectImage pageObjectImage;

            try
            {
                pageObjectImage = new PageObjectImage(scalingFactor);

                pageObjectImage.Name = docFormImage.Name;

                pageObjectImage.Top = docFormImage.Top;
                pageObjectImage.Bottom = docFormImage.Bottom;
                pageObjectImage.Left = docFormImage.Left;
                pageObjectImage.Right = docFormImage.Right;

                pageObjectImage.PaddingTop = docFormImage.PaddingTop;
                pageObjectImage.PaddingBottom = docFormImage.PaddingBottom;

                pageObjectImage.Anchor = docFormImage.Anchor;

                pageObjectImage.XAlign = ConvertToAligns(docFormImage.XAlign);
                pageObjectImage.YAlign = ConvertToAligns(docFormImage.YAlign);

                pageObjectImage.Rotation = docFormImage.Rotation;

                if (docFormImage.Source == string.Empty) return null;

                if (docFormImage.SourceType == "Data")
                {
                    //TODO: Where to store images?
                    pageObjectImage.ImageData = imageCache.GetImageData(docFormImage.Source);
                }
                else if (docFormImage.SourceType == "Variable")
                {
                    string strImageNamn = string.Empty;

                    if (strData != null)
                    {
                        if (strData == string.Empty) return null;

                        strImageNamn = strData;
                    }
                    else
                    {
                        if (pageData.ContainsKey(docFormImage.Source))
                        {
                            strImageNamn = pageData[docFormImage.Source];
                        }

                        if (strImageNamn == null || strImageNamn == string.Empty) return null;
                    }

                    pageObjectImage.ImageData = imageCache.GetImageData(strImageNamn);
                }

                if (pageObjectImage.ImageData == null)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildPageObjectImage - Failed", ex);
                pageObjectImage = null;
            }

            return pageObjectImage;
        }

        private PageObjectBarCode BuildPageObjectBarCode(DocFormBarCode docFormBarCode, Dictionary<string, string> pageData, string strData, double scalingFactor)
        {
            PageObjectBarCode pageObjectBarCode;

            try
            {
                pageObjectBarCode = new PageObjectBarCode(scalingFactor);

                pageObjectBarCode.Name = docFormBarCode.Name;

                pageObjectBarCode.Top = docFormBarCode.Top;
                pageObjectBarCode.Bottom = docFormBarCode.Bottom;
                pageObjectBarCode.Left = docFormBarCode.Left;
                pageObjectBarCode.Right = docFormBarCode.Right;

                pageObjectBarCode.PaddingTop = docFormBarCode.PaddingTop;
                pageObjectBarCode.PaddingBottom = docFormBarCode.PaddingBottom;

                pageObjectBarCode.Anchor = docFormBarCode.Anchor;

                pageObjectBarCode.XAlign = ConvertToAligns(docFormBarCode.XAlign);
                pageObjectBarCode.YAlign = ConvertToAligns(docFormBarCode.YAlign);

                pageObjectBarCode.Rotation = docFormBarCode.Rotation;

                pageObjectBarCode.BarCodeType = docFormBarCode.BarCodeType;
                pageObjectBarCode.CheckDigit = docFormBarCode.CheckDigit;

                pageObjectBarCode.BarHeight = (float)docFormBarCode.BarHeight;
                pageObjectBarCode.BarWidth = (float)docFormBarCode.BarWidth;
                pageObjectBarCode.BarRatio = (int)docFormBarCode.Ratio;

                if (docFormBarCode.SourceType == "Data")
                {
                    pageObjectBarCode.Data = docFormBarCode.Source;
                }
                else if (docFormBarCode.SourceType == "Variable")
                {
                    if (strData != null)
                    {
                        pageObjectBarCode.Data = strData;
                    }
                    else
                    {
                        if (pageData.ContainsKey(docFormBarCode.Source))
                        {
                            pageObjectBarCode.Data = pageData[docFormBarCode.Source];
                        }
                    }
                }

                // Remove if no data
                if (pageObjectBarCode.Data == null || pageObjectBarCode.Data == string.Empty) pageObjectBarCode = null;
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildPageObjectBarCode - Failed", ex);
                pageObjectBarCode = null;
            }

            return pageObjectBarCode;
        }

        private PageObjectLine BuildPageObjectLine(DocFormLine docFormLine, double scalingFactor)
        {
            PageObjectLine pageObjectLine;

            try
            {
                pageObjectLine = new PageObjectLine(scalingFactor);

                pageObjectLine.Name = docFormLine.Name;

                pageObjectLine.Top = docFormLine.Top;
                pageObjectLine.Bottom = docFormLine.Bottom;
                pageObjectLine.Left = docFormLine.Left;
                pageObjectLine.Right = docFormLine.Right;

                pageObjectLine.LineStart = docFormLine.LineStart;

                pageObjectLine.PaddingTop = docFormLine.PaddingTop;
                pageObjectLine.PaddingBottom = docFormLine.PaddingBottom;

                pageObjectLine.Anchor = docFormLine.Anchor;

                pageObjectLine.PenColor = docFormLine.PenColor;
                pageObjectLine.PenStyle = docFormLine.PenStyle;
                pageObjectLine.PenWidth = docFormLine.PenWidth;
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildPageObjectLine - Failed", ex);
                pageObjectLine = null;
            }

            return pageObjectLine;
        }

        private PageObjectRectangle BuildPageObjectRectangle(DocFormRectangle docFormRectangle, double scalingFactor)
        {
            PageObjectRectangle pageObjectRectangle;

            try
            {
                pageObjectRectangle = new PageObjectRectangle(scalingFactor);

                pageObjectRectangle.Name = docFormRectangle.Name;

                pageObjectRectangle.Top = docFormRectangle.Top;
                pageObjectRectangle.Bottom = docFormRectangle.Bottom;
                pageObjectRectangle.Left = docFormRectangle.Left;
                pageObjectRectangle.Right = docFormRectangle.Right;

                pageObjectRectangle.PaddingTop = docFormRectangle.PaddingTop;
                pageObjectRectangle.PaddingBottom = docFormRectangle.PaddingBottom;

                pageObjectRectangle.Anchor = docFormRectangle.Anchor;

                pageObjectRectangle.PenColor = docFormRectangle.PenColor;
                pageObjectRectangle.PenStyle = docFormRectangle.PenStyle;
                pageObjectRectangle.PenWidth = docFormRectangle.PenWidth;

                pageObjectRectangle.FillColor = docFormRectangle.FillColor;
                pageObjectRectangle.FillStyle = docFormRectangle.FillStyle;
                pageObjectRectangle.FillHatchStyle = docFormRectangle.FillHatchStyle;
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildPageObjectRectangle - Failed", ex);
                pageObjectRectangle = null;
            }

            return pageObjectRectangle;
        }

        private PageObjectEllipse BuildPageObjectEllipse(DocFormEllipse docFormEllipse, double scalingFactor)
        {
            PageObjectEllipse pageObjectEllipse;

            try
            {
                pageObjectEllipse = new PageObjectEllipse(scalingFactor);

                pageObjectEllipse.Name = docFormEllipse.Name;

                pageObjectEllipse.Top = docFormEllipse.Top;
                pageObjectEllipse.Bottom = docFormEllipse.Bottom;
                pageObjectEllipse.Left = docFormEllipse.Left;
                pageObjectEllipse.Right = docFormEllipse.Right;

                pageObjectEllipse.PaddingTop = docFormEllipse.PaddingTop;
                pageObjectEllipse.PaddingBottom = docFormEllipse.PaddingBottom;

                pageObjectEllipse.Anchor = docFormEllipse.Anchor;

                pageObjectEllipse.PenColor = docFormEllipse.PenColor;
                pageObjectEllipse.PenStyle = docFormEllipse.PenStyle;
                pageObjectEllipse.PenWidth = docFormEllipse.PenWidth;

                pageObjectEllipse.FillColor = docFormEllipse.FillColor;
                pageObjectEllipse.FillStyle = docFormEllipse.FillStyle;
                pageObjectEllipse.FillHatchStyle = docFormEllipse.FillHatchStyle;
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.BuildPageObjectEllipse - Failed", ex);
                pageObjectEllipse = null;
            }
            return pageObjectEllipse;
        }

        private Aligns ConvertToAligns(string strAlign)
        {
            Aligns aAlign = Aligns.Center;

            switch (strAlign.ToLower())
            {
                case "top":
                    aAlign = Aligns.Top;
                    break;

                case "bottom":
                    aAlign = Aligns.Bottom;
                    break;

                case "left":
                    aAlign = Aligns.Left;
                    break;

                case "right":
                    aAlign = Aligns.Right;
                    break;

                default:
                    break;
            }

            return aAlign;
        }

        private Font CreateFont(DocFormFont docFormFont)
        {
            Font font;
            float height = docFormFont.FontHeight * 254f / 72f;

            try
            {
                FontStyle fontStyle = FontStyle.Regular;

                if (docFormFont.Bold) fontStyle = fontStyle | FontStyle.Bold;
                if (docFormFont.Italic) fontStyle = fontStyle | FontStyle.Italic;
                if (docFormFont.Strikeout) fontStyle = fontStyle | FontStyle.Strikeout;
                if (docFormFont.Underline) fontStyle = fontStyle | FontStyle.Underline;

                //docFormFont.FontWidth;
                //docFormFont.PitchAndFamily;

                font = new Font(docFormFont.Face, height, fontStyle, GraphicsUnit.World);
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.ConvertToFont - Failed", ex);
                font = new Font("Courier New", 9f * 254f / 72f, GraphicsUnit.World);
            }

            return font;
        }

        private void RunMacro(PrintJobDocument printJobDocument)
        {
            try
            {
                string timeAsString = DateTime.Now.ToString("HH:mm");
                string dateAsString = DateTime.Now.ToString("yyyy-MM-dd");
                string totPages = printJobDocument.Pages.Count.ToString();
                int pageNumber = 0;

                foreach (Page page in printJobDocument.Pages)
                {
                    pageNumber++;

                    foreach (PageObject pageObject in page.PageObjects)
                    {
                        if (pageObject.GetType() == typeof(PageObjectText))
                        {
                            PageObjectText pot = (PageObjectText)pageObject;

                            if (pot.IsMacro)
                            {
                                pot.Text = pot.Text.Replace("#Time", timeAsString);
                                pot.Text = pot.Text.Replace("#Date", dateAsString);
                                pot.Text = pot.Text.Replace("#Page", pageNumber.ToString());
                                pot.Text = pot.Text.Replace("#TotPages", totPages);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.DocumentBuilder.RunMacro", ex);
            }
        }

    }
}