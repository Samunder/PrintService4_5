namespace Butterfly.Print.Objects
{
    using System;

    using DocFormObjects;

    public class Layout
    {
        public string PrimaryKeyName
        {
            get { return "LayoutId"; }
        }

        public Guid LayoutId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DocFormLayout DocFormLayout { get; set; }

        public State State { get; set; }

        public byte[] Timestamp { get; set; }

        public void InitializeDocFormLayout()
        {
            try
            {
                DocFormLayout.InitializeLayout();
            }
            catch (Exception ex)
            {
                throw new Exception("Print.Layout.InitializeDocFormLayout - Failed", ex);
            }
        }
    }
}