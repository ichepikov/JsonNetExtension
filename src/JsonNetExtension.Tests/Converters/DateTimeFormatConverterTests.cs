using System;
using DeepEqual.Syntax;
using JsonNetExtension.Converters;
using Newtonsoft.Json;
using Xunit;

namespace JsonNetExtension.Tests.Converters
{
    public class DateTimeFormatConverterTests
    {
        [Fact]
        public void SimpleDeserializationDateFormatTest()
        {
            var jsonString =
                "{\"id\":1,\"date\":\"2000-05-01\"}";

            var expectedResult = new TestModelWithConverter {Id = 1, Date = new DateTime(2000, 05, 01)};

            var result = JsonConvert.DeserializeObject<TestModelWithConverter>(jsonString);

            expectedResult.ShouldDeepEqual(result);
        }

        [Fact]
        public void SimpleSerializationDateFormatTest()
        {
            var jsonString =
                "{\"id\":1,\"date\":\"2000-05-01\"}";

            var expectedResult = new TestModelWithConverter {Id = 1, Date = new DateTime(2000, 05, 01)};

            var result = JsonConvert.SerializeObject(expectedResult);
            Assert.Equal(jsonString, result);
        }

        [Fact]
        public void DeserializationWithConverterDateFormatTest()
        {
            var jsonString =
                "{\"Id\":1,\"Date\":\"2000{}05{}01\"}";

            var expectedResult = new TestModel {Id = 1, Date = new DateTime(2000, 05, 01)};

            var result =
                JsonConvert.DeserializeObject<TestModel>(jsonString, new DateTimeFormatConverter("yyyy{}MM{}dd"));

            expectedResult.ShouldDeepEqual(result);
        }

        public class TestModel
        {
            public int Id { get; set; }

            public DateTime Date { get; set; }
        }

        public class TestModelWithConverter
        {
            [JsonProperty("id")]
            public int Id { get; set; }


            [JsonProperty("date")]
            [JsonConverter(typeof(DateTimeFormatConverter), "yyyy-MM-dd")]
            public DateTime Date { get; set; }
        }
    }
}
