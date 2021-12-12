using BingoWeb;
using CosmoBingoSample;
using Microsoft.AspNetCore.Http;
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
    public class BingoNameSaveController : ControllerBase
    {
        private readonly ILogger<BingoNameSaveController> _logger;
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;

        public BingoNameSaveController(ILogger<BingoNameSaveController> logger,WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
            this.cache = cache;
            this.cosmosCall= cosmosCall;
        }

        [HttpPost]
        public BingoName Post([FromBody]BingoName data)
        {
            var bingo = new BingoUtil(webSettings,cache,cosmosCall);
            bingo.AddOrReplaceBingo(data);
            return data;
        }


     }
}
