namespace Butterfly.Print.Objects
{
    using System;
    using System.Collections.Generic;

    public class DataMapping
    {
        public DataMapping()
        {
        }

        public string PrimaryKeyName
        {
            get { return "DataMappingId"; }
        }

        public Guid DataMappingId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DataObjectName { get; set; }

        public List<DataMappingItem> DataMappingItems { get; set; }

        public List<DataMappingBlock> DataMappingBlocks { get; set; }

        public State State { get; set; }

        public byte[] Timestamp { get; set; }
    }
}