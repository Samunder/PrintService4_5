namespace Butterfly.Print.Objects
{
    using System;

    using Newtonsoft.Json;

    public class DocumentDetail
    {
        public DocumentDetail()
        {
        }

        public string PrimaryKeyName
        {
            get { return "DocumentDetailId"; }
        }
        public Guid DocumentDetailId { get; set; }

        [JsonIgnore]
        public virtual Document Document { get; set; }

        [JsonIgnore]
        public Guid DocumentId { get; set; }

        public string LayoutName { get; set; }

        public string SourceDataType { get; set; }

        public string SourceDataFramework { get; set; }

        public string SourceDataEntity { get; set; }

        public string DataMappingName { get; set; }

        public int SortOrder { get; set; }

        public int Copies { get; set; }

        public State State { get; set; }

        public byte[] Timestamp { get; set; }
    }
}