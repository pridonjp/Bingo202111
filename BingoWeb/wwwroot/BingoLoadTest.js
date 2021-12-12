(function ($) {
    var bingo = {
        url: {
            BingoGetCard: "BingoGetCard",
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
        $("#run").on("click", bingo.events.run);
        $("#run2").on("click", bingo.events.run2);
    }


    bingo.events.run = function () {
        $("#log").empty();
        $("#success").text("0");
        $("#error").text("0");
        var env = "";
        for (var i = 0; i < parseInt($("#times").val()); i++) {
            (function (idx) {
                var query = {
                    url: bingo.url.BingoGetCard + "?id=Bingo." + (env && env.length > 0 ? env + "." : "") + "0",
                    type: "get",
                }
                var start = Date();
                bingo.events.log(idx, false, undefined, start);
                return $.ajax(query)
                    .then(function (result) {
                        $("#success").text(parseInt($("#success").text())+1);
                        bingo.events.log(idx, true, undefined, start, Date());
                    })
                    .fail(function (error) {
                        $("#error").text(parseInt($("#error").text()) + 1);
                        bingo.events.log(idx, true, JSON.stringify(error), start, Date());
                    })

            }) (i);
        }
    }

    bingo.events.run2 = function () {
        for (var i = 1; i <= parseInt($("#times2").val()); i++) {
            var url = "BingoCard.html?card=" + i + "&loadtest=" + $("#span2").val();
            window.open(url);
        }
    }

    bingo.events.log = function (idx, done, error, start) {
        var id = "idx"+idx;
        if ($("#" + id).length == 0) {
            var e = $("<li>NEW</li>").attr("id", id);
            $("#log").append(e);
        }
        var log = idx + " start:" + start + " " + (done ? "Done:" + Date() + " " + (error ? error : "Success") : "Running... ");
        $("#" + id).text(log);
    }

})(jQuery);