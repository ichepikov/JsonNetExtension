using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace JsonNetExtension.Converters
{
    public class NestedPropertyJsonConverter : JsonConverter
    {
        private readonly char _pathSeparator;

        public NestedPropertyJsonConverter(char pathSeparator)
        {
            _pathSeparator = pathSeparator;
        }

        public NestedPropertyJsonConverter() : this('.')
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonContract = serializer.ContractResolver.ResolveContract(value.GetType()) as JsonObjectContract;

            var properties = jsonContract.Properties.Where(e => !e.Ignored && e.Readable);
            var result = new JObject();

            foreach (var jsonProperty in properties)
            {
                var propertyPath = jsonProperty.PropertyName.Split(_pathSeparator);

                JObject currentLevel = result;
                for (int i = 0; i < propertyPath.Length - 1; i++)
                {
                    if (currentLevel[propertyPath[i]] == null)
                        currentLevel[propertyPath[i]] = new JObject();

                    currentLevel = (JObject) currentLevel[propertyPath[i]];
                }

                var tokenValue = jsonProperty.ValueProvider.GetValue(value);
                
                JToken propretyValueToken;
                if (jsonProperty.Converter != null && jsonProperty.Converter.CanWrite)
                {
                    using (var stringWriter = new StringWriter())
                    using (var jsonWriter = new JsonTextWriter(stringWriter))
                    {
                        jsonProperty.Converter.WriteJson(jsonWriter, tokenValue, serializer);
                        propretyValueToken = JToken.Parse(stringWriter.ToString());
                    }
                }
                else
                {
                    propretyValueToken = JToken.FromObject(tokenValue);
                }

                currentLevel[propertyPath[propertyPath.Length - 1]] = propretyValueToken;
            }

            serializer.Serialize(writer, result);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            var jsonContract = serializer.ContractResolver.ResolveContract(objectType) as JsonObjectContract;

            var result = jsonContract.DefaultCreator();

            foreach (JsonProperty prop in jsonContract.Properties.Where(p => p.Writable && !p.Ignored))
            {
                string jsonPath = prop.PropertyName;

                JToken token = SelectToken(jsonObject, jsonPath);

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

        private JToken SelectToken(JObject jsonObject, string path)
        {
            var jsonNodesNames = path.Split(_pathSeparator);

            JToken currentNode = jsonObject;
            foreach (var jsonNodesName in jsonNodesNames)
            {
                if (currentNode is JObject currentObject)
                    currentNode = currentObject[jsonNodesName];
                else
                    return null;
            }

            return currentNode;
        }

        public override bool CanConvert(Type objectType)
        {
            var contract =
                JsonSerializer.Create().ContractResolver.ResolveContract(objectType);

            return contract is JsonObjectContract objectContract &&
                   objectContract.Properties.Any(e => !e.Ignored && e.PropertyName.Contains(_pathSeparator));
        }
    }
}
