using BingoWeb;
using CosmoBingoSample;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BindoWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BingoCreateCardController : ControllerBase
    {
        private readonly ILogger<BingoCreateCardController> _logger;
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;
        private Random random;

        public BingoCreateCardController(ILogger<BingoCreateCardController> logger,WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
            this.cache = cache;
            this.cosmosCall = cosmosCall;

            //乱数初期化（2021.11.21時点ではビンゴカード生成のみに使用）
            if (webSettings.RandomSeed == null)
            {
                random = new Random();
            }
            else
            {
                random = new Random((int)webSettings.RandomSeed);
            }
        }

        /// <summary>
        /// ビンゴカードを生成してDBに保存
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public void Get(string env,int n)
        {
            //env未指定はnullに正規化
            if (String.IsNullOrWhiteSpace(env) || env == "null" || env == "undefined") env = null;

            var bingo = new BingoUtil(webSettings,cache,cosmosCall);

            //変更前のカードを取得
            List<BingoData> before=null;
            before = bingo.QueryItems<BingoData>(env,"Card");

            //n=0の場合Cardを全件削除
            if (n == 0)
            {
                foreach (var item in before)
                {
                    bingo.DeleteById<BingoData>(item.id, item.category);
                }
            }

            //カード枚数をDBに記録
            var data = new BingoData
            {
                id= BingoUtil.IdFormat(env,"MaxCardNo",0),
                category=BingoUtil.CategoryFormat(env, "MaxCardNo"),
                numberData=new int[] {n}
            };
            bingo.AddOrReplaceBingo(data);

            if (n > 0)
            {
                //Cardを生成
                for (var i = 1; i <= n; i++)
                {
                    var id = BingoUtil.IdFormat(env,"Card", i);
                    var bingodata = OneCard(id,env);
                    bingo.AddOrReplaceBingo(bingodata);
                }
            }
        }

        BingoData OneCard(string id,string env)
        {
            var item=new BingoData();
            item.id = id;
            item.category = BingoUtil.CategoryFormat(env,"Card");

            var listUsed = new List<int>();
            var listNum = new List<int>();
            for(var i = 0; i < 25; i++)
            {
                listNum.Add(NextNumber(listUsed));
            }
            item.numberData = listNum.ToArray();

            return item;
        }

        /// <summary>
        /// 重ならない1-75の数値を返す
        /// </summary>
        /// <param name="list">使用済みを覚える</param>
        /// <returns></returns>
        int NextNumber(List<int> list)
        {
            var num = random.Next(1,75);
            for(var i = 0; i < 75; i++)
            {
                if (list.Contains(num))
                {
                    num++;
                    if (num > 75) num = 1;
                    continue;
                }
            }
            list.Add(num);
            return num;
        }
    }
}
