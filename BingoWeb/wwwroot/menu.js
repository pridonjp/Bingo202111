(function ($) {
    var bingo = {
        url: {
            BongoSample:"BongoSample", //APIコントローラURLサンプル
        },
        elements: {
        },
        events: {},
        data: {},
        values: {},
        async: {
            success: function () {var d = $.Deferred();d.resolve();return d.promise();},
            fail: function () {var d = $.Deferred();d.reject();return d.promise();}
        }
    }

    $(document).ready(function () {
        bingo.events.initialize();
    });

    bingo.events.initialize = function () {
        if (false) {
            $("#").on("click", bingo.events);　//クリックイベントサンプル

            //APIコントローラ呼び出しサンプル
            var query = {
                url: bingo.url.BongoSample,
                type: "get",
            }
            return $.ajax(query)
                .then(function (result) {
                })
                .fail(function (error) {
                })
        }
    }

    bingo.events.run = function () {
    }
    bingo.events.delete = function () {
        $("#log").empty();
        var query = {
            url: bingo.url.BingoDeleteLog,
            type: "get",
        }
        return $.ajax(query);
    }

})(jQuery);