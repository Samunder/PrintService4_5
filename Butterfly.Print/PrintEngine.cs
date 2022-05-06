namespace Butterfly.Print
{
    using System;

    using Interfaces;
    using Objects;
    using PrintJobObjects;

    using C1.C1Pdf;

    public class PrintEngine
    {
        private DocumentBuilder documentBuilder = null;

        private const double graphicsScalingFactor = 1;

        //When printing using DocumentRenderType.ComponentOne page size is 3.528 times more than
        //printing the document with DocumentRenderType.Graphics method.
        //To correct the page size for printing - scaling factor needs to be applied
        //For more details refer below
        //Bug 52364: Wrong size when printing EAD for SE export
        private const double componentOneScalingFactor = 1.0 / 3.528;

        private ILogService logService;

        /// <summary>
        /// Action is defined like this LogLevel, Message, Exception
        /// LogLevel can be: INFO, DEBUG, ERROR, FATAL, WARN
        /// </summary>
        private Action<string, string, Exception> log;

        public PrintEngine()
        {
            this.logService = new LogService.LogService();
            this.documentBuilder = new DocumentBuilder(this.logService);
        }

        public PrintEngine(ILogService logService)
        {
            this.logService = logService;
            this.documentBuilder = new DocumentBuilder(this.logService);
        }

        public PrintEngine(Action<string, string, Exception> log)
        {
            this.log = log;
            this.logService = new LogService.LogService(this.log);
            this.documentBuilder = new DocumentBuilder(this.logService);
        }

        public byte[] BuildDocumentAsPDFByteArray(
            PrintJobDocument printJobDocument, 
            Layout layout, 
            ImageCache imgCache, 
            DocumentRenderType documentRenderType = DocumentRenderType.Graphics
        )
        {
            try
            {
                double scalingFactor = documentRenderType == DocumentRenderType.Graphics ? graphicsScalingFactor : componentOneScalingFactor;

                var result = documentBuilder.BuildDocument(printJobDocument, layout, imgCache, scalingFactor);

                if (result == false)
                {
                    string errorMessage = $"Print.PrintEngine.BuildDocumentAsPDFByteArray - BuildDocument failed.Layout -{layout?.Name}, documentRenderType - {documentRenderType}";
                    this.logService.Error(errorMessage);
                    return null;
                }

                var pdfCreator = new PDFCreator(this.logService);
                var pdf = pdfCreator.CreatePDFAsByteArray(printJobDocument.Pages, documentRenderType);

                if (pdf == null)
                {
                    string errorMessage = $"Print.PrintEngine.BuildDocumentAsPDFByteArray - Convert to PDF stream failed.Layout -{layout?.Name}, documentRenderType - {documentRenderType}";
                    this.logService.Error(errorMessage);
                    return null;
                }
                
                return pdf;
            }
            catch (Exception ex)
            {
                this.logService.Error($"Print.PrintEngine.BuildDocumentAsPDFByteArray - Failed.Layout -{layout?.Name}, documentRenderType - {documentRenderType}", ex);
                throw;
                // return null;
            }
        }
    }
}
