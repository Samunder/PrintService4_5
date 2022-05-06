namespace Butterfly.Print.DocFormObjects
{
    using System;
    using System.ComponentModel;
    using System.Xml;

    public class DocFormLine : DocFormPageObject
    {
        internal DocFormLine()
        {
            this.Name = "Line";

            this.LineStart = "TopLeft";

            this.PenColor = "000000";
            this.PenStyle = "Solid";
            this.PenWidth = 1;
        }

        [DefaultValue("TopLeft")]
        public string LineStart { get; set; }

        [DefaultValue("000000")]
        public string PenColor { get; set; }

        [DefaultValue("Solid")]
        public string PenStyle { get; set; }

        [DefaultValue(1)]
        public int PenWidth { get; set; }

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
                    else if (attr.Name == "StartY")
                    {
                        this.Top = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "EndY")
                    {
                        this.Bottom = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "StartX")
                    {
                        this.Left = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "EndX")
                    {
                        this.Right = int.Parse(attr.Value);
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
                    else if (attr.Name == "Anchor")
                    {
                        this.Anchor = attr.Value;
                    }
                }

                if ((this.Left < this.Right && this.Top > this.Bottom) || (this.Left > this.Right && this.Top < this.Bottom))
                {
                    this.LineStart = "TopRight";
                }

                if (this.Left > this.Right)
                {
                    int temp = Left;
                    this.Left = this.Right;
                    this.Right = temp;
                }

                if (this.Top > this.Bottom)
                {
                    int temp = this.Top;
                    this.Top = this.Bottom;
                    this.Bottom = temp;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Loading node failed.", ex);
            }
        }
    }
}