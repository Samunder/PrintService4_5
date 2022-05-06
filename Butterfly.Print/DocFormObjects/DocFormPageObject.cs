namespace Butterfly.Print.DocFormObjects
{
    using System;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public abstract class DocFormPageObject
    {
        public DocFormPageObject()
        {
            this.Name = "Object";
            this.Top = 100;
            this.Bottom = 300;
            this.Left = 100;
            this.Right = 500;
            this.Anchor = "Top";
        }

        [DefaultValue("Top")]
        public string Anchor { get; set; }

        public int Bottom { get; set; }

        public int Left { get; set; }

        public string Name { get; set; }

        public int Right { get; set; }

        public int Top { get; set; }

        public int PaddingTop { get; set; }

        public int PaddingBottom { get; set; }

        public double ScalingFactor { get; set; } = 1;

        public int ApplyScalingFactor(int value)
        {
            if (value == 0)
            {
                return value;
            }

            return this.ScalingFactor == 1 ? value : Convert.ToInt16(value * this.ScalingFactor);
        }
    }
    public class DocFormPageObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(DocFormPageObject));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (jo["_t"].Value<string>()?.Equals("DocFormText") ?? false)
            {
                return jo.ToObject<DocFormText>(serializer);
            }
            if (jo["_t"].Value<string>()?.Equals("DocFormRectangle") ?? false)
            {
                return jo.ToObject<DocFormRectangle>(serializer);
            }
            if (jo["_t"].Value<string>()?.Equals("DocFormLine") ?? false)
            {
                return jo.ToObject<DocFormLine>(serializer);
            }
            if (jo["_t"].Value<string>()?.Equals("DocFormImage") ?? false)
            {
                return jo.ToObject<DocFormImage>(serializer);
            }
            if (jo["_t"].Value<string>()?.Equals("DocFormEllipse") ?? false)
            {
                return jo.ToObject<DocFormEllipse>(serializer);
            }
            if (jo["_t"].Value<string>()?.Equals("DocFormBarCode") ?? false)
            {
                return jo.ToObject<DocFormBarCode>(serializer);
            }
            return null;
        }

        public override bool CanWrite
        {
            get { return false; }
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        //[BsonKnownTypes(typeof(DocFormBarCode), typeof(DocFormEllipse), typeof(DocFormImage), typeof(DocFormLine), typeof(DocFormRectangle), typeof(DocFormText))]
    }
}