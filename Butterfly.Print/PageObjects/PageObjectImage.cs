namespace Butterfly.Print.PageObjects
{
    using System;
    using System.Drawing;
    using System.IO;

    using C1.C1Pdf;

    /// <summary>
    /// Summary description for PageObjectImage.
    /// </summary>
    public class PageObjectImage : PageObject, IDisposable
    {
        private bool disposed = false;

        private int miRotation = 0;
        private byte[] mbaImageData = null;
        private Image miImage = null;
        private bool mbIsPrepared = false;

        public PageObjectImage(double scalingFactor)
        {
            ScalingFactor = scalingFactor;
        }

        public Aligns XAlign { get; set; }

        public Aligns YAlign { get; set; }

        public int Rotation
        {
            get
            {
                return this.miRotation;
            }

            set
            {
                this.miRotation = value;
                this.mbIsPrepared = false;
            }
        }

        public byte[] ImageData
        {
            get
            {
                return this.mbaImageData;
            }

            set
            {
                this.mbaImageData = value;
                this.mbIsPrepared = false;
            }
        }

        public override void Draw(Graphics gfx)
        {
            try
            {
                DrawImage(gfx.VisibleClipBounds, gfx.DrawImage);
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectImage.Draw failed.", ex);
            }
        }

        public override void Draw(C1PdfDocument docPdf)
        {
            try
            {
                DrawImage(docPdf.PageRectangle, docPdf.DrawImage);
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectImage.Draw-C1PdfDocument failed.", ex);
            }
        }
        private void DrawImage(RectangleF pageRectangle, Action<Image, RectangleF> imageAction)
        {
            try
            {
                if (pageRectangle.IntersectsWith(new RectangleF(Left, Top, Right - Left, Bottom - Top)))
                {
                    if (this.mbaImageData == null)
                    {
                       throw new Exception("PageObjectImage.Draw ImageData == null");
                    }

                    this.PrepareImage();


                    var imageBoundaries = GetImageBoundaries();
                    imageAction(this.miImage, imageBoundaries);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectImage.Draw failed.", ex);
            }
        }

        private RectangleF GetImageBoundaries()
        {
            int iRectHeight = Bottom - Top;
            int iRectWidth = Right - Left;

            int iImageLeft = Left;
            int iImageTop = Top;
            int iImgHeight = iRectHeight;
            int iImgWidth = iRectWidth;

            float fHScale = (float)iRectHeight / (float)this.miImage.Height;
            float fWScale = (float)iRectWidth / (float)this.miImage.Width;
            float fImageRatio = (float)this.miImage.Height / (float)this.miImage.Width;

            if (fHScale < fWScale)
            {
                iImgHeight = iRectHeight;
                iImgWidth = (int)(iRectHeight / fImageRatio);

                if (this.Rotation == 0)
                {
                    if (this.XAlign == Aligns.Center)
                    {
                        iImageLeft = this.Left + ((iRectWidth - iImgWidth) / 2);
                    }

                    if (this.XAlign == Aligns.Right)
                    {
                        iImageLeft = this.Left + (iRectWidth - iImgWidth);
                    }
                }
                else if (this.Rotation == 90)
                {
                    if (this.YAlign == Aligns.Center)
                    {
                        iImageLeft = this.Left + ((iRectWidth - iImgWidth) / 2);
                    }

                    if (this.YAlign == Aligns.Bottom)
                    {
                        iImageLeft = this.Left + (iRectWidth - iImgWidth);
                    }
                }
                else if (this.Rotation == 180)
                {
                    if (this.XAlign == Aligns.Center)
                    {
                        iImageLeft = this.Left + ((iRectWidth - iImgWidth) / 2);
                    }

                    if (this.XAlign == Aligns.Left)
                    {
                        iImageLeft = this.Left + (iRectWidth - iImgWidth);
                    }
                }
                else if (this.Rotation == 270)
                {
                    if (this.YAlign == Aligns.Center)
                    {
                        iImageLeft = this.Left + ((iRectWidth - iImgWidth) / 2);
                    }

                    if (this.YAlign == Aligns.Top)
                    {
                        iImageLeft = this.Left + (iRectWidth - iImgWidth);
                    }
                }
            }
            else
            {
                iImgHeight = (int)(iRectWidth * fImageRatio);
                iImgWidth = iRectWidth;

                if (this.Rotation == 0)
                {
                    if (this.YAlign == Aligns.Center)
                    {
                        iImageTop = this.Top + ((iRectHeight - iImgHeight) / 2);
                    }

                    if (this.YAlign == Aligns.Bottom)
                    {
                        iImageTop = this.Top + (iRectHeight - iImgHeight);
                    }
                }
                else if (this.Rotation == 90)
                {
                    if (this.XAlign == Aligns.Center)
                    {
                        iImageTop = this.Top + ((iRectHeight - iImgHeight) / 2);
                    }

                    if (this.XAlign == Aligns.Left)
                    {
                        iImageTop = this.Top + (iRectHeight - iImgHeight);
                    }
                }
                else if (this.Rotation == 180)
                {
                    if (this.YAlign == Aligns.Center)
                    {
                        iImageTop = this.Top + ((iRectHeight - iImgHeight) / 2);
                    }

                    if (this.YAlign == Aligns.Top)
                    {
                        iImageTop = this.Top + (iRectHeight - iImgHeight);
                    }
                }
                else if (this.Rotation == 270)
                {
                    if (this.XAlign == Aligns.Center)
                    {
                        iImageTop = this.Top + ((iRectHeight - iImgHeight) / 2);
                    }

                    if (this.XAlign == Aligns.Right)
                    {
                        iImageTop = this.Top + (iRectHeight - iImgHeight);
                    }
                }
            }

            return new RectangleF(iImageLeft, iImageTop, iImgWidth, iImgHeight);
        }

        private void PrepareImage()
        {
            try
            {
                if (this.mbIsPrepared == false)
                {
                    using (var ms = new MemoryStream(this.mbaImageData))
                    {
                        this.miImage = Image.FromStream(ms);

                        if (this.miRotation == 90)
                        {
                            this.miImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        }
                        else if (this.miRotation == 180)
                        {
                            this.miImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        }
                        else if (this.miRotation == 270)
                        {
                            this.miImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        }

                        this.mbIsPrepared = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectImage.PrepareImage failed.", ex);
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
                miImage?.Dispose();
            }
            
            disposed = true;
        }
    }
}