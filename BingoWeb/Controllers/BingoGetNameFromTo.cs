using BingoWeb;
using CosmoBingoSample;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace BindoWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BingoGetNameFromToController : ControllerBase
    {
        private readonly ILogger<BingoGetNameFromToController> _logger;
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;

        public BingoGetNameFromToController(ILogger<BingoGetNameFromToController> logger,WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
            this.cache = cache;
            this.cosmosCall = cosmosCall;
        }

        [HttpGet]
        public List<BingoName> Get(string env, string category, int from, int to)
        {
            var ret = new List<BingoName>();
            var bingo = new BingoUtil(webSettings, cache,cosmosCall);
            for(int i = from; i <= to; i++)
            {
                ret.Add(bingo.GetItemById<BingoName>(BingoUtil.IdFormat(env,category,i)));
            }
            return ret;
        }

    }
}
