using Newtonsoft.Json;

namespace CosmoBingoSample
{
    public class BingoData
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string category { get; set; }
        public int[] numberData { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}