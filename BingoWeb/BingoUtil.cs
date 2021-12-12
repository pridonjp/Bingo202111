using BingoWeb;
using CosmoBingoSample;
//using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BindoWeb
{
    /// <summary>
    ///メモリキャッシュに無かった事を返す用 
    /// </summary>
    class BingoCacheNotFoundException : Exception
    {
    }

    public class BingoUtil
    {
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;

        private readonly string cachekey;
        private readonly int cacheSpanSeconds;
        private readonly bool debugLog;

        /// <summary>
        /// コンストラクタ（2021.11.21時点唯一のコンストラクタ））
        /// </summary>
        /// <param name="webSettings">webSettings appsettings.jsonから必要な情報を読み込んだクラス</param>
        public BingoUtil(WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            this.webSettings = webSettings;
            this.cache = cache;
            this.cosmosCall = cosmosCall;

            this.cachekey = webSettings.Cachekey;
            this.cacheSpanSeconds = int.Parse(webSettings.CacheSpanSeconds);

            this.debugLog = webSettings.DebugLog;
        }

        /// <summary>
        /// 実行状況デバッグログの専用キャッシュを取得
        /// </summary>
        /// <returns></returns>
        private List<string> GetLogCache()
        {
            if (!debugLog) return null;
            var key = cachekey + "_" + "log";

            List<string> cachedata = null;
            lock (cache)
            {
                if (!cache.TryGetValue(key, out cachedata))
                {
                    var option = new MemoryCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromSeconds(cacheSpanSeconds));
                    cache.Set(key, new List<string>(), option);
                    cachedata = (List<string>)cache.Get(key);
                }
            }
            return cachedata;
        }
        /// <summary>
        /// 実行状況デバッグログの専用キャッシュを空にする
        /// </summary>
        public void DeleteLoagCache()
        {
            if (!debugLog) return;
            GetLogCache().Clear();
        }

        const int MaxLogNum=1000;//実行状況デバッグログの保管上限

        /// <summary>
        /// 実行状況デバッグログの記録
        /// </summary>
        /// <param name="data"></param>
        public void LogWrite(string data)
        {
            if (!debugLog) return;
            var list = GetLogCache();
            list.Add(data);
            if (list.Count > MaxLogNum)
            {
                list.RemoveAt(0);
            }
        }

        /// <summary>
        /// 実行状況デバッグログの取得
        /// </summary>
        /// <returns></returns>
        public List<string> LogRead()
        {
            return GetLogCache();
        }


        /// <summary>
        /// メモリキャッシュに保管
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public void WriteCache<T>(string id,T data) where T : IBingo
        {
            var key = cachekey+"_"+typeof(T).Name;

            ConcurrentDictionary<string, T> cachedata=null;
            lock (cache)
            {
                if (!cache.TryGetValue(key, out cachedata))
                {
                    var option = new MemoryCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromSeconds(cacheSpanSeconds));
                    cache.Set(key, new ConcurrentDictionary<string, T>(), option);
                    cachedata=(ConcurrentDictionary<string,T>)cache.Get(key);
                }
            }
            cachedata[id] = data;
        }

        /// <summary>
        /// メモリキャッシュから読み込み
        /// キャッシュに無かった場合はBingoCacheNotFoundException発生
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T ReadCache<T>(string id) where T : class,IBingo
        {
            var key = cachekey + "_" + typeof(T).Name;

            ConcurrentDictionary<string, T> cachedata = null;
            lock (cache)
            {
                if (!cache.TryGetValue(key, out cachedata))
                {
                    var option = new MemoryCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromSeconds(cacheSpanSeconds));
                    cache.Set(key, new ConcurrentDictionary<string, T>(), option);
                    cachedata = (ConcurrentDictionary<string, T>)cache.Get(key);
                }
            }
            if (cachedata.ContainsKey(id))
            {
                return cachedata[id];
            }
            else
            {
                throw new BingoCacheNotFoundException();
            }
        }

        /// <summary>
        /// メモリキャッシュから削除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        public void RemoveCache<T>(string id)
        {
            var key = cachekey + "_" + typeof(T).Name;

            ConcurrentDictionary<string, T> cachedata = null;
            lock (cache)
            {
                if (!cache.TryGetValue(key, out cachedata))
                {
                    var option = new MemoryCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromSeconds(cacheSpanSeconds));
                    cache.Set(key, new ConcurrentDictionary<string, T>(), option);
                    cachedata = (ConcurrentDictionary<string, T>)cache.Get(key);
                }
            }
            if (cachedata.ContainsKey(id))
            {
                T data;
                cachedata.Remove(id,out data);
            }
        }

        /// <summary>
        /// キャッシュをクリヤ(BingoDataとBingoNameそれぞれ格納クラス名単位)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ClearCache<T>()
        {
            var key = cachekey + "_" + typeof(T).Name;

            ConcurrentDictionary<string, T> cachedata = null;
            lock (cache)
            {
                if (!cache.TryGetValue(key, out cachedata))
                {
                    var option = new MemoryCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromSeconds(cacheSpanSeconds));
                    cache.Set(key, new ConcurrentDictionary<string, T>(), option);
                    cachedata = (ConcurrentDictionary<string, T>)cache.Get(key);
                }
            }
            cache.Remove(key);
        }

        /// <summary>
        /// idからcategoryを抽出（仕様上id文字列にcategoryも含めている）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string id2category(string id)
        {
            var al= (id.Split(new char[] { '.' }));
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
        public static string CategoryFormat(string env,string category)
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
        public static string IdFormat(string env,string category, int id)
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

        static long logCount = 0; //実行状況デバッグログの行番号

        /// <summary>
        /// 実行状況デバッグログ 行番号、現在秒、時刻付きで記録
        /// </summary>
        /// <param name="name">実行中のメソッド名.ConsosDB呼び出しメソッド名</param>
        /// <param name="data">ログ識別（デバッグ）用にパラメータを記録</param>
        private void LogWriteWithTime(string name,string data)
        {
            var t=DateTime.Now;
            var s = String.Format("{0},{1},{2},{3},{4}", logCount++, t.ToString("ss"),t.ToString("yyyy-MM-dd HH:mm:ss.fff"),name,data);
            LogWrite(s);
        }

        /// <summary>
        /// categoryを指定して一覧を取得
        /// </summary>
        public List<T> QueryItems<T>(string env, string category) where T : IBingo
        {
            try
            {
                LogWriteWithTime("QueryItems.GetItemQueryIterator", env + " " + category);
                var items =cosmosCall.QueryItems<T>(env, category);
                foreach(var item in items)
                {
                    WriteCache(item.id, item);
                }
                return items;

            }catch (Exception ex){
                LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// idを指定して削除
        /// </summary>
        /// <param name="id"></param>
        public void DeleteById<T>(string id,string category) where T:IBingo
        {
            try
            {

                RemoveCache<T>(id);
                LogWriteWithTime("DeleteById.DeleteItemAsync",id);
                cosmosCall.DeleteById<T>(id, category);
            }catch (Exception ex){
                LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
                throw;
            }
       }
 
        /// <summary>
        /// 追加または変更
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void AddOrReplaceBingo<T>(T data) where T:IBingo
        {
            try
            {

                WriteCache(data.id, data);
                LogWriteWithTime("AddOrReplaceBingo.ReadItemAsync", data.id);
                cosmosCall.AddOrReplaceBingo(data);
            }catch (Exception ex){
                LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public T GetItemById<T>(string id) where T:class,IBingo
        {
            try
            {
                var category = id2category(id);

                T data = null;
                try {
                    data= ReadCache<T>(id);
                } catch (BingoCacheNotFoundException) { 
                    LogWriteWithTime("GetItemById.ReadItemAsync",id);
                    data=cosmosCall.GetItemById<T>(id);
                    WriteCache(id, data);
                }
                return data;
            }catch (Exception ex){
                LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
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
                ClearCache<BingoData>();
                ClearCache<BingoName>();
                LogWriteWithTime("DeleteContainer.DeleteContainerAsync","");
                cosmosCall.DeleteContainer();
            }
            catch (Exception ex)
            {
                LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
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
                LogWriteWithTime("GetItemById.CreateContainerAsync","");
                cosmosCall.CreateContainer();
            }
            catch (Exception ex)
            {
                LogWriteWithTime("[ERROR]", ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

    }
}
