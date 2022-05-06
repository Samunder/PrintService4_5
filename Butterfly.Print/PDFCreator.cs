namespace Butterfly.Print
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Drawing.Printing;
    using System.IO;

    using Interfaces;
    using PageObjects;

    using C1.C1Pdf;

    internal class PDFCreator
    {
        private ILogService logService;
        private readonly Dictionary<DocumentRenderType, Action<Page, C1PdfDocument>> PdfDocumentMapper;

        internal PDFCreator(ILogService logService)
        {
            this.logService = logService;
            PdfDocumentMapper = new Dictionary<DocumentRenderType, Action<Page, C1PdfDocument>>();
            InitializePdfDocumentMapper();
        }

        internal byte[] CreatePDFAsByteArray(List<Page> alPages, DocumentRenderType documentRenderType)
        {
            try
            {
                byte[] pdf;

                //// this.logService.Info("Print.PDFCreator.CreatePDFAsByteArray - CreatePDFAsByteArray start");

                using (var docPdf = CreatePdf(alPages, documentRenderType))
                {
                    //Debug
                    //docPDF.Save("C:\\_temp\\CF8\\LYB_AI.pdf");

                    using (var memoryStream = new MemoryStream())
                    {
                        docPdf.Save(memoryStream);
                        pdf = memoryStream.ToArray();
                    }

                    //// this.logService.Info("Print.PDFCreator.CreatePDFAsByteArray - CreatePDFAsByteArray done");

                    return pdf;
                }
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.PDFCreator.CreatePDFAsByteArray - Failed", ex);
                throw;
                // return null;
            }
        }

        private C1PdfDocument CreatePdf(List<Page> alPages, DocumentRenderType documentRenderType)
        {
            C1PdfDocument docPdf = null;
            try
            {
                // this.logService.Info("Print.PDFCreator.CreatePDF - Create PDF start");

                if (alPages.Count == 0)
                {
                    this.logService.Error("Print.PDFCreator.CreatePDF - No Pages to PDF.");
                    return null;
                }

                // Create new PDF
                docPdf = new C1PdfDocument();
                
                // Set paper size
                if (alPages[0].PageSize == 0)
                {
                    // this.logService.Info("Print.PDFCreator.CreatePDF - Setting User Custom Paper Size: " + alPages[0].Width + " x " + alPages[0].Height);

                    if (alPages[0].Orientation == "Landscape")
                    {
                        SizeF oSize = new SizeF((float)(72 * alPages[0].Height / 254f),
                            (float)(72 * alPages[0].Width / 254f));
                        docPdf.PageSize = oSize;

                        docPdf.Landscape = true;
                    }
                    else
                    {
                        SizeF oSize = new SizeF((float)(72 * alPages[0].Width / 254f),
                            (float)(72 * alPages[0].Height / 254f));
                        docPdf.PageSize = oSize;
                    }
                }
                else
                {
                    // this.logService.Info("Print.PDFCreator.CreatePDF - Setting User Paper Size: " + alPages[0].PageSize);

                    var oPaperSize = new PaperSize
                    {
                        RawKind = alPages[0].PageSize
                    };

                    docPdf.PaperKind = oPaperSize.Kind;

                    if (alPages[0].Orientation == "Landscape") docPdf.Landscape = true;
                }

                foreach (var oPage in alPages)
                {
                    // this.logService.Info("Print.PDFCreator.CreatePDF - Starting to PDF page " + alPages.IndexOf(oPage) + 1);

                    //docPDF.CurrentPage = alPages.IndexOf(oPage);

                    if (alPages.IndexOf(oPage) != 0)
                    {
                        docPdf.NewPage();
                    }

                    docPdf.Landscape = (oPage.Orientation == "Landscape");

                    PdfDocumentMapper[documentRenderType](oPage, docPdf);
                }

                // this.logService.Info("Print.PDFCreator.CreatePDF - Create PDF done");

                return docPdf;
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.PDFCreator.CreatePDF - Failed", ex);

                docPdf?.Dispose();

                return null;
            }
        }

        internal C1PdfDocument CreatePDFAsC1Doc(List<Page> pages)
        {
            try
            {
                // this.logService.Info("Print.PDFCreator.CreatePDFAsC1Doc - CreatePDFAsC1Doc start");
                return CreatePdf(pages, DocumentRenderType.Graphics);
            }
            catch (Exception ex)
            {
                this.logService.Error("Print.PDFCreator.CreatePDFAsC1Doc - Failed", ex);
                return null;
            }
        }

        private void RenderPdf(Page oPage, C1PdfDocument docPdf)
        {
            // Draw Page to Metafile
            using (var stream = new MemoryStream())
            {
                var iWidthPixels = (int) (600 * ((float) oPage.Width / 254f) + 0.5);
                var iHeightPixels = (int) (600 * ((float) oPage.Height / 254f) + 0.5);

                using (var b = new Bitmap(iWidthPixels, iHeightPixels, PixelFormat.Format24bppRgb))
                {
                    var r = new Rectangle(0, 0, iWidthPixels, iHeightPixels);
                    b.SetResolution(600, 600);
                    using (var g = Graphics.FromImage(b))
                    {
                        g.PageUnit = GraphicsUnit.Pixel;

                        IntPtr hdc = g.GetHdc();
                        using (var mf = new Metafile(stream, hdc, r, MetafileFrameUnit.Pixel))
                        {
                            g.ReleaseHdc();

                            using (var g2 = Graphics.FromImage(mf))
                            {
                                g2.PageUnit = GraphicsUnit.Pixel;

                                //MetafileHeader h = mf.GetMetafileHeader();
                                //g2.ScaleTransform(h.DpiX / g2.DpiX, h.DpiY / g2.DpiY);

                                float fXScale = 600 / 254F;
                                float fYScale = 600 / 254F;

                                g2.ScaleTransform(fXScale, fYScale);

                                oPage.DrawPage(g2);
                            }

                            //// Debug
                            //Bitmap b2 = new Bitmap(4960, 7016, PixelFormat.Format24bppRgb);
                            //b2.SetResolution(600, 600);
                            //Graphics gb2 = Graphics.FromImage(b2);
                            //gb2.PageUnit = GraphicsUnit.Pixel;
                            //gb2.FillRectangle(Brushes.White, 0, 0, 4960, 7016);

                            //float fXScale2 = 600 / 254F;
                            //float fYScale2 = 600 / 254F;

                            //gb2.ScaleTransform(fXScale2, fYScale2);

                            //oPage.DrawPage(gb2);

                            //// Save Bitmap
                            //string fileNameBMP2 = Application.StartupPath + "\\Bitmap.png";
                            //b2.Save(fileNameBMP2);

                            //// Save Metafile
                            //string fileNameBMP = Application.StartupPath + "\\Metafile.png";
                            //mf.Save(fileNameBMP);

                            docPdf.FontType = FontTypeEnum.Embedded;
                            docPdf.DrawImage(mf, docPdf.PageRectangle);
                        }
                    }
                }
            }
        }

        private void RenderPdfOnC1Document(Page oPage, C1PdfDocument docPdf)
        {
            if (oPage.Orientation == "Landscape")
            {
                SizeF oSize = new SizeF(oPage.Height, oPage.Width);
                docPdf.PageSize = oSize;

                docPdf.Landscape = true;
            }
            else
            {
                SizeF oSize = new SizeF(oPage.Width, oPage.Height);
                docPdf.PageSize = oSize;
            }

            docPdf.FontType = FontTypeEnum.Embedded;
            oPage.DrawPage(docPdf);
        }

        private void InitializePdfDocumentMapper()
        {
            PdfDocumentMapper.Add(DocumentRenderType.ComponentOne, RenderPdfOnC1Document);
            PdfDocumentMapper.Add(DocumentRenderType.Graphics, RenderPdf);
        }
    }
}