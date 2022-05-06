namespace Butterfly.Print.Objects
{
    using System;

    public class Image
    {
        public Image()
        {
        }

        public string PrimaryKeyName
        {
            get { return "ImageId"; }
        }

        public Guid ImageId { get; set; }

        public string Description { get; set; }

        public byte[] ImageData { get; set; }

        public string Name { get; set; }

        public State State { get; set; }

        public byte[] Timestamp { get; set; }
    }
}