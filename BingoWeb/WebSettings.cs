namespace BindoWeb
{
    /// <summary>
    /// 全体設定のシングルインスタンス appsettingsのインジェクションに使用
    /// </summary>
    public class WebSettings
    {
        // The Azure Cosmos DB endpoint for running this sample.
        public string EndpointUri { get; set; }

        // The primary key for the Azure Cosmos account.
        public string PrimaryKey { get; set; }

        // The name of the database and container we will create
        public string DatabaseId { get; set; }
        public string ContainerId { get; set; }
        public int? RandomSeed { get; set; }
        public int MaxBingoCard { get; set; }
        public string Cachekey { get; set; } //メモリキャッシュのキャッシュ名
        public string CacheSpanSeconds { get; set; } //メモリキャッシュの有効期限秒数
        public bool DebugLog { get; set; }
        public string ApplicationName { get; set; }
    }
}
