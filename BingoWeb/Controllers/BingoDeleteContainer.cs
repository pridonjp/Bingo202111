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
    public class BingoDeleteContainerController : ControllerBase
    {
        private readonly ILogger<BingoDeleteContainerController> _logger;
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;

        public BingoDeleteContainerController(ILogger<BingoDeleteContainerController> logger,WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
            this.cache = cache;
            this.cosmosCall = cosmosCall;
        }

        [HttpGet]
        public void Get()
        {
            if(!webSettings.ContainerDeletable)throw new OperationCanceledException("ContainerDeletable設定がfalseのためコンテナは削除できません");
            var bingo = new BingoUtil(webSettings,cache,cosmosCall);
            bingo.DeleteContainer();
        }

    }
}
