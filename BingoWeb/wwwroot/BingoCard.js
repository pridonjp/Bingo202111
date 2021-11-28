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
            success: new Promise(function (resolve, reject) { resolve(); }),
            fail: new Promise(function (resolve, reject) { reject(); })
        }
    }

    $(document).ready(function () {
        bingo.events.initialize();
    });

    bingo.events.initialize = function () {
        bingo.elements.CardId = $("#CardId");
        bingo.elements.Load = $("#Load");
        bingo.elements.result = $("#result");
        bingo.elements.bingoBox = $(".bingoBox");

        bingo.elements.Load.on("click",bingo.events.Load)

        $(window).resize(bingo.events.resize);
        bingo.events.resize();

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
        bingo.values.id = bingo.elements.CardId.val();

        if (bingo.data.result && bingo.data.result.id == "Card." + bingo.values.id) {
            var o = bingo.async.success;
        } else {
            bingo.elements.bingoBox.find(".bingoNum").removeClass("bingoNumBingo");
            bingo.elements.bingoBox.find(".bingoNum.bingo-13").addClass("bingoNumBingo");

            var query = {
                url: bingo.url.BingoGetCard + "?id=Card." + bingo.values.id,
                type: "get",
            }
            var o=$.ajax(query)
                .then(function (result) {
                    if (!result || result == "") return bingo.async.fail;
                    bingo.data.result = result;
                    //bingo.elements.result.text(JSON.stringify(bingo.data.result));//デバッグ用

                    return bingo.events.bingoSetNumber();
                })
                .fail(function (error) {
                    alert(JSON.stringify(error));
                })
        }
        o=o.then(function () {
            var query = {
                url: bingo.url.BingoGetCard + "?id=Bingo.0",
                type: "get",
            }
            return $.ajax(query)
                .then(function (result) {
                    bingo.data.bingo = result;
                    //bingo.elements.result.text(bingo.elements.result.text() + " " + JSON.stringify(bingo.data.bingo));//デバッグ用
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