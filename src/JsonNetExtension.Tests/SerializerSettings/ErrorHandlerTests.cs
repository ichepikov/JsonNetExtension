using System.ComponentModel;
using DeepEqual.Syntax;
using Newtonsoft.Json;
using Xunit;

namespace JsonNetExtension.Tests.SerializerSettings
{
    public class ErrorHandlerTests
    {
        [Fact]
        public void PropertyWithConverterDeserializationTest()
        {
            var jsonString =
                "{\"date\":\"2000{}05{}01\",\"number2\":\"2000{}05{}01\",\"number3\":5,\"enum1\":\"Value1\",\"enum2\":2}";

            var expectedResult = new TestModel
            {
                Number = 2,
                Number2 = 2,
                Number3 = 5,
                EnumValue1 = TestEnum.Value1,
                EnumValue2 = TestEnum.Value1,
                EnumValue3 = TestEnum.Default,
                EnumValue4 = TestEnum.Default,
            };

            var result = JsonConvert.DeserializeObject<TestModel>(
                jsonString, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    Error = (s, a) => a.ErrorContext.Handled = true,
                });
            expectedResult.ShouldDeepEqual(result);
        }

        public class TestModel
        {
            [DefaultValue(2)]
            [JsonProperty("number1")]
            public int Number { get; set; }

            [DefaultValue(2)]
            [JsonProperty("number2")]
            public int Number2 { get; set; }

            [DefaultValue(2)]
            [JsonProperty("number3")]
            public int Number3 { get; set; }

            [DefaultValue(TestEnum.Default)]
            [JsonProperty("enum1")]
            public TestEnum EnumValue1 { get; set; }


            [DefaultValue(TestEnum.Default)]
            [JsonProperty("enum2")]
            public TestEnum EnumValue2 { get; set; }


            [DefaultValue(TestEnum.Default)]
            [JsonProperty("enum3")]
            public TestEnum EnumValue3 { get; set; }

            [DefaultValue(TestEnum.Default)]
            [JsonProperty("enum4")]
            public TestEnum EnumValue4 { get; set; }
        }

        public enum TestEnum
        {
            Unknown = 0,
            Default = 1,
            Value1 = 2,
            Value2 = 3,
        }
    }
}