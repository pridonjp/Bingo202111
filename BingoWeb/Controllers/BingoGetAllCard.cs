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
    public class BingoGetAllCardController : ControllerBase
    {
        private readonly ILogger<BingoGetAllCardController> _logger;
        private readonly WebSettings webSettings;

        public BingoGetAllCardController(ILogger<BingoGetAllCardController> logger,WebSettings webSettings)
        {
            _logger = logger;

            //appsettingsを取得 Startup.csで準備しておく必要がある
            this.webSettings= webSettings;
        }

        [HttpGet]
        public BingoData[] Get(string env)
        {
            var category = "Card";
            if (!String.IsNullOrEmpty(env) && env!="null" && env!="undefined")
            {
                category = String.Format("{0}.{1}", category, env);
            }

            var bingo = new BingoUtil(webSettings);

            //Cardを全件取得
            var bingos = bingo.QueryItems(category);

            var list = new BingoData[bingos.Count] ;
            foreach(var item in bingos)
            {
                var al = item.id.Split('.');
                var idx = int.Parse(al[al.Length-1])-1;
                if (list.Length - 1 < idx)
                {
                    Array.Resize(ref list, idx+1);
                }
                list[idx]= item;
            }
            return list;
        }

    }
}
