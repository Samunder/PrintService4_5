namespace Butterfly.Print.Objects
{
    using System;
    using System.Collections.Generic;

    public class Document
    {
        public Document()
        {
        }

        public string PrimaryKeyName
        {
            get { return "DocumentId"; }
        }

        public Guid DocumentId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<DocumentDetail> DocumentDetails { get; set; }

        public State State { get; set; }

        public byte[] Timestamp { get; set; }
    }
}