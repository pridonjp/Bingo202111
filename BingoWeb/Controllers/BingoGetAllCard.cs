using BingoWeb;
using CosmoBingoSample;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace BindoWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BingoGetAllCardController : ControllerBase
    {
        private readonly ILogger<BingoGetAllCardController> _logger;
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;

        public BingoGetAllCardController(ILogger<BingoGetAllCardController> logger,WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
            this.cache = cache;
            this.cosmosCall = cosmosCall;
        }

        [HttpGet]
        public BingoData[] Get(string env)
        {
            //var category = BingoUtil.CategoryFormat(env,"Card");
            var category = "Card";

            var bingo = new BingoUtil(webSettings,cache,cosmosCall);

            //Cardを全件取得
            var bingos = bingo.QueryItems<BingoData>(env,category);

            var list = new BingoData[bingos.Count] ;
            foreach(var item in bingos)
            {
                var al = item.id.Split('.');
                var idx = int.Parse(al[al.Length-1])-1;
                if (list.Length - 1 < idx)
                {
                    Array.Resize(ref list, idx+1);
                }
                list[idx]= item;
            }
            return list;
        }

    }
}
