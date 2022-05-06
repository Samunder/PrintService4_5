using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Butterfly.Print;
using Butterfly.Print.DocFormObjects;
using Butterfly.Print.Objects;
using Butterfly.Print.PrintJobObjects;
using Newtonsoft.Json;

namespace ButterflyPrint.Service45.Controllers
{
    public class PrintController : ApiController
    {
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Converters = new JsonConverter[] { new DocFormPageObjectConverter() }
        };

        [HttpPost]
        public byte[] Post(PrintRequest request)
        {
            // request = JsonConvert.DeserializeObject<PrintRequest>("");
            var printEngine = new PrintEngine();

            var printJobDocument = GetPrintDocument(request.DocumentJson, request.Mappings);

            var layout = InitializePrintLayout(request.Layout);


            SetupImageCache(request.ImageCache);

            layout.InitializeDocFormLayout();

            var response = printEngine.BuildDocumentAsPDFByteArray(printJobDocument, layout, ImageCache.Instance, request.DocumentRenderType);


            return response;
        }

        public PrintJobDocument GetPrintDocument(string json, IList<Tuple<string, string>> mappings)
        {
            var printDocument = InitializePrintDocument(json);
            using (var printJobDocument = new PrintJobDocument())
            {
                foreach (var detail in printDocument.DocumentDetails)
                {
                    // Add PrintJobLayout
                    var printJobLayout = new PrintJobLayout();
                    printJobDocument.PrintJobLayouts.Add(printJobLayout);
                    // Set LayoutName
                    printJobLayout.LayoutName = detail.LayoutName;
                    // Set Copies
                    printJobLayout.Copies = detail.Copies;

                    printJobLayout.PrintJobDataItems =
                        mappings.Select(x => new PrintJobDataItem(x.Item1, x.Item2)).ToList();
                }

                return printJobDocument;
            }
        }

        public class PrintRequest
        {
            public string DocumentJson { get; set; }
            public IList<Tuple<string, string>> Mappings { get; set; }
            public string Layout { get; set; }
            public List<string> ImageCache { get; set; }
            public DocumentRenderType DocumentRenderType { get; set; }
        }

        private Document InitializePrintDocument(string objectJson)
        {
            return JsonConvert.DeserializeObject<Document>(objectJson, this._jsonSettings);
        }

        private Layout InitializePrintLayout(string objectJson)
        {
            var layout = JsonConvert.DeserializeObject<Layout>(objectJson, this._jsonSettings);

            layout.InitializeDocFormLayout();

            return layout;
        }

        private void SetupImageCache(List<string> imageJsons)
        {
            foreach (var json in imageJsons)
            {
                var img = JsonConvert.DeserializeObject<Image>(json);
                ImageCache.Instance.AddToCache(img.Name, img.ImageData);
            }
        }
    }
}
