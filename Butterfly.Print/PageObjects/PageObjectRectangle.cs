namespace Butterfly.Print.PageObjects
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    using C1.C1Pdf;

    /// <summary>
    /// Summary description for PageObjectRectangle.
    /// </summary>
    public class PageObjectRectangle : PageObject
    {
        private float _penWidth;

        public PageObjectRectangle(double scalingFactor)
        {
            this.ScalingFactor = scalingFactor;

            this.Radius = 0;

            this.PenColor = "000000";
            this.PenStyle = "Solid";
            this.PenWidth = 1;

            this.FillColor = "000000";
            this.FillStyle = "Hollow";
            this.FillHatchStyle = "Cross";
        }

        public int Radius { get; set; }

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
                if (gfx.VisibleClipBounds.IntersectsWith(new RectangleF(Left, Top, Right - Left, Bottom - Top)))
                {
                    using (var pen = this.CreatePen(this.PenColor, this.PenStyle, this.PenWidth))
                    {
                        using (var fill = this.CreateBrush(this.FillColor, this.FillStyle, this.FillHatchStyle))
                        {
                            if (this.Radius == 0)
                            {
                                if (fill != null)
                                {
                                    gfx.FillRectangle(fill, this.Left, this.Top, this.Right - this.Left,
                                        this.Bottom - this.Top);
                                }

                                gfx.DrawRectangle(pen, this.Left, this.Top, this.Right - this.Left,
                                    this.Bottom - this.Top);
                            }
                            else
                            {
                                using (var g = new GraphicsPath())
                                {
                                    var diameter = this.Radius * 2;
                                    g.AddArc(this.Left + pen.Width, this.Top, diameter, diameter, 180, 90);
                                    g.AddArc(this.Left + (this.Right - this.Left - diameter - pen.Width), this.Top,
                                        diameter,
                                        diameter, 270, 90);
                                    g.AddArc(this.Left + (this.Right - this.Left - diameter - pen.Width),
                                        this.Top + (this.Bottom - this.Top - diameter - pen.Width), diameter, diameter,
                                        360,
                                        90);
                                    g.AddArc(this.Left + pen.Width,
                                        this.Top + (this.Bottom - this.Top - diameter - pen.Width),
                                        diameter, diameter, 90, 90);
                                    g.CloseFigure();

                                    if (fill != null)
                                    {
                                        gfx.FillPath(fill, g);
                                    }

                                    gfx.DrawPath(pen, g);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectRectangle.Draw failed.", ex);
            }
        }

        public override void Draw(C1PdfDocument docPdf)
        {
            try
            {
                if (docPdf.PageRectangle.IntersectsWith(new RectangleF(Left, Top, Right - Left, Bottom - Top)))
                {
                    using (var pen = this.CreatePen(this.PenColor, this.PenStyle, this.PenWidth))
                    {
                        using (var fill = this.CreateBrush(this.FillColor, this.FillStyle, this.FillHatchStyle))
                        {
                            if (this.Radius == 0)
                            {
                                if (fill != null)
                                {
                                    docPdf.FillRectangle(fill, this.Left, this.Top, this.Right - this.Left,
                                        this.Bottom - this.Top);
                                }

                                docPdf.DrawRectangle(pen, this.Left, this.Top, this.Right - this.Left,
                                    this.Bottom - this.Top);
                            }
                            else
                            {
                                using (var g = new GraphicsPath())
                                {
                                    var diameter = this.Radius * 2;
                                    g.AddArc(this.Left + pen.Width, this.Top, diameter, diameter, 180, 90);
                                    g.AddArc(this.Left + (this.Right - this.Left - diameter - pen.Width), this.Top,
                                        diameter,
                                        diameter, 270, 90);
                                    g.AddArc(this.Left + (this.Right - this.Left - diameter - pen.Width),
                                        this.Top + (this.Bottom - this.Top - diameter - pen.Width), diameter, diameter,
                                        360,
                                        90);
                                    g.AddArc(this.Left + pen.Width,
                                        this.Top + (this.Bottom - this.Top - diameter - pen.Width),
                                        diameter, diameter, 90, 90);
                                    g.CloseFigure();

                                    if (fill != null)
                                    {
                                        docPdf.FillPath(fill, g);
                                    }

                                    docPdf.DrawPath(pen, g);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectRectangle.Draw-C1PdfDocument failed.", ex);
            }
        }
    }
}