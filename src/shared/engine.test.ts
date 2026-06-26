import { describe, expect, it } from "vitest";
import { defuseChipsTotal, handDistributionFor } from "./config.js";
import {
  advanceRound,
  allReady,
  applyCut,
  assignRoles,
  beginPlay,
  buildDeck,
  canStart,
  createInitialState,
  dealRound,
  pickRandomFirstCutter,
  restart,
  startGame
} from "./engine.js";
import { identityShuffle } from "./random.js";
import type { Player } from "./types.js";

function makePlayers(n: number): Player[] {
  return Array.from({ length: n }, (_, i) => ({
    id: `p${i}`,
    name: `P${i}`,
    role: null,
    hand: [],
    connected: true,
    ready: false,
    isHost: i === 0
  }));
}

function setup4() {
  const s = createInitialState();
  s.players = makePlayers(4);
  startGame(s, identityShuffle);
  beginPlay(s, identityShuffle);
  s.players.forEach((p) => {
    p.hand = ["silence", "silence", "silence", "silence", "silence"];
  });
  return s;
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
    players.forEach((p) => {
      counts[p.role as "bomber" | "police" | "spy"]++;
    });
    expect(counts).toEqual({ bomber: 1, police: 2, spy: 1 });
  });

  it("pickRandomFirstCutter は誰か一人のidを返す", () => {
    const players = makePlayers(4);
    const id = pickRandomFirstCutter(players, identityShuffle);
    expect(players.some((p) => p.id === id)).toBe(true);
  });
});

describe("lifecycle", () => {
  it("canStart は 4〜6人 lobby のみ true", () => {
    const s = createInitialState();
    s.players = makePlayers(3);
    expect(canStart(s)).toBe(false);
    s.players = makePlayers(4);
    expect(canStart(s)).toBe(true);
    s.phase = "role_reveal";
    expect(canStart(s)).toBe(false);
  });

  it("startGame は役職を配り role_reveal へ", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    expect(s.phase).toBe("role_reveal");
    expect(s.round).toBe(1);
    expect(s.players.every((p) => p.role)).toBe(true);
    expect(s.players.every((p) => p.hand.length === 5)).toBe(true);
    expect(s.currentCutterId).not.toBeNull();
  });

  it("allReady は全員 ready のみ true", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    expect(allReady(s)).toBe(false);
    s.players.forEach((p) => {
      p.ready = true;
    });
    expect(allReady(s)).toBe(true);
  });

  it("beginPlay は配布して round_play へ", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    beginPlay(s, identityShuffle);
    expect(s.phase).toBe("round_play");
    expect(s.players.every((p) => p.hand.length === 5)).toBe(true);
  });

  it("beginPlay は確認後に各プレイヤーの手札配置をシャッフルする", () => {
    const s = createInitialState();
    s.players = makePlayers(4);
    startGame(s, identityShuffle);
    s.players[0].hand = ["defuse", "bomb", "silence", "defuse", "silence"];
    s.players[1].hand = ["silence", "defuse", "silence", "bomb", "silence"];
    s.players[2].hand = ["bomb", "silence", "defuse", "silence", "defuse"];
    s.players[3].hand = ["silence", "bomb", "defuse", "silence", "defuse"];
    const beforeHands = s.players.map((player) => [...player.hand]);

    beginPlay(s, (items) => [...items].reverse());

    expect(s.phase).toBe("round_play");
    s.players.forEach((player, index) => {
      expect(player.hand).toEqual([...beforeHands[index]].reverse());
      expect([...player.hand].sort()).toEqual([...beforeHands[index]].sort());
    });
  });
});

describe("applyCut", () => {
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
    const before = s.cutsThisRound;
    const r = applyCut(s, cutter, { targetId: target.id, cardIndex: 0 });
    expect(r.kind).toBe("ok");
    expect(s.currentCutterId).toBe(target.id);
    expect(s.cutsThisRound).toBe(before + 1);
  });

  it("bomb カットで即ボマー勝ち", () => {
    const s = setup4();
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
    s.defuseChipsFlipped = defuseChipsTotal(4) - 1;
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
    for (let i = 0; i < 4; i++) {
      const cutter = s.currentCutterId!;
      const target = s.players.find((p) => p.id !== cutter && p.hand.length > 0)!;
      const r = applyCut(s, cutter, { targetId: target.id, cardIndex: 0 });
      expect(r.kind).toBe("ok");
    }
    expect(s.phase).toBe("round_end");
  });
});

describe("advanceRound / restart", () => {
  it("round_end から次Rの確認フェイズへ。ターン継承で先手は前R最終被カット者", () => {
    const s = setup4();
    for (let i = 0; i < 4; i++) {
      const cutter = s.currentCutterId!;
      const target = s.players.find((p) => p.id !== cutter && p.hand.length > 0)!;
      applyCut(s, cutter, { targetId: target.id, cardIndex: 0 });
    }
    expect(s.phase).toBe("round_end");
    expect(s.revealedCards).toHaveLength(4);
    const lastCutTarget = s.lastCut!.targetId;
    advanceRound(s, identityShuffle);
    expect(s.phase).toBe("role_reveal");
    expect(s.round).toBe(2);
    expect(s.currentCutterId).toBe(lastCutTarget);
    expect(s.players.every((p) => p.hand.length === 4)).toBe(true);
    expect(s.players.every((p) => !p.ready)).toBe(true);
    expect(s.revealedCards).toEqual([]);
  });

  it("次ラウンドの配布では公開済みカードを山に戻さない", () => {
    const s = setup4();
    for (let i = 0; i < 4; i++) {
      const cutter = s.currentCutterId!;
      const target = s.players.find((p) => p.id !== cutter && p.hand.length > 0)!;
      applyCut(s, cutter, { targetId: target.id, cardIndex: 0 });
    }
    expect(s.phase).toBe("round_end");
    expect(s.revealedCards.map((card) => card.type)).toEqual([
      "silence",
      "silence",
      "silence",
      "silence"
    ]);

    advanceRound(s, identityShuffle);

    const nextRoundCards = s.players.flatMap((player) => player.hand);
    expect(nextRoundCards).toHaveLength(16);
    expect(s.players.every((player) => player.hand.length === 4)).toBe(true);
    expect(nextRoundCards.filter((card) => card === "silence")).toHaveLength(11);
    expect(nextRoundCards.filter((card) => card === "defuse")).toHaveLength(4);
    expect(nextRoundCards.filter((card) => card === "bomb")).toHaveLength(1);
  });
  it("R4 終了で未決着: spyありなら spy 勝ち", () => {
    const s = setup4();
    s.spyEnabled = true;
    s.round = 4;
    s.phase = "round_end";
    advanceRound(s, identityShuffle);
    expect(s.winners).toEqual(["spy"]);
  });

  it("R4 終了で未決着: spyなしなら bomber 勝ち", () => {
    const s = setup4();
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
