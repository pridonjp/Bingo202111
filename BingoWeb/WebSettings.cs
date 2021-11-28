namespace BindoWeb
{
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

        //2021.11.21 追加予定昨日
        //public readonly string IdPrifix = "Card.{0}";
    }
}
