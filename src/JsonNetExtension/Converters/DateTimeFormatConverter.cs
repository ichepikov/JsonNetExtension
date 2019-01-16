using Newtonsoft.Json.Converters;

namespace JsonNetExtension.Converters
{
    public class DateTimeFormatConverter : IsoDateTimeConverter
    {
        public DateTimeFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
