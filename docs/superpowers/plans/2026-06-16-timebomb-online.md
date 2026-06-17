# Timebomb Online 実装計画

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 正体隠匿ボードゲーム「TIME BOMB」をブラウザオンライン（4〜6人、ルームコード合流、Discord併用）で遊べる Web アプリを完成させる。

**Architecture:** サーバー権威型。サーバーだけが全役職・全カードの実体を保持し、各プレイヤーに見せてよい情報だけをフィルタした `PlayerView` を配信（チート不可）。純粋なゲームエンジン（`shared/`）はネットワーク非依存で単体テスト可能。クライアントは React/Vite、サーバーは Express + `ws`、デプロイは Render。`private-dice-racing-online` の構成を踏襲。

**Tech Stack:** React 19 + Vite 6 + TypeScript 5.6 / Express 5 + ws 8 / vitest + jsdom + React Testing Library / lefthook / Render

**前提:** 作業ディレクトリは `00_Source_Codes/Timebomb`（リポジトリルートは `00_Source_Codes` の上位のモノレポ）。既にブランチ `timebomb/design-spec` 上にスペックと `.gitignore` が存在。本計画のコミットは同じブランチで進める。

**モジュール解決の重要ルール（dice-racing 準拠）:**
- `src/server/**` と `tests/server/**` は `NodeNext`。**共有/サーバーファイルの import には必ず `.js` 拡張子を付ける**（例: `import { applyCut } from "../shared/engine.js"`）。
- `src/client/**` と `tests/client/**` は `Bundler` 解決。拡張子なしで OK。

---

## ファイル構成（責務）

| ファイル | 責務 |
|---|---|
| `package.json` | 依存関係・スクリプト |
| `tsconfig.client.json` | クライアント(React/Bundler)型検証 |
| `tsconfig.server.json` | サーバー(NodeNext)型検証・ビルド |
| `tsconfig.test.json` | サーバーテスト型検証 |
| `tsconfig.client.test.json` | クライアントテスト型検証 |
| `vite.config.ts` | クライアントビルド + vitest 設定 + `/ws` プロキシ |
| `lefthook.yml` / `AGENTS.md` | 品質ゲート・エージェント指示 |
| `index.html` | Vite エントリHTML |
| `src/shared/types.ts` | ドメイン型・メッセージ型（純粋） |
| `src/shared/config.ts` | 人数別 配布/役職/チップ テーブル（純粋） |
| `src/shared/random.ts` | システムシャッフル生成（Math.random） |
| `src/shared/engine.ts` | ゲーム状態遷移の純粋関数群 |
| `src/shared/engine.test.ts` | エンジン単体テスト（正確性の大部分） |
| `src/shared/view.test.ts` | `toPlayerView` 秘匿フィルタ検証 |
| `src/server/room.ts` | Room: 状態保持・コマンド適用・スナップショット配信 |
| `src/server/room.test.ts` | Room フロー＋viewフィルタの結合テスト |
| `src/server/app.ts` | Express アプリ（/healthz + 静物 + SPAフォールバック） |
| `src/server/websocket.ts` | ws サーバー・セッション管理・ルーティング |
| `src/server/index.ts` | HTTP+WS 起点 |
| `src/client/main.tsx` | React エントリ |
| `src/client/useSocket.ts` | WebSocket 接続フック |
| `src/client/App.tsx` | フェーズ切替・状態管理 |
| `src/client/components/Lobby.tsx` | ロビー画面 |
| `src/client/components/RoleReveal.tsx` | 役職確認オーバーレイ |
| `src/client/components/GameTable.tsx` | グリッド盤面 |
| `src/client/components/PlayerPanel.tsx` | プレイヤーパネル |
| `src/client/components/GameOver.tsx` | 結果画面 |
| `src/client/styles.css` | スタイル |
| `tests/client/*` | クライアントコンポーネントテスト |

---

## Task 1: プロジェクトスキャフォールド

**Files:**
- Create: `package.json`, `tsconfig.client.json`, `tsconfig.server.json`, `tsconfig.test.json`, `tsconfig.client.test.json`, `vite.config.ts`, `index.html`, `AGENTS.md`, `lefthook.yml`

- [ ] **Step 1: `package.json` を作成**

```json
{
  "name": "timebomb-online",
  "version": "0.1.0",
  "private": true,
  "type": "module",
  "scripts": {
    "dev": "concurrently \"tsx watch src/server/index.ts\" \"vite\"",
    "dev:server": "tsx watch src/server/index.ts",
    "dev:client": "vite",
    "test": "vitest run",
    "test:watch": "vitest",
    "typecheck": "tsc --noEmit -p tsconfig.server.json && tsc --noEmit -p tsconfig.client.json && tsc --noEmit -p tsconfig.test.json && tsc --noEmit -p tsconfig.client.test.json",
    "build": "vite build && tsc -p tsconfig.server.json",
    "start": "node dist-server/server/index.js",
    "render-build": "npm run build"
  },
  "dependencies": {
    "express": "^5.1.0",
    "react": "^19.0.0",
    "react-dom": "^19.0.0",
    "ws": "^8.18.0"
  },
  "devDependencies": {
    "@testing-library/jest-dom": "^6.6.0",
    "@testing-library/react": "^16.0.0",
    "@testing-library/user-event": "^14.5.0",
    "@types/express": "^5.0.0",
    "@types/node": "^22.0.0",
    "@types/react": "^19.0.0",
    "@types/react-dom": "^19.0.0",
    "@types/ws": "^8.5.0",
    "@vitejs/plugin-react": "^5.0.0",
    "concurrently": "^9.0.0",
    "jsdom": "^25.0.0",
    "lefthook": "^2.1.4",
    "tsx": "^4.19.0",
    "typescript": "^5.6.0",
    "vite": "^6.0.0",
    "vitest": "^2.1.0"
  }
}
```

- [ ] **Step 2: tsconfig 4ファイルを作成**

`tsconfig.client.json`:
```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ESNext",
    "moduleResolution": "Bundler",
    "jsx": "react-jsx",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "resolveJsonModule": true,
    "types": ["vite/client"]
  },
  "include": ["src/client/**/*.ts", "src/client/**/*.tsx", "src/shared/**/*.ts"]
}
```

`tsconfig.server.json`:
```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "NodeNext",
    "moduleResolution": "NodeNext",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "resolveJsonModule": true,
    "rootDir": "src",
    "outDir": "dist-server",
    "types": ["node"]
  },
  "include": ["src/server/**/*.ts", "src/shared/**/*.ts"]
}
```

`tsconfig.test.json`:
```json
{
  "extends": "./tsconfig.server.json",
  "compilerOptions": {
    "noEmit": true,
    "rootDir": ".",
    "types": ["node", "vitest/globals"]
  },
  "include": ["src/server/**/*.ts", "src/shared/**/*.ts", "tests/server/**/*.ts"]
}
```

`tsconfig.client.test.json`:
```json
{
  "extends": "./tsconfig.client.json",
  "compilerOptions": {
    "noEmit": true,
    "types": ["vite/client", "vitest/globals", "@testing-library/jest-dom"]
  },
  "include": ["src/client/**/*.ts", "src/client/**/*.tsx", "src/shared/**/*.ts", "tests/client/**/*.ts", "tests/client/**/*.tsx"]
}
```

- [ ] **Step 3: `vite.config.ts` を作成**

```ts
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/ws": {
        target: "ws://localhost:3000",
        ws: true
      }
    }
  },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: "./tests/client/setup.ts"
  }
});
```

- [ ] **Step 4: `index.html` を作成**

```html
<!doctype html>
<html lang="ja">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>タイムボム Online</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/client/main.tsx"></script>
  </body>
</html>
```

- [ ] **Step 5: `AGENTS.md` を作成（エージェント指示）**

```markdown
# Timebomb Online

正体隠匿ボードゲーム TIME BOMB のブラウザオンライン版。

## コマンド
- 開発: `npm run dev`（サーバー:3000 / クライアント:5173）
- テスト: `npm test`
- 型検証: `npm run typecheck`
- ビルド: `npm run build`

## 設計の絶対ルール
- サーバー権威。他プレイヤーのカードのオモテ・役職は **いかなる PlayerView にも含めない**。
- サーバー(`src/server/**`, `tests/server/**`)の import は NodeNext なので **`.js` 拡張子必須**。クライアントは拡張子なし。
- 純粋エンジン(`src/shared/engine.ts`)はネットワーク非依存。ロジックはここに置き、vitest で検証する。

## スペック
`docs/superpowers/specs/2026-06-16-timebomb-online-design.md`
```

- [ ] **Step 6: `lefthook.yml` を作成**

```yaml
pre-commit:
  parallel: true
  commands:
    typecheck:
      run: npm run typecheck
    tests:
      run: npm test
```

- [ ] **Step 7: インストールと検証**

Run: `npm install`
Expected: `package-lock.json` が生成され、依存関係のインストールが成功する

Note: この時点では `src/` が未作成のため、`npm run typecheck` は Task 2 の `src/shared/types.ts` 作成後に実行する。

- [ ] **Step 8: コミット**

```bash
git add package.json package-lock.json tsconfig.client.json tsconfig.server.json tsconfig.test.json tsconfig.client.test.json vite.config.ts index.html AGENTS.md lefthook.yml
git commit -m "chore: scaffold timebomb project (vite+react+ts+ws)"
```

---

## Task 2: 共有型 `shared/types.ts`

**Files:**
- Create: `src/shared/types.ts`

- [ ] **Step 1: 型を定義**

```ts
export type CardType = "defuse" | "bomb" | "silence";
export type Role = "police" | "bomber" | "spy";
export type Phase =
  | "lobby"
  | "role_reveal"
  | "round_deal"
  | "round_play"
  | "round_end"
  | "game_end";

export interface Player {
  id: string;
  name: string;
  role: Role | null;
  hand: CardType[];
  connected: boolean;
  ready: boolean;
  isHost: boolean;
}

export interface CutEvent {
  cutterId: string;
  targetId: string;
  cardIndex: number;
  revealedType: CardType;
}

export interface RevealedRole {
  playerId: string;
  role: Role;
}

export interface GameState {
  phase: Phase;
  round: number;
  spyEnabled: boolean;
  players: Player[];
  currentCutterId: string | null;
  defuseChipsFlipped: number;
  cutsThisRound: number;
  lastCut: CutEvent | null;
  winners: Role[] | null;
}

export interface PlayerViewPublicPlayer {
  id: string;
  name: string;
  handSize: number;
  connected: boolean;
  ready: boolean;
  isHost: boolean;
}

export interface PlayerView {
  phase: Phase;
  round: number;
  spyEnabled: boolean;
  myPlayerId: string;
  myRole: Role | null;
  myHand: CardType[];
  currentCutterId: string | null;
  defuseChipsFlipped: number;
  defuseChipsTotal: number;
  cutsThisRound: number;
  cutsPerRound: number;
  players: PlayerViewPublicPlayer[];
  lastCut: CutEvent | null;
  winners: Role[] | null;
  revealedRoles: RevealedRole[] | null;
}

// ---- Client -> Server ----
export type ClientMessage =
  | { type: "join"; name: string; roomCode: string; playerId?: string }
  | { type: "setSpy"; enabled: boolean }
  | { type: "start" }
  | { type: "ready" }
  | { type: "cut"; targetId: string; cardIndex: number }
  | { type: "restart" };

// ---- Server -> Client ----
export type ServerMessage =
  | { type: "joined"; playerId: string; roomCode: string; isHost: boolean }
  | { type: "state"; view: PlayerView }
  | { type: "error"; message: string };

export const MIN_PLAYERS = 4;
export const MAX_PLAYERS = 6;
export const TOTAL_ROUNDS = 4;
export const HAND_SIZE = 5;
```

- [ ] **Step 2: 型検証**

Run: `npm run typecheck`
Expected: 成功

- [ ] **Step 3: コミット**

```bash
git add src/shared/types.ts
git commit -m "feat(shared): add domain and message types"
```

---

## Task 3: 設定テーブル `shared/config.ts`（TDD）

**Files:**
- Create: `src/shared/config.ts`
- Test: `src/shared/config.test.ts`

- [ ] **Step 1: 失敗テストを書く**

`src/shared/config.test.ts`:
```ts
import { describe, expect, it } from "vitest";
import {
  handDistributionFor,
  roleDistributionFor,
  cutsPerRound,
  defuseChipsTotal,
} from "./config.js";

describe("config", () => {
  it("手札配布: defuse=N, bomb=1, silence=4N-1", () => {
    expect(handDistributionFor(4)).toEqual({ defuse: 4, bomb: 1, silence: 15 });
    expect(handDistributionFor(5)).toEqual({ defuse: 5, bomb: 1, silence: 19 });
    expect(handDistributionFor(6)).toEqual({ defuse: 6, bomb: 1, silence: 23 });
  });

  it("手札合計は常に 5N", () => {
    for (const n of [4, 5, 6]) {
      const d = handDistributionFor(n);
      expect(d.defuse + d.bomb + d.silence).toBe(n * 5);
    }
  });

  it("チップ总数とカット/ラウンドは N に等しい", () => {
    expect(defuseChipsTotal(4)).toBe(4);
    expect(cutsPerRound(4)).toBe(4);
    expect(defuseChipsTotal(6)).toBe(6);
  });

  it("役職: police が常に最多。spyなし", () => {
    expect(roleDistributionFor(4, false)).toEqual({ bomber: 1, police: 3, spy: 0 });
    expect(roleDistributionFor(6, false)).toEqual({ bomber: 2, police: 4, spy: 0 });
  });

  it("役職: spyあり", () => {
    expect(roleDistributionFor(4, true)).toEqual({ bomber: 1, police: 2, spy: 1 });
    expect(roleDistributionFor(5, true)).toEqual({ bomber: 1, police: 3, spy: 1 });
    expect(roleDistributionFor(6, true)).toEqual({ bomber: 2, police: 3, spy: 1 });
  });

  it("未対応人数は明示的に失敗する", () => {
    expect(() => roleDistributionFor(3, false)).toThrow("Unsupported player count");
    expect(() => roleDistributionFor(7, true)).toThrow("Unsupported player count");
  });
});
```

- [ ] **Step 2: テストが失敗するのを確認**

Run: `npx vitest run src/shared/config.test.ts`
Expected: FAIL（モジュール不在）

- [ ] **Step 3: 実装**

`src/shared/config.ts`:
```ts
export interface HandDistribution {
  defuse: number;
  bomb: number;
  silence: number;
}

export interface RoleDistribution {
  bomber: number;
  police: number;
  spy: number;
}

export function handDistributionFor(playerCount: number): HandDistribution {
  const defuse = playerCount;
  const bomb = 1;
  const silence = playerCount * 5 - defuse - bomb;
  return { defuse, bomb, silence };
}

export function roleDistributionFor(
  playerCount: number,
  spyEnabled: boolean
): RoleDistribution {
  const table: Record<number, { base: RoleDistribution; spy: RoleDistribution }> = {
    4: { base: { bomber: 1, police: 3, spy: 0 }, spy: { bomber: 1, police: 2, spy: 1 } },
    5: { base: { bomber: 1, police: 4, spy: 0 }, spy: { bomber: 1, police: 3, spy: 1 } },
    6: { base: { bomber: 2, police: 4, spy: 0 }, spy: { bomber: 2, police: 3, spy: 1 } },
  };
  const entry = table[playerCount];
  if (!entry) throw new Error(`Unsupported player count: ${playerCount}`);
  return spyEnabled ? entry.spy : entry.base;
}

export function cutsPerRound(playerCount: number): number {
  return playerCount;
}

export function defuseChipsTotal(playerCount: number): number {
  return playerCount;
}
```

- [ ] **Step 4: テストが通るのを確認**

Run: `npx vitest run src/shared/config.test.ts`
Expected: PASS（全件）

- [ ] **Step 5: コミット**

```bash
git add src/shared/config.ts src/shared/config.test.ts
git commit -m "feat(shared): add distribution config tables with tests"
```

---

## Task 4: シャッフル `shared/random.ts`

**Files:**
- Create: `src/shared/random.ts`

- [ ] **Step 1: 実装**

```ts
export type Shuffle = <T>(arr: readonly T[]) => T[];

/** Math.random ベースの Fisher-Yates。サーバー側の非セキュリティ用途。 */
export function createSystemShuffle(): Shuffle {
  return <T>(arr: readonly T[]): T[] => {
    const a = [...arr];
    for (let i = a.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [a[i], a[j]] = [a[j], a[i]];
    }
    return a;
  };
}

/** テスト用: 引数をそのままコピーして返す（順序保存）。 */
export const identityShuffle: Shuffle = <T>(arr: readonly T[]): T[] => [...arr];
```

- [ ] **Step 2: コミット**

```bash
git add src/shared/random.ts
git commit -m "feat(shared): add system shuffle and identity shuffle for tests"
```

---

## Task 5: エンジン基礎（状態生成・デッキ・配布・役職）（TDD）

**Files:**
- Create: `src/shared/engine.ts`, `src/shared/engine.test.ts`

- [ ] **Step 1: 失敗テストを書く（engine.test.ts の第1ブロック）**

```ts
import { describe, expect, it } from "vitest";
import { identityShuffle } from "./random.js";
import {
  buildDeck,
  createInitialState,
  dealRound,
  assignRoles,
  pickRandomFirstCutter,
} from "./engine.js";
import { handDistributionFor } from "./config.js";
import type { Player } from "./types.js";

function makePlayers(n: number): Player[] {
  return Array.from({ length: n }, (_, i) => ({
    id: `p${i}`,
    name: `P${i}`,
    role: null,
    hand: [],
    connected: true,
    ready: false,
    isHost: i === 0,
  }));
}

describe("engine basics", () => {
  it("createInitialState は lobby で空", () => {
    const s = createInitialState();
    expect(s.phase).toBe("lobby");
    expect(s.players).toEqual([]);
    expect(s.round).toBe(0);
    expect(s.winners).toBeNull();
  });

  it("buildDeck は配布テーブルに一致", () => {
    const d = handDistributionFor(4);
    const deck = buildDeck(4);
    expect(deck.filter((c) => c === "defuse").length).toBe(d.defuse);
    expect(deck.filter((c) => c === "bomb").length).toBe(d.bomb);
    expect(deck.filter((c) => c === "silence").length).toBe(d.silence);
    expect(deck.length).toBe(20);
  });

  it("dealRound は各員に5枚ずつ配る", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    dealRound(s, identityShuffle);
    expect(s.players.every((p) => p.hand.length === 5)).toBe(true);
    expect(s.cutsThisRound).toBe(0);
  });

  it("assignRoles は構成通りに役職を配る（identity）", () => {
    const players = makePlayers(4);
    assignRoles(players, true, identityShuffle);
    const counts = { bomber: 0, police: 0, spy: 0 };
    players.forEach((p) => { counts[p.role as "bomber" | "police" | "spy"]++; });
    expect(counts).toEqual({ bomber: 1, police: 2, spy: 1 });
  });

  it("pickRandomFirstCutter は誰か一人のidを返す", () => {
    const players = makePlayers(4);
    const id = pickRandomFirstCutter(players, identityShuffle);
    expect(players.some((p) => p.id === id)).toBe(true);
  });
});
```

- [ ] **Step 2: テストが失敗するのを確認**

Run: `npx vitest run src/shared/engine.test.ts`
Expected: FAIL（engine 未実装）

- [ ] **Step 3: 実装（engine.ts 第1部）**

```ts
import {
  cutsPerRound,
  defuseChipsTotal,
  handDistributionFor,
  roleDistributionFor,
} from "./config.js";
import type { Shuffle } from "./random.js";
import {
  HAND_SIZE,
  TOTAL_ROUNDS,
  type CardType,
  type CutEvent,
  type GameState,
  type Phase,
  type Player,
  type Role,
} from "./types.js";

export function createInitialState(): GameState {
  return {
    phase: "lobby",
    round: 0,
    spyEnabled: false,
    players: [],
    currentCutterId: null,
    defuseChipsFlipped: 0,
    cutsThisRound: 0,
    lastCut: null,
    winners: null,
  };
}

export function buildDeck(playerCount: number): CardType[] {
  const d = handDistributionFor(playerCount);
  const deck: CardType[] = [];
  for (let i = 0; i < d.defuse; i++) deck.push("defuse");
  for (let i = 0; i < d.bomb; i++) deck.push("bomb");
  for (let i = 0; i < d.silence; i++) deck.push("silence");
  return deck;
}

export function dealRound(state: GameState, shuffle: Shuffle): void {
  const deck = shuffle(buildDeck(state.players.length));
  state.players.forEach((p, i) => {
    p.hand = deck.slice(i * HAND_SIZE, i * HAND_SIZE + HAND_SIZE);
  });
  state.cutsThisRound = 0;
  state.lastCut = null;
}

export function assignRoles(
  players: Player[],
  spyEnabled: boolean,
  shuffle: Shuffle
): void {
  const dist = roleDistributionFor(players.length, spyEnabled);
  const roles: Role[] = [];
  for (let i = 0; i < dist.bomber; i++) roles.push("bomber");
  for (let i = 0; i < dist.spy; i++) roles.push("spy");
  for (let i = 0; i < dist.police; i++) roles.push("police");
  const ordered = shuffle(roles);
  players.forEach((p, i) => {
    p.role = ordered[i];
  });
}

export function pickRandomFirstCutter(players: Player[], shuffle: Shuffle): string {
  return shuffle(players)[0].id;
}
```

- [ ] **Step 4: テストが通るのを確認**

Run: `npx vitest run src/shared/engine.test.ts`
Expected: PASS

- [ ] **Step 5: コミット**

```bash
git add src/shared/engine.ts src/shared/engine.test.ts
git commit -m "feat(engine): state creation, deck, deal, role assignment"
```

---

## Task 6: エンジン ライフサイクルとカット（TDD）

**Files:**
- Modify: `src/shared/engine.ts`（追記）
- Modify: `src/shared/engine.test.ts`（追記）

- [ ] **Step 1: 失敗テストを追記**

`src/shared/engine.test.ts` に追記:
```ts
import { startGame, beginPlay, applyCut, advanceRound, restart, canStart } from "./engine.js";

describe("engine lifecycle", () => {
  it("canStart は 4〜6 人で true", () => {
    const s = createInitialState();
    expect(canStart(s)).toBe(false);
    s.players = makePlayers(3);
    expect(canStart(s)).toBe(false);
    s.players = makePlayers(4);
    expect(canStart(s)).toBe(true);
    s.players = makePlayers(7);
    expect(canStart(s)).toBe(false);
  });

  it("startGame は役職を割り当て role_reveal へ", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    s.spyEnabled = true;
    startGame(s, identityShuffle);
    expect(s.phase).toBe("role_reveal");
    expect(s.players.every((p) => p.role !== null)).toBe(true);
    expect(s.players.every((p) => p.ready === false)).toBe(true);
  });

  it("beginPlay は R1 配布して round_play。先手は誰か一人", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    beginPlay(s, identityShuffle);
    expect(s.phase).toBe("round_play");
    expect(s.round).toBe(1);
    expect(s.defuseChipsFlipped).toBe(0);
    expect(s.players.every((p) => p.hand.length === 5)).toBe(true);
    expect(s.currentCutterId).not.toBeNull();
  });
});

describe("applyCut", () => {
  function setup4(): GameState {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    beginPlay(s, identityShuffle);
    return s;
  }

  it("手番外のカットは拒否", () => {
    const s = setup4();
    const notCutter = s.players.find((p) => p.id !== s.currentCutterId)!;
    const r = applyCut(s, notCutter.id, { targetId: s.currentCutterId!, cardIndex: 0 });
    expect(r.kind).toBe("invalid");
    expect(s.phase).toBe("round_play");
  });

  it("自分切りは拒否", () => {
    const s = setup4();
    const r = applyCut(s, s.currentCutterId!, { targetId: s.currentCutterId!, cardIndex: 0 });
    expect(r.kind).toBe("invalid");
  });

  it("範囲外 index は拒否", () => {
    const s = setup4();
    const target = s.players.find((p) => p.id !== s.currentCutterId)!;
    const r = applyCut(s, s.currentCutterId!, { targetId: target.id, cardIndex: 99 });
    expect(r.kind).toBe("invalid");
  });

  it("小数 index は拒否", () => {
    const s = setup4();
    const target = s.players.find((p) => p.id !== s.currentCutterId)!;
    const r = applyCut(s, s.currentCutterId!, { targetId: target.id, cardIndex: 0.5 });
    expect(r.kind).toBe("invalid");
  });

  it("silence は継続・ターン継承（切られた人が次）", () => {
    const s = setup4();
    const cutter = s.currentCutterId!;
    const target = s.players.find((p) => p.id !== cutter)!;
    // identity デッキ順を制御できないため、結果型で分岐
    const before = s.cutsThisRound;
    const r = applyCut(s, cutter, { targetId: target.id, cardIndex: 0 });
    expect(r.kind).toBe("ok");
    expect(s.currentCutterId).toBe(target.id); // ターン継承
    expect(s.cutsThisRound).toBe(before + 1);
  });

  it("bomb カットで即ボマー勝ち", () => {
    const s = setup4();
    // ターゲットの手札0番を強制的に bomb にする
    const cutter = s.currentCutterId!;
    const target = s.players.find((p) => p.id !== cutter)!;
    target.hand = ["bomb", "silence", "silence", "silence", "silence"];
    const r = applyCut(s, cutter, { targetId: target.id, cardIndex: 0 });
    expect(r.kind).toBe("ok");
    expect(r.kind === "ok" && r.revealedType).toBe("bomb");
    expect(s.phase).toBe("game_end");
    expect(s.winners).toEqual(["bomber"]);
  });

  it("最後の defuse で警察勝ち", () => {
    const s = setup4();
    s.defuseChipsFlipped = defuseChipsTotal(4) - 1; // あと1個
    const cutter = s.currentCutterId!;
    const target = s.players.find((p) => p.id !== cutter)!;
    target.hand = ["defuse", "silence", "silence", "silence", "silence"];
    const r = applyCut(s, cutter, { targetId: target.id, cardIndex: 0 });
    expect(r.kind === "ok" && r.revealedType).toBe("defuse");
    expect(s.phase).toBe("game_end");
    expect(s.winners).toEqual(["police"]);
  });

  it("N回カットで round_end へ", () => {
    const s = setup4();
    const N = 4;
    for (let i = 0; i < N; i++) {
      if (s.phase !== "round_play") break;
      const cutter = s.currentCutterId!;
      const target = s.players.find((p) => p.id !== cutter && p.hand.length > 0)!;
      const r = applyCut(s, cutter, { targetId: target.id, cardIndex: 0 });
      expect(r.kind).toBe("ok");
    }
    expect(s.phase).toBe("round_end");
  });
});

describe("advanceRound / restart", () => {
  it("round_end から次Rへ。ターン継承で先手は前R最終被カット者", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    beginPlay(s, identityShuffle);
    // 強制的に round_end まで進める（上記テスト同様）
    for (let i = 0; i < 4; i++) {
      if (s.phase !== "round_play") break;
      const cutter = s.currentCutterId!;
      const target = s.players.find((p) => p.id !== cutter && p.hand.length > 0)!;
      applyCut(s, cutter, { targetId: target.id, cardIndex: 0 });
    }
    expect(s.phase).toBe("round_end");
    const lastCutTarget = s.lastCut!.targetId;
    advanceRound(s, identityShuffle);
    expect(s.phase).toBe("round_play");
    expect(s.round).toBe(2);
    expect(s.currentCutterId).toBe(lastCutTarget);
    expect(s.players.every((p) => p.hand.length === 5)).toBe(true);
  });

  it("R4 終了で未決着: spyありなら spy 勝ち", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    s.spyEnabled = true;
    startGame(s, identityShuffle);
    beginPlay(s, identityShuffle);
    s.round = 4;
    s.phase = "round_end";
    advanceRound(s, identityShuffle);
    expect(s.winners).toEqual(["spy"]);
  });

  it("R4 終了で未決着: spyなしなら bomber 勝ち", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    beginPlay(s, identityShuffle);
    s.round = 4;
    s.phase = "round_end";
    advanceRound(s, identityShuffle);
    expect(s.winners).toEqual(["bomber"]);
  });

  it("restart は lobby に戻し役職/手札をクリア（プレイヤーは維持）", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    restart(s);
    expect(s.phase).toBe("lobby");
    expect(s.round).toBe(0);
    expect(s.winners).toBeNull();
    expect(s.players.length).toBe(4);
    expect(s.players.every((p) => p.role === null && p.hand.length === 0)).toBe(true);
  });
});
```

- [ ] **Step 2: テストが失敗するのを確認**

Run: `npx vitest run src/shared/engine.test.ts`
Expected: FAIL（未実装関数）

- [ ] **Step 3: 実装を engine.ts に追記**

```ts
import { MIN_PLAYERS, MAX_PLAYERS } from "./types.js";

export type CutInput = { targetId: string; cardIndex: number };
export type CutResult =
  | { kind: "ok"; revealedType: CardType }
  | { kind: "invalid"; reason: string };

export function canStart(state: GameState): boolean {
  return state.players.length >= MIN_PLAYERS && state.players.length <= MAX_PLAYERS;
}

function endGame(state: GameState, winners: Role[]): void {
  state.phase = "game_end";
  state.winners = winners;
}

export function startGame(state: GameState, shuffle: Shuffle): void {
  assignRoles(state.players, state.spyEnabled, shuffle);
  state.players.forEach((p) => { p.ready = false; });
  state.phase = "role_reveal";
}

export function beginPlay(state: GameState, shuffle: Shuffle): void {
  state.round = 1;
  state.defuseChipsFlipped = 0;
  state.cutsThisRound = 0;
  state.lastCut = null;
  state.winners = null;
  state.currentCutterId = pickRandomFirstCutter(state.players, shuffle);
  dealRound(state, shuffle);
  state.phase = "round_play";
}

export function applyCut(
  state: GameState,
  cutterId: string,
  input: CutInput
): CutResult {
  if (state.phase !== "round_play") return { kind: "invalid", reason: "play 中ではありません" };
  if (state.currentCutterId !== cutterId) return { kind: "invalid", reason: "あなたの手番ではありません" };
  if (input.targetId === cutterId) return { kind: "invalid", reason: "自分は切れません" };
  const target = state.players.find((p) => p.id === input.targetId);
  if (!target) return { kind: "invalid", reason: "対象が存在しません" };
  if (!Number.isInteger(input.cardIndex) || input.cardIndex < 0 || input.cardIndex >= target.hand.length)
    return { kind: "invalid", reason: "カード位置が不正です" };

  const [revealed] = target.hand.splice(input.cardIndex, 1);
  state.lastCut = {
    cutterId,
    targetId: input.targetId,
    cardIndex: input.cardIndex,
    revealedType: revealed,
  };

  if (revealed === "defuse") {
    state.defuseChipsFlipped++;
    if (state.defuseChipsFlipped >= defuseChipsTotal(state.players.length)) {
      endGame(state, ["police"]);
      return { kind: "ok", revealedType: revealed };
    }
  } else if (revealed === "bomb") {
    endGame(state, ["bomber"]);
    return { kind: "ok", revealedType: revealed };
  }

  // ターン継承: 切られた人が次の手番
  state.currentCutterId = input.targetId;
  state.cutsThisRound++;
  if (state.cutsThisRound >= cutsPerRound(state.players.length)) {
    state.phase = "round_end";
  }
  return { kind: "ok", revealedType: revealed };
}

export function advanceRound(state: GameState, shuffle: Shuffle): void {
  if (state.phase !== "round_end") return;
  if (state.round >= TOTAL_ROUNDS) {
    endGame(state, state.spyEnabled ? ["spy"] : ["bomber"]);
    return;
  }
  state.round++;
  dealRound(state, shuffle);
  state.phase = "round_play";
  // currentCutterId は前R最終被カット者を維持（ターン継承が自然に流れる）
}

export function restart(state: GameState): void {
  state.phase = "lobby";
  state.round = 0;
  state.spyEnabled = false;
  state.defuseChipsFlipped = 0;
  state.cutsThisRound = 0;
  state.currentCutterId = null;
  state.lastCut = null;
  state.winners = null;
  state.players.forEach((p) => {
    p.role = null;
    p.hand = [];
    p.ready = false;
  });
}

export function hasValidCutTarget(state: GameState): boolean {
  if (!state.currentCutterId) return false;
  return state.players.some(
    (p) => p.id !== state.currentCutterId && p.hand.length > 0
  );
}

export function allReady(state: GameState): boolean {
  return (
    state.players.length >= MIN_PLAYERS && state.players.every((p) => p.ready)
  );
}
```

- [ ] **Step 4: テストが通るのを確認**

Run: `npx vitest run src/shared/engine.test.ts`
Expected: PASS（全件）

- [ ] **Step 5: コミット**

```bash
git add src/shared/engine.ts src/shared/engine.test.ts
git commit -m "feat(engine): lifecycle, cut resolution, turn succession, win conditions"
```

---

## Task 7: `toPlayerView` 秘匿フィルタ（TDD・チート防止の要）

**Files:**
- Modify: `src/shared/engine.ts`（追記）
- Create: `src/shared/view.test.ts`

- [ ] **Step 1: 失敗テストを書く**

`src/shared/view.test.ts`:
```ts
import { describe, expect, it } from "vitest";
import { identityShuffle } from "./random.js";
import { beginPlay, createInitialState, startGame, toPlayerView } from "./engine.js";
import type { Player } from "./types.js";

function makePlayers(n: number): Player[] {
  return Array.from({ length: n }, (_, i) => ({
    id: `p${i}`, name: `P${i}`, role: null, hand: [],
    connected: true, ready: false, isHost: i === 0,
  }));
}

describe("toPlayerView 秘匿フィルタ", () => {
  it("自分の手札オモテは自分にだけ見える", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    beginPlay(s, identityShuffle);
    const me = s.players[0];
    const view = toPlayerView(s, me.id);
    expect(view.myHand).toEqual(me.hand);
  });

  it("他人の手札オモテは一切含まれない（handSize のみ）", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    beginPlay(s, identityShuffle);
    for (const viewer of s.players) {
      const view = toPlayerView(s, viewer.id);
      const others = view.players.filter((p) => p.id !== viewer.id);
      for (const o of others) {
        // handSize のみで、カード型の配列フィールドは存在しない
        expect(typeof o.handSize).toBe("number");
        expect((o as unknown as Record<string, unknown>).hand).toBeUndefined();
        expect((o as unknown as Record<string, unknown>).role).toBeUndefined();
      }
      // public players には他人の手札配列やカード種別が漏れない
      const publicPlayersJson = JSON.stringify(view.players);
      expect(publicPlayersJson).not.toContain('"hand"');
      expect(publicPlayersJson).not.toContain("defuse");
      expect(publicPlayersJson).not.toContain("bomb");
      expect(publicPlayersJson).not.toContain("silence");
    }
  });

  it("役職は自分の分のみ。他人の役職は view に無い", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    s.spyEnabled = true;
    startGame(s, identityShuffle);
    const view = toPlayerView(s, "p0");
    expect(view.myRole).toBe(s.players[0].role);
    // players 配列に role キーが無いことを再確認
    expect(view.players.every((p) => !("role" in p))).toBe(true);
    expect(view.revealedRoles).toBeNull();
  });

  it("チップ/カット情報は全員に同一", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    beginPlay(s, identityShuffle);
    const v0 = toPlayerView(s, "p0");
    const v1 = toPlayerView(s, "p1");
    expect(v0.defuseChipsFlipped).toBe(v1.defuseChipsFlipped);
    expect(v0.defuseChipsTotal).toBe(4);
    expect(v0.cutsPerRound).toBe(4);
  });

  it("spyEnabled は公開情報として見え、revealedRoles は game_end のみ全員に見える", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    s.spyEnabled = true;
    startGame(s, identityShuffle);
    const beforeEnd = toPlayerView(s, "p0");
    expect(beforeEnd.spyEnabled).toBe(true);
    expect(beforeEnd.revealedRoles).toBeNull();

    s.phase = "game_end";
    s.winners = ["bomber"];
    const afterEnd = toPlayerView(s, "p0");
    expect(afterEnd.revealedRoles).toEqual(
      s.players.map((p) => ({ playerId: p.id, role: p.role }))
    );
  });
});
```

- [ ] **Step 2: テストが失敗するのを確認**

Run: `npx vitest run src/shared/view.test.ts`
Expected: FAIL（toPlayerView 未実装）

- [ ] **Step 3: 実装を engine.ts に追記**

```ts
import type { PlayerView, PlayerViewPublicPlayer } from "./types.js";

export function toPlayerView(state: GameState, viewerId: string): PlayerView {
  const me = state.players.find((p) => p.id === viewerId);
  const n = state.players.length;
  const players: PlayerViewPublicPlayer[] = state.players.map((p) => ({
    id: p.id,
    name: p.name,
    handSize: p.hand.length,
    connected: p.connected,
    ready: p.ready,
    isHost: p.isHost,
  }));
  return {
    phase: state.phase,
    round: state.round,
    spyEnabled: state.spyEnabled,
    myPlayerId: viewerId,
    myRole: me ? me.role : null,
    myHand: me ? [...me.hand] : [],
    currentCutterId: state.currentCutterId,
    defuseChipsFlipped: state.defuseChipsFlipped,
    defuseChipsTotal: defuseChipsTotal(n),
    cutsThisRound: state.cutsThisRound,
    cutsPerRound: cutsPerRound(n),
    players,
    lastCut: state.lastCut,
    winners: state.winners,
    revealedRoles:
      state.phase === "game_end"
        ? state.players.map((p) => ({ playerId: p.id, role: p.role! }))
        : null,
  };
}
```

- [ ] **Step 4: テストが通るのを確認**

Run: `npx vitest run src/shared/view.test.ts`
Expected: PASS

- [ ] **Step 5: コミット**

```bash
git add src/shared/engine.ts src/shared/view.test.ts
git commit -m "feat(engine): add toPlayerView with strict info-hiding filter"
```

---

## Task 8: Room クラス（状態管理・コマンド適用）（TDD）

**Files:**
- Create: `src/server/room.ts`, `src/server/room.test.ts`

- [ ] **Step 1: 失敗テストを書く**

`src/server/room.test.ts`:
```ts
import { describe, expect, it } from "vitest";
import { identityShuffle } from "../shared/random.js";
import { Room } from "./room.js";

function fillRoom(r: Room, n: number): string[] {
  const ids: string[] = [];
  for (let i = 0; i < n; i++) {
    const res = r.handleJoin({ type: "join", name: `P${i}`, roomCode: r.code });
    if (res.type === "joined") ids.push(res.playerId);
  }
  return ids;
}

describe("Room", () => {
  it("join でプレイヤーが増え、最初の人がホスト", () => {
    const r = new Room("ABCD", identityShuffle);
    const a = r.handleJoin({ type: "join", name: "A", roomCode: "ABCD" });
    expect(a.type).toBe("joined");
    if (a.type !== "joined") return;
    expect(a.isHost).toBe(true);
    expect(r.state.players.length).toBe(1);
    expect(r.state.players[0].isHost).toBe(true);
  });

  it("満員(6)超えの join は error", () => {
    const r = new Room("ABCD", identityShuffle);
    fillRoom(r, 6);
    const res = r.handleJoin({ type: "join", name: "X", roomCode: "ABCD" });
    expect(res.type).toBe("error");
  });

  it("重複名は error", () => {
    const r = new Room("ABCD", identityShuffle);
    r.handleJoin({ type: "join", name: "A", roomCode: "ABCD" });
    const res = r.handleJoin({ type: "join", name: "A", roomCode: "ABCD" });
    expect(res.type).toBe("error");
  });

  it("start はホストのみ・4人未満は不可", () => {
    const r = new Room("ABCD", identityShuffle);
    const ids = fillRoom(r, 3);
    expect(r.handleCommand(ids[0], { type: "start" }).some((m) => m.type === "error")).toBe(true);
    r.handleJoin({ type: "join", name: "P3", roomCode: "ABCD" });
    expect(r.state.phase).toBe("lobby");
    r.handleCommand(ids[0], { type: "start" });
    expect(r.state.phase).toBe("role_reveal");
  });

  it("全員 ready で round_play に遷移", () => {
    const r = new Room("ABCD", identityShuffle);
    const ids = fillRoom(r, 4);
    r.handleCommand(ids[0], { type: "start" });
    ids.forEach((id) => r.handleCommand(id, { type: "ready" }));
    expect(r.state.phase).toBe("round_play");
  });

  it("cut 適用でスナップショットが更新される（lastCut）", () => {
    const r = new Room("ABCD", identityShuffle);
    const ids = fillRoom(r, 4);
    r.handleCommand(ids[0], { type: "start" });
    ids.forEach((id) => r.handleCommand(id, { type: "ready" }));
    const cutter = r.state.currentCutterId!;
    const target = r.state.players.find((p) => p.id !== cutter)!;
    r.handleCommand(cutter, { type: "cut", targetId: target.id, cardIndex: 0 });
    expect(r.state.lastCut).not.toBeNull();
    expect(r.state.lastCut!.targetId).toBe(target.id);
  });

  it("切断で connected=false、ホスト切断時は別人へ委譲", () => {
    const r = new Room("ABCD", identityShuffle);
    const ids = fillRoom(r, 4);
    r.handleDisconnect(ids[0]);
    expect(r.state.players[0].connected).toBe(false);
    expect(r.state.players[1].isHost).toBe(true);
  });

  it("リコネクト: 同 playerId で再参加すると席復元", () => {
    const r = new Room("ABCD", identityShuffle);
    const ids = fillRoom(r, 4);
    r.handleDisconnect(ids[1]);
    expect(r.state.players[1].connected).toBe(false);
    const res = r.handleJoin({ type: "join", name: "P1", roomCode: "ABCD", playerId: ids[1] });
    expect(res.type).toBe("joined");
    expect(r.state.players[1].connected).toBe(true);
  });

  it("満員でも既存 playerId のリコネクトは許可", () => {
    const r = new Room("ABCD", identityShuffle);
    const ids = fillRoom(r, 6);
    r.handleDisconnect(ids[5]);
    const res = r.handleJoin({ type: "join", name: "P5", roomCode: "ABCD", playerId: ids[5] });
    expect(res.type).toBe("joined");
    expect(r.state.players.length).toBe(6);
    expect(r.state.players[5].connected).toBe(true);
  });

  it("リコネクト時も他人の名前には変更できない", () => {
    const r = new Room("ABCD", identityShuffle);
    const ids = fillRoom(r, 4);
    r.handleDisconnect(ids[1]);
    const res = r.handleJoin({ type: "join", name: "P0", roomCode: "ABCD", playerId: ids[1] });
    expect(res.type).toBe("error");
  });

  it("snapshot(view) は他人の手札オモテを含まない", () => {
    const r = new Room("ABCD", identityShuffle);
    const ids = fillRoom(r, 4);
    r.handleCommand(ids[0], { type: "start" });
    ids.forEach((id) => r.handleCommand(id, { type: "ready" }));
    for (const id of ids) {
      const snap = r.snapshotFor(id);
      // 他人の手札型文字列が直接 view に現れない（myHand は自分のみ）
      expect(snap.myPlayerId).toBe(id);
      expect(snap.players.every((p) => !("hand" in p))).toBe(true);
      expect(snap.players.every((p) => !("role" in p))).toBe(true);
      expect(snap.revealedRoles).toBeNull();
      expect(JSON.stringify(snap.players)).not.toContain('"hand"');
    }
  });
});
```

- [ ] **Step 2: テストが失敗するのを確認**

Run: `npx vitest run src/server/room.test.ts`
Expected: FAIL（Room 未実装）

- [ ] **Step 3: 実装 `src/server/room.ts`**

```ts
import { randomUUID } from "node:crypto";
import {
  advanceRound,
  allReady,
  applyCut,
  beginPlay,
  canStart,
  createInitialState,
  restart,
  startGame,
  toPlayerView,
} from "../shared/engine.js";
import type { Shuffle } from "../shared/random.js";
import { MAX_PLAYERS } from "../shared/types.js";
import type {
  ClientMessage,
  GameState,
  PlayerView,
  ServerMessage,
} from "../shared/types.js";

export class Room {
  readonly code: string;
  readonly state: GameState;
  private shuffle: Shuffle;

  constructor(code: string, shuffle: Shuffle) {
    this.code = code;
    this.shuffle = shuffle;
    this.state = createInitialState();
  }

  handleJoin(msg: Extract<ClientMessage, { type: "join" }>): ServerMessage {
    // リコネクト: 同 playerId の既存席があれば復元
    if (msg.playerId) {
      const existing = this.state.players.find((p) => p.id === msg.playerId);
      if (existing) {
        if (this.state.players.some((p) => p.id !== existing.id && p.name === msg.name)) {
          return { type: "error", message: "その名前は既に使われています" };
        }
        existing.connected = true;
        existing.name = msg.name;
        return { type: "joined", playerId: existing.id, roomCode: this.code, isHost: existing.isHost };
      }
    }
    if (this.state.players.length >= MAX_PLAYERS) {
      return { type: "error", message: "ルームが満員です" };
    }
    if (this.state.players.some((p) => p.name === msg.name)) {
      return { type: "error", message: "その名前は既に使われています" };
    }
    const id = randomUUID();
    const isHost = this.state.players.length === 0;
    this.state.players.push({
      id,
      name: msg.name,
      role: null,
      hand: [],
      connected: true,
      ready: false,
      isHost,
    });
    return { type: "joined", playerId: id, roomCode: this.code, isHost };
  }

  handleCommand(playerId: string, msg: ClientMessage): ServerMessage[] {
    const player = this.state.players.find((p) => p.id === playerId);
    if (!player) return [{ type: "error", message: "プレイヤーが存在しません" }];
    const out: ServerMessage[] = [];
    switch (msg.type) {
      case "setSpy": {
        if (!player.isHost) return [{ type: "error", message: "ホストのみ操作できます" }];
        if (this.state.phase !== "lobby") return [{ type: "error", message: "ロビーでのみ変更できます" }];
        this.state.spyEnabled = msg.enabled;
        break;
      }
      case "start": {
        if (!player.isHost) return [{ type: "error", message: "ホストのみ開始できます" }];
        if (!canStart(this.state)) return [{ type: "error", message: "4〜6人で開始できます" }];
        startGame(this.state, this.shuffle);
        break;
      }
      case "ready": {
        if (this.state.phase !== "role_reveal")
          return [{ type: "error", message: "役職確認中のみ操作できます" }];
        player.ready = true;
        if (allReady(this.state)) beginPlay(this.state, this.shuffle);
        break;
      }
      case "cut": {
        if (this.state.phase !== "round_play")
          return [{ type: "error", message: "play 中ではありません" }];
        const result = applyCut(this.state, playerId, {
          targetId: msg.targetId,
          cardIndex: msg.cardIndex,
        });
        if (result.kind === "invalid") return [{ type: "error", message: result.reason }];
        if (this.state.phase === "round_end") advanceRound(this.state, this.shuffle);
        break;
      }
      case "restart": {
        if (!player.isHost) return [{ type: "error", message: "ホストのみ再開できます" }];
        if (this.state.phase !== "game_end")
          return [{ type: "error", message: "終了時のみ再開できます" }];
        restart(this.state);
        break;
      }
      case "join":
        return [{ type: "error", message: "join は handleJoin で処理してください" }];
    }
    out.push({ type: "state", view: toPlayerView(this.state, playerId) });
    return out;
  }

  handleDisconnect(playerId: string): void {
    const player = this.state.players.find((p) => p.id === playerId);
    if (!player) return;
    player.connected = false;
    // ホスト委譲
    if (player.isHost) {
      const next = this.state.players.find((p) => p.connected);
      if (next) {
        player.isHost = false;
        next.isHost = true;
      }
    }
  }

  snapshotFor(playerId: string): PlayerView {
    return toPlayerView(this.state, playerId);
  }

  /** ルーム内の全プレイヤーの playerId（接続状態問わない）。配信先特定に使用。 */
  playerIds(): string[] {
    return this.state.players.map((p) => p.id);
  }
}
```

- [ ] **Step 4: テストが通るのを確認**

Run: `npx vitest run src/server/room.test.ts`
Expected: PASS（全件）

- [ ] **Step 5: 型検証**

Run: `npm run typecheck`
Expected: 成功

- [ ] **Step 6: コミット**

```bash
git add src/server/room.ts src/server/room.test.ts
git commit -m "feat(server): Room with join/start/cut/reconnect and filtered snapshots"
```

---

## Task 9: Express アプリ `server/app.ts`

**Files:**
- Create: `src/server/app.ts`

- [ ] **Step 1: 実装（dice-racing 準拠）**

```ts
import express from "express";
import path from "node:path";

export function createApp(clientDist: string): express.Express {
  const app = express();

  app.get("/healthz", (_req, res) => {
    res.status(200).json({ ok: true });
  });

  app.use(express.static(clientDist));
  app.use((_req, res) => {
    res.sendFile(path.join(clientDist, "index.html"), (error) => {
      if (error && !res.headersSent) {
        res.status(200).send("クライアント未ビルド。npm run dev で開発してください。");
      }
    });
  });

  return app;
}
```

- [ ] **Step 2: コミット**

```bash
git add src/server/app.ts
git commit -m "feat(server): express app with healthz and static serving"
```

---

## Task 10: WebSocket サーバー `server/websocket.ts`

**Files:**
- Create: `src/server/websocket.ts`

- [ ] **Step 1: 実装**

```ts
import type { Server as HttpServer } from "node:http";
import { WebSocket, WebSocketServer } from "ws";
import { createSystemShuffle } from "../shared/random.js";
import type { ClientMessage, ServerMessage } from "../shared/types.js";
import { Room } from "./room.js";

interface Session {
  roomCode: string;
  playerId: string;
}

export function attachWebSocketServer(
  server: HttpServer,
  rooms = new Map<string, Room>()
): { wss: WebSocketServer; rooms: Map<string, Room> } {
  const shuffle = createSystemShuffle();
  const wss = new WebSocketServer({ server, path: "/ws" });
  const sessions = new Map<WebSocket, Session>();
  const roomSockets = new Map<string, Set<WebSocket>>();

  wss.on("connection", (socket) => {
    socket.on("message", (data) => {
      const parsed = parseMessage(data);
      if ("error" in parsed) {
        send(socket, { type: "error", message: parsed.error });
        return;
      }
      const msg = parsed.message;

      // join はセッション未確定でも受付
      if (msg.type === "join") {
        const roomCode = msg.roomCode.trim().toUpperCase();
        const room = getOrCreateRoom(rooms, roomCode, shuffle);
        const result = room.handleJoin({ ...msg, roomCode });
        send(socket, result);
        if (result.type === "joined") {
          // 同 playerId の古い接続があれば置き換え
          detachDupSocket(socket, room.code, result.playerId, sessions, roomSockets);
          const session: Session = { roomCode: room.code, playerId: result.playerId };
          bind(socket, session, sessions, roomSockets);
          broadcastRoom(room, rooms, sessions, roomSockets);
        }
        return;
      }

      const session = sessions.get(socket);
      if (!session) {
        send(socket, { type: "error", message: "先に join してください" });
        return;
      }
      const room = rooms.get(session.roomCode);
      if (!room) {
        send(socket, { type: "error", message: "ルームが存在しません" });
        return;
      }
      const replies = room.handleCommand(session.playerId, msg);
      for (const r of replies) send(socket, r);
      broadcastRoom(room, rooms, sessions, roomSockets);
    });

    socket.on("close", () => {
      const session = sessions.get(socket);
      if (!session) {
        sessions.delete(socket);
        return;
      }
      const room = rooms.get(session.roomCode);
      unbind(socket, session, sessions, roomSockets);
      if (room) {
        room.handleDisconnect(session.playerId);
        broadcastRoom(room, rooms, sessions, roomSockets);
      }
    });
  });

  return { wss, rooms };
}

function getOrCreateRoom(
  rooms: Map<string, Room>,
  code: string,
  shuffle: ReturnType<typeof createSystemShuffle>
): Room {
  const normalizedCode = code.trim().toUpperCase();
  let room = rooms.get(normalizedCode);
  if (!room) {
    room = new Room(normalizedCode, shuffle);
    rooms.set(room.code, room);
  }
  return room;
}

function bind(
  socket: WebSocket,
  session: Session,
  sessions: Map<WebSocket, Session>,
  roomSockets: Map<string, Set<WebSocket>>
): void {
  sessions.set(socket, session);
  let set = roomSockets.get(session.roomCode);
  if (!set) {
    set = new Set();
    roomSockets.set(session.roomCode, set);
  }
  set.add(socket);
}

function unbind(
  socket: WebSocket,
  session: Session,
  sessions: Map<WebSocket, Session>,
  roomSockets: Map<string, Set<WebSocket>>
): void {
  sessions.delete(socket);
  const set = roomSockets.get(session.roomCode);
  set?.delete(socket);
  if (set && set.size === 0) roomSockets.delete(session.roomCode);
}

function detachDupSocket(
  current: WebSocket,
  roomCode: string,
  playerId: string,
  sessions: Map<WebSocket, Session>,
  roomSockets: Map<string, Set<WebSocket>>
): void {
  const set = roomSockets.get(roomCode);
  if (!set) return;
  for (const s of [...set]) {
    if (s === current) continue;
    const sess = sessions.get(s);
    if (sess?.playerId === playerId) {
      sessions.delete(s);
      set.delete(s);
      s.close(1000, "replaced");
    }
  }
}

function broadcastRoom(
  room: Room,
  rooms: Map<string, Room>,
  sessions: Map<WebSocket, Session>,
  roomSockets: Map<string, Set<WebSocket>>
): void {
  const set = roomSockets.get(room.code);
  if (!set) return;
  for (const s of set) {
    const sess = sessions.get(s);
    if (!sess) continue;
    send(s, { type: "state", view: room.snapshotFor(sess.playerId) });
  }
}

function send(socket: WebSocket, msg: ServerMessage): void {
  if (socket.readyState !== WebSocket.OPEN) return;
  socket.send(JSON.stringify(msg));
}

function parseMessage(
  data: WebSocket.RawData
): { message: ClientMessage } | { error: string } {
  let parsed: unknown;
  try {
    const text =
      typeof data === "string"
        ? data
        : Buffer.isBuffer(data)
        ? data.toString("utf8")
        : Buffer.concat(data as readonly Buffer[]).toString("utf8");
    parsed = JSON.parse(text);
  } catch {
    return { error: "JSON 形式が不正です" };
  }
  if (!parsed || typeof parsed !== "object") return { error: "メッセージが不正です" };
  const r = parsed as Record<string, unknown>;
  switch (r.type) {
    case "join":
      if (typeof r.name === "string" && typeof r.roomCode === "string")
        return { message: { type: "join", name: r.name, roomCode: r.roomCode, playerId: typeof r.playerId === "string" ? r.playerId : undefined } };
      return { error: "join のパラメータが不正です" };
    case "setSpy":
      if (typeof r.enabled === "boolean") return { message: { type: "setSpy", enabled: r.enabled } };
      return { error: "setSpy のパラメータが不正です" };
    case "start":
      return { message: { type: "start" } };
    case "ready":
      return { message: { type: "ready" } };
    case "cut":
      if (typeof r.targetId === "string" && typeof r.cardIndex === "number" && Number.isInteger(r.cardIndex))
        return { message: { type: "cut", targetId: r.targetId, cardIndex: r.cardIndex } };
      return { error: "cut のパラメータが不正です" };
    case "restart":
      return { message: { type: "restart" } };
    default:
      return { error: "未対応のコマンドです" };
  }
}
```

- [ ] **Step 2: 型検証**

Run: `npm run typecheck`
Expected: 成功

- [ ] **Step 3: コミット**

```bash
git add src/server/websocket.ts
git commit -m "feat(server): websocket server with session and room broadcast"
```

---

## Task 11: サーバー起点 `server/index.ts`

**Files:**
- Create: `src/server/index.ts`

- [ ] **Step 1: 実装**

```ts
import { createServer } from "node:http";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { createApp } from "./app.js";
import { attachWebSocketServer } from "./websocket.js";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const clientDist = path.resolve(__dirname, "../../dist");
const app = createApp(clientDist);
const server = createServer(app);
const port = Number(process.env.PORT ?? 3000);

attachWebSocketServer(server);

server.listen(port, "0.0.0.0", () => {
  console.log(`server listening on 0.0.0.0:${port}`);
});
```

- [ ] **Step 2: サーバー起動確認**

Run: `npx tsx src/server/index.ts`（Ctrl+C で終了）
Expected: `server listening on 0.0.0.0:3000` が表示される

- [ ] **Step 3: コミット**

```bash
git add src/server/index.ts
git commit -m "feat(server): http+ws entry point"
```

---

## Task 12: クライアント共通 `setup.ts` と `useSocket` フック（TDD）

**Files:**
- Create: `tests/client/setup.ts`
- Create: `src/client/useSocket.ts`, `src/client/useSocket.test.ts`

- [ ] **Step 1: テストセットアップ**

`tests/client/setup.ts`:
```ts
import "@testing-library/jest-dom";
```

- [ ] **Step 2: 失敗テストを書く**

`src/client/useSocket.test.ts`:
```ts
import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { useSocket } from "./useSocket";

class FakeSocket {
  onopen: (() => void) | null = null;
  onmessage: ((ev: { data: string }) => void) | null = null;
  onclose: (() => void) | null = null;
  sent: string[] = [];
  static last: FakeSocket | null = null;
  constructor(public url: string) {
    FakeSocket.last = this;
    setTimeout(() => this.onopen?.(), 0);
  }
  send(data: string) { this.sent.push(data); }
  close() { this.onclose?.(); }
}

describe("useSocket", () => {
  beforeEach(() => {
    FakeSocket.last = null;
    window.localStorage.clear();
    (globalThis as unknown as { WebSocket: typeof WebSocket }).WebSocket = FakeSocket as unknown as typeof WebSocket;
  });
  afterEach(() => { vi.useRealTimers(); });

  it("接続後にメッセージを受信して state を更新", async () => {
    vi.useFakeTimers();
    const { result } = renderHook(() => useSocket());
    await act(async () => { await vi.runAllTimersAsync(); });
    act(() => {
      FakeSocket.last!.onmessage?.({ data: JSON.stringify({ type: "joined", playerId: "x", roomCode: "ABCD", isHost: true }) });
    });
    expect(result.current.playerId).toBe("x");
    expect(window.localStorage.getItem("timebomb:ABCD:playerId")).toBe("x");
  });

  it("send で JSON を送信", async () => {
    vi.useFakeTimers();
    const { result } = renderHook(() => useSocket());
    await act(async () => { await vi.runAllTimersAsync(); });
    act(() => { result.current.send({ type: "ready" }); });
    expect(FakeSocket.last!.sent).toContain(JSON.stringify({ type: "ready" }));
  });

  it("join は roomCode を正規化し、保存済み playerId があれば同送する", async () => {
    vi.useFakeTimers();
    window.localStorage.setItem("timebomb:ABCD:playerId", "saved-player");
    const { result } = renderHook(() => useSocket());
    await act(async () => { await vi.runAllTimersAsync(); });
    act(() => { result.current.join("Alice", "abcd"); });
    expect(FakeSocket.last!.sent).toContain(
      JSON.stringify({ type: "join", name: "Alice", roomCode: "ABCD", playerId: "saved-player" })
    );
  });
});
```

- [ ] **Step 3: テストが失敗するのを確認**

Run: `npx vitest run src/client/useSocket.test.ts`
Expected: FAIL

- [ ] **Step 4: 実装 `src/client/useSocket.ts`**

```ts
import { useCallback, useEffect, useRef, useState } from "react";
import type { ClientMessage, PlayerView, ServerMessage } from "../shared/types";

interface SocketState {
  view: PlayerView | null;
  playerId: string | null;
  roomCode: string | null;
  isHost: boolean;
  error: string | null;
  join: (name: string, roomCode: string) => void;
  send: (msg: ClientMessage) => void;
}

export function useSocket(): SocketState {
  const wsRef = useRef<WebSocket | null>(null);
  const [view, setView] = useState<PlayerView | null>(null);
  const [playerId, setPlayerId] = useState<string | null>(null);
  const [roomCode, setRoomCode] = useState<string | null>(null);
  const [isHost, setIsHost] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const wsUrl = `${window.location.protocol === "https:" ? "wss" : "ws"}://${window.location.host}/ws`;
    const ws = new WebSocket(wsUrl);
    wsRef.current = ws;
    ws.onmessage = (ev) => {
      const msg = JSON.parse(ev.data as string) as ServerMessage;
      switch (msg.type) {
        case "joined":
          setPlayerId(msg.playerId);
          setRoomCode(msg.roomCode);
          setIsHost(msg.isHost);
          window.localStorage.setItem(`timebomb:${msg.roomCode}:playerId`, msg.playerId);
          window.localStorage.setItem("timebomb:lastRoomCode", msg.roomCode);
          break;
        case "state":
          setView(msg.view);
          setPlayerId(msg.view.myPlayerId);
          break;
        case "error":
          setError(msg.message);
          break;
      }
    };
    return () => ws.close();
  }, []);

  const send = useCallback((msg: ClientMessage) => {
    wsRef.current?.send(JSON.stringify(msg));
  }, []);

  const join = useCallback((name: string, roomCode: string) => {
    const normalizedRoomCode = roomCode.trim().toUpperCase();
    const trimmedName = name.trim();
    if (!trimmedName || !normalizedRoomCode) return;
    const savedPlayerId =
      window.localStorage.getItem(`timebomb:${normalizedRoomCode}:playerId`) ?? undefined;
    send({ type: "join", name: trimmedName, roomCode: normalizedRoomCode, playerId: savedPlayerId });
  }, [send]);

  return { view, playerId, roomCode, isHost, error, join, send };
}
```

- [ ] **Step 5: テストが通るのを確認**

Run: `npx vitest run src/client/useSocket.test.ts`
Expected: PASS

- [ ] **Step 6: コミット**

```bash
git add tests/client/setup.ts src/client/useSocket.ts src/client/useSocket.test.ts
git commit -m "feat(client): useSocket hook with tests"
```

---

## Task 13: コンポーネント `Lobby`（TDD）

**Files:**
- Create: `src/client/components/Lobby.tsx`, `tests/client/Lobby.test.tsx`

- [ ] **Step 1: 失敗テスト**

`tests/client/Lobby.test.tsx`:
```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { Lobby } from "../../src/client/components/Lobby";

describe("Lobby", () => {
  it("参加者リストとスパイトグル・開始ボタンを表示", () => {
    const send = vi.fn();
    render(
      <Lobby
        myId="p0"
        joined={true}
        isHost={true}
        spyEnabled={false}
        players={[
          { id: "p0", name: "A", handSize: 0, connected: true, ready: false, isHost: true },
          { id: "p1", name: "B", handSize: 0, connected: true, ready: false, isHost: false },
          { id: "p2", name: "C", handSize: 0, connected: true, ready: false, isHost: false },
        ]}
        roomCode="ABCD"
        canStart={false}
        onJoin={() => {}}
        send={send}
      />
    );
    expect(screen.getByText("ABCD")).toBeInTheDocument();
    expect(screen.getByText(/A/)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /開始/ })).toBeDisabled();
  });

  it("ホストがスパイトグルを切替可能", async () => {
    const send = vi.fn();
    const user = userEvent.setup();
    render(
      <Lobby myId="p0" joined={true} isHost={true} spyEnabled={false} players={[]} roomCode="X" canStart={true} onJoin={() => {}} send={send} />
    );
    await user.click(screen.getByRole("switch", { name: /スパイ/ }));
    expect(send).toHaveBeenCalledWith({ type: "setSpy", enabled: true });
  });

  it("未参加なら名前とルームコードで join できる", async () => {
    const onJoin = vi.fn();
    const user = userEvent.setup();
    render(
      <Lobby myId="" joined={false} isHost={false} spyEnabled={false} players={[]} roomCode="----" canStart={false} onJoin={onJoin} send={() => {}} />
    );
    await user.type(screen.getByLabelText("名前"), "Alice");
    await user.type(screen.getByLabelText("ルームコード"), "abcd");
    await user.click(screen.getByRole("button", { name: /参加/ }));
    expect(onJoin).toHaveBeenCalledWith("Alice", "abcd");
  });
});
```

- [ ] **Step 2: テストが失敗を確認**

Run: `npx vitest run tests/client/Lobby.test.tsx`
Expected: FAIL

- [ ] **Step 3: 実装 `src/client/components/Lobby.tsx`**

```tsx
import { useState } from "react";
import type { ClientMessage, PlayerViewPublicPlayer } from "../../shared/types";

interface Props {
  myId: string;
  joined: boolean;
  isHost: boolean;
  spyEnabled: boolean;
  players: PlayerViewPublicPlayer[];
  roomCode: string;
  canStart: boolean;
  onJoin: (name: string, roomCode: string) => void;
  send: (msg: ClientMessage) => void;
}

export function Lobby({ myId, joined, isHost, spyEnabled, players, roomCode, canStart, onJoin, send }: Props) {
  const [name, setName] = useState("");
  const [joinRoomCode, setJoinRoomCode] = useState("");

  if (!joined) {
    return (
      <section className="lobby">
        <h1>タイムボム Online</h1>
        <form
          className="join-form"
          onSubmit={(e) => {
            e.preventDefault();
            onJoin(name, joinRoomCode);
          }}
        >
          <label>
            名前
            <input aria-label="名前" value={name} onChange={(e) => setName(e.target.value)} />
          </label>
          <label>
            ルームコード
            <input aria-label="ルームコード" value={joinRoomCode} onChange={(e) => setJoinRoomCode(e.target.value)} />
          </label>
          <button disabled={!name.trim() || !joinRoomCode.trim()}>参加</button>
        </form>
      </section>
    );
  }

  return (
    <section className="lobby">
      <h1>タイムボム Online</h1>
      <p>ルームコード: <strong data-testid="room-code">{roomCode}</strong>（これを仲間に共有）</p>

      <h2>参加者 ({players.length}/6)</h2>
      <ul>
        {players.map((p) => (
          <li key={p.id}>
            {p.name}{p.id === myId ? "（あなた）" : ""}{p.isHost ? " ★ホスト" : ""}
            {!p.connected ? "（切断）" : ""}
          </li>
        ))}
      </ul>

      <label className="spy-toggle">
        <input
          type="checkbox"
          role="switch"
          aria-label="スパイ(第三陣営)を有効化"
          checked={spyEnabled}
          disabled={!isHost}
          onChange={(e) => send({ type: "setSpy", enabled: e.target.checked })}
        />
        スパイ（第三陣営）あり
      </label>

      <button disabled={!isHost || !canStart} onClick={() => send({ type: "start" })}>
        開始
      </button>
      {!canStart && <p className="hint">4〜6人で開始できます</p>}
    </section>
  );
}
```

- [ ] **Step 4: テストが通るを確認**

Run: `npx vitest run tests/client/Lobby.test.tsx`
Expected: PASS

- [ ] **Step 5: コミット**

```bash
git add src/client/components/Lobby.tsx tests/client/Lobby.test.tsx
git commit -m "feat(client): Lobby component with tests"
```

---

## Task 14: コンポーネント `RoleReveal`（TDD）

**Files:**
- Create: `src/client/components/RoleReveal.tsx`, `tests/client/RoleReveal.test.tsx`

- [ ] **Step 1: 失敗テスト**

`tests/client/RoleReveal.test.tsx`:
```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { RoleReveal } from "../../src/client/components/RoleReveal";

describe("RoleReveal", () => {
  it("自分の役職を表示し、確認で ready を送信", async () => {
    const send = vi.fn();
    const user = userEvent.setup();
    render(<RoleReveal role="bomber" ready={false} send={send} />);
    expect(screen.getByText(/ボマー/)).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: /確認/ }));
    expect(send).toHaveBeenCalledWith({ type: "ready" });
  });

  it("ready 済みならボタン無効", () => {
    render(<RoleReveal role="police" ready={true} send={() => {}} />);
    expect(screen.getByRole("button", { name: /確認/ })).toBeDisabled();
  });
});
```

- [ ] **Step 2: 失敗を確認**

Run: `npx vitest run tests/client/RoleReveal.test.tsx`
Expected: FAIL

- [ ] **Step 3: 実装 `src/client/components/RoleReveal.tsx`**

```tsx
import type { ClientMessage, Role } from "../../shared/types";

const ROLE_LABEL: Record<Role, string> = {
  police: "時空警察",
  bomber: "ボマー",
  spy: "スパイ",
};

interface Props {
  role: Role;
  ready: boolean;
  send: (msg: ClientMessage) => void;
}

export function RoleReveal({ role, ready, send }: Props) {
  return (
    <section className="role-reveal">
      <h2>あなたの役職</h2>
      <p className={`role role-${role}`}>{ROLE_LABEL[role]}</p>
      <button disabled={ready} onClick={() => send({ type: "ready" })}>
        確認した
      </button>
    </section>
  );
}
```

- [ ] **Step 4: 通るを確認**

Run: `npx vitest run tests/client/RoleReveal.test.tsx`
Expected: PASS

- [ ] **Step 5: コミット**

```bash
git add src/client/components/RoleReveal.tsx tests/client/RoleReveal.test.tsx
git commit -m "feat(client): RoleReveal overlay with tests"
```

---

## Task 15: コンポーネント `GameTable`（グリッド）（TDD）

**Files:**
- Create: `src/client/components/PlayerPanel.tsx`, `src/client/components/GameTable.tsx`, `tests/client/GameTable.test.tsx`

- [ ] **Step 1: 失敗テスト**

`tests/client/GameTable.test.tsx`:
```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { GameTable } from "../../src/client/components/GameTable";
import type { PlayerView } from "../../src/shared/types";

const baseView: PlayerView = {
  phase: "round_play",
  round: 1,
  spyEnabled: false,
  myPlayerId: "p0",
  myRole: "police",
  myHand: ["defuse", "silence"],
  currentCutterId: "p0",
  defuseChipsFlipped: 1,
  defuseChipsTotal: 4,
  cutsThisRound: 1,
  cutsPerRound: 4,
  players: [
    { id: "p0", name: "Me", handSize: 2, connected: true, ready: true, isHost: true },
    { id: "p1", name: "Other", handSize: 5, connected: true, ready: true, isHost: false },
  ],
  lastCut: null,
  winners: null,
  revealedRoles: null,
};

describe("GameTable", () => {
  it("自分のパネルはオモテ表示、他人は裏向き（枚数のみ）", () => {
    render(<GameTable view={baseView} send={() => {}} />);
    expect(screen.getByText("解除")).toBeInTheDocument(); // 自分の手札オモテ
    expect(screen.getAllByLabelText(/手札/).length).toBeGreaterThan(0);
  });

  it("手番時に他人のカードをクリックで cut 送信", async () => {
    const send = vi.fn();
    const user = userEvent.setup();
    render(<GameTable view={baseView} send={send} />);
    const cards = screen.getAllByRole("button", { name: /カード/ });
    await user.click(cards[0]);
    expect(send).toHaveBeenCalledWith(expect.objectContaining({ type: "cut", targetId: "p1" }));
  });

  it("手番でないとクリックできない", () => {
    const notMine = { ...baseView, currentCutterId: "p1" };
    render(<GameTable view={notMine} send={() => {}} />);
    expect(screen.queryByRole("button", { name: /カード/ })).toBeNull();
  });
});
```

- [ ] **Step 2: 失敗を確認**

Run: `npx vitest run tests/client/GameTable.test.tsx`
Expected: FAIL

- [ ] **Step 3: `PlayerPanel.tsx` 実装**

```tsx
import type { CardType, PlayerViewPublicPlayer } from "../../shared/types";

const CARD_LABEL: Record<CardType, string> = {
  defuse: "解除",
  bomb: "ボム",
  silence: "しーん",
};

interface Props {
  player: PlayerViewPublicPlayer;
  isMe: boolean;
  isCurrent: boolean;
  myHand: CardType[] | null;
  canCut: boolean;
  onCut: (cardIndex: number) => void;
}

export function PlayerPanel({ player, isMe, isCurrent, myHand, canCut, onCut }: Props) {
  return (
    <div
      className={[
        "panel",
        isMe ? "panel-me" : "",
        isCurrent ? "panel-current" : "",
        !player.connected ? "panel-disconnected" : "",
      ].join(" ")}
    >
      <header>
        <span className="name">{player.name}{isMe ? "（あなた）" : ""}{player.isHost ? " ★" : ""}</span>
        <span className="status">{!player.connected ? "切断" : ""}</span>
      </header>
      <div className="hand" aria-label="手札">
        {isMe && myHand
          ? myHand.map((c, i) => (
              <span key={i} className={`card card-${c}`}>{CARD_LABEL[c]}</span>
            ))
          : Array.from({ length: player.handSize }, (_, i) =>
              canCut ? (
                <button key={i} aria-label={`カード${i}`} className="card card-back" onClick={() => onCut(i)} />
              ) : (
                <span key={i} className="card card-back" />
              )
            )}
      </div>
    </div>
  );
}
```

- [ ] **Step 4: `GameTable.tsx` 実装**

```tsx
import type { ClientMessage, PlayerView } from "../../shared/types";
import { PlayerPanel } from "./PlayerPanel";

interface Props {
  view: PlayerView;
  send: (msg: ClientMessage) => void;
}

export function GameTable({ view, send }: Props) {
  const isMyTurn = view.currentCutterId === view.myPlayerId;
  return (
    <section className="game-table">
      <div className="hud">
        <span>R{view.round}/4</span>
        <span>解除 {view.defuseChipsFlipped}/{view.defuseChipsTotal}</span>
        <span>カット {view.cutsThisRound}/{view.cutsPerRound}</span>
        <span>{isMyTurn ? "あなたの手番" : "他プレイヤーの手番"}</span>
      </div>
      <div className="last-cut">
        {view.lastCut && (
          <span>直前: {CARD_LABEL(view.lastCut.revealedType)}</span>
        )}
      </div>
      <div className="grid">
        {view.players.map((p) => (
          <PlayerPanel
            key={p.id}
            player={p}
            isMe={p.id === view.myPlayerId}
            isCurrent={p.id === view.currentCutterId}
            myHand={p.id === view.myPlayerId ? view.myHand : null}
            canCut={isMyTurn && p.id !== view.myPlayerId}
            onCut={(cardIndex) => send({ type: "cut", targetId: p.id, cardIndex })}
          />
        ))}
      </div>
    </section>
  );
}

function CARD_LABEL(c: string): string {
  return c === "defuse" ? "解除" : c === "bomb" ? "ボム" : "しーん";
}
```

- [ ] **Step 5: 通るを確認**

Run: `npx vitest run tests/client/GameTable.test.tsx`
Expected: PASS

- [ ] **Step 6: コミット**

```bash
git add src/client/components/PlayerPanel.tsx src/client/components/GameTable.tsx tests/client/GameTable.test.tsx
git commit -m "feat(client): grid GameTable and PlayerPanel with tests"
```

---

## Task 16: コンポーネント `GameOver` と `App` 配線・スタイル（TDD）

**Files:**
- Create: `src/client/components/GameOver.tsx`, `tests/client/GameOver.test.tsx`
- Create: `src/client/App.tsx`, `src/client/main.tsx`, `src/client/styles.css`, `tests/client/App.test.tsx`

- [ ] **Step 1: GameOver 失敗テスト**

`tests/client/GameOver.test.tsx`:
```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { GameOver } from "../../src/client/components/GameOver";

describe("GameOver", () => {
  it("勝者陣営と公開役職を表示し、ホストがリマッチ可", async () => {
    const send = vi.fn();
    const user = userEvent.setup();
    render(
      <GameOver
        winners={["bomber"]}
        revealedRoles={[{ playerId: "p0", role: "bomber" }, { playerId: "p1", role: "police" }]}
        players={[
          { id: "p0", name: "A", handSize: 0, connected: true, ready: true, isHost: true },
          { id: "p1", name: "B", handSize: 0, connected: true, ready: true, isHost: false },
        ]}
        isHost={true}
        send={send}
      />
    );
    expect(screen.getByText(/ボマー/)).toBeInTheDocument();
    expect(screen.getByText(/A: ボマー/)).toBeInTheDocument();
    expect(screen.getByText(/B: 時空警察/)).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: /もう一度/ }));
    expect(send).toHaveBeenCalledWith({ type: "restart" });
  });
});
```

- [ ] **Step 2: GameOver 実装**

`src/client/components/GameOver.tsx`:
```tsx
import type { ClientMessage, PlayerViewPublicPlayer, RevealedRole, Role } from "../../shared/types";

const WINNER_LABEL: Record<Role, string> = {
  police: "時空警察",
  bomber: "ボマー",
  spy: "スパイ",
};

interface Props {
  winners: Role[];
  revealedRoles: RevealedRole[];
  players: PlayerViewPublicPlayer[];
  isHost: boolean;
  send: (msg: ClientMessage) => void;
}

export function GameOver({ winners, revealedRoles, players, isHost, send }: Props) {
  const nameById = new Map(players.map((p) => [p.id, p.name]));
  return (
    <section className="game-over">
      <h2>勝者: {winners.map((w) => WINNER_LABEL[w]).join(" / ")}</h2>
      <ul>
        {revealedRoles.map((r) => (
          <li key={r.playerId}>{nameById.get(r.playerId) ?? r.playerId}: {WINNER_LABEL[r.role]}</li>
        ))}
      </ul>
      {isHost && <button onClick={() => send({ type: "restart" })}>もう一度</button>}
    </section>
  );
}
```

- [ ] **Step 3: 通るを確認＆コミット**

Run: `npx vitest run tests/client/GameOver.test.tsx` → PASS
```bash
git add src/client/components/GameOver.tsx tests/client/GameOver.test.tsx
git commit -m "feat(client): GameOver component with tests"
```

- [ ] **Step 4: App 配線の失敗テスト**

`tests/client/App.test.tsx`:
```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { App } from "../../src/client/App";
import type { PlayerView } from "../../src/shared/types";

const socketState = vi.hoisted(() => ({
  current: {
    view: null as PlayerView | null,
    playerId: null as string | null,
    roomCode: null as string | null,
    isHost: false,
    error: null as string | null,
    join: vi.fn(),
    send: vi.fn(),
  },
}));

vi.mock("../../src/client/useSocket", () => ({
  useSocket: () => socketState.current,
}));

function lobbyView(myPlayerId: string, isHost: boolean): PlayerView {
  return {
    phase: "lobby",
    round: 0,
    spyEnabled: false,
    myPlayerId,
    myRole: null,
    myHand: [],
    currentCutterId: null,
    defuseChipsFlipped: 0,
    defuseChipsTotal: 4,
    cutsThisRound: 0,
    cutsPerRound: 4,
    players: Array.from({ length: 4 }, (_, i) => ({
      id: `p${i}`,
      name: `P${i}`,
      handSize: 0,
      connected: true,
      ready: false,
      isHost: isHost && i === 1,
    })),
    lastCut: null,
    winners: null,
    revealedRoles: null,
  };
}

describe("App", () => {
  beforeEach(() => {
    socketState.current.view = null;
    socketState.current.playerId = null;
    socketState.current.roomCode = null;
    socketState.current.isHost = false;
    socketState.current.join.mockClear();
    socketState.current.send.mockClear();
  });

  it("未参加なら join フォームから useSocket.join を呼ぶ", async () => {
    const user = userEvent.setup();
    render(<App />);
    await user.type(screen.getByLabelText("名前"), "Alice");
    await user.type(screen.getByLabelText("ルームコード"), "abcd");
    await user.click(screen.getByRole("button", { name: /参加/ }));
    expect(socketState.current.join).toHaveBeenCalledWith("Alice", "abcd");
  });

  it("ホスト状態は joined 応答ではなく最新 view.players から導出する", () => {
    socketState.current.playerId = "p1";
    socketState.current.roomCode = "ABCD";
    socketState.current.isHost = false;
    socketState.current.view = lobbyView("p1", true);
    render(<App />);
    expect(screen.getByRole("button", { name: /開始/ })).toBeEnabled();
  });
});
```

Run: `npx vitest run tests/client/App.test.tsx`
Expected: FAIL（App 未実装）

- [ ] **Step 5: `App.tsx` 配線**

`src/client/App.tsx`:
```tsx
import { GameOver } from "./components/GameOver";
import { GameTable } from "./components/GameTable";
import { Lobby } from "./components/Lobby";
import { RoleReveal } from "./components/RoleReveal";
import { useSocket } from "./useSocket";

export function App() {
  const sock = useSocket();
  const v = sock.view;
  const me = v?.players.find((p) => p.id === v.myPlayerId);
  const isHost = me?.isHost ?? sock.isHost;

  if (!sock.playerId || !v || v.phase === "lobby") {
    return (
      <Lobby
        myId={sock.playerId ?? ""}
        joined={Boolean(sock.playerId)}
        isHost={isHost}
        spyEnabled={v?.spyEnabled ?? false}
        players={v?.players ?? []}
        roomCode={sock.roomCode ?? "----"}
        canStart={(v?.players.length ?? 0) >= 4 && (v?.players.length ?? 0) <= 6}
        onJoin={sock.join}
        send={sock.send}
      />
    );
  }

  if (v.phase === "role_reveal") {
    return <RoleReveal role={v.myRole ?? "police"} ready={v.players.find((p) => p.id === v.myPlayerId)?.ready ?? false} send={sock.send} />;
  }

  if (v.phase === "game_end") {
    return <GameOver winners={v.winners ?? []} revealedRoles={v.revealedRoles ?? []} players={v.players} isHost={isHost} send={sock.send} />;
  }

  return <GameTable view={v} send={sock.send} />;
}
```

- [ ] **Step 6: 追加フィールドとホスト導出を確認**

`PlayerView.spyEnabled` と `PlayerView.revealedRoles` は Task 2/7 で追加済み。
`tests/client/*` の `baseView` 等に `spyEnabled: false` と `revealedRoles: null` を補完。
`App.tsx` は `v.players` から自分の `isHost` を導出し、ホスト委譲後も表示が追従すること。

- [ ] **Step 7: `main.tsx` と `styles.css`**

`src/client/main.tsx`:
```tsx
import React from "react";
import { createRoot } from "react-dom/client";
import { App } from "./App";
import "./styles.css";

createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
```

`src/client/styles.css`（最小）:
```css
:root { color-scheme: dark; }
* { box-sizing: border-box; }
body { margin: 0; font-family: system-ui, sans-serif; background: #15152a; color: #eee; }
.lobby, .role-reveal, .game-over { max-width: 640px; margin: 2rem auto; padding: 1rem; }
.join-form { display: grid; gap: .75rem; }
.join-form label { display: grid; gap: .25rem; }
.join-form input { padding: .5rem; border-radius: 6px; border: 1px solid #555580; background: #22223a; color: #eee; }
.hud { display: flex; gap: 1rem; justify-content: center; padding: .5rem; background: #2a2a4a; }
.grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: .75rem; padding: 1rem; }
.panel { background: #22223a; border-radius: 8px; padding: .5rem; border: 2px solid transparent; }
.panel-me { border-color: #7ee08a; }
.panel-current { border-color: #ffd166; }
.panel-disconnected { opacity: .5; }
.hand { display: flex; gap: 4px; flex-wrap: wrap; margin-top: .25rem; }
.card { display: inline-block; width: 34px; height: 46px; border-radius: 5px; line-height: 46px; text-align: center; font-size: 11px; }
.card-defuse { background: #2e7d4f; }
.card-bomb { background: #b23b3b; }
.card-silence { background: #6b6b80; }
.card-back { background: #3a3a5e; border: 1px solid #555580; }
button.card-back { cursor: pointer; }
.hint { color: #9a9ac0; font-size: 12px; }
```

- [ ] **Step 8: 型検証と全テスト**

Run: `npm run typecheck`
Run: `npm test`
Expected: 両方成功

- [ ] **Step 9: コミット**

```bash
git add src/client/App.tsx src/client/main.tsx src/client/styles.css src/shared/types.ts src/shared/engine.ts tests/client
git commit -m "feat(client): wire App phases and add minimal styles"
```

---

## Task 17: Render デプロイと最終検証

**Files:**
- Create: `render.yaml`

- [ ] **Step 1: `render.yaml` 作成**

```yaml
services:
  - type: web
    name: timebomb-online
    runtime: node
    plan: free
    buildCommand: npm ci && npm run render-build
    startCommand: npm run start
    healthCheckPath: /healthz
    envVars:
      - key: NODE_VERSION
        value: 22
```

- [ ] **Step 2: ローカルで production ビルドを検証**

Run: `npm run build`
Expected: `dist/`（クライアント）と `dist-server/`（サーバー）が生成される
Run: `npm start`（別ターミナルで `curl http://localhost:3000/healthz`）
Expected: `{"ok":true}`

- [ ] **Step 3: ローカル手動プレイ煙テスト**

Run: `npm run dev`
Expected: クライアント http://localhost:5173 ・サーバー :3000
手順:
1. ブラウザ2タブを開く。それぞれルームコード `TEST` で異なる名前で join（4人分は同じブラウザで4タブ、または dev なので同一ブラウザ複数タブ可）
2. ホストがスパイON→開始
3. 全員「確認」で round_play へ
4. 手番者が他人カードをクリックして cut → 全タブに lastCut が反映される
5. ボムを引くか4R回すと GameOver

- [ ] **Step 4: コミット**

```bash
git add render.yaml
git commit -m "chore: add render deployment config"
```

---

## 完了条件（Definition of Done）

- [ ] `npm run typecheck` が成功
- [ ] `npm test` が全件緑（engine / view / room / client コンポーネント）
- [ ] view フィルタテストが「他人の手札オモテ・役職が一切含まれない」ことを表明的アサート
- [ ] ローカル `npm run dev` で4〜6人が1ゲーム完走できる
- [ ] `npm run build && npm start` で `/healthz` が 200 を返す
- [ ] `render.yaml` が存在し Render にデプロイ可能

---

## 自己レビューメモ（執筆後チェック）

- **Spec coverage**: ルール（Task 3,5,6,7）/ サーバー権威＋フィルタ（Task 7,8）/ join導線（Task 12,13,16）/ グリッドUI（Task 15）/ リコネクト＋重複排除（Task 8,10,12）/ 3陣営勝敗（Task 6）/ 終了時全役職公開（Task 7,16）/ Render（Task 17）→ v1 成功基準を網羅。
- **Type 一貫性**: `PlayerView.spyEnabled` と `PlayerView.revealedRoles` は Task 2 で定義し、Task 7 の `toPlayerView`、Task 15/16 のクライアントテスト、Task 16 の `App`/`GameOver` で一貫して参照する。
- **実装時メモ**: 6人時ボマー2枚は `config.ts` テーブル固定（プレイ後に調整）。E2E(playwright) は本計画では手動煙テスト（Task 17 Step 3）で代替、自動化は後続バックログ。
