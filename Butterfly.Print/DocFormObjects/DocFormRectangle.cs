namespace Butterfly.Print.DocFormObjects
{
    using System;
    using System.ComponentModel;
    using System.Xml;

    public class DocFormRectangle : DocFormPageObject
    {
        public DocFormRectangle()
        {
            this.Name = "Rectangle";

            this.Radius = 0;

            this.PenColor = "000000";
            this.PenStyle = "Solid";
            this.PenWidth = 1;

            this.FillColor = "000000";
            this.FillStyle = "Hollow";
            this.FillHatchStyle = "Cross";
        }

        [DefaultValue("000000")]
        public string FillColor { get; set; }

        [DefaultValue("Cross")]
        public string FillHatchStyle { get; set; }

        [DefaultValue("Hollow")]
        public string FillStyle { get; set; }

        [DefaultValue("000000")]
        public string PenColor { get; set; }

        [DefaultValue("Solid")]
        public string PenStyle { get; set; }

        [DefaultValue(1)]
        public int PenWidth { get; set; }

        [DefaultValue(0)]
        public int Radius { get; set; }

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
                    else if (attr.Name == "Radius")
                    {
                        this.Radius = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "PenColor")
                    {
                        this.PenColor = attr.Value;
                    }
                    else if (attr.Name == "PenStyle")
                    {
                        this.PenStyle = attr.Value;
                    }
                    else if (attr.Name == "PenWidth")
                    {
                        this.PenWidth = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "FillColor")
                    {
                        this.FillColor = attr.Value;
                    }
                    else if (attr.Name == "FillStyle")
                    {
                        this.FillStyle = attr.Value;
                    }
                    else if (attr.Name == "FillHatchStyle")
                    {
                        this.FillHatchStyle = attr.Value;
                    }
                    else if (attr.Name == "Anchor")
                    {
                        this.Anchor = attr.Value;
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