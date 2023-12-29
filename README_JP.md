# mocopi_protocol_supplement

SonyのモーションキャプチャmocopiのUDP通信フォーマットの[非公式ドキュメント(seagetch/mcp-receiver)](https://github.com/seagetch/mcp-receiver/blob/main/doc/Protocol.md#skdf-packet-structure)の補足資料とサンプルコード

## 動作確認環境
- mocopi-receiver-plugin-for-unity_1.0.5のサンプルページ

## 補足

### データフォーマットについて
- [`skdf`パケット](https://github.com/seagetch/mcp-receiver/blob/main/doc/Protocol.md#skdf-packet-structure)については，記載の通り．
- [`fram`パケット](https://github.com/seagetch/mcp-receiver/blob/main/doc/Protocol.md#fram-packet-structure)については，最新バージョン(2023.12.29)では，`time`フィールドのあとに`uttm`という8byte（length, field nameも含めると16byte）のフィールドが必要である
  - UnixTimeをリトルエンディアンの浮動小数点として保存している模様.
  - [各SDKのバージョン情報](https://www.sony.net/Products/mocopi-dev/jp/downloads/DownloadInfo.html)からこのフィールドが必要か確認できる．
- `tran`フィールドの座標系はY-Upの右手系である．
  - Unityへの変換は以下のようになる．
    ```
    Position: (x,y,z) -> (-x,y,z)
    Rotation: (x,y,z,w) -> (-x,y,z,-w)
    ```

### mocopi-receiver-plugin-for-unityの仕様について
- mocopi-receiver-plugin-for-unityでは，パケットサイズから行う処理を決定しており，異なるサイズで送ると処理がスキップされる．

## サンプル

### [パケットパーサー](./parser)
`skdf`, `fram`形式のバイナリファイルをパースするサンプルコード．

#### 使い方
```cmd
py mocopi_parser.py ./sample/sample_skdf.bin type=skdf :: Parse skdf packet format data
py mocopi_parser.py ./sample/sample_fram.bin type=fram :: Parse fram packet format data
```
