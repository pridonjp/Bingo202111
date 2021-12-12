(function ($) {
    var bingo = {
        url: {
            BingoGetCard: "BingoGetCard",
            BingoGetName: "BingoGetName",
            BingoSave: "BingoSave",
            BingoNameSave: "BingoNameSave",
        },
        elements: {
        },
        events: {},
        data: {},
        values: {},
        async: {
            success: function () {var d = $.Deferred();d.resolve();return d.promise();},
            fail: function () {var d = $.Deferred();d.reject();return d.promise();}
        },
        util: {
            CategoryFormat: function (env, category) {
                return category + (env && env.length > 0 ? "." + env : "");
            },
            IdFormat: function (env, category, id) {
                return category + (env && env.length > 0 ? "." + env : "") + "." + id;
            }

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
        bingo.elements.autoreload = $("#autoreload");
        bingo.elements.changename = $("#changename");

        //以下の何れかで指定
        //url?Card=カード番号
        //url?Env=環境コード
        //url?Card=カード番号&Env=環境コード
        //url#カード番号
        //url#環境コード
        //url#環境コード,カード番号
        var cardid = bingo.events.getParam("[cC][aA][rR][dD]");
        var env = bingo.events.getParam("[eE][nN][vV]");
        bingo.values.loadtest = bingo.events.getParam("[lL][oO][aA][dD][tT][eE][sS][tT]");
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

        bingo.elements.Load.on("click", function () {
            bingo.data.result = {};
            bingo.events.Load();
        })

        $(window).resize(bingo.events.resize);
        bingo.events.resize();

        bingo.elements.autoreload.on("change", bingo.events.changereload);

        bingo.elements.changename.on("click", bingo.events.changename);

        var cardid = bingo.elements.CardId.val();
        if (cardid && cardid.length > 0) {
            bingo.events.Load().then(function () {
                bingo.events.LoadName();

            })
        }

        if (bingo.values.loadtest) {
            var c = parseInt(bingo.values.loadtest) * 1000;
            var f = false;
            $("#autoreload").find("option").each(function (i, v) {
                v = $(v);
                if (v.val() == String(c)) f = true;
            });
            if (!f) $("#autoreload").append('<option value="'+c+'">'+c+'</option>');
            $("#autoreload").val(c);
            setTimeout(bingo.events.changereload, Math.floor(Math.random() * 2000));
        }
    }

    bingo.events.changename = function () {
        var env = bingo.elements.Env.text();
        var id = bingo.elements.CardId.val();
        if (id.length == 0) return;
        var data = {
            id: bingo.util.IdFormat(env,"Name",id),
            category: bingo.util.CategoryFormat(env,"Name"),
            name: $("#name").val()
        }
        var query = {
            url: bingo.url.BingoNameSave,
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
        }
        $.ajax(query)
    }

    bingo.events.changereload = function () {
        if (bingo.elements.autoreload.val() > 0) {
            setTimeout(bingo.events.reload, parseInt(bingo.elements.autoreload.val()));
        }
    }

    bingo.events.reload = function () {
        if (bingo.elements.autoreload.val() == 0) return;
        var now = new Date();
        $("#last").text(now.getMinutes() + ":" + now.getSeconds())

        bingo.events.Load()
            .always(function () {
                setTimeout(bingo.events.reload, parseInt(bingo.elements.autoreload.val()));
            });
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
        var cardid = bingo.elements.CardId.val();
        if (cardid.length == 0) return bingo.async.success();

        $("#loading").removeClass("hideloading");

        return bingo.async.success()
            .then(function () {
                if (bingo.data.result && bingo.data.result.id == bingo.util.IdFormat(env, "Card", cardid)) {
                } else {
                    bingo.elements.bingoBox.find(".bingoNum").removeClass("bingoNumBingo");
                    bingo.elements.bingoBox.find(".bingoNum.bingo-13").addClass("bingoNumBingo");

                    var query = {
                        url: bingo.url.BingoGetCard + "?id=" + bingo.util.IdFormat(env, "Card", cardid),
                        type: "get",
                    }
                     return $.ajax(query)
                        .then(function (result) {
                            if (!result || result == "") return bingo.async.fail;
                            bingo.data.result = result;

                            return bingo.events.bingoSetNumber();
                        })
                        .fail(function (error) {
                            if (!bingo.values.loadtest)alert(JSON.stringify(error)+" query="+JSON.stringify(query));
                        })
                }

            })
        .then(function () {
            var env = bingo.elements.Env.text();
            var query = {
                url: bingo.url.BingoGetCard + "?id="+bingo.util.IdFormat(env,"Bingo","0"),
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
                    if (!bingo.values.loadtest)alert(JSON.stringify(error) + " query=" + JSON.stringify(query));
                })
        })
        .always(function () {
            $("#loading").addClass("hideloading");
        });
    }

    bingo.events.LoadName = function () {
        var env = bingo.elements.Env.text();
        var cardid = bingo.elements.CardId.val();
        var query = {
            url: bingo.url.BingoGetName + "?id=" + bingo.util.IdFormat(env, "Name", cardid),
            type: "get",
        }
        return $.ajax(query)
            .then(function (result) {
                $("#name").val(result.name);
            })
            .fail(function (error) {
                if (!bingo.values.loadtest) alert(JSON.stringify(error) + " query=" + JSON.stringify(query));
            })
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