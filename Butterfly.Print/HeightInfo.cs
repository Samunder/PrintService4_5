namespace Butterfly.Print
{
    internal class HeightInfo
    {
        //private HeightInfo()
        //{ }

        public HeightInfo(int maxHeightBeforeUnsplittablePartMustBeSplitted)
        {
            MaxHeightBeforeUnsplittablePartMustBeSplitted = maxHeightBeforeUnsplittablePartMustBeSplitted;
            UnSplittableHeight = 0;
        }

        public int MaxHeightBeforeUnsplittablePartMustBeSplitted { get; private set; }

        public int UnSplittableHeight { get; set; }


        public HeightInfo IncreaseUnsplittableHeight(int unsplittableIncrement)
        {
            UnSplittableHeight += unsplittableIncrement;
            return this;
        }

        public bool IsForcedSplitNeeded() => IsForcedSplitNeeded(0);

        public bool IsForcedSplitNeeded(int unsplittableIncrement)
        {
            return (UnSplittableHeight + unsplittableIncrement) > MaxHeightBeforeUnsplittablePartMustBeSplitted;
        }

        public  HeightInfo Copy()
        {
            return new HeightInfo(this.MaxHeightBeforeUnsplittablePartMustBeSplitted)
            {
                UnSplittableHeight = UnSplittableHeight
            };
        }

    }
}