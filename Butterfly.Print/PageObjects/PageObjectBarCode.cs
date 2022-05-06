namespace Butterfly.Print.PageObjects
{
    using System;
    using System.Drawing;
    using Spire.Barcode;

    using C1.C1Pdf;

    /// <summary>
    /// Summary description for PageObjectBarCode.
    /// </summary>
    public class PageObjectBarCode : PageObject, IDisposable
    {
        private bool disposed = false;

        private Aligns m_XAlign = Aligns.Left;
        private Aligns m_YAlign = Aligns.Bottom;

        private int m_iRotation = 0;

        private string m_strBarCodeType = "3of9";
        private bool m_bShowText = false;
        private bool m_bCheckDigit = false;

        private float m_fBarHeight = 100;
        private float m_fBarWidth = 4;
        private int m_iBarRatio = 3;

        private Font m_Font = new Font("Courier New", 9f * 254f / 72f, GraphicsUnit.World);

        private string m_strData = "";

        private int m_iX = 100;
        private int m_iY = 100;

        private int m_iBCHeight = 200;
        private int m_iBCWidth = 400;

        private Bitmap barCodeBitmap = null;

        private bool m_bIsPrepared = false;
        private readonly int defaultHorizontalResolution = 600;
        private readonly int defaultVerticalResolution = 600;
        private readonly int defaultHorizontalResolutionC1 = 370;
        private readonly int defaultVerticalResolutionC1 = 150;

        public PageObjectBarCode(double scalingFactor)
        {
            ScalingFactor = scalingFactor;
        }

        public Aligns XAlign
        {
            get
            {
                return m_XAlign;
            }

            set
            {
                m_bIsPrepared = false;
                m_XAlign = value;
            }
        }

        public Aligns YAlign
        {
            get
            {
                return m_YAlign;
            }

            set
            {
                m_bIsPrepared = false;
                m_YAlign = value;
            }
        }

        public int Rotation
        {
            get
            {
                return m_iRotation;
            }

            set
            {
                m_bIsPrepared = false;
                m_iRotation = value;
            }
        }

        public string BarCodeType
        {
            get
            {
                return m_strBarCodeType;
            }

            set
            {
                m_bIsPrepared = false;
                m_strBarCodeType = value;
            }
        }

        public bool ShowText
        {
            get
            {
                return m_bShowText;
            }

            set
            {
                m_bIsPrepared = false;
                m_bShowText = value;
            }
        }

        public bool CheckDigit
        {
            get
            {
                return m_bCheckDigit;
            }

            set
            {
                m_bIsPrepared = false;
                m_bCheckDigit = value;
            }
        }

        public float BarHeight
        {
            get
            {
                return m_fBarHeight;
            }

            set
            {
                m_bIsPrepared = false;
                m_fBarHeight = ApplyScalingFactor(value);
            }
        }

        public float BarWidth
        {
            get
            {
                return m_fBarWidth;
            }

            set
            {
                m_bIsPrepared = false;
                m_fBarWidth = ApplyScalingFactor(value);
            }
        }

        public int BarRatio
        {
            get
            {
                return m_iBarRatio;
            }

            set
            {
                m_bIsPrepared = false;
                m_iBarRatio = value;
            }
        }

        public Font Font
        {
            get
            {
                return m_Font;
            }

            set
            {
                m_bIsPrepared = false;
                m_Font = value;
            }
        }

        public string Data
        {
            get
            {
                return m_strData;
            }

            set
            {
                m_bIsPrepared = false;
                m_strData = value;
            }
        }

        public override void Draw(Graphics gfx)
        {
            try
            {
                if (gfx.VisibleClipBounds.IntersectsWith(new RectangleF(Left, Top, Right - Left, Bottom - Top)))
                {
                    PrepareBarCode(gfx.DpiX, gfx.DpiY);

                    float m_fScale = gfx.DpiX / 254f;

                    // DEBUG: BackColor
                    //m_bcp.BackColor = Color.Yellow;

                    // Reset transform
                    gfx.ScaleTransform((1 / m_fScale), (1 / m_fScale));

                    // Draw Spire Barcode
                    gfx.DrawImage(barCodeBitmap, new Point((int)Math.Round((float)m_iX * m_fScale), (int)Math.Round((float)m_iY * m_fScale)));

                    // Set Transform
                    gfx.ScaleTransform(m_fScale, m_fScale);

                    // DEBUG: Draw BarCode Rect gfx.DrawRectangle(Pens.Blue, m_iX, m_iY, m_iBCWidth, m_iBCHeight);
                    // DEBUG: Draw Rect gfx.DrawRectangle(Pens.Red, Left, Top, Right-Left,
                    // Bottom-Top); gfx.DrawString("BarCode", m_Font, Brushes.Black, Left, Top);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectBarCode.Draw failed.", ex);
     
            }
        }

        public override void Draw(C1PdfDocument docPdf)
        {
            try
            {
                var barcodeRect = new RectangleF(Left, Top, Right - Left, Bottom - Top);
                if (docPdf.PageRectangle.IntersectsWith(barcodeRect))
                {
                    PrepareBarCode(defaultHorizontalResolutionC1, defaultVerticalResolutionC1, false);

                    docPdf.DrawImage(
                        barCodeBitmap,
                        new RectangleF(new Point(Left, Top), new SizeF(barcodeRect.Width, barcodeRect.Height)),
                        ContentAlignment.BottomCenter,
                        ImageSizeModeEnum.Scale
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectBarCode.Draw-C1PdfDocument failed.", ex);

            }
        }

        private void PrepareBarCode(float horizontalResolution, float verticalResolution, bool isFromGraphics = true)
        {
            try
            {
                if (m_bIsPrepared == false)
                {
                    m_iX = Left;
                    m_iY = Top;

                    // Initialize Spire.Barcode
                    BarcodeSettings barsetting = new BarcodeSettings();
                    Spire.Barcode.BarcodeSettings.ApplyKey("1OQBR-1VKB2-YDGBI-RQDWK-0J14T");

                    //set the dimensions
                    barsetting.X = m_fBarWidth / 10f;
                    barsetting.BarHeight = m_fBarHeight / 10f;

                    barsetting.WideNarrowRatio = m_iBarRatio;

                    //set margins
                    barsetting.TopMargin = 0;
                    barsetting.BottomMargin = 0;
                    barsetting.LeftMargin = 0;
                    barsetting.RightMargin = 0;

                    // set text
                    barsetting.ShowText = false;

                    // set dpi
                    if (isFromGraphics) { 
                        barsetting.DpiX = horizontalResolution > 200 ? horizontalResolution : defaultHorizontalResolution;
                        barsetting.DpiY = verticalResolution > 200 ? verticalResolution : defaultVerticalResolution;
                    }
                    else
                    {
                        barsetting.DpiX = horizontalResolution;
                        barsetting.DpiY = verticalResolution;
                    }

                    //set the data
                    barsetting.Data = m_strData;
                    barsetting.Data2D = m_strData;

                    //set BarCodeType
                    switch (m_strBarCodeType)
                    {
                        case "Codabar":
                            barsetting.Type = Spire.Barcode.BarCodeType.Codabar;
                            break;

                        case "Code11":
                            barsetting.Type = Spire.Barcode.BarCodeType.Code11;
                            break;

                        case "Code25":
                            barsetting.Type = Spire.Barcode.BarCodeType.Code25;
                            break;

                        case "Interleaved25":
                            barsetting.Type = Spire.Barcode.BarCodeType.Interleaved25;
                            break;

                        case "Code39":
                            barsetting.Type = Spire.Barcode.BarCodeType.Code39;
                            break;

                        case "Code39Extended":
                            barsetting.Type = Spire.Barcode.BarCodeType.Code39Extended;
                            break;

                        case "Code93":
                            barsetting.Type = Spire.Barcode.BarCodeType.Code93;
                            break;

                        case "Code93Extended":
                            barsetting.Type = Spire.Barcode.BarCodeType.Code93Extended;
                            break;

                        case "Code128":
                            barsetting.Type = Spire.Barcode.BarCodeType.Code128;
                            break;

                        case "EAN8":
                            barsetting.Type = Spire.Barcode.BarCodeType.EAN8;
                            break;

                        case "EAN13":
                            barsetting.Type = Spire.Barcode.BarCodeType.EAN13;
                            break;

                        case "EAN128":
                            barsetting.Type = Spire.Barcode.BarCodeType.EAN128;
                            break;

                        case "EAN14":
                            barsetting.Type = Spire.Barcode.BarCodeType.EAN14;
                            break;

                        case "SCC14":
                            barsetting.Type = Spire.Barcode.BarCodeType.SCC14;
                            break;

                        case "SSCC18":
                            barsetting.Type = Spire.Barcode.BarCodeType.SSCC18;
                            break;

                        case "ITF14":
                            barsetting.Type = Spire.Barcode.BarCodeType.ITF14;
                            break;

                        case "ITF6":
                            barsetting.Type = Spire.Barcode.BarCodeType.ITF6;
                            break;

                        case "UPCA":
                            barsetting.Type = Spire.Barcode.BarCodeType.UPCA;
                            break;

                        case "UPCE":
                            barsetting.Type = Spire.Barcode.BarCodeType.UPCE;
                            break;

                        case "PostNet":
                            barsetting.Type = Spire.Barcode.BarCodeType.PostNet;
                            break;

                        case "Planet":
                            barsetting.Type = Spire.Barcode.BarCodeType.Planet;
                            break;

                        case "MSI":
                            barsetting.Type = Spire.Barcode.BarCodeType.MSI;
                            break;

                        case "DataMatrix":
                            barsetting.Type = Spire.Barcode.BarCodeType.DataMatrix;
                            break;

                        case "QRCode":
                            barsetting.Type = Spire.Barcode.BarCodeType.QRCode;
                            break;

                        case "Pdf417":
                            barsetting.Type = Spire.Barcode.BarCodeType.Pdf417;
                            break;

                        case "Pdf417Macro":
                            barsetting.Type = Spire.Barcode.BarCodeType.Pdf417Macro;
                            break;

                        case "RSS14":
                            barsetting.Type = Spire.Barcode.BarCodeType.RSS14;
                            break;

                        case "RSS14Truncated":
                            barsetting.Type = Spire.Barcode.BarCodeType.RSS14Truncated;
                            break;

                        case "RSSLimited":
                            barsetting.Type = Spire.Barcode.BarCodeType.RSSLimited;
                            break;

                        case "RSSExpanded":
                            barsetting.Type = Spire.Barcode.BarCodeType.RSSExpanded;
                            break;

                        case "USPS":
                            barsetting.Type = Spire.Barcode.BarCodeType.USPS;
                            break;

                        case "SwissPostParcel":
                            barsetting.Type = Spire.Barcode.BarCodeType.SwissPostParcel;
                            break;

                        case "PZN":
                            barsetting.Type = Spire.Barcode.BarCodeType.PZN;
                            break;

                        case "OPC":
                            barsetting.Type = Spire.Barcode.BarCodeType.OPC;
                            break;

                        case "DeutschePostIdentcode":
                            barsetting.Type = Spire.Barcode.BarCodeType.DeutschePostIdentcode;
                            break;

                        case "DeutschePostLeitcode":
                            barsetting.Type = Spire.Barcode.BarCodeType.DeutschePostLeitcode;
                            break;

                        case "RoyalMail4State":
                            barsetting.Type = Spire.Barcode.BarCodeType.RoyalMail4State;
                            break;

                        case "SingaporePost4State":
                            barsetting.Type = Spire.Barcode.BarCodeType.SingaporePost4State;
                            break;

                        case "Aztec":
                            barsetting.Type = Spire.Barcode.BarCodeType.Aztec;
                            break;

                        default:
                            barsetting.Type = Spire.Barcode.BarCodeType.Code39;
                            break;
                    }

                    //set Rotation (Spire rotates opposite way)
                    barsetting.Rotate = m_iRotation == 90 ? 270 : m_iRotation == 270 ? 90 : m_iRotation;

                    // Generate image
                    var bargenerator = new BarCodeGenerator(barsetting);
                    barCodeBitmap = (Bitmap)bargenerator.GenerateImage();
                
                    barCodeBitmap.SetResolution(barsetting.DpiX, barsetting.DpiY);

                    // DEBUG: Save Barcode
                    //barCodeBitmap.Save("barcode.png");

                    // Calculate sizes
                    m_iBCHeight = (int) Math.Round(((float) barCodeBitmap.Height / barsetting.DpiY) * 254f);
                    m_iBCWidth = (int) Math.Round(((float) barCodeBitmap.Width / barsetting.DpiX) * 254f);
                    

                    //Calculate Barcode position
                    if (m_iRotation == 0)
                    {
                        if (m_XAlign == Aligns.Center) m_iX = Left + (Right - Left - m_iBCWidth) / 2;
                        if (m_XAlign == Aligns.Right) m_iX = Right - m_iBCWidth;

                        if (m_YAlign == Aligns.Center) m_iY = Top + (Bottom - Top - m_iBCHeight) / 2;
                        if (m_YAlign == Aligns.Bottom) m_iY = Bottom - m_iBCHeight;
                    }
                    else if (m_iRotation == 90)
                    {
                        if (m_YAlign == Aligns.Center) m_iX = Left + (Right - Left - m_iBCWidth) / 2;
                        if (m_YAlign == Aligns.Bottom) m_iX = Right - m_iBCWidth;

                        if (m_XAlign == Aligns.Center) m_iY = Top + (Bottom - Top - m_iBCHeight) / 2;
                        if (m_XAlign == Aligns.Left) m_iY = Bottom - m_iBCHeight;
                    }
                    else if (m_iRotation == 180)
                    {
                        if (m_XAlign == Aligns.Center) m_iX = Left + (Right - Left - m_iBCWidth) / 2;
                        if (m_XAlign == Aligns.Left) m_iX = Right - m_iBCWidth;

                        if (m_YAlign == Aligns.Center) m_iY = Top + (Bottom - Top - m_iBCHeight) / 2;
                        if (m_YAlign == Aligns.Top) m_iY = Bottom - m_iBCHeight;
                    }
                    else if (m_iRotation == 270)
                    {
                        if (m_YAlign == Aligns.Center) m_iX = Left + (Right - Left - m_iBCWidth) / 2;
                        if (m_YAlign == Aligns.Top) m_iX = Right - m_iBCWidth;

                        if (m_XAlign == Aligns.Center) m_iY = Top + (Bottom - Top - m_iBCHeight) / 2;
                        if (m_XAlign == Aligns.Right) m_iY = Bottom - m_iBCHeight;
                    }

                    m_bIsPrepared = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PageObjectBarCode.Draw failed.", ex);
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
                barCodeBitmap?.Dispose();
            }

            disposed = true;
        }
    }
}