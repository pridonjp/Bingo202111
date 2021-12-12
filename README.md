# Bingo202111
Bingo for online party
開発者 pridonjp

オンライン飲み会用に作成したものです。
元のリポジトリは個人管理のため、変更分をコピーして公開しています。
本プログラムは、MITライセンスを指定しています。

## 環境
ASP.NET core 3.1のVisual Studio 2022ソリューションです
設定(appsettings.json)ではローカルのAzure Cosmos DB Emulatorへの接続になっています。事前にデータベース名Bingoが必要です

## 使い方
BingoTop.html画面で以下の事前準備が必要です
1. 環境コード入力（任意。空もOK）
2. Optionsチェックで管理ボタンを開き、「コンテナ作成」ボタンでBingoデータベースにコンテナItemsを作成する
3. 「カード再作成」ボタンで人数分のカードを生成する

幹事はBingoTop.html画面を画面共有し、以下の手順でゲームを進めます
1. 参加者に1からの連番の番号を振って、各自の番号を連絡する
2. 「抽選」ボタンで抽選を行う。
3. 参加者はBingoCard.html画面に自分の番号を入力し、「更新」ボタンでカードと現在の開き状況を確認できる

## 補足説明
2021.11.28 BingoTop.html画面はBingoTop.html#環境コード が指定可能。BingoCard.htmlはBingoCard.html#環境コード,カード番号 が指定可能なので、参加者には環境コード付きでURLを案内し、,カード番号を付けて開いてもらう
2021.12.12 Azure Cosmos DB接続のCosmosClientをシングルインスタンス化しないと複数人の参加者で「One or more errors occurred. (Response status code does not indicate success: ServiceUnavailable (503); Substatus: 0; ActivityId: *****; Reason: (The request failed because the client was unable to establish connections to 4 endpoints across 1 regions.」というエラーでAzure Cosmos DBに接続出来なくなるので、プログラム対応
           メモリキャッシュに存在するデータはCosmos DBに読みに行かないので、Webアプリはシングルインスタンスでしか実行できない(厳密にはIISのワーカープロセスが別れただけで不具合となるが、とりあえず忘年会が迫っているので短時間実用になればOKの稼働優先で)


以上

