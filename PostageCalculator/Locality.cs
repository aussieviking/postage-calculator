using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PostageCalculator
{
    public class Locality
    {
        [JsonPropertyName("state")]
        public string State { get; set; }
    }

    public class Localities
    {
        [JsonPropertyName("locality")]
        [JsonConverter(typeof(SingleValueArrayConverter<Locality>))]
        public ICollection<Locality> Locality { get; set; }
    }

    public class RootObject
    {
        [JsonPropertyName("localities")]
        public Localities Localities { get; set; }
    }
}