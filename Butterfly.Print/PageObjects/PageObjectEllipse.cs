namespace Butterfly.Print.PageObjects
{
    using System;
    using System.Drawing;

    using C1.C1Pdf;

    /// <summary>
    /// Summary description for PageObjectEllipse.
    /// </summary>
    public class PageObjectEllipse : PageObject
    {
        private float _penWidth;

        public PageObjectEllipse(double scalingFactor)
        {
            ScalingFactor = scalingFactor;

            PenColor = "000000";
            PenStyle = "Solid";
            PenWidth = 1;

            FillColor = "000000";
            FillStyle = "Hollow";
            FillHatchStyle = "Cross";
        }

        public string PenColor { get; set; }

        public string PenStyle { get; set; }

        public float PenWidth
        {
            get { return _penWidth; }
            set { _penWidth = ApplyScalingFactor(value); }
        }

        public string FillColor { get; set; }

        public string FillStyle { get; set; }

        public string FillHatchStyle { get; set; }

        public override void Draw(Graphics gfx)
        {
            try
            {
                DrawEllipse(gfx.VisibleClipBounds, gfx.FillEllipse, gfx.DrawEllipse);
            }
            catch (Exception ex)
            {
               throw new Exception("PageObjectEllipse.Draw failed.", ex);
            }
        }

        public override void Draw(C1PdfDocument docPdf)
        {
            try
            {
                DrawEllipse(docPdf.PageRectangle, docPdf.FillEllipse, docPdf.DrawEllipse);
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectEllipse.Draw-C1PdfDocument failed.", ex);
            }
        }

        private void DrawEllipse(
            RectangleF pageRectangle, 
            Action<Brush, float, float, float, float> ellipseFillAction, 
            Action<Pen, float, float, float, float> ellipseAction
        )
        {
            try
            {
                if (pageRectangle.IntersectsWith(new RectangleF(Left, Top, Right - Left, Bottom - Top)))
                {
                    using (var pen = CreatePen(PenColor, PenStyle, PenWidth))
                    {
                        using (var fill = CreateBrush(FillColor, FillStyle, FillHatchStyle))
                        {
                            if (fill != null)
                            {
                                ellipseFillAction(fill, Left, Top, Right - Left, Bottom - Top);
                            }

                            ellipseAction(pen, Left, Top, Right - Left, Bottom - Top);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectEllipse.Draw-C1PdfDocument failed.", ex);
            }
        }
    }
}