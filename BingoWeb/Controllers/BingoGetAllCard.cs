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
            var category = "Card";

            var bingo = new BingoUtil(webSettings,cache,cosmosCall);
            var maxcard = bingo.GetItemById<BingoData>(BingoUtil.IdFormat(env, "MaxCardNo", 0));
            var maxcardNum = maxcard.numberData[0];
            var list = new BingoData[maxcardNum];
            for (var i = 0; i < maxcardNum; i++)
            {
                var card = bingo.GetItemById<BingoData>(BingoUtil.IdFormat(env, category, i + 1));
                list[i] = card;
            }
            return list;
        }

    }
}
