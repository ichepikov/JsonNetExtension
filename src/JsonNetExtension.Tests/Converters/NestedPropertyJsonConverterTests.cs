using System;
using DeepEqual.Syntax;
using JsonNetExtension.Converters;
using Newtonsoft.Json;
using Xunit;

namespace JsonNetExtension.Tests.Converters
{
    public class NestedPropertyJsonConverterTests
    {
        [Fact]
        public void SerializationWithSeparatorPropertyTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":\"test\"}}";

            var obj =
                new PathSeparatorTestModel<string> { Data = "test" };

            var result = JsonConvert.SerializeObject(obj);

            result.ShouldDeepEqual(jsonString);
        }

        [Fact]
        public void SerializationWithObjectPropertyTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":{\"name\":\"test\"}}}";

            var obj =
                new PropertyTestModel<ModelWithJsonProperty> {Data = new ModelWithJsonProperty {Name = "test"}};

            var result = JsonConvert.SerializeObject(obj);

            result.ShouldDeepEqual(jsonString);
        }

        [Fact]
        public void SerializationWithPropertyConverterTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":\"2000{}05{}01\"}}";

            var obj = new PropertyTestModelWithConverter {Data = new DateTime(2000, 5, 1)};

            var result =
                JsonConvert.SerializeObject(obj);

            result.ShouldDeepEqual(jsonString);
        }

        [Fact]
        public void SimpleTestModelSerializationTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":\"test\"}}";

            var obj = new SimpleTestModel<string> {Data = "test"};

            var result =
                JsonConvert.SerializeObject(obj, new NestedPropertyJsonConverter());

            result.ShouldDeepEqual(jsonString);
        }

        [Fact]
        public void SimpleTestModelDeserializationTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":\"test\"}}";

            var expectedResult = new SimpleTestModel<string> {Data = "test"};

            var result =
                JsonConvert.DeserializeObject<SimpleTestModel<string>>(jsonString, new NestedPropertyJsonConverter());

            expectedResult.ShouldDeepEqual(result);
        }

        [Fact]
        public void SimplePropertyDeserializationTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":\"test\"}}";

            var expectedResult = new PropertyTestModel<string> {Data = "test"};

            var result = JsonConvert.DeserializeObject<PropertyTestModel<string>>(jsonString);

            expectedResult.ShouldDeepEqual(result);
        }

        [Fact]
        public void NestedObjectPropertyDeserializationTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":{\"name\":\"test\"}}}";

            var expectedResult =
                new PropertyTestModel<ModelWithJsonProperty> {Data = new ModelWithJsonProperty {Name = "test"}};

            var result = JsonConvert.DeserializeObject<PropertyTestModel<ModelWithJsonProperty>>(jsonString);

            expectedResult.ShouldDeepEqual(result);
        }

        [Fact]
        public void PropertyWithConverterDeserializationTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":\"2000{}05{}01\"}}";

            var expectedResult = new PropertyTestModelWithConverter {Data = new DateTime(2000, 5, 1)};

            var result = JsonConvert.DeserializeObject<PropertyTestModelWithConverter>(jsonString);

            expectedResult.ShouldDeepEqual(result);
        }

        [JsonConverter(typeof(NestedPropertyJsonConverter))]
        public class PropertyTestModel<T>
        {
            [JsonProperty("NestedNode.Data")]
            public T Data { get; set; }
        }

        [JsonConverter(typeof(NestedPropertyJsonConverter))]
        public class PropertyTestModelWithConverter
        {
            [JsonConverter(typeof(DateTimeFormatConverter), "yyyy{}MM{}dd")]
            [JsonProperty("NestedNode.Data")]
            public DateTime Data { get; set; }
        }

        public class SimpleTestModel<T>
        {
            [JsonProperty("NestedNode.Data")]
            public T Data { get; set; }
        }

        [JsonConverter(typeof(NestedPropertyJsonConverter), '/')]
        public class PathSeparatorTestModel<T>
        {
            [JsonProperty("NestedNode/Data")]
            public T Data { get; set; }
        }

        public class ModelWithJsonProperty
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}
