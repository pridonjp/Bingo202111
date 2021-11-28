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
    public class BingoCreateContainerController : ControllerBase
    {
        private readonly ILogger<BingoCreateContainerController> _logger;
        private readonly WebSettings webSettings;

        public BingoCreateContainerController(ILogger<BingoCreateContainerController> logger,WebSettings webSettings)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
        }

        [HttpGet]
        public void Get()
        {
            var bingo = new BingoUtil(webSettings);
            bingo.CreateContainer();
        }

    }
}
