namespace Butterfly.Print.DocFormObjects
{
    public class DocFormDynamicTargetItem
    {
        public DocFormDynamicTargetItem()
        {
        }

        public DocFormDynamicTargetItem(string name, string blockPath)
        {
            Name = name;
            BlockPath = blockPath;
        }

        public string Name { get; set; }

        public string BlockPath { get; set; }
    }
}