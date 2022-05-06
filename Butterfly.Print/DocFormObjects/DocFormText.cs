namespace Butterfly.Print.DocFormObjects
{
    using System;
    using System.ComponentModel;
    using System.Xml;

    public class DocFormText : DocFormPageObject
    {
        public DocFormText()
        {
            this.Name = "Text";

            this.Source = string.Empty;
            this.SourceType = "Data";
            this.SourceEncoding = "None";

            this.XAlign = "Left";
            this.YAlign = "Bottom";

            this.Rotation = 0;
            this.MultiLine = false;
            this.Clip = true;

            this.IsStatic = true;

            this.Font = null;
        }

        [DefaultValue(true)]
        public bool Clip { get; set; }

        [DefaultValue(null)]
        public DocFormFont Font { get; set; }

        [DefaultValue(false)]
        public bool MultiLine { get; set; }

        [DefaultValue(0)]
        public int Rotation { get; set; }

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
                    else if (attr.Name == "Multiline")
                    {
                        this.MultiLine = attr.Value == "True";
                    }
                    else if (attr.Name == "Clip")
                    {
                        this.Clip = attr.Value == "True";
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