namespace Butterfly.Print.DocFormObjects
{
    using System;
    using System.ComponentModel;
    using System.Xml;

    public class DocFormFont
    {
        public DocFormFont()
        {
            this.Face = "Courier New";
            this.FontHeight = 9;
            this.FontWidth = 0;
            this.Bold = false;
            this.Italic = false;
            this.Underline = false;
            this.Strikeout = false;
            this.PitchAndFamily = 0;
            this.Color = "000000";
            this.BgColor = "None";
        }

        [DefaultValue("None")]
        public string BgColor { get; set; }

        [DefaultValue(false)]
        public bool Bold { get; set; }

        [DefaultValue("000000")]
        public string Color { get; set; }

        [DefaultValue("Courier New")]
        public string Face { get; set; }

        [DefaultValue(9)]
        public int FontHeight { get; set; }

        [DefaultValue(0)]
        public int FontWidth { get; set; }

        [DefaultValue(false)]
        public bool Italic { get; set; }

        [DefaultValue(0)]
        public int PitchAndFamily { get; set; }

        [DefaultValue(false)]
        public bool Strikeout { get; set; }

        [DefaultValue(false)]
        public bool Underline { get; set; }

        internal bool IsEqual(DocFormFont font)
        {
            DocFormFont font1 = this;
            DocFormFont font2 = font;

            if (font1.Face != font2.Face)
            {
                return false;
            }

            if (font1.FontHeight != font2.FontHeight)
            {
                return false;
            }

            if (font1.FontWidth != font2.FontWidth)
            {
                return false;
            }

            if (font1.Bold != font2.Bold)
            {
                return false;
            }

            if (font1.Italic != font2.Italic)
            {
                return false;
            }

            if (font1.Underline != font2.Underline)
            {
                return false;
            }

            if (font1.Strikeout != font2.Strikeout)
            {
                return false;
            }

            if (font1.PitchAndFamily != font2.PitchAndFamily)
            {
                return false;
            }

            if (font1.Color != font2.Color)
            {
                return false;
            }

            if (font1.BgColor != font2.BgColor)
            {
                return false;
            }

            return true;
        }

        internal void Load(XmlNode xmlNode)
        {
            try
            {
                foreach (XmlAttribute attr in xmlNode.Attributes)
                {
                    if (attr.Name == "Face")
                    {
                        this.Face = attr.Value;
                    }
                    else if (attr.Name == "FontHeight")
                    {
                        this.FontHeight = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "FontWidth")
                    {
                        this.FontWidth = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "Bold")
                    {
                        this.Bold = attr.Value == "True";
                    }
                    else if (attr.Name == "Italic")
                    {
                        this.Italic = attr.Value == "True";
                    }
                    else if (attr.Name == "Underline")
                    {
                        this.Underline = attr.Value == "True";
                    }
                    else if (attr.Name == "Strikeout")
                    {
                        this.Strikeout = attr.Value == "True";
                    }
                    else if (attr.Name == "PitchAndFamily")
                    {
                        this.PitchAndFamily = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "Color")
                    {
                        this.Color = attr.Value;
                    }
                    else if (attr.Name == "BgColor")
                    {
                        this.BgColor = attr.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Loading node failed.", ex);
            }
        }
    }
}