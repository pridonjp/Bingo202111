﻿using BingoWeb;
using Newtonsoft.Json;

namespace CosmoBingoSample
{
    /// <summary>
    /// ビンゴカードのデータと当たり番号のデータ、ビンゴ履歴の記録に使用するデータモデル
    /// </summary>
    public class BingoData : IBingo
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