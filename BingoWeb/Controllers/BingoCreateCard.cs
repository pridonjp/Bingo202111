using CosmoBingoSample;
using Microsoft.AspNetCore.Mvc;
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
        private Random random;

        public BingoCreateCardController(ILogger<BingoCreateCardController> logger,WebSettings webSettings)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;

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
        public Object Get(string env)
        {
            var category = "Card";
            if (!String.IsNullOrEmpty(env) && env != "null" && env != "undefined")
            {
                category=String.Format("{0}.{1}",category,env);
            }

            var bingo = new BingoUtil(webSettings);

            //変更前のカードを取得
            List<BingoData> before=null;
            before = bingo.QueryItems(category);

            //Cardを全件削除
            foreach (var item in before)
            {
                bingo.DeleteById(item.id, item.category);
            }

            //Cardを生成
            for (var i = 1; i <= webSettings.MaxBingoCard; i++)
            {
                var id = String.Format(String.Format("{0}.{1}",category,i));
                var bingodata = OneCard(id,category);
                bingo.AddBingo(bingodata);
            }

            var after = bingo.QueryItems(category);

            return new { before, after };
        }

        BingoData OneCard(string id,string category)
        {
            var item=new BingoData();
            item.id = id;
            item.category = category;

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
