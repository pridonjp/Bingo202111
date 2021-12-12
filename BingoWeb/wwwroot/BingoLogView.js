(function ($) {
    var bingo = {
        url: {
            BingoGetLog: "BingoGetLog",
            BingoDeleteLog: "BingoDeleteLog",
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
        $("#run,#run2").on("click", bingo.events.run);
        $("#delete").on("click", bingo.events.delete);
    }

    bingo.events.run = function () {
        $("#log").empty();
        var query = {
            url: bingo.url.BingoGetLog,
            type: "get",
        }
       return $.ajax(query)
            .then(function (result) {
                for (var i = 0; i < result.length; i++) {
                    $("#log").append("<li>"+result[i]+"</li>");
                }
            })
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