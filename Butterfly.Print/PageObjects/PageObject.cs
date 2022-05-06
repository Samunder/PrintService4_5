namespace Butterfly.Print.PageObjects
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    using C1.C1Pdf;

    // Enum Aligns
    public enum Aligns { Top, Left, Center, Right, Bottom };

    /// <summary>
    /// Summary description for PageObject.
    /// </summary>
    public abstract class PageObject
    {
        private int _top;
        private int _left;
        private int _right;
        private int _bottom;

        private int _paddingTop;
        private int _paddingBottom;

        public string Name { get; set; }

        public int Top
        {
            get { return _top; }
            set { _top = ApplyScalingFactor(value); }
        }

        public int Bottom
        {
            get { return _bottom; }
            set { _bottom = ApplyScalingFactor(value); }
        }

        public int Left
        {
            get { return _left; }
            set { _left = ApplyScalingFactor(value); }
        }

        public int Right
        {
            get { return _right; }
            set { _right = ApplyScalingFactor(value); }
        }

        public int PaddingTop
        {
            get { return _paddingTop; }
            set { _paddingTop = ApplyScalingFactor(value); }
        }

        public int PaddingBottom
        {
            get { return _paddingBottom; }
            set { _paddingBottom = ApplyScalingFactor(value); }
        }

        public string Anchor { get; set; }

        public double ScalingFactor { get; set; } = 1;

        public abstract void Draw(Graphics gfx);

        public abstract void Draw(C1PdfDocument docPdf);

        public void Offset(int xOffset, int yOffset, bool applyScale = false)
        {
            try
            {
                if (applyScale)
                {
                    _left += ApplyScalingFactor(xOffset);
                    _right += ApplyScalingFactor(xOffset);
                    _top += ApplyScalingFactor(yOffset);
                    _bottom += ApplyScalingFactor(yOffset);
                }
                else
                {
                    _left += xOffset;
                    _right += xOffset;
                    _top += yOffset;
                    _bottom += yOffset;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObject.Offset failed.", ex);

            }
        }

        protected Pen CreatePen(string penColor, string penStyle, float penWidth)
        {
            Pen pen = new Pen(Color.Black);

            if (penColor != "")
            {
                try
                {
                    int red = Convert.ToInt32(penColor.Substring(0, 2), 16);
                    int green = Convert.ToInt32(penColor.Substring(2, 2), 16);
                    int blue = Convert.ToInt32(penColor.Substring(4, 2), 16);

                    pen.Color = Color.FromArgb(red, green, blue);
                }
                catch (Exception ex)
                {
                    throw new Exception("Print.DocumentBuilder.CreatePen - Convert color failed", ex);
                }
            }

            pen.Width = penWidth;

            if (penStyle != "")
            {
                switch (penStyle.ToLower())
                {
                    case "dash":
                        pen.DashStyle = DashStyle.Dash;
                        break;

                    case "dot":
                        pen.DashStyle = DashStyle.Dot;
                        break;

                    case "dashdot":
                        pen.DashStyle = DashStyle.DashDot;
                        break;

                    case "dashdotdot":
                        pen.DashStyle = DashStyle.DashDotDot;
                        break;

                    default:
                        pen.DashStyle = DashStyle.Solid;
                        break;
                }
            }

            return pen;
        }

        protected Brush CreateBrush(string fillColor, string fillStyle, string fillHatchStyle)
        {
            //FillColor="0000AA" FillHatchStyle="BackDiagonal" FillStyle="Hatched"

            Brush brush = null;
            Color color = Color.Black;

            if (fillColor == "None") return null;

            if (fillColor != "")
            {
                try
                {
                    int red = Convert.ToInt32(fillColor.Substring(0, 2), 16);
                    int green = Convert.ToInt32(fillColor.Substring(2, 2), 16);
                    int blue = Convert.ToInt32(fillColor.Substring(4, 2), 16);

                    color = Color.FromArgb(red, green, blue);
                }
                catch (Exception ex)
                {
                    throw new Exception("Print.DocumentBuilder.CreateBrush - Convert fillColor failed", ex);
                }
            }

            if (fillStyle.ToLower() == "solid")
            {
                brush = new SolidBrush(color);
            }

            if (fillStyle.ToLower() == "hatched")
            {
                switch (fillHatchStyle.ToLower())
                {
                    case "backdiagonal":
                        brush = new HatchBrush(HatchStyle.BackwardDiagonal, color, Color.White);
                        break;

                    case "frontdiagonal":
                        brush = new HatchBrush(HatchStyle.ForwardDiagonal, color, Color.White);
                        break;

                    case "diagonalcross":
                        brush = new HatchBrush(HatchStyle.DiagonalCross, color, Color.White);
                        break;

                    case "horizontal":
                        brush = new HatchBrush(HatchStyle.Horizontal, color, Color.White);
                        break;

                    case "vertical":
                        brush = new HatchBrush(HatchStyle.Vertical, color, Color.White);
                        break;

                    default:
                        brush = new HatchBrush(HatchStyle.Cross, color, Color.White);
                        break;
                }
            }

            return brush;
        }

        public void SetTop(int value)
        {
            _top = value;
        }

        public void SetBottom(int value)
        {
            _bottom = value;
        }

        public int ApplyScalingFactor(int value)
        {
            if (value == 0)
            {
                return value;
            }

            return this.ScalingFactor == 1 ? value : Convert.ToInt16(value * this.ScalingFactor);
        }

        protected float ApplyScalingFactor(float value)
        {
            if (value == 0)
            {
                return 0;
            }

            return this.ScalingFactor == 1 ? value : (float)(value * this.ScalingFactor);
        }
    }
}