namespace Butterfly.Print.PrintJobObjects
{
    using System;

    [Serializable]
    public class PrintJobDataItem
    {
        public PrintJobDataItem()
        {
        }

        public PrintJobDataItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}