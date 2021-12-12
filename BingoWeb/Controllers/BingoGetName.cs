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
    public class BingoGetNameController : ControllerBase
    {
        private readonly ILogger<BingoGetNameController> _logger;
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;

        public BingoGetNameController(ILogger<BingoGetNameController> logger,WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;

            this.cache = cache;
            this.cosmosCall = cosmosCall;
        }

        [HttpGet]
        public BingoName Get(string id)
        {
                    var bingo = new BingoUtil(webSettings,cache,cosmosCall);
                    var result = bingo.GetItemById<BingoName>(id);
                    return result;
        }

    }
}
