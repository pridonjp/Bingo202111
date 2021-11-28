using CosmoBingoSample;
using Microsoft.AspNetCore.Mvc;
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
    public class BingoGetCardController : ControllerBase
    {
        private readonly ILogger<BingoGetCardController> _logger;
        private readonly WebSettings webSettings;

        public BingoGetCardController(ILogger<BingoGetCardController> logger,WebSettings webSettings)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
        }

        [HttpGet]
        public BingoData Get(string id)
        {
            var bingo = new BingoUtil(webSettings);
            return bingo.QueryItemById(id);
        }

    }
}
