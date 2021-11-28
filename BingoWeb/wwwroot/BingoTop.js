﻿(function ($) {
    var bingo = {
        url: {
            BingoCreateCard: "BingoCreateCard",
            BingoGetAllCard: "BingoGetAllCard",
            BingoGetCard: "BingoGetCard",
            BingoDeleteContainer: "BingoDeleteContainer",
            BingoCreateContainer: "BingoCreateContainer",
            BingoSave: "BingoSave",
        },
        elements: {},
        events: {},
        data: {
            bingos: [],//既出の当たった数値はtrue 添え字は1～75
            bingolist: [],//当たった番号のみ配列
            currentBingo: [],//ビンゴはtrue 添え字は1～75
            previousBingo: [],
            currentReach: [],//リーチはtrue（実際は4以上なのでビンゴ含む） 添え字は1～75
            previousReach: [],
            BingoData: {
                id: "",
                category: "Bingo",
                numberData:[]
            }
        },
        values: {
            BingoVersion: "2021.11.27.17",
            Env: null,
            done_rendercard: false,//ビンゴカードの描画が終わったらtrue
            BingoRunning:false,//当たり数値保存中の連続実行を防止
            BingoRunning2:false,//少なくとも1秒以内の連続当たり処理はしない（連続押しなどで数字が気付かれずに進んでしまうのを防止する）
            current: 1,//最後に当たった数値 1～75
            rouletteSpeed: [
                { speed:30},//初期スピード ms
                { count: 40, speed:60},//{count:残りカウント(降順),speed:スピード}
                { count: 20, speed: 80 },//{count:残りカウント(降順),speed:スピード}
                { count: 10, speed:100},
                { count: 3, speed:200},
            ],
            rouletteCountRandom: 76,//ルーレットカウント長さ乱数分
            rouletteCountExt: 150,//ルーレットカウント長さ初期加算

            maxCardNumber: 50, //カードの数
            maxNumEtc: 500, //無限ループ回避の適当な値
        },
        async: {
            success: new Promise(function (resolve, reject) { resolve(); }),
            fail: new Promise(function (resolve, reject) { reject(); })
        }
    }

    $(document).ready(function () {
        bingo.events.initialize();
    });

    bingo.events.createbingocard = function () {
        $("#loading").text("Running...");
        bingo.data.bingolist = [];
        return bingo.events.BingoSave() //当たり番号保存id=Bingo.0 で空[]を保存(id自体が無いとエラーになるので)
            .then(function () {
                var query = {
                    url: bingo.url.BingoCreateCard+"?env="+bingo.values.Env,
                    type: "get",
                }
                $.ajax(query)
                    .then(function (result) {
                        $("#loading").text("");
                        return bingo.events.maxcardnumberchange();
                    })
                    .fail(function (error) {
                        $("#loading").text("");
                        alert(JSON.stringify(error));
                    })
            })
    }

    bingo.events.initialize = function () {
        bingo.elements.bingocontainer = $(".bingocontainer");
        bingo.elements.bingoroulette = $(".bingoroulette");

        $("#BingoVersion").text(bingo.values.BingoVersion);

        var env = bingo.events.getParam("[Ee][Nn][Vv]");
        if (env && env.length > 0) bingo.values.Env = env;
        $("#Env").val(bingo.values.Env);
        $("#Env").on("change", bingo.events.env);

        bingo.values.maxCardNumber = parseInt($("#maxcardnumber").val());

        bingo.data.bingos = [];
        for (var i = 1; i <= 75; i++) {
            bingo.data.bingos[i] = false;
        }
        bingo.values.current = 1;

        $("#detailCheck").on("click", function () {
            if ($("#detailOptions").is(":visible")) {
                $("#detailOptions").hide();
            } else {
                $("#detailOptions").show();
            }
        });
        $("#createbingocard").on("click", bingo.events.createbingocard);
        $("#bingo").on("click", function (e) { //抽選
            if (bingo.values.count > 0) {
                //既にカウント中の場合は回数延長
                bingo.values.count += 50;
                //複数回クリックしても上限以上は延長しない
                if (bingo.values.count > bingo.values.rouletteCountExt + bingo.values.rouletteCountRandom) bingo.values.count = bingo.values.rouletteCountExt + bingo.values.rouletteCountRandom;
            } else {
                bingo.events.bingoFirst(e, bingo.values.rouletteCountExt);
            }
        });
        $("#bingo2").on("click", function (e) { //抽選巻き
            if (bingo.values.count > 0) {
                //既にカウント中の場合は回数残30程度に設定
                bingo.values.count = Math.floor(Math.random() * 10) + 25;
            } else {
                bingo.events.bingoFirst(e, 0);
            }
        });
        $("#bingo3").on("click", function (e) { //抽選超巻き
            if (bingo.values.count > 0) {
                //既にカウント中の場合はほぼ即時停止
                bingo.values.count = Math.floor(Math.random() * 3) + 1;
            } else {
                bingo.events.bingoFirst(e, -1);
            }
        });
        $("#deletecontainer").on("click", function () {
            $("#loading").text("Running...");
            $.ajax({ url: bingo.url.BingoDeleteContainer, type: "get", })
                .fail(function (error) {
                    alert(JSON.stringify(error));
                })
                .always(function () {
                    $("#loading").text("");
                })
        });
        $("#createcontainer").on("click", function () {
            $("#loading").text("Running...");
            $.ajax({ url: bingo.url.BingoCreateContainer, type: "get", })
                .fail(function (error) {
                    alert(JSON.stringify(error));
                })
                .always(function () {
                    $("#loading").text("");
                })
        });

        $("#maxcardnumber").on("change", bingo.events.maxcardnumberchange);
        $("#reset").on("click", bingo.events.reset);

        bingo.events.createRoulette();
        return bingo.events.maxcardnumberchange();
    }

    bingo.events.getParam = function(name){
        var regex= new RegExp("[?&]" + name + "=([^&#]*)(&|#|$)");
        var matches=regex.exec(location.search);
        if (!matches)return null;
        if (!matches[1] || matches[1].length==0)return null;
        return decodeURIComponent(matches[1].replace(/\+/g, " "));
    }

    bingo.events.env = function () {
        bingo.values.Env = $("#Env").val();
        if (!bingo.values.Env || bingo.values.Env.length == 0) bingo.values.Env = null;
        return bingo.events.maxcardnumberchange();
    }

    //あたり番号を初期化する
    bingo.events.reset = function () {
        $("#loading").text("Running...");
        var data = Object.assign({}, bingo.data.BingoData);
        data.id = "Bingo" + ((bingo.values.Env) ? ("."+bingo.values.Env):"")+".0";
        data.category = "Bingo" + ((bingo.values.Env) ? ("." + bingo.values.Env) : "");
        data.numberData = [];
        var query = {
            url: bingo.url.BingoSave,
            type: "post",
            data: JSON.stringify(data),
            contentType:"application/json",
            dataType: "json",
        }
        return $.ajax(query)
            .then(function (result) {

                //リロードする
                setTimeout("location.reload()", 500);
                return;

            //リセット時リロードにしたので、初期化コード（バグあり）をコメントアウト
            //    //抽選結果全書式クリヤ

            //    $(".bingoNum").removeClass("bingoNumBingo");
            //    $(".bingoNum.bingo-13").addClass("bingoNumBingo");

            //    var box = $(".bingoONE");
            //    box.removeClass("boxBingo").removeClass("boxBingoDone").removeClass("boxReach");

            //    var titles = box.find(".bingoTITLE");
            //    titles.removeClass("boxTitleBingo").removeClass("boxTitleReach");

            //    titles.each(function (i, v) {
            //        var e = $(v);
            //        var s = e.text();
            //        var so = s;
            //        s = s.replace("[ビンゴ今回]", "");
            //        s = s.replace("[ビンゴ]", "");
            //        s = s.replace("[リーチ今回]", "");
            //        s = s.replace("[リーチ]", "");
            //        if (so != s) e.text(s);
            //    })

            //    //抽選結果テキストボックスを空にする
            //    $("#bingolist").val("");

            //    //当たった数字リストを空にする
            //    bingo.data.bingolist = [];
            //    for (var i = 1; i <= bingo.data.bingos.length; i++) { if(i!=13)bingo.data.bingos[i] = false; }
            //    bingo.data.currentBingo = [];
            //    bingo.data.previousBingo = [];
            //    bingo.data.currentReach = [];
            //    bingo.data.previousReach = [];

            //    bingo.events.maxcardnumberchange();
            })
            .fail(function (error) {
                alert(JSON.stringify(error));
            })
    }

    bingo.events.createRoulette = function () {
        //ルーレット数値を削除
        for (var i = 1; i <= 75; i++) {
            var e = bingo.elements.bingoroulette.find(".roulette-" + i).closest(".bingoRouletteCell");
            e.remove();
        }
        for (var i = 1; i <= 75 ; i++) {
                var box = bingo.elements.bingoroulette.find(".roulette-n").closest(".bingoRouletteCell").clone();
                box.css("display", "");
                var e = box.find(".bingoRouletteNum");
                e.text(i);
                e.removeClass("roulette-n").addClass("roulette-" + i);
            box.appendTo(bingo.elements.bingoroulette)
        }
        bingo.elements.bingoroulette.find(".roulette-n").closest(".bingoRouletteCell").css("display", "none");//テンプレートを非表示にする
    }

    bingo.events.maxcardnumberchange = function () {
        bingo.values.done_rendercard = false;

        $("#loading").text("Loading...");
        //前回値は無しで
        bingo.data.currentBingo = [];
        bingo.data.currentReach = [];
        //当たり番号の読み込み
        var query = {
            url: bingo.url.BingoGetCard + "?id=Bingo" + ((bingo.values.Env) ? ("." + bingo.values.Env) : "") + ".0",
            type: "get",
        }
        return $.ajax(query)
            .then(function (result) {
                bingo.data.bingolist = result.numberData;
                for (var i = 1; i < 75; i++) {
                    if (bingo.data.bingolist.indexOf(i) > 0) {
                        bingo.data.bingos[i] = true;
                    } else {
                        bingo.data.bingos[i] = false;
                    }
                }

                $("#bingolist").val("");
                bingo.elements.bingoroulette.find(".bingoRouletteNum").removeClass("doneRoulette");
                for (var i = 0; i < bingo.data.bingolist.length; i++) {
                    //抽選テキストボックスに追加
                    var s = $("#bingolist").val();
                    if (s.length > 0) s = "," + s;
                    $("#bingolist").val(bingo.data.bingolist[i] + s);

                    //ルーレットをマーク
                    bingo.elements.bingoroulette.find(".roulette-" + bingo.data.bingolist[i]).addClass("doneRoulette");

                }
                return bingo.events.renderBingoCard()
                    .then(function () {
                        bingo.values.done_rendercard = true;
                    })
            })
            .fail(function (error) {
                alert(JSON.stringify(error));
            })
            .always(function () {
                $("#loading").text("");
            })

    }

    //抽選ボタンで実行　rouletteCountExtはルーレットの長さ（カウント）-1の場合超巻き
    bingo.events.bingoFirst = function (e, rouletteCountExt) {
        if (!bingo.values.done_rendercard) return;//カード描画中は中止
        if (bingo.data.bingolist.length >= 75) return;//終了済み
        if (bingo.values.BingoRunning) return;//当たり保存処理中二度押し防止
        if (bingo.values.BingoRunning2) return;////少なくとも1秒以内の連続当たり処理はしない（連続押しなどで数字が気付かれずに進んでしまうのを防止する）

        $("#BigNum .BigNum").remove();//前のあたり数値画面中央表示を削除

        if (rouletteCountExt < 0) {
            //超巻きの場合。回数でなく、場所をランダムで一発設定
            bingo.values.count = 1;//回数内で未当たりの数が出なかった場合の初期値
            for (var i = 1; i < bingo.values.maxNumEtc; i++) {
                var random = Math.floor(Math.random() * 76);
                bingo.values.roulette = bingo.values.current + random;
                if (bingo.values.roulette > 75) bingo.values.roulette -= 75;
                if (bingo.data.bingolist.indexOf(bingo.values.roulette) < 0) {
                    bingo.values.count = 0;//現在のbingo.values.rouletteで即当たり
                    break;
                }
            }
        } else {
            //ルーレット回数と開始位置を設定
            var random = Math.floor(Math.random() * bingo.values.rouletteCountRandom);
            bingo.values.count = random + rouletteCountExt + 1;
            bingo.values.roulette = bingo.values.current;
        }
        bingo.elements.bingoroulette.find(".bingoRouletteNum").removeClass("currentRoulette");

        bingo.events.bingo(e);
    }

    bingo.events.bingo = function (e) {
        if (bingo.values.count > 0 || bingo.data.bingos[bingo.values.roulette]) {
            var e = bingo.elements.bingoroulette.find(".roulette-" + bingo.values.roulette);
            e.removeClass("currentRoulette");

            //保存中の場合はカウントをキャンセルする
            if (bingo.values.BingoRunning) { bingo.values.count = 0;return };

            bingo.values.roulette++;
            if (bingo.values.roulette > 75) bingo.values.roulette = 1;

            var e = bingo.elements.bingoroulette.find(".roulette-" + bingo.values.roulette);
            e.addClass("currentRoulette");

            if (bingo.values.count < 5 && bingo.data.bingos[bingo.values.roulette]) {
                //5以下であたり済みの場合はカウントを減らさない（塊のとなりで止まりやすいのを防止する）
            } else {
                bingo.values.count--;
            }

            var wait = 0;
            for (var i = 0; i < bingo.values.rouletteSpeed.length; i++) {
                if (!bingo.values.rouletteSpeed[i].count) wait = bingo.values.rouletteSpeed[i].speed;
                if (bingo.values.count < bingo.values.rouletteSpeed[i].count) wait = bingo.values.rouletteSpeed[i].speed;
            }

            setTimeout(bingo.events.bingo, wait);

            return;
        } else {
            //保存中の場合はかぶるのでキャンセルする
            if (bingo.values.BingoRunning) return;
        }

        //少なくとも1秒以内の連続当たり処理はしない（連続押しなどで数字が気付かれずに進んでしまうのを防止する）
        bingo.values.BingoRunning2 = true;
        setTimeout(function () {
            bingo.values.BingoRunning2 = false;
        }, 1000)

        bingo.values.BingoRunning = true;//当たり処理中二度押し防止

        bingo.elements.bingoroulette.find(".roulette-" + bingo.values.roulette).addClass("currentRoulette"); //ルーレット表示を再度付けておく（超巻きで回さないで来た時の処理）

        bingo.values.current =bingo.values.roulette;
        bingo.elements.bingoroulette.find(".roulette-" + bingo.values.current).addClass("doneRoulette");
        bingo.data.bingos[bingo.values.current] = true;
        bingo.data.bingolist.push(bingo.values.current);

        //当たった丸数字をマークする
        bingo.events.mark(bingo.values.current);
        bingo.events.checkBingo();

        //抽選テキストボックスに追加
        var s = $("#bingolist").val();
        if (s.length > 0) s = ","+s;
        $("#bingolist").val(bingo.values.current + s);

        //当たった数値を画面中央に表示
        var w = $(window).width() / 2;
        var h = $(window).height() / 2;
        var e = $('<div class="BigNum">' + bingo.values.current + '</div>');
        $("#BigNum").append(e);
        e.css("left", (w - (e.outerWidth() / 2)) + "px").css("top", (h - (e.outerHeight() / 2)) + "px");
        bingo.values.delBigNumCount = 30;
        bingo.events.delBigNum();

        //当たった数字を保存
        bingo.events.BingoSave().always(function () {
            bingo.values.BingoRunning = false;//二度押し防止解除
        });
    }

    bingo.events.BingoSave = function () {
        $("#loading").text("Saving...");
        var data = Object.assign({}, bingo.data.BingoData);
        data.id = "Bingo" + ((bingo.values.Env) ? ("."+bingo.values.Env):"")+".0";
        data.category = "Bingo";
        data.numberData = bingo.data.bingolist;
        var query = {
            url: bingo.url.BingoSave,
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
        }
        return $.ajax(query)
            .then(function (result) {
            })
            .fail(function (error) {
                alert(JSON.stringify(error));
            })
            .always(function () {
                $("#loading").text("");
            })
    }

    bingo.events.delBigNum = function() {
        bingo.values.delBigNumCount--;
        if (bingo.values.delBigNumCount >= 0) {
            if (bingo.values.delBigNumCount == 4) $("#BigNum .BigNum").css("opacity", "0.8");
            if (bingo.values.delBigNumCount == 3) $("#BigNum .BigNum").css("opacity", "0.6");
            if (bingo.values.delBigNumCount == 2) $("#BigNum .BigNum").css("opacity", "0.4");
            if (bingo.values.delBigNumCount == 1) $("#BigNum .BigNum").css("opacity", "0.2");
            if (bingo.values.delBigNumCount == 0) $("#BigNum .BigNum").remove();
            setTimeout(bingo.events.delBigNum, 100);
        }
    }


    //指定した数にマッチした丸数字をマーク
    bingo.events.mark = function (num) {
        for (var i = 0; i < bingo.values.maxCardNumber; i++) {
            var idx = i + 1;
            var bingodata = bingo.data.result[idx - 1];
            for (var j = 0; j < 25; j++) {
                if (bingodata.numberData[j] == num) {
                    var box = bingo.elements.bingocontainer.find(".bingobox-" + idx);
                    var cell = box.find(".bingo-" + (j + 1));
                    cell.addClass("bingoNumBingo");//border:4px solid red
                }
            }

        }
    }

    //リーチとビンゴをチェック
    bingo.events.checkBingo = function () {
        //前回の状態を保管
        bingo.data.previousBingo = Object.assign({}, bingo.data.currentBingo);
        bingo.data.previousReach = Object.assign({}, bingo.data.currentReach);

        var result = bingo.data.result;
        for (var i = 0; i < bingo.values.maxCardNumber; i++) {
            var idx = i + 1;
            var bingodata = bingo.data.result[idx - 1];

            var m = [];
            for (var j = 1; j <= 25; j++) {
                if (bingo.data.bingos[bingodata.numberData[j-1]]) {
                    m[j] = true;
                } else {
                    m[j] = false;
                }
            }
            m[13] = true;
            bingo.events.count5(idx, m, [1, 2, 3, 4, 5]);
            bingo.events.count5(idx, m, [6, 7, 8, 9, 10]);
            bingo.events.count5(idx, m, [11, 12, 13, 14,15]);
            bingo.events.count5(idx, m, [16, 17, 18, 19, 20]);
            bingo.events.count5(idx, m, [21, 22, 23, 24, 25]);
            bingo.events.count5(idx, m, [1, 6, 11, 16, 21]);
            bingo.events.count5(idx, m, [2, 7, 12, 17, 22]);
            bingo.events.count5(idx, m, [3, 8, 13, 18, 23]);
            bingo.events.count5(idx, m, [4, 9, 14, 19, 24]);
            bingo.events.count5(idx, m, [5, 10, 15, 20, 25]);
            bingo.events.count5(idx, m, [1, 7, 13, 19, 25]);
            bingo.events.count5(idx, m, [5, 9, 13, 17, 21]);

            var box = $(".bingobox-"+idx);
            var e = box.find(".bingoTITLE");
            var s = e.text();
            s=s.replace("[ビンゴ今回]","");
            s=s.replace("[ビンゴ]", "");
            s = s.replace("[リーチ今回]", "");
            s = s.replace("[リーチ]", "");
            box.removeClass("boxBingo");
            box.removeClass("boxReach");
            e.removeClass("boxTitleBingo");
            e.removeClass("boxTitleReach");
            if (bingo.data.currentBingo[idx] && !bingo.data.previousBingo[idx]) {
                s = s + "[ビンゴ今回]";
                e.addClass("boxTitleBingo");
                box.addClass("boxBingo");
            } else if (bingo.data.currentBingo[idx]) {
                s = s + "[ビンゴ]";
                box.addClass("boxBingoDone");
            } else if (bingo.data.currentReach[idx] && !bingo.data.previousReach[idx]) {
                s = s + "[リーチ今回]";
                e.addClass("boxTitleReach");
                box.addClass("boxReach");
            } else if (bingo.data.currentReach[idx]) {
                s = s + "[リーチ]";
            }
            e.text(s);
            
        }
    }

    bingo.events.count5 = function (idx,m,al) {
        var cnt = 0;
        for (var i = 0; i < al.length; i++) {
            if (m[al[i]]) cnt++;
        }
        if (cnt >= 5) bingo.data.currentBingo[idx] = true;
        if (cnt >= 4) bingo.data.currentReach[idx] = true;
    }

    //画面にカードを書く。bingo.data.bingosの内容にしたがって、カードへのあたり表示とリーチビンゴチェックを行う
    bingo.events.renderBingoCard = function () {

        //画面のカードを削除
        for (var i = 1; i < bingo.values.maxNumEtc; i++) {
            var e = bingo.elements.bingocontainer.find(".bingobox-" + i);
            if (e.length == 0) break;
            e.remove();
        }
        for (var idx =1 ; idx <= bingo.values.maxCardNumber; idx++) {
            var box = bingo.elements.bingocontainer.find(".bingobox-n").clone();
            box.css("display", "");
            box.find(".bingoTITLE").text(idx + "番");
            box.removeClass("bingobox-n").addClass("bingobox-"+idx);
            box.appendTo(bingo.elements.bingocontainer)
        }
        $(".bingobox-n").css("display", "none");//テンプレートを非表示にする

        return bingo.events.dispbingoNumber()
            .then(function () {
                for (var i = 1; i <= 75; i++) {
                    if (bingo.data.bingos[i]) {
                        bingo.events.mark(i);
                    }
                }
                bingo.events.checkBingo();
            })

    }
    bingo.events.dispbingoNumber = function () {
        var query = {
            url: bingo.url.BingoGetAllCard + "?env=" + bingo.values.Env,
            type: "get",
        }
        return $.ajax(query)
            .then(function (result) {
                bingo.data.result = result;
                for (var idx = 1; idx <= bingo.values.maxCardNumber; idx++) {
                    var bingodata = result[idx-1];
                    var box = bingo.elements.bingocontainer.find(".bingobox-" + idx);
                    for (var i = 0; i < 25; i++) {
                        var cell = box.find(".bingo-" + (i + 1));
                        var num = bingodata.numberData[i];
                        cell.text(num);
                    }
                }
            })
            .fail(function (error) {
                alert(JSON.stringify(error));
            })

    }

})(jQuery);