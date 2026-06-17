import { describe, expect, it } from "vitest";
import { beginPlay, createInitialState, startGame, toPlayerView } from "./engine.js";
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
        expect(typeof o.handSize).toBe("number");
        expect((o as unknown as Record<string, unknown>).hand).toBeUndefined();
        expect((o as unknown as Record<string, unknown>).role).toBeUndefined();
      }
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
