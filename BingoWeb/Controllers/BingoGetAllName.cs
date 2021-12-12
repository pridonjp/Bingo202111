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
    public class BingoGetAllNameController : ControllerBase
    {
        private readonly ILogger<BingoGetAllNameController> _logger;
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;

        public BingoGetAllNameController(ILogger<BingoGetAllNameController> logger,WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
            this.cache = cache;
            this.cosmosCall = cosmosCall;
        }

        [HttpGet]
        public List<BingoName> Get(string env)
        {
            //var category = BingoUtil.CategoryFormat(env,"Name");
            var category = "Name";
            var bingo = new BingoUtil(webSettings,cache,cosmosCall);
            //Nameを全件取得
            return bingo.QueryItems<BingoName>(env,category);
        }

    }
}
