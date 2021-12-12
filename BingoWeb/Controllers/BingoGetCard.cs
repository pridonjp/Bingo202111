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
    public class BingoGetCardController : ControllerBase
    {
        private readonly ILogger<BingoGetCardController> _logger;
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;

        public BingoGetCardController(ILogger<BingoGetCardController> logger,WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
            this.cache = cache;
            this.cosmosCall = cosmosCall;
        }

        [HttpGet]
        public BingoData Get(string id)
        {
                    var bingo = new BingoUtil(webSettings,cache,cosmosCall);
                    var result = bingo.GetItemById<BingoData>(id);
                    return result;
        }

    }
}
