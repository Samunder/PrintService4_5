namespace Butterfly.Print.DocFormObjects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml;

    using Newtonsoft.Json;

    public class DocFormBlock : DocFormPageObject
    {
        private bool scalingApplied = false;
        
        public DocFormBlock()
        {
            this.Name = "Block";

            this.DockToBlock = string.Empty;
            this.DockOffset = 0;

            this.RepeatsTo = 0;
            this.AllowSplit = true;
            this.DiscardIfStatic = false;
            this.ShrinkToContent = false;

            this.Objects = new List<DocFormPageObject>();
            this.StaticObjects = new List<DocFormPageObject>();
            this.DynamicObjects = new List<DocFormPageObject>();
            this.Blocks = new List<DocFormBlock>();

            PossibleBlockData = new List<string>();
            PossibleBlockDataRecursive = new List<string>();
            PossibleBlockInBlockData = new List<string>();

            DockedBlock = null;
        }

        [DefaultValue(true)]
        public bool AllowSplit { get; set; }

        [DefaultValue(false)]
        public bool ShrinkToContent { get; set; }

        [DefaultValue(false)]
        public bool DiscardIfStatic { get; set; }

        [DefaultValue(0)]
        public int DockOffset { get; set; }

        [DefaultValue("")]
        public string DockToBlock { get; set; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<DocFormPageObject> Objects { get; set; }

        [DefaultValue(0)]
        public int RepeatsTo { get; set; }

        public List<DocFormBlock> Blocks
        {
            get;
            set;
        }

        [JsonIgnore]
        public List<DocFormPageObject> StaticObjects { get; set; }

        [JsonIgnore]
        public List<DocFormPageObject> DynamicObjects { get; set; }

        [JsonIgnore]
        internal List<string> PossibleBlockData { get; set; }

        [JsonIgnore]
        internal List<string> PossibleBlockDataRecursive { get; set; }

        [JsonIgnore]
        internal List<string> PossibleBlockInBlockData { get; set; }

        [JsonIgnore]
        internal DocFormBlock DockedBlock { get; set; }

        public void InitializeBlock(List<DocFormDynamicTargetItem> dynamicTargets, string blockPath)
        {
            try
            {
                foreach (DocFormPageObject pageObject in Objects)
                {
                    pageObject.PaddingTop = pageObject.Top;
                    pageObject.PaddingBottom = (this.Bottom - this.Top) - pageObject.Bottom;

                    if (pageObject is DocFormText)
                    {
                        if ((((DocFormText)pageObject).SourceType == "Variable") || (((DocFormText)pageObject).SourceType == "VarFile") || (((DocFormText)pageObject).SourceType == "VarTemp"))
                        {
                            PossibleBlockData.Add(((DocFormText)pageObject).Source);
                            ((DocFormText)pageObject).IsStatic = false;
                            DynamicObjects.Add(pageObject);
                            dynamicTargets.Add(new DocFormDynamicTargetItem(((DocFormText)pageObject).Source, blockPath));
                        }
                        else
                        {
                            StaticObjects.Add(pageObject);
                        }
                    }
                    else if (pageObject is DocFormBarCode)
                    {
                        if ((((DocFormBarCode)pageObject).SourceType == "Variable") || (((DocFormBarCode)pageObject).SourceType == "VarFile") || (((DocFormBarCode)pageObject).SourceType == "VarTemp"))
                        {
                            PossibleBlockData.Add(((DocFormBarCode)pageObject).Source);
                            ((DocFormBarCode)pageObject).IsStatic = false;
                            DynamicObjects.Add(pageObject);
                            dynamicTargets.Add(new DocFormDynamicTargetItem(((DocFormBarCode)pageObject).Source, blockPath));
                        }
                        else
                        {
                            StaticObjects.Add(pageObject);
                        }
                    }
                    else if (pageObject is DocFormImage)
                    {
                        if ((((DocFormImage)pageObject).SourceType == "Variable") || (((DocFormImage)pageObject).SourceType == "VarFile") || (((DocFormImage)pageObject).SourceType == "VarTemp"))
                        {
                            PossibleBlockData.Add(((DocFormImage)pageObject).Source);
                            ((DocFormImage)pageObject).IsStatic = false;
                            DynamicObjects.Add(pageObject);
                            dynamicTargets.Add(new DocFormDynamicTargetItem(((DocFormImage)pageObject).Source, blockPath));
                        }
                        else
                        {
                            StaticObjects.Add(pageObject);
                        }
                    }
                    else
                    {
                        StaticObjects.Add(pageObject);
                    }
                }

                foreach (DocFormBlock block in Blocks)
                {
                    block.PaddingTop = block.Top;
                    block.PaddingBottom = (this.Bottom - this.Top) - block.Bottom;

                    block.InitializeBlock(dynamicTargets, blockPath + "." + block.Name);

                    PossibleBlockDataRecursive.AddRange(block.PossibleBlockDataRecursive);
                    PossibleBlockInBlockData.AddRange(block.PossibleBlockDataRecursive);
                }

                PossibleBlockDataRecursive.AddRange(PossibleBlockData);

                // Get PossibleBlockDataRecursive from docked blocks
                foreach (DocFormBlock block in Blocks)
                {
                    if (block.DockToBlock == "")
                    {
                        GetDockedBlocksPossibleData(block);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Print.DocFormBlock.InitializeBlock - Failed", ex);
            }
        }

        internal void Load(XmlNode blockNode, DocFormFont parentFont)
        {
            try
            {
                // Add Attributes
                foreach (XmlAttribute attr in blockNode.Attributes)
                {
                    if (attr.Name == "Name")
                    {
                        this.Name = attr.Value;
                    }
                    else if (attr.Name == "Top")
                    {
                        this.Top = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "Bottom")
                    {
                        this.Bottom = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "DockToBlock")
                    {
                        this.DockToBlock = attr.Value;
                    }
                    else if (attr.Name == "DockOffset")
                    {
                        this.DockOffset = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "Anchor")
                    {
                        this.Anchor = attr.Value;
                    }
                    else if (attr.Name == "RepeatsTo")
                    {
                        this.RepeatsTo = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "AllowSplit")
                    {
                        this.AllowSplit = attr.Value == "True";
                    }
                    else if (attr.Name == "DiscardIfStatic")
                    {
                        this.DiscardIfStatic = attr.Value == "True";
                    }
                }

                // Add ChildNodes
                foreach (XmlNode xmlNode in blockNode.ChildNodes)
                {
                    if (xmlNode.Name == "Font")
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
                        docFormText.Font = parentFont;
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
                        docFormBlock.Load(xmlNode, parentFont);

                        this.Blocks.Add(docFormBlock);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Loading node failed.", ex);
            }
        }

        private void GetDockedBlocksPossibleData(DocFormBlock topBlock)
        {
            try
            {
                foreach (DocFormBlock block in Blocks)
                {
                    if (block.DockToBlock == topBlock.Name)
                    {
                        GetDockedBlocksPossibleData(block);

                        topBlock.PossibleBlockDataRecursive.AddRange(block.PossibleBlockDataRecursive);
                        topBlock.DockedBlock = block;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Print.DocFormBlock.GetDockedBlocksPossibleData - Failed", ex);
            }
        }

        internal DocFormPageObject GetDynamicObjectByKey(string key)
        {
            try
            {
                foreach (DocFormPageObject obj in DynamicObjects)
                {
                    if (obj is DocFormText)
                    {
                        if (((DocFormText)obj).Source == key) return obj;
                    }
                    else if (obj is DocFormImage)
                    {
                        if (((DocFormImage)obj).Source == key) return obj;
                    }
                    else if (obj is DocFormBarCode)
                    {
                        if (((DocFormBarCode)obj).Source == key) return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Print.DocFormBlock.GetDynamicObjectByKey - Failed", ex);
            }

            return null;
        }

        public void ApplyScaling(double scalingFactor)
        {
            if (!scalingApplied)
            {
                ScalingFactor = scalingFactor;
                RepeatsTo = ApplyScalingFactor(RepeatsTo);
                Top = ApplyScalingFactor(Top);
                Bottom = ApplyScalingFactor(Bottom);
                Left = ApplyScalingFactor(Left);
                Right = ApplyScalingFactor(Right);
                PaddingTop = ApplyScalingFactor(PaddingTop);
                PaddingBottom = ApplyScalingFactor(PaddingBottom);

                foreach (var block in Blocks)
                {
                    block.ApplyScaling(ScalingFactor);
                }

                scalingApplied = true;
            }
        }
    }
}