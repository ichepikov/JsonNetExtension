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

                foreach (var propertyNode in propertyPath.Take(propertyPath.Length - 1))
                {
                    var nextLevel = currentLevel[propertyNode] as JObject;
                   
                    if (nextLevel == null)
                        currentLevel[propertyNode] = nextLevel = new JObject();

                    currentLevel = nextLevel;
                }
                
                var tokenValue = jsonProperty.ValueProvider.GetValue(value);
                
                JToken propertyValueToken;
                if (jsonProperty.Converter != null && jsonProperty.Converter.CanWrite)
                {
                    using (var stringWriter = new StringWriter())
                    using (var jsonWriter = new JsonTextWriter(stringWriter))
                    {
                        jsonProperty.Converter.WriteJson(jsonWriter, tokenValue, serializer);
                        propertyValueToken = JToken.Parse(stringWriter.ToString());
                    }
                }
                else
                {
                    propertyValueToken = JToken.FromObject(tokenValue);
                }

                currentLevel[propertyPath[propertyPath.Length - 1]] = propertyValueToken;
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

                if (token != null)
                {
                    object value;
                    if (prop.Converter == null || !prop.Converter.CanRead)
                    {
                        value = token.ToObject(prop.PropertyType, serializer);
                    }
                    else
                    {
                        using (var jsonReader = token.CreateReader())
                        {
                            jsonReader.Read();
                            value = prop.Converter.ReadJson(jsonReader, prop.PropertyType, null, serializer);
                        }
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
            JsonContract contract =
                JsonSerializer.Create().ContractResolver.ResolveContract(objectType);
            
            return contract is JsonObjectContract objectContract &&
                   objectContract.Properties.Any(e => !e.Ignored && e.PropertyName.Contains(_pathSeparator));
        }
    }
}
