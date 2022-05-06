namespace Butterfly.Print.Objects
{
    using System.Collections.Generic;

    public class DataMappingBlock
    {
        public DataMappingBlock()
        {
        }

        public string Name { get; set; }

        public string ItterationBril { get; set; }

        public string ItterationObjectName { get; set; }

        public List<DataMappingItem> DataMappingItems { get; set; }

        public List<DataMappingBlock> DataMappingBlocks { get; set; }
    }
}