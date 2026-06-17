# Timebomb Online

正体隠匿ボードゲーム TIME BOMB のブラウザオンライン版。

## コマンド
- 開発: `npm run dev`（サーバー:3000 / クライアント:5173）
- テスト: `npm test`
- 型検証: `npm run typecheck`
- ビルド: `npm run build`

## 設計の絶対ルール
- サーバー権威。他プレイヤーのカードのオモテ・役職は通常時の `PlayerView` に含めない。
- `revealedRoles` は `game_end` の全役職公開時のみ使う。
- サーバー(`src/server/**`, `tests/server/**`)の import は NodeNext なので `.js` 拡張子必須。クライアントは拡張子なし。
- 純粋エンジン(`src/shared/engine.ts`)はネットワーク非依存。ロジックはここに置き、vitest で検証する。

## スペック
`docs/superpowers/specs/2026-06-16-timebomb-online-design.md`
