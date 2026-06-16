# Timebomb Online — 設計ドキュメント

- 作成日: 2026-06-16
- 対象: 正体隠匿ボードゲーム「TIME BOMB」（タカヒロ・アミオカ設計 / アークライト）のブラウザオンライン化
- 目的: 身内（4〜6人）がインターネット越しに Discord 等の外部通話を併用して遊べること

---

## 1. 目標と成功基準

### 目標
- 友人同士がルームコードで合流し、1プレイ（約15分）を完走できる
- 隠し情報（手札・役職）が**絶対にクライアントに漏れない**（サーバー権威・チート不可）
- 3陣営（時空警察 / ボマー / スパイ）の勝敗条件を正しく処理する

### 成功基準（v1）
- 4〜6人でルーム作成〜ゲーム終了〜リマッチまで一通り動く
- 他プレイヤーのカードのオモテが、いかなる `PlayerView` にも含まれないことが自動テストで保証される
- Render 上でデプロイされ、別ネットワークからも参加できる

### 対象外（v1）
- ゲーム内ボイス/テキストチャット（Discord 等の外部通話で代替）
- 観戦モード・対戦ログ再生・アカウント永続化・派手なアニメーション

---

## 2. ゲームルール（実装対象）

### 構成要素
- **カード型（手札）**: `defuse`（解除） / `bomb`（ボム） / `silence`（しーん＝何も起きない）
- **役職**: `police`（時空警察） / `bomber`（ボマー） / `spy`（スパイ：オプション第三陣営）

### 手札配布（人数 N、毎ラウンド各員5枚）
| N | 解除(defuse) | ボム(bomb) | しーん(silence) | 計 |
|---|---|---|---|---|
| 4 | 4 | 1 | 15 | 20 |
| 5 | 5 | 1 | 19 | 25 |
| 6 | 6 | 1 | 23 | 30 |

- 解除チップは **N 個**（= defuse 枚数）。defuse が公開されるたび1個反転。**ラウンドを越えて持続**
- 総ラウンド数は **4**

### 役職構成（提案デフォルト＝チューニング対象。常に police が最多）
| N | spyなし | spyあり |
|---|---|---|
| 4 | 1B / 3P | 1B / 1S / 2P |
| 5 | 1B / 4P | 1B / 1S / 3P |
| 6 | 2B / 4P | 2B / 1S / 3P |

> B=bomber, P=police, S=spy。実数値は `shared/config.ts` に定数テーブルで持ち、プレイフィードバックで調整可能にする。

### プレイの流れ
1. 全員に手札5枚を配布
2. 手番プレイヤーが**他人**の手札からカード1枚を指定して公開（「導線カット」）
   - `defuse` → 解除チップ+1（全 N 個反転で警察の勝ち）
   - `bomb` → **即座にボマーの勝ち（ゲーム終了）**
   - `silence` → 何も起きず継続
3. **ターン継承**: 公開された（切られた）プレイヤーが次の手番
4. そのラウンドで **N 回**（=人数）カットされたら1ラウンド終了 → 全カード回収・シャッフル・再配布（チップは維持）
5. 4ラウンド終了まで繰り返す

### 勝敗条件
- **警察**: 解除チップ N 個すべて反転
- **ボマー**: ボムカードが公開された瞬間。**spy なし時**は「4R 終了で警察未達成」でもボマー勝ち
- **スパイ（spy あり時）**: 4R 終了時、警察もボマーも勝利条件を満たしていない（＝解除全完了でもボム公開でもない）ならスパイ勝ち
  - ※ spy ありの場合、4R 未決着はボマーではなくスパイの勝ちになる

---

## 3. アーキテクチャ

### サーバー権威型（Server-Authoritative）
- **クライアントは「描画係」**: サーバーから受け取った `PlayerView` を描画するだけ。検証・状態保持はすべてサーバー
- **サーバーだけが「真実」を保持**: 全役職・全カードの実体。手札のオモテはネットワークに乗らない
- アクションごとにサーバーが**プレイヤーごとに異なる `PlayerView` を生成し全員へ配信**

### 純粋エンジンの分離
- `shared/engine.ts` は配布・カット・勝敗判定の**純粋関数**（ネットワーク非依存）
- `shared/config.ts` に人数別の配布/役職テーブル
- これらは vitest で単体テスト可能。サーバーの `room.ts` がエンジンを呼び出す

### 採用構成（案A: モノレポ）
- 1パッケージ構成（`private-dice-racing-online` と同じ思想）
- Vite/React/TS クライアント ＋ Express/`ws` サーバーを同一リポジトリ
- クライアント/サーバー/テストで tsconfig を分離（型を共有しつつ責務を分ける）

---

## 4. リポジトリ構成

```
Timebomb/
  package.json
  vite.config.ts
  tsconfig.json  tsconfig.client.json  tsconfig.server.json  tsconfig.test.json
  render.yaml
  lefthook.yml
  AGENTS.md
  src/
    shared/                         # ネットワーク非依存の純粋ドメイン
      types.ts                      # CardType, Role, GameState, PlayerView, Messages
      config.ts                     # 人数別 配布/役職/チップ テーブル
      engine.ts                     # deal, cut, resolveRound, checkWin（純粋関数）
      engine.test.ts
    server/
      index.ts                      # Express + ws 起点・ルームレジストリ
      room.ts                       # Room: GameState保持・アクション適用・View配信
      room.test.ts                  # viewフィルタ検証を含む
    client/
      main.tsx
      App.tsx
      components/                   # Lobby, RoleReveal, GameTable, PlayerPanel, Card, Hud, GameOver
      hooks/useSocket.ts
      styles/
  docs/superpowers/specs/
```

---

## 5. ドメインモデルと状態

### 型（`shared/types.ts` 想定）
```ts
type CardType = 'defuse' | 'bomb' | 'silence';
type Role = 'police' | 'bomber' | 'spy';
type Phase = 'lobby' | 'role_reveal' | 'round_deal' | 'round_play' | 'round_end' | 'game_end';

interface Player {
  id: string;
  name: string;
  role: Role | null;          // 通知前/ゲーム外は null
  hand: CardType[];           // サーバーのみが実体を知る
  connected: boolean;
  ready: boolean;             // role_reveal の Ready
  isHost: boolean;
}

interface CutEvent {
  cutterId: string;
  targetId: string;
  cardIndex: number;
  revealedType: CardType;     // 公開情報
}

interface GameState {
  phase: Phase;
  round: number;              // 1..4
  spyEnabled: boolean;
  players: Player[];
  currentCutterId: string | null;
  defuseChipsFlipped: number;
  cutsThisRound: number;
  lastCut: CutEvent | null;
  winners: Role[] | null;     // game_end で設定
}
```

### サーバーが各プレイヤーへ送る `PlayerView`（秘匿フィルタ済み）
```ts
interface PlayerView {
  phase: Phase;
  round: number;
  myPlayerId: string;
  myRole: Role | null;
  myHand: CardType[];                 // 自分の手札オモテ（自分のみ）
  currentCutterId: string | null;
  defuseChipsFlipped: number;
  defuseChipsTotal: number;           // = N
  cutsThisRound: number;
  cutsPerRound: number;               // = N
  players: {
    id: string;
    name: string;
    handSize: number;                 // 他人は「枚数」のみ。オモテは送らない
    connected: boolean;
    ready: boolean;
    isHost: boolean;
  }[];
  lastCut: CutEvent | null;           // 公開イベント（カード型含む）
  winners: Role[] | null;
}
```

> **不変条件**: `PlayerView.myHand` には自分の手札のみ。他人の手札は `handSize` のみ送信。これを `room.test.ts` で表明的アサートする。

---

## 6. ゲームフロー / 状態遷移

```
LOBBY → ROLE_REVEAL → ROUND_DEAL → ROUND_PLAY → ROUND_END ─(次R)→ ROUND_DEAL…
        (即時勝敗: 全解除/ボム) ──────────────────────────────────→ GAME_END
        (4R終了・未決着) ─────────────────────────────────────────→ GAME_END
GAME_END → (restart) → LOBBY
```

### フェーズごとの振る舞い
- **LOBBY**: ルームコードで参加（4〜6人）。ホストが `spyEnabled` トグルと `start` 操作。4人以上で開始可。参加時にルーム内で名前一意。
- **ROLE_REVEAL**: 各自が個人オーバーレイで役職を確認。全員 `ready` で次へ。
- **ROUND_DEAL**: サーバーが手札をシャッフル配布（毎ラウンド5枚）。
  - **先手**: R1 はランダム。R2〜4 は**ターン継承がそのまま流れる**（前ラウンド最後に切られた人が次の手番＝そのまま次Rの先手）。特別ルール不要。
- **ROUND_PLAY**:
  1. `currentCutterId` のみが `cut{targetId, cardIndex}` 送信可。`targetId !== myPlayerId` 必須
  2. サーバー検証 → `target.hand[cardIndex]` を除去して公開
     - `defuse` → `defuseChipsFlipped++`。`defuseChipsFlipped === N` で **GAME_END（警察勝ち）**
     - `bomb` → **GAME_END（ボマー勝ち）**
     - `silence` → 継続
  3. `currentCutterId = targetId`（ターン継承）
  4. `cutsThisRound++`。`cutsThisRound === N` で ROUND_END
- **ROUND_END**:
  - `round === 4` かつ未決着 → GAME_END（spy ありならスパイ勝ち / なしならボマー勝ち）
  - それ以外 → `round++` して ROUND_DEAL
- **GAME_END**: 勝者表示・全役職公開。`restart` で LOBBY へ。

### エッジ
- 手番者の手札が0枚でも他人を切れる（自分は切れない）
- カット対象が1人もいない（全員0枚等）→ ラウンド強制終了
- 不正アクション（手番外・自分切・範囲外・フェーズ違反）→ サーバー拒否・状態不変・`error` 応答

---

## 7. プロトコル（WebSocket メッセージ）

### Client → Server
| メッセージ | ペイロード | 備考 |
|---|---|---|
| `join` | `{ roomCode, name }` | 参加・リコネクト |
| `start` | `{}` | ホストのみ |
| `setSpy` | `{ enabled }` | ホストのみ・LOBBY |
| `ready` | `{}` | ROLE_REVEAL |
| `cut` | `{ targetId, cardIndex }` | ROUND_PLAY・手番者のみ |
| `restart` | `{}` | GAME_END・ホストのみ |

### Server → Client
| メッセージ | ペイロード | 備考 |
|---|---|---|
| `joined` | `{ playerId, roomCode, isHost }` | |
| `state` | `PlayerView` | 状態変更のたび全員へ（個人化済み） |
| `error` | `{ message }` | 不正アクション等 |

> サーバーはアクションごとに**接続ごとに個別の `PlayerView` を生成**して送信する。

---

## 8. リコネクト・エラー処理

### リコネクト（簡易・MVP）
- 初回 `join` 成功時、サーバーは `playerId` を発行し `joined` で返却。クライアントは `playerId` を localStorage に保存
- 再接続時: クライアントは `join{ roomCode, name, playerId? }` を送信。サーバーは `roomCode` 内の `playerId`（なければ `name`）で既存席を特定
- 席が特定できれば `connected = true` に戻し、最新 `PlayerView` を再送して**席と手札を復元**（手札実体はサーバーにあるため安全）
- 手札の実体はサーバーにあるため、復元時に秘匿情報は安全
- **ホスト切断時**: 残存プレイヤーの最古参加者へホスト委譲

### その他エッジ
- ルーム満員（6人）→ `join` 拒否
- 開始時 <4 人 → `start` 拒否
- spy 構成不能（人数不足等）→ ロビーでバリデーション
- 重複名 → ルーム内で一意性を要求

---

## 9. UI（画面構成）

採用レイアウト: **グリッド・パネル（案C）** — 全プレイヤー（自分含む）を等サイズパネルで配置。自分のパネルのみ緑枠＋カードオモテ表示、他は裏向き。手番者は黄枠ハイライト。中央HUDに解除チップ / ラウンド / 手番 / 直前の結果。

### 主な画面
1. **ロビー**: ルームコード表示・参加者リスト・スパイトグル・開始ボタン
2. **役職公開（個人オーバーレイ）**: 自分の役職と「確認」ボタン
3. **ゲームテーブル（グリッド）**: 全プレイヤーパネル ＋ 中央HUD
   - 自分のパネル: 手札オモテ（解除=緑/ボム=赤/しーん=灰）
   - 他人パネル: カード裏向き ＋ 枚数
   - 手番時: 他人パネルのカードをクリックしてカット指定
4. **ゲームオーバー**: 勝者陣営・全役職公開・リマッチボタン

> レスポンシブはデスクトップ最優先（モバイルは副次的）。

---

## 10. 技術スタック

| レイヤ | 技术 |
|---|---|
| クライアント | React 19 + Vite 6 + TypeScript 5.6 |
| サーバー | Node + Express 5 + `ws`（素のWebSocket） |
| サーバー dev | `tsx watch`、`concurrently` で client/server 同時起動 |
| 単体テスト | vitest（+ jsdom / React Testing Library） |
| Lint/品質 | ESLint（兄弟プロジェクト準拠）・lefthook |
| デプロイ | Render（`render.yaml`） |

---

## 11. テスト戦略

- **純粋エンジン**（`shared/engine.test.ts`）: 配布数・カット解決・ターン継承・チップ・3陣営すべての勝敗条件・ラウンド終了・人数別のカード枚数。← 正確性の大部分
- **サーバー room**（`server/room.test.ts`）: 参加/開始/cut/restart フロー ＋ **viewフィルタ検証**（全フェーズ・全プレイヤーの `PlayerView` を生成し、他人のカードオモテが1つも含まれないことを表明的アサート）・不正アクション拒否・エッジ
- **クライアント**: 主要コンポーネントを React Testing Library で軽く（RoleReveal・GameTable の描画など）
- **E2E（stretch）**: Playwright で2タブ開き1ラウンドを回す煙テスト

---

## 12. デプロイ・運用

- **Render Web Service**（`render.yaml`）。単一 Node サービスでビルド済みクライアント（静的）配信＋WebSocket
- ビルド: `vite build && tsc -p tsconfig.server.json`
- 起動: `node dist-server/server/index.js`
- 環境変数: `PORT`（Render 任せ）
- ルームは**インメモリ**（身内向け・DB不要）。プロセス再起動で消える前提

---

## 13. 未解決・チューニング項目

- 役職構成の実数値（6人時にボマー2枚か等）→ プレイフィードバックで調整。`config.ts` 定数化済み
- E2E を v1 に含めるか stretch にするか → 実装計画で決定
- 先手ローテーションの体感（ターン継承のみで十分か）→ プレイインで確認
