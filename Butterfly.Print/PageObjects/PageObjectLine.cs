namespace Butterfly.Print.PageObjects
{
    using System;
    using System.Drawing;

    using C1.C1Pdf;

    /// <summary>
    /// Summary description for PageObjectLine.
    /// </summary>
    public class PageObjectLine : PageObject
    {
        private float _penWidth;

        public PageObjectLine(double scalingFactor)
        {
            this.ScalingFactor = scalingFactor;

            this.LineStart = "TopLeft";
            this.PenColor = "000000";
            this.PenStyle = "Solid";
            this.PenWidth = 1;
        }

        public string LineStart { get; set; }

        public string PenColor { get; set; }

        public string PenStyle { get; set; }

        public float PenWidth
        {
            get { return _penWidth; }
            set { _penWidth = ApplyScalingFactor(value); }
        }

        public override void Draw(Graphics gfx)
        {
            try
            {
                DrawLine(gfx.VisibleClipBounds, gfx.DrawLine);
            }
            catch (Exception ex)
            {
               throw new Exception("PageObjectLine.Draw failed.", ex);
            }
        }

        public override void Draw(C1PdfDocument docPdf)
        {
            try
            {
                DrawLine(docPdf.PageRectangle, docPdf.DrawLine);
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectLine.Draw-C1PdfDocument failed.", ex);
            }
        }

        private void DrawLine(RectangleF pageRectangle, Action<Pen, float, float, float, float> drawLineAction)
        {
            if (pageRectangle.IntersectsWith(new RectangleF(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top)))
            {
                using (var pen = CreatePen(this.PenColor, this.PenStyle, this.PenWidth))
                {
                    if (this.LineStart == "TopLeft")
                    {
                        drawLineAction(pen, this.Left, this.Top, this.Right, this.Bottom);
                    }
                    else
                    {
                        drawLineAction(pen, this.Left, this.Bottom, this.Right, this.Top);
                    }
                }
            }
        }
    }
}