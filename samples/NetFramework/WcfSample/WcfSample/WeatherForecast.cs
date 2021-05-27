using System;
using System.Runtime.Serialization;

namespace WcfSample
{
    [DataContract]
    public class WeatherForecast
    {
        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public int TemperatureC { get; set; }

        [DataMember]
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [DataMember]
        public string Summary { get; set; }
    }
}
