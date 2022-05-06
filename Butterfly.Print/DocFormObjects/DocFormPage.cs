namespace Butterfly.Print.DocFormObjects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml;
    using Butterfly.Print.PrintJobObjects;
    using Newtonsoft.Json;

    public class DocFormPage
    {
        private bool scalingApplied = false;

        public DocFormPage()
        {
            this.Name = "Page";
            this.PageWidth = 2100;
            this.PageHeight = 2970;
            this.Orientation = "Portrait";
            this.PageSize = 9;
            this.DiscardIfStatic = false;
            this.Objects = new List<DocFormPageObject>();
            this.Blocks = new List<DocFormBlock>();

            this.PossiblePageData = new List<string>();
            this.PossibleBlockDataRecursive = new List<string>();
        }

        public double PageScale { get; set; } = 1;

        public List<DocFormBlock> Blocks { get; set; }

        [DefaultValue(false)]
        public bool DiscardIfStatic { get; set; }

        [DefaultValue(null)]
        public DocFormFont Font { get; set; }

        public string Name { get; set; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<DocFormPageObject> Objects { get; set; }

        [DefaultValue("Portrait")]
        public string Orientation { get; set; }

        public int PageHeight { get; set; }

        public int PageSize { get; set; }

        public int PageWidth { get; set; }

        [JsonIgnore]
        public DocFormLayout Parent { get; set; }

        [JsonIgnore]
        internal List<string> PossiblePageData { get; set; }

        [JsonIgnore]
        internal List<string> PossibleBlockDataRecursive { get; set; }

        internal void InitializePage(DocFormLayout parent)
        {
            try
            {
                Parent = parent;

                foreach (var pageObject in Objects)
                {
                    pageObject.PaddingTop = pageObject.Top;
                    pageObject.PaddingBottom = this.PageHeight - pageObject.Bottom;

                    if (pageObject is DocFormText)
                    {
                        if ((((DocFormText)pageObject).SourceType == "Variable") || (((DocFormText)pageObject).SourceType == "VarFile") || (((DocFormText)pageObject).SourceType == "VarTemp"))
                        {
                            PossiblePageData.Add(((DocFormText)pageObject).Source);
                            ((DocFormText)pageObject).IsStatic = false;
                        }
                    }
                    else if (pageObject is DocFormBarCode)
                    {
                        if ((((DocFormBarCode)pageObject).SourceType == "Variable") || (((DocFormBarCode)pageObject).SourceType == "VarFile") || (((DocFormBarCode)pageObject).SourceType == "VarTemp"))
                        {
                            PossiblePageData.Add(((DocFormBarCode)pageObject).Source);
                            ((DocFormBarCode)pageObject).IsStatic = false;
                        }
                    }
                    else if (pageObject is DocFormImage)
                    {
                        if ((((DocFormImage)pageObject).SourceType == "Variable") || (((DocFormImage)pageObject).SourceType == "VarFile") || (((DocFormImage)pageObject).SourceType == "VarTemp"))
                        {
                            PossiblePageData.Add(((DocFormImage)pageObject).Source);
                            ((DocFormImage)pageObject).IsStatic = false;
                        }
                    }
                }

                foreach (DocFormBlock block in Blocks)
                {
                    block.PaddingTop = block.Top;
                    block.PaddingBottom = this.PageHeight - block.Bottom;

                    block.InitializeBlock(parent.DynamicTargets, block.Name);

                    PossibleBlockDataRecursive.AddRange(block.PossibleBlockDataRecursive);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Print.DocFormPage.PopulatePossibleDataCollections - Failed", ex);
            }
        }

        internal bool IsBlockDataMatching(List<PrintJobDataItem> blockData)
        {
            try
            {
                List<string> alKeys = new List<string>();

                foreach (PrintJobDataItem di in blockData)
                {
                    if (alKeys.IndexOf(di.Key) == -1)
                    {
                        alKeys.Add(di.Key);
                    }
                }

                foreach (string strPossSource in PossibleBlockDataRecursive)
                {
                    if (alKeys.IndexOf(strPossSource) != -1) return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Print.DocFormPage.IsBlockDataMatching - Failed", ex);
            }
        }

        internal void Load(XmlNode pageNode)
        {
            try
            {
                // Add Attributes
                foreach (XmlAttribute attr in pageNode.Attributes)
                {
                    if (attr.Name == "Name")
                    {
                        this.Name = attr.Value;
                    }
                    else if (attr.Name == "PageWidth")
                    {
                        this.PageWidth = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "PageHeight")
                    {
                        this.PageHeight = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "Orientation")
                    {
                        this.Orientation = attr.Value;
                    }
                    else if (attr.Name == "PageSize")
                    {
                        this.PageSize = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "DiscardIfStatic")
                    {
                        this.DiscardIfStatic = attr.Value == "True";
                    }
                }

                // Add ChildNodes
                foreach (XmlNode xmlNode in pageNode.ChildNodes)
                {
                    if (xmlNode.Name == "Defaults")
                    {
                        foreach (XmlNode fontNode in xmlNode.ChildNodes)
                        {
                            if (fontNode.Name == "Font")
                            {
                                this.Font = new DocFormFont();
                                this.Font.Load(fontNode);
                            }
                        }
                    }
                    else if (xmlNode.Name == "Font")
                    {
                        DocFormFont tempFont = new DocFormFont();
                        tempFont.Load(xmlNode);

                        foreach (XmlNode objectNode in xmlNode.ChildNodes)
                        {
                            if (objectNode.Name == "Text")
                            {
                                DocFormText docFormText = new DocFormText();
                                docFormText.Font = tempFont;
                                docFormText.Load(objectNode);

                                this.Objects.Add(docFormText);
                            }
                            else if (objectNode.Name == "BarCode")
                            {
                                DocFormBarCode docFormBarCode = new DocFormBarCode();
                                docFormBarCode.Load(objectNode);

                                this.Objects.Add(docFormBarCode);
                            }
                        }
                    }
                    else if (xmlNode.Name == "Text")
                    {
                        DocFormText docFormText = new DocFormText();
                        docFormText.Font = this.Font;
                        docFormText.Load(xmlNode);

                        this.Objects.Add(docFormText);
                    }
                    else if (xmlNode.Name == "BarCode")
                    {
                        DocFormBarCode docFormBarCode = new DocFormBarCode();
                        docFormBarCode.Load(xmlNode);

                        this.Objects.Add(docFormBarCode);
                    }
                    else if (xmlNode.Name == "Image")
                    {
                        DocFormImage docFormImage = new DocFormImage();
                        docFormImage.Load(xmlNode);

                        this.Objects.Add(docFormImage);
                    }
                    else if (xmlNode.Name == "Rectangle")
                    {
                        DocFormRectangle docFormRect = new DocFormRectangle();
                        docFormRect.Load(xmlNode);

                        this.Objects.Add(docFormRect);
                    }
                    else if (xmlNode.Name == "Line")
                    {
                        DocFormLine docFormLine = new DocFormLine();
                        docFormLine.Load(xmlNode);

                        this.Objects.Add(docFormLine);
                    }
                    else if (xmlNode.Name == "Ellipse")
                    {
                        DocFormEllipse docFormEllipse = new DocFormEllipse();
                        docFormEllipse.Load(xmlNode);

                        this.Objects.Add(docFormEllipse);
                    }
                    else if (xmlNode.Name == "Block")
                    {
                        DocFormBlock docFormBlock = new DocFormBlock();
                        docFormBlock.Load(xmlNode, this.Font);

                        this.Blocks.Add(docFormBlock);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Loading node failed.", ex);
            }
        }

        public void ApplyScaling()
        {
            if (!scalingApplied)
            {
                PageScale = Parent.ScaleFactor;
                scalingApplied = true;
                PageWidth = ApplyScalingFactor(this.PageWidth);
                PageHeight = ApplyScalingFactor(this.PageHeight);

                //Apply scaling on Blocks(DocFormBlock) not on static and dynamic block objects(DocFormPageObject)
                //to get correct calculation of blocks height and width with dynamic data (like multiline text box)
                foreach (var block in Blocks)
                {
                    block.ApplyScaling(PageScale);
                }
            }
        }

        public int ApplyScalingFactor(int value)
        {
            if (value == 0)
            {
                return value;
            }

            return this.PageScale == 1 ? value : Convert.ToInt16(value * this.PageScale);
        }
    }
}