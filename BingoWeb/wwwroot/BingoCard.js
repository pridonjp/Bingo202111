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
        bingo.elements.CardId = $("#CardId");
        bingo.elements.Env = $("#Env");
        bingo.elements.Load = $("#Load");
        bingo.elements.result = $("#result");
        bingo.elements.bingoBox = $(".bingoBox");

        //以下の何れかで指定
        //url?Card=カード番号
        //url?Env=環境コード
        //url?Card=カード番号&Env=環境コード
        //url#カード番号
        //url#環境コード
        //url#環境コード,カード番号
        var cardid = bingo.events.getParam("[cC][aA][rR][dD]");
        var env = bingo.events.getParam("[eE][nN][vV]");
        var tag = location.hash;
        if (tag && tag.length > 0) {
            if (tag.substr(0, 1) == "#") tag = tag.substr(1);
            var tags = tag.split(",");
            if (tags.length == 1) {
                if (tags[0].search(/^[0-9]+$/)>=0) {
                    cardid = tags[0];
                } else {
                    env = tags[0];
                }
            } else {
                env = tags[0];
                cardid = tags[1];
            }
        }
        if (cardid) bingo.elements.CardId.val(cardid);
        if (env) bingo.elements.Env.text(env);

        bingo.elements.Load.on("click", bingo.events.Load)

        $(window).resize(bingo.events.resize);
        bingo.events.resize();

        var cardid = bingo.elements.CardId.val();
        if (cardid && cardid.length > 0) bingo.events.Load();

    }

    bingo.events.getParam = function (name) {
        var regex = new RegExp("[?&]" + name + "=([^&#]*)(&|#|$)");
        var matches = regex.exec(location.search);
        if (!matches) return null;
        if (!matches[1] || matches[1].length == 0) return null;
        return decodeURIComponent(matches[1].replace(/\+/g, " "));
    }

    bingo.events.resize = function () {
        var w = $(window).width();
        if (w > 767) w = 767;
        var width = w * 0.8;
        var left = w * 0.1;
        var cell= (w * 0.8 / 5) * 0.8 
        bingo.elements.bingoBox.width(width).height(width).css("margin-left", left);
        bingo.elements.bingoBox.find(".bingoCell").width(cell).height(cell).css("margin", cell * 0.15);
        bingo.elements.bingoBox.find(".bingoCell").css("font-size",cell * 0.32 +"px");
    }

    bingo.events.Load = function () {
        var env = bingo.elements.Env.text();
        var id = bingo.elements.CardId.val();
        id = "Card." + (env && env.length > 0 ? env + "." : "") + id;

        $("#loading").text("Loading...");

        if (bingo.data.result && bingo.data.result.id == id) {
            var o = bingo.async.success();
        } else {
            bingo.elements.bingoBox.find(".bingoNum").removeClass("bingoNumBingo");
            bingo.elements.bingoBox.find(".bingoNum.bingo-13").addClass("bingoNumBingo");

            var query = {
                url: bingo.url.BingoGetCard + "?id=" + id,
                type: "get",
            }
            var o=$.ajax(query)
                .then(function (result) {
                    if (!result || result == "") return bingo.async.fail;
                    bingo.data.result = result;

                    return bingo.events.bingoSetNumber();
                })
                .fail(function (error) {
                    alert(JSON.stringify(error));
                })
        }
        o=o.then(function () {
            var env = bingo.elements.Env.text();
            var query = {
                url: bingo.url.BingoGetCard + "?id=Bingo." + (env && env.length > 0 ? env + "." : "")+"0",
                type: "get",
            }
            return $.ajax(query)
                .then(function (result) {
                    bingo.data.bingo = result;
                    var box = bingo.elements.bingoBox;
                    for (var i = 0; i < 25; i++) {
                        var cell = box.find(".bingo-" + (i + 1));
                        var num = bingo.data.result.numberData[i];
                        if (bingo.data.bingo.numberData.indexOf(num)>=0) cell.addClass("bingoNumBingo");
                    }
                })
                .fail(function (error) {
                    alert(JSON.stringify(error));
                })
        })

        o.always(function () {
            $("#loading").text("");
        });
        return o;
    }

    bingo.events.bingoSetNumber = function () {
        var box = bingo.elements.bingoBox;
        for (var i = 0; i < 25; i++) {
            var cell = box.find(".bingo-" + (i + 1));
            var num = bingo.data.result.numberData[i];
            cell.text(num);
        }
    }

})(jQuery);