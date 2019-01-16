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
        public void SimplePropertyDesirealizationTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":\"test\"}}";

            var expectedResult = new PropertyTestModel<string> {Data = "test"};

            var result = JsonConvert.DeserializeObject<PropertyTestModel<string>>(jsonString);

            expectedResult.ShouldDeepEqual(result);
        }

        [Fact]
        public void NestedObjectPropertyDesirealizationTest()
        {
            var jsonString =
                "{\"NestedNode\":{\"Data\":{\"name\":\"test\"}}}";

            var expectedResult = new PropertyTestModel<ModelWithJsonProperty> {Data = new ModelWithJsonProperty { Name = "test"}};

            var result = JsonConvert.DeserializeObject<PropertyTestModel<ModelWithJsonProperty>>(jsonString);

            expectedResult.ShouldDeepEqual(result);
        }

        [Fact]
        public void PropertyWithConverterDesirealizationTest()
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

        public class ModelWithJsonProperty
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}
