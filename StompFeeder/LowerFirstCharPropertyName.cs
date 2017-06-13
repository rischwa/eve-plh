using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StompFeeder
{
    public class LowerFirstCharPropertyName : DefaultContractResolver
    {

        private class MyConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is bool)
                {
                    writer.WriteValue((bool)value ? "1" : "0");
                    return;
                }

                writer.WriteValue(value != null ? value.ToString() : null);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return true;
            }
        }

        private static readonly MyConverter CONVERTER = new MyConverter();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            prop.PropertyName = prop.PropertyName.Substring(0, 1)
                                    .ToLowerInvariant() + prop.PropertyName.Substring(1);

            prop.PropertyType = typeof (String);

            prop.MemberConverter = CONVERTER;

            return prop;
        }
    }
}
