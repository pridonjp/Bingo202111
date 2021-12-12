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
    public class BingoGetLogController : ControllerBase
    {
        private readonly ILogger<BingoGetLogController> _logger;
        private readonly WebSettings webSettings;
        private readonly IMemoryCache cache;
        private readonly CosmosCall cosmosCall;

        public BingoGetLogController(ILogger<BingoGetLogController> logger,WebSettings webSettings, IMemoryCache cache,CosmosCall cosmosCall)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
            this.cache = cache;
            this.cosmosCall = cosmosCall;
        }

        [HttpGet]
        public List<string> Get(string from)
        {
            int ifrom;
            if (String.IsNullOrWhiteSpace(from))
            {
                ifrom = 0;
            }
            else
            {
                ifrom = int.Parse(from);
            }

            var bingo = new BingoUtil(webSettings,cache,cosmosCall);
            var ret=new List<string>();
            foreach(var row in bingo.LogRead())
            {
                var al=row.Split(',');
                if (al.Length >0)
                {
                    int n;
                    if(int.TryParse(al[0].Trim(), out n))
                    {
                        if(n>=ifrom)ret.Add(row);
                    }
                    else
                    {
                        ret.Add(row);
                    }
                }
            }
            return ret;
        }

    }
}
