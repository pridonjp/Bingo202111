using BingoWeb;
using Newtonsoft.Json;

namespace CosmoBingoSample
{
    /// <summary>
    /// Bingoカードに参加者別表示名称に使用するデータモデル
    /// </summary>
    public class BingoName : IBingo
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string category { get; set; }
        public string name { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}