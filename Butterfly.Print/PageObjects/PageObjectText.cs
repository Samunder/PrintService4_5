namespace Butterfly.Print.PageObjects
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    using C1.C1Pdf;

    /// <summary>
    /// Summary description for PageObjectText.
    /// </summary>
    public class PageObjectText : PageObject, IDisposable
    {
        private bool disposed = false;
        private Font _font;

        public PageObjectText(double scalingFactor)
        {
            ScalingFactor = scalingFactor;

            XAlign = Aligns.Left;
            YAlign = Aligns.Bottom;

            MultiLine = false;
            Clip = true;
            IsMacro = false;

            Rotation = 0;

            Font = new Font("Courier New", 9f * 254f / 72f, GraphicsUnit.World);

            FontColor = "000000";
            FontBgColor = "None";

            Text = "";
        }

        public Aligns XAlign { get; set; }

        public Aligns YAlign { get; set; }

        public bool MultiLine { get; set; }

        public bool Clip { get; set; }

        public bool IsMacro { get; set; }

        public int Rotation { get; set; }

        public Font Font
        {
            get { return _font; }
            set { _font = new Font(value.FontFamily, ApplyScalingFactor(value.Size), value.Unit); }
        }

        public string FontBgColor { get; set; }

        public string FontColor { get; set; }

        public string Text { get; set; }

        public override void Draw(Graphics gfx)
        {
            try
            {
                if (gfx.VisibleClipBounds.IntersectsWith(new RectangleF(Left, Top, Right - Left, Bottom - Top)))
                {
                    SizeF sizeText = new SizeF();
                    int iTextX = Left;
                    int iTextY = Top;
                    int iXErr = 0;
                    float fMultiLineWidth = 0;

                    if (Text != "")
                    {
                        if (MultiLine)
                        {
                            RectangleF layoutRect;
                            CharacterRange[] characterRanges = { new CharacterRange(0, Text.Length) };
                            using (var stringFormat = new StringFormat())
                            {
                                stringFormat.SetMeasurableCharacterRanges(characterRanges);

                                if ((Rotation == 90) || (Rotation == 270)) fMultiLineWidth = Bottom - Top;
                                else fMultiLineWidth = Right - Left;

                                layoutRect = new RectangleF(Left, Top, fMultiLineWidth, 10000);

                                var stringRegions = gfx.MeasureCharacterRanges(Text, Font, layoutRect, stringFormat);
                                using (var region = stringRegions[0])
                                {
                                    var measureRect1 = region.GetBounds(gfx);

                                    // DEBUG: Draw rectText
                                    //gfx.DrawRectangle(Pens.Green, measureRect1.Left, measureRect1.Top, measureRect1.Width, measureRect1.Height);

                                    sizeText.Width = measureRect1.Width;
                                    sizeText.Height = measureRect1.Height;

                                    iXErr = (int) measureRect1.Left - Left;
                                }
                            }
                        }
                        else
                        {
                            CharacterRange[] characterRanges = { new CharacterRange(0, Text.Length) };
                            using (var stringFormat = new StringFormat())
                            {
                                stringFormat.SetMeasurableCharacterRanges(characterRanges);
                                RectangleF layoutRect = new RectangleF(Left, Top, 10000, 10000);
                                var stringRegions = gfx.MeasureCharacterRanges(Text, Font, layoutRect, stringFormat);
                                using (var region = stringRegions[0])
                                {
                                    var measureRect1 = region.GetBounds(gfx);

                                    sizeText.Width = measureRect1.Width;
                                    sizeText.Height = measureRect1.Height;

                                    iXErr = (int) measureRect1.Left - Left;
                                }
                            }
                        }
                    }

                    // Backup Matrix
                    Matrix oldMatrix = gfx.Transform.Clone();
                    Matrix tempMatrix = gfx.Transform.Clone();

                    if (Rotation == 0)
                    {
                        if (XAlign == Aligns.Left) iTextX = Left - iXErr;
                        if (XAlign == Aligns.Center) iTextX = Left - iXErr + (Right - Left - (int)sizeText.Width) / 2;
                        if (XAlign == Aligns.Right) iTextX = Right - (int)sizeText.Width - iXErr;

                        if (YAlign == Aligns.Top) iTextY = Top;
                        if (YAlign == Aligns.Center) iTextY = Top + (Bottom - Top - (int)sizeText.Height) / 2;
                        if (YAlign == Aligns.Bottom) iTextY = Bottom - (int)sizeText.Height;
                    }
                    else if (Rotation == 90)
                    {
                        if (XAlign == Aligns.Left) iTextY = Bottom + iXErr;
                        if (XAlign == Aligns.Center) iTextY = Bottom + iXErr - (Bottom - Top - (int)sizeText.Width) / 2;
                        if (XAlign == Aligns.Right) iTextY = Top + (int)sizeText.Width + iXErr;

                        if (YAlign == Aligns.Top) iTextX = Left;
                        if (YAlign == Aligns.Center) iTextX = Left + (Right - Left - (int)sizeText.Height) / 2;
                        if (YAlign == Aligns.Bottom) iTextX = Right - (int)sizeText.Height;

                        tempMatrix.RotateAt(-90, new PointF(iTextX, iTextY));
                    }
                    else if (Rotation == 180)
                    {
                        if (XAlign == Aligns.Left) iTextX = Right + iXErr;
                        if (XAlign == Aligns.Center) iTextX = Right + iXErr - (Right - Left - (int)sizeText.Width) / 2;
                        if (XAlign == Aligns.Right) iTextX = Left + (int)sizeText.Width + iXErr;

                        if (YAlign == Aligns.Top) iTextY = Bottom;
                        if (YAlign == Aligns.Center) iTextY = Bottom - (Bottom - Top - (int)sizeText.Height) / 2;
                        if (YAlign == Aligns.Bottom) iTextY = Top + (int)sizeText.Height;

                        tempMatrix.RotateAt(-180, new PointF(iTextX, iTextY));
                    }
                    else if (Rotation == 270)
                    {
                        if (XAlign == Aligns.Left) iTextY = Top - iXErr;
                        if (XAlign == Aligns.Center) iTextY = Top - iXErr + (Bottom - Top - (int)sizeText.Width) / 2;
                        if (XAlign == Aligns.Right) iTextY = Bottom - (int)sizeText.Width - iXErr;

                        if (YAlign == Aligns.Top) iTextX = Right;
                        if (YAlign == Aligns.Center) iTextX = Right - (Right - Left - (int)sizeText.Height) / 2;
                        if (YAlign == Aligns.Bottom) iTextX = Left + (int)sizeText.Height;

                        tempMatrix.RotateAt(-270, new PointF(iTextX, iTextY));
                    }

                    Region oldClip = null;

                    if (Clip)
                    {
                        // Set Clip
                        oldClip = gfx.Clip.Clone();
                        Rectangle rectClip = new Rectangle(Left, Top, Right - Left, Bottom - Top);
                        rectClip.Inflate(2, 2);
                        gfx.SetClip(rectClip);
                    }

                    // Set rotation transform
                    gfx.Transform = tempMatrix;

                    // Draw Text Background
                    if (FontBgColor != "None")
                    {
                        using (var bgBrush = CreateBrush(FontBgColor, "Solid", ""))
                        {
                            gfx.FillRectangle(bgBrush, iTextX + iXErr, iTextY, (int)sizeText.Width, (int)sizeText.Height);
                        }
                    }

                    // Draw string
                    using (var fontBrush = CreateBrush(FontColor, "Solid", ""))
                    {
                        if (MultiLine)
                        {
                            RectangleF rectText = new RectangleF(iTextX, iTextY, fMultiLineWidth, sizeText.Height);
                            gfx.DrawString(Text, Font, fontBrush, rectText);
                        }
                        else
                        {
                            gfx.DrawString(Text, Font, fontBrush, iTextX, iTextY);
                        }

                        if (Clip)
                        {
                            //Reset Clip
                            gfx.SetClip(oldClip, CombineMode.Replace);
                        }

                        // DEBUG: Draw rectText
                        //gfx.DrawRectangle(Pens.Red, iTextX + iXErr, iTextY, (int)sizeText.Width, (int)sizeText.Height);

                        // Reset Transform
                        gfx.Transform = oldMatrix;

                        // DEBUG: Draw rectTextBox
                        //gfx.DrawRectangle(Pens.Black, Left, Top, Right - Left, Bottom - Top);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectText.Draw failed.", ex);
            }
        }

        internal void CalculateRequiredHeight()
        {
            try
            {
                if (Text != "")
                {
                    // Set up a Graphics
                    using (var panel = new Panel())
                    {
                        using (var gfx = panel.CreateGraphics())
                        {
                            gfx.PageUnit = GraphicsUnit.Pixel;
                            float fScale = gfx.DpiX / 254F;
                            gfx.ScaleTransform(fScale, fScale);

                            // Measure Text
                            CharacterRange[] characterRanges = {new CharacterRange(0, Text.Length)};
                            using (var stringFormat = new StringFormat())
                            {
                                stringFormat.SetMeasurableCharacterRanges(characterRanges);
                                RectangleF layoutRect = new RectangleF(Left, Top, Right - Left, 10000);
                                var stringRegions = gfx.MeasureCharacterRanges(Text, Font, layoutRect, stringFormat);
                                using (var region = stringRegions[0])
                                {
                                    RectangleF measureRect1 = region.GetBounds(gfx);

                                    int iRequiredHeight = 1 + (int) measureRect1.Height;

                                    // Change Properties
                                    if (Rotation == 0)
                                    {
                                        if (YAlign == Aligns.Top) SetBottom(Top + iRequiredHeight);
                                        if (YAlign == Aligns.Center)
                                        {
                                            SetTop(Top - iRequiredHeight / 2);
                                            SetBottom(Top + iRequiredHeight);
                                        }

                                        if (YAlign == Aligns.Bottom) SetTop(Bottom - iRequiredHeight);
                                    }
                                    else
                                    {
                                        if (YAlign == Aligns.Top) SetTop(Bottom - iRequiredHeight);
                                        if (YAlign == Aligns.Center)
                                        {
                                            SetTop(Top - iRequiredHeight / 2);
                                            SetBottom(Top + iRequiredHeight);
                                        }

                                        if (YAlign == Aligns.Bottom) SetBottom(Top + iRequiredHeight);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectText.CalculateRequiredHeight failed.", ex);
            }
        }

        public override void Draw(C1PdfDocument docPdf)
        {
            try
            {
                if (docPdf.PageRectangle.IntersectsWith(new RectangleF(Left, Top, Right - Left, Bottom - Top)))
                {
                    SizeF sizeText = new SizeF();
                    int iTextX = Left;
                    int iTextY = Top;
                    int iXErr = 0;
                    var testStringFormat = new StringFormat();

                    if (!string.IsNullOrWhiteSpace(Text))
                    {
                        float textWidth = 0;
                        textWidth = (Rotation == 90) || (Rotation == 270) ? Bottom - Top : Right - Left;

                        var textSize = docPdf.MeasureString(Text, Font, textWidth);

                        sizeText.Width = textSize.Width;
                        sizeText.Height = textSize.Height;
                    }

                    RectangleF testRectangle = new RectangleF(iTextX, iTextY, sizeText.Width, sizeText.Height);
                    docPdf.RotateAngle = Rotation;

                    if (Rotation == 0)
                    {
                        if (XAlign == Aligns.Left) iTextX = Left - iXErr;
                        if (XAlign == Aligns.Center) iTextX = Left - iXErr + (Right - Left - (int)sizeText.Width) / 2;
                        if (XAlign == Aligns.Right) iTextX = Right - (int)sizeText.Width - iXErr;

                        if (YAlign == Aligns.Top) iTextY = Top;
                        if (YAlign == Aligns.Center) iTextY = Top + (Bottom - Top - (int)sizeText.Height) / 2;
                        if (YAlign == Aligns.Bottom) iTextY = Bottom - (int)sizeText.Height;

                        if (XAlign == Aligns.Left) testStringFormat.Alignment = StringAlignment.Near;
                        if (XAlign == Aligns.Center) testStringFormat.Alignment = StringAlignment.Center;
                        if (XAlign == Aligns.Right) testStringFormat.Alignment = StringAlignment.Far;

                        if (YAlign == Aligns.Top) testStringFormat.LineAlignment = StringAlignment.Near;
                        if (YAlign == Aligns.Center) testStringFormat.LineAlignment = StringAlignment.Center;
                        if (YAlign == Aligns.Bottom) testStringFormat.LineAlignment = StringAlignment.Far;

                        testRectangle = new RectangleF(iTextX, iTextY, sizeText.Width, sizeText.Height);
                    }
                    else if (Rotation == 90)
                    {
                        var widthText1 = Bottom - sizeText.Height;
                        testRectangle = new RectangleF(Left + sizeText.Height, widthText1, sizeText.Width, sizeText.Height);

                        if (XAlign == Aligns.Left) testStringFormat.Alignment = StringAlignment.Near;
                        if (XAlign == Aligns.Center) testStringFormat.Alignment = StringAlignment.Center;
                        if (XAlign == Aligns.Right) testStringFormat.Alignment = StringAlignment.Far;

                        if (YAlign == Aligns.Top) testStringFormat.LineAlignment = StringAlignment.Near;
                        if (YAlign == Aligns.Center) testStringFormat.LineAlignment = StringAlignment.Center;
                    }

                    // Draw Text Background
                    if (FontBgColor != "None")
                    {
                        using (var bgBrush = CreateBrush(FontBgColor, "Solid", ""))
                        {
                            docPdf.FillRectangle(bgBrush, iTextX + iXErr, iTextY, (int)sizeText.Width, (int)sizeText.Height);
                        }
                    }

                    // Draw string
                    using (var fontBrush = CreateBrush(FontColor, "Solid", ""))
                    {
                        docPdf.DrawString(Text, Font, fontBrush, testRectangle, testStringFormat);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectText.Draw-C1PdfDocument failed.", ex);
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                Font?.Dispose();
            }

            disposed = true;
        }
    }
}