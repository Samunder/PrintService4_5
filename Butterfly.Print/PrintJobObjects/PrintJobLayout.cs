namespace Butterfly.Print.PrintJobObjects
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class PrintJobLayout
    {
        public PrintJobLayout()
        {
            PrintJobDataItems = new List<PrintJobDataItem>();
            Copies = 1;
            PaperSource = -1;
            XOffset = 0;
            YOffset = 0;
        }

        public string LayoutName { get; set; }

        public List<PrintJobDataItem> PrintJobDataItems
        {
            get;
            set;
        }

        public int Copies { get; set; }

        public int PaperSource { get; set; }

        public int XOffset { get; set; }

        public int YOffset { get; set; }

        public void AddDataItem(string key, string value)
        {
            PrintJobDataItem printJobDataItem = new PrintJobDataItem(key, value);
            PrintJobDataItems.Add(printJobDataItem);
        }
    }
}