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
        public void SimpleDateFormatTest()
        {
            var jsonString =
                "{\"id\": 1, \"date\": \"2000-05-01\"}";

            var expectedResult = new TestModel {Id = 1, Date = new DateTime(2000, 05, 01)};

            var result = JsonConvert.DeserializeObject<TestModel>(jsonString);

            expectedResult.ShouldDeepEqual(result);
        }

        public class TestModel
        {
            [JsonProperty("id")]
            public int Id { get; set; }


            [JsonProperty("date")]
            [JsonConverter(typeof(DateTimeFormatConverter), "yyyy-MM-dd")]
            public DateTime Date { get; set; }
        }
    }
}
