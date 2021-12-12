using BindoWeb;
using CosmoBingoSample;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BingoWeb
{
    /// <summary>
    /// Azure Cosmos DBにアクセスするシングルインスタンス
    /// CosmosClient自体をシングルインスタンス化したので、もう要らないかも…
    /// でも、同時実行数など将来的にここで絞るかも
    /// </summary>
    public class CosmosCall
    {
        // The Cosmos client instance
        private CosmosClient cosmosClient;
        private Database database;
        private Container container;
        private WebSettings webSettings;

        /// <summary>
        /// コンストラクタ（2021.11.21時点唯一のコンストラクタ））
        /// </summary>
        /// <param name="webSettings">webSettings appsettings.jsonから必要な情報を読み込んだクラス</param>
        public CosmosCall(WebSettings webSettings,CosmosClient cosmosClient)
        {
            this.webSettings = webSettings;

            //cosmosClient = new CosmosClient(webSettings.EndpointUri, webSettings.PrimaryKey, new CosmosClientOptions() { ApplicationName = "Bingo" });
            this.cosmosClient = cosmosClient;
            database=this.cosmosClient.GetDatabase(webSettings.DatabaseId);
            container = this.database.GetContainer(webSettings.ContainerId);            
        }

        /// <summary>
        /// idからcategoryを抽出（仕様上id文字列にcategoryも含めている）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string id2category(string id)
        {
            var al = (id.Split(new char[] { '.' }));
            if (al.Length > 2)
            {
                return String.Format("{0}.{1}", al[0], al[1]);
            }
            return al[0];
        }

        /// <summary>
        /// 環境コード付きのcategory文字列を返す
        /// </summary>
        /// <param name="env"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string CategoryFormat(string env, string category)
        {
            //env未指定はnullに正規化
            if (String.IsNullOrWhiteSpace(env) || env == "null" || env == "undefined") env = null;
            if (env == null)
            {
                return category;
            }
            else
            {
                return String.Format("{0}.{1}", category, env);
            }
        }
        /// <summary>
        /// 環境コード、カテゴリー付きのidを返す
        /// </summary>
        /// <param name="env"></param>
        /// <param name="category"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string IdFormat(string env, string category, int id)
        {
            //env未指定はnullに正規化
            if (String.IsNullOrWhiteSpace(env) || env == "null" || env == "undefined") env = null;
            if (env == null)
            {
                return String.Format("{0}.{1}", category, id);
            }
            else
            {
                return String.Format("{0}.{1}.{2}", category, env, id);
            }

        }

        /// <summary>
        /// categoryを指定して一覧を取得
        /// </summary>
        public List<T> QueryItems<T>(string env, string category) where T : IBingo
        {
            try
            {
                var sqlQueryText = String.Format("SELECT * FROM c WHERE c.category = '{0}'", CategoryFormat(env, category));
                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                //LogWriteWithTime("QueryItems.GetItemQueryIterator", env + " " + category);
                FeedIterator<T> queryResultSetIterator = this.container.GetItemQueryIterator<T>(queryDefinition);

                List<T> items = new List<T>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    //LogWriteWithTime("QueryItems.ReadNextAsync", env + " " + category);
                    Task<FeedResponse<T>> currentResultSet = queryResultSetIterator.ReadNextAsync();
                    currentResultSet.Wait();
                    foreach (T row in currentResultSet.Result)
                    {
                        items.Add(row);
                        //WriteCache(row.id, row);
                    }
                }

                return items;

            }
            catch (Exception ex)
            {
                //LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// idを指定して削除
        /// </summary>
        /// <param name="id"></param>
        public void DeleteById<T>(string id, string category) where T : IBingo
        {
            try
            {
                var task = this.container.DeleteItemAsync<T>(id, new PartitionKey(category));
                task.Wait();
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }
        }

        /// <summary>
        /// 追加または変更
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void AddOrReplaceBingo<T>(T data) where T : IBingo
        {
            try
            {
                try
                {
                    //LogWriteWithTime("AddOrReplaceBingo.ReadItemAsync", data.id);
                    Task<ItemResponse<T>> task = this.container.ReadItemAsync<T>(data.id, new PartitionKey(data.category));
                    task.Wait();
                }
                catch (Exception ex) when (ex is CosmosException && ((CosmosException)ex).StatusCode == HttpStatusCode.NotFound || ex.InnerException is CosmosException && ((CosmosException)ex.InnerException).StatusCode == HttpStatusCode.NotFound)
                {
                    //LogWriteWithTime("AddOrReplaceBingo.CreateItemAsync", data.id);
                    Task<ItemResponse<T>> task = this.container.CreateItemAsync<T>(data, new PartitionKey(data.category));
                    task.Wait();
                    return;
                }
                {
                    //LogWriteWithTime("AddOrReplaceBingo.ReplaceItemAsync", data.id);
                    Task<ItemResponse<T>> task = this.container.ReplaceItemAsync<T>(data, data.id, new PartitionKey(data.category));
                    task.Wait();
                    return;
                }
            }
            catch (Exception ex)
            {
                //LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public T GetItemById<T>(string id) where T : class, IBingo
        {
            try
            {
                var category = id2category(id);

                T data = null;
                try
                {
                    //LogWriteWithTime("GetItemById.ReadItemAsync", id);
                    Task<ItemResponse<T>> task = this.container.ReadItemAsync<T>(id, new PartitionKey(category));
                    task.Wait();
                    data = (T)task.Result;
                }
                catch (Exception ex) when (ex is CosmosException && ((CosmosException)ex).StatusCode == HttpStatusCode.NotFound || ex.InnerException is CosmosException && ((CosmosException)ex.InnerException).StatusCode == HttpStatusCode.NotFound)
                {
                    data = null;
                }
                return data;
            }
            catch (Exception ex)
            {
                //LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// コンテナを削除
        /// </summary>
        public void DeleteContainer()
        {
            try
            {
                var task = this.container.DeleteContainerAsync();
                task.Wait();
            }
            catch (Exception ex)
            {
                //LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// コンテナを作成
        /// </summary>
        public void CreateContainer()
        {
            try
            {
                var task = this.database.CreateContainerAsync(webSettings.ContainerId, "/category", 400);
                task.Wait();

            }
            catch (Exception ex)
            {
                //LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

    }
}
