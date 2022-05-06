namespace Butterfly.Print.DocFormObjects
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xml;

    public class DocFormBarCode : DocFormPageObject
    {
        public DocFormBarCode()
        {
            this.Name = "BarCode";

            this.Source = string.Empty;
            this.SourceType = "Data";
            this.SourceEncoding = "None";

            this.XAlign = "Left";
            this.YAlign = "Bottom";

            this.Rotation = 0;

            this.BarCodeType = "3of9";
            this.CheckDigit = false;
            this.C39Ascii = false;

            this.BarHeight = 100;
            this.BarWidth = 3.0;
            this.Ratio = 20;

            this.ShrinkToFit = false;
            this.Clip = false;

            this.IsStatic = true;
        }

        [DefaultValue("3of9")]
        public string BarCodeType { get; set; }

        [DefaultValue(100)]
        public double BarHeight { get; set; }

        [DefaultValue(3.0)]
        public double BarWidth { get; set; }

        [DefaultValue(false)]
        public bool C39Ascii { get; set; }

        [DefaultValue(false)]
        public bool CheckDigit { get; set; }

        [DefaultValue(false)]
        public bool Clip { get; set; }

        [DefaultValue(20)]
        public double Ratio { get; set; }

        [DefaultValue(0)]
        public int Rotation { get; set; }

        [DefaultValue(false)]
        public bool ShrinkToFit { get; set; }

        [DefaultValue("")]
        public string Source { get; set; }

        [DefaultValue("None")]
        public string SourceEncoding { get; set; }

        [DefaultValue("Data")]
        public string SourceType { get; set; }

        [DefaultValue("Left")]
        public string XAlign { get; set; }

        [DefaultValue("Bottom")]
        public string YAlign { get; set; }

        [DefaultValue(true)]
        public bool IsStatic { get; set; }

        internal void Load(XmlNode node)
        {
            try
            {
                // Add Attributes
                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == "Name")
                    {
                        this.Name = attr.Value;
                    }
                    else if (attr.Name == "SourceType")
                    {
                        this.SourceType = attr.Value;
                    }
                    else if (attr.Name == "Top")
                    {
                        this.Top = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "Bottom")
                    {
                        this.Bottom = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "Left")
                    {
                        this.Left = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "Right")
                    {
                        this.Right = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "XAlign")
                    {
                        this.XAlign = attr.Value;
                    }
                    else if (attr.Name == "YAlign")
                    {
                        this.YAlign = attr.Value;
                    }
                    else if (attr.Name == "Rotation")
                    {
                        this.Rotation = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "BarCodeType")
                    {
                        this.BarCodeType = attr.Value;
                    }
                    else if (attr.Name == "CheckDigit")
                    {
                        this.CheckDigit = attr.Value == "True";
                    }
                    else if (attr.Name == "C39Ascii")
                    {
                        this.C39Ascii = attr.Value == "True";
                    }
                    else if (attr.Name == "BarHeight")
                    {
                        this.BarHeight = double.Parse(attr.Value, CultureInfo.InvariantCulture);
                    }
                    else if (attr.Name == "BarWidth")
                    {
                        this.BarWidth = double.Parse(attr.Value, CultureInfo.InvariantCulture);
                    }
                    else if (attr.Name == "Ratio")
                    {
                        this.Ratio = double.Parse(attr.Value, CultureInfo.InvariantCulture);
                    }
                    else if (attr.Name == "Anchor")
                    {
                        this.Anchor = attr.Value;
                    }
                }

                // Add inner text
                this.Source = node.InnerText;
            }
            catch (Exception ex)
            {
                throw new Exception("Loading node failed.", ex);
            }
        }
    }
}