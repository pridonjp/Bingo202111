using CosmoBingoSample;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BindoWeb
{
    public class BingoUtil
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private readonly string EndpointUri;

        // The primary key for the Azure Cosmos account.
        private readonly string PrimaryKey;

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private readonly string databaseId;
        private readonly string containerId;

        /// <summary>
        /// コンストラクタ（2021.11.21時点唯一のコンストラクタ））
        /// </summary>
        /// <param name="webSettings">webSettings appsettings.jsonから必要な情報を読み込んだクラス</param>
        public BingoUtil(WebSettings webSettings)
        {
            EndpointUri = webSettings.EndpointUri;
            PrimaryKey = webSettings.PrimaryKey;
            databaseId=webSettings.DatabaseId;
            containerId = webSettings.ContainerId;
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "Bingo" });

            database=this.cosmosClient.GetDatabase(databaseId);
            container = this.database.GetContainer(containerId);            
        }

        /// <summary>
        /// categoryを指定して一覧を取得
        /// </summary>
        public List<BingoData> QueryItems(string category)
        {
            var sqlQueryText = String.Format("SELECT * FROM c WHERE c.category = '{0}'",category);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<BingoData> queryResultSetIterator = this.container.GetItemQueryIterator<BingoData>(queryDefinition);

            List<BingoData> bingos = new List<BingoData>();
            while (queryResultSetIterator.HasMoreResults)
            {
                Task<FeedResponse<BingoData>> currentResultSet = queryResultSetIterator.ReadNextAsync();
                currentResultSet.Wait();
                foreach (BingoData family in currentResultSet.Result)
                {
                    bingos.Add(family);
                }
            }

            return bingos;
        }

        /// <summary>
        /// idを指定して取得
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BingoData QueryItemById(string id)
        {
            var sqlQueryText = String.Format("SELECT * FROM c WHERE c.id = '{0}'", id);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<BingoData> queryResultSetIterator = this.container.GetItemQueryIterator<BingoData>(queryDefinition);

            List<BingoData> bingos = new List<BingoData>();
            while (queryResultSetIterator.HasMoreResults)
            {
                Task<FeedResponse<BingoData>> currentResultSet = queryResultSetIterator.ReadNextAsync();
                currentResultSet.Wait();
                foreach (BingoData row in currentResultSet.Result)
                {
                    bingos.Add(row);
                }
            }

            if (bingos.Count == 0)
            {
                return null;
            }
            else
            {
                return bingos[0];
            }
        }

        /// <summary>
        /// idを指定して削除
        /// </summary>
        /// <param name="id"></param>
        public void DeleteById(string id,string category)
        {
            try
            {
                var task = this.container.DeleteItemAsync<BingoData>(id, new PartitionKey(category));
                task.Wait();
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }
        }
 
        /// <summary>
        /// Cardを追加
        /// </summary>
        /// <param name="bingo"></param>
        public void AddBingo(BingoData bingo)
        {
                var item = QueryItemById(bingo.id);
                if (item!=null)
                {
                    DeleteById(bingo.id,bingo.category);
                }
                Task<ItemResponse<BingoData>> createResponse = this.container.CreateItemAsync<BingoData>(bingo, new PartitionKey(bingo.category));
                createResponse.Wait();
        }

        /// <summary>
        /// コンテナを削除
        /// </summary>
        public void DeleteContainer()
        {
            var task = this.container.DeleteContainerAsync();
            task.Wait();
        }

        /// <summary>
        /// コンテナを作成
        /// </summary>
        public void CreateContainer()
        {
            var task = this.database.CreateContainerAsync(containerId, "/category", 400);
            task.Wait();
        }

    }
}
