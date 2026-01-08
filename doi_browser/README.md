# DOI Browser

DOI情報をブラウザで表示するアプリケーションです。

## セットアップ

1. 依存関係のインストール:
```bash
npm install
```

2. 開発サーバーの起動:
```bash
npm run dev
```

3. 本番用ビルド:
```bash
npm run build
```

## 使用方法

開発サーバーを起動すると、`http://localhost:9000`でアプリケーションにアクセスできます。

## ファイル構造

- `index.ts` - エントリーポイント
- `browser.ts` - DOIInfoCollectionクラスなど
- `browser_info.ts` - BrowserInfo、DOIInfoSearchInputクラス
- `render.ts` - レンダリング処理
- `gzip_loader.ts` - GZIPファイルの読み込み処理
- `data_browser.html` - HTMLテンプレート
- `webpack.config.js` - Webpack設定

