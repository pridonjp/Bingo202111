(function ($) {
    var bingo = {
        url: {
            BingoGetCard: "BingoGetCard",
            BingoGetAllCard: "BingoGetAllCard",
            BingoGetAllName: "BingoGetAllName",
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
        $("#run").on("click", bingo.events.run);
        $("#run2").on("click", bingo.events.run2);
        $("#runCopy").on("click", bingo.events.runCopy);
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

    bingo.events.runCopy = function () {
        $("#log").empty();
        var env=$("#from").val();
        var envTo = $("#to").val();
        if (env == envTo) return;

        //maxcardをコピー
        var id = bingo.util.IdFormat(env, "MaxCardNo", "0");
        var maxcard = 0;
        var query = {
            url: bingo.url.BingoGetCard + "?id=" + id,
            type: "get",
        }
        var o = $.ajax(query)
            .then(function (result) {
                if (result && result.numberData[0] > 0) {
                    maxcard = result.numberData[0];
                    result.id = bingo.util.IdFormat(envTo, "MaxCardNo", "0");
                    result.category = bingo.util.CategoryFormat(envTo, "MaxCardNo");
                    var query = {
                        url: bingo.url.BingoSave,
                        type: "post",
                        data: JSON.stringify(result),
                        contentType: "application/json",
                        dataType: "json",
                    }
                    return $.ajax(query)
                        .then(function (result) {
                            bingo.events.logCopy(id, result.id);
                        })
                        .fail(function (error) {
                            bingo.events.logCopy(id, result.id,error);
                        })
                }
            })
        //Bingo.0結果をコピー
        o = o.then(function () {
            //Bingo0をコピー
            var id = bingo.util.IdFormat(env, "Bingo", "0");
            var query = {
                url: bingo.url.BingoGetCard + "?id=" + id,
                type: "get",
            }
            return $.ajax(query)
                .then(function (result) {
                    if (result && result.id) {
                        result.id = bingo.util.IdFormat(envTo, "Bingo", "0");
                        result.category = bingo.util.CategoryFormat(envTo, "Bingo");
                        var query = {
                            url: bingo.url.BingoSave,
                            type: "post",
                            data: JSON.stringify(result),
                            contentType: "application/json",
                            dataType: "json",
                        }
                        return $.ajax(query)
                            .then(function (result) {
                                bingo.events.logCopy(id, result.id);
                            })
                            .fail(function (error) {
                                bingo.events.logCopy(id, result.id, error);
                            })
                    }
                })

        });

        //History.0結果をコピー
        o = o.then(function () {
            //Bingo0をコピー
            var id = bingo.util.IdFormat(env, "History", "0");
            var query = {
                url: bingo.url.BingoGetCard + "?id=" + id,
                type: "get",
            }
            return $.ajax(query)
                .then(function (result) {
                    if (result && result.id) {
                        result.id = bingo.util.IdFormat(envTo, "History", "0");
                        result.category = bingo.util.CategoryFormat(envTo, "History");
                        var query = {
                            url: bingo.url.BingoSave,
                            type: "post",
                            data: JSON.stringify(result),
                            contentType: "application/json",
                            dataType: "json",
                        }
                        return $.ajax(query)
                            .then(function (result) {
                                bingo.events.logCopy(id, result.id);
                            })
                            .fail(function (error) {
                                bingo.events.logCopy(id, result.id, error);
                            })
                    }
                })

        });

        //Cardをコピー
        o = o.then(function () {
            var query = {
                url: bingo.url.BingoGetAllCard + "?env=" + env,
                type: "get",
            }
            return $.ajax(query)
                .then(function (result) {
                    var o2 = bingo.async.success();
                    for (var idx = 1; idx <= maxcard; idx++) {
                        (function(data) {
                            var id = data.id;
                            data.id = bingo.util.IdFormat(envTo, "Card", idx);
                            data.category = bingo.util.CategoryFormat(envTo, "Card");
                            var query = {
                                url: bingo.url.BingoSave,
                                type: "post",
                                data: JSON.stringify(data),
                                contentType: "application/json",
                                dataType: "json",
                            }

                            o2 = o2.then(function(){
                                return $.ajax(query)
                                    .then(function (result) {
                                        bingo.events.logCopy(id, data.id);
                                    })
                                    .fail(function (error) {
                                        bingo.events.logCopy(id, data.id, error);
                                    })
                            })
                        }) (result[idx - 1]);
                    }
                    return o2;
                })
                .fail(function (error) {
                    alert("137 " + JSON.stringify(error));
                })
        })

        //Nameをコピー
        o = o.then(function () {
            var query = {
                url: bingo.url.BingoGetAllName + "?env=" + env,
                type: "get",
            }
            return $.ajax(query)
                .then(function (result) {
                    var o2 = bingo.async.success();
                    for (var i = 0; i <= result.length; i++) {
                        (function (data) {
                            if (data && data.id) {
                                var id = data.id;
                                var idx = data.id.split(".");
                                idx = parseInt(idx[idx.length - 1]);
                                data.id = bingo.util.IdFormat(envTo, "Name", idx);
                                data.category = bingo.util.CategoryFormat(envTo, "Name");
                                var query = {
                                    url: bingo.url.BingoNameSave,
                                    type: "post",
                                    data: JSON.stringify(data),
                                    contentType: "application/json",
                                    dataType: "json",
                                }

                                o2 = o2.then(function () {
                                    return $.ajax(query)
                                        .then(function (result) {
                                            bingo.events.logCopy(id, data.id);
                                        })
                                        .fail(function (error) {
                                            bingo.events.logCopy(id, data.id, error);
                                        })
                                })

                            }
                        })(result[i]);
                    }
                    return o2;
                })
                .fail(function (error) {
                    alert("137 " + JSON.stringify(error));
                })
        })
        o.always(function () {
            var e = $("<li></li>").text("End");
            $("#log").append(e);
        })

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
    bingo.events.logCopy = function (id,id2,error) {
        var e = $("<li></li>").text(id + "→" + id2 + (error?" "+JSON.stringify(error):""));
        $("#log").append(e);
    }

})(jQuery);