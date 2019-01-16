using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace JsonNetExtension.Converters
{
    public class NestedPropertyJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonContract = serializer.ContractResolver.ResolveContract(value.GetType()) as JsonObjectContract;

            var properties = jsonContract.Properties.Where(e => !e.Ignored && e.Readable);
            var result = new JObject();

            foreach (var jsonProperty in properties)
            {
                string jsonPath = jsonProperty.PropertyName;

                var nesting = jsonPath.Split('.');
                JObject lastLevel = result;

                for (int i = 0; i < nesting.Length; ++i)
                {
                    if (i == (nesting.Length - 1))
                    {
                        lastLevel[nesting[i]] =
                            JToken.FromObject(jsonProperty.ValueProvider.GetValue(value), serializer);
                    }
                    else
                    {
                        if (lastLevel[nesting[i]] == null)
                            lastLevel[nesting[i]] = new JObject();

                        lastLevel = (JObject) lastLevel[nesting[i]];
                    }
                }
            }

            serializer.Serialize(writer, result);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JToken.Load(reader);

            var jsonContract = serializer.ContractResolver.ResolveContract(objectType) as JsonObjectContract;

            var result = jsonContract.DefaultCreator();

            foreach (JsonProperty prop in jsonContract.Properties.Where(p => p.Writable && !p.Ignored))
            {
                string jsonPath = prop.PropertyName;

                if (!Regex.IsMatch(jsonPath, "^[a-zA-Z0-9_.-]+$"))
                    throw new InvalidOperationException(
                        $"JProperties of JsonPathConverter can have only letters, numbers, underscores, hyphens and dots but name was ${jsonPath}."); // Array operations not permitted

                JToken token = jsonObject.SelectToken(jsonPath);

                if (token != null && token.Type != JTokenType.Null)
                {
                    object value;
                    if (prop.Converter == null || !prop.Converter.CanRead)
                    {
                        value = token.ToObject(prop.PropertyType, serializer);
                    }
                    else
                    {
                        var r = token.CreateReader();
                        r.Read();
                        value = prop.Converter.ReadJson(r, prop.PropertyType, null, serializer);
                    }

                    prop.ValueProvider.SetValue(result, value);
                }
            }

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            var contract =
                JsonSerializer.Create().ContractResolver.ResolveContract(objectType);

            return contract is JsonObjectContract objectContract &&
                   objectContract.Properties.Any(e => !e.Ignored && e.PropertyName.Contains('.'));
        }
    }
}
