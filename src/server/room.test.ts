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

function readyAll(r: Room, ids: string[]): void {
  ids.forEach((id) => r.handleCommand(id, { type: "ready" }));
  r.state.players.forEach((p) => {
    p.hand = ["silence", "silence", "silence", "silence", "silence"];
  });
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
    readyAll(r, ids);
    const cutter = r.state.currentCutterId!;
    const target = r.state.players.find((p) => p.id !== cutter)!;
    r.handleCommand(cutter, { type: "cut", targetId: target.id, cardIndex: 0 });
    expect(r.state.lastCut).not.toBeNull();
    expect(r.state.lastCut!.targetId).toBe(target.id);
  });

  it("ラウンド最後の cut 直後は次Rへ進まず round_end の公開状態を返す", () => {
    const r = new Room("ABCD", identityShuffle);
    const ids = fillRoom(r, 4);
    r.handleCommand(ids[0], { type: "start" });
    readyAll(r, ids);
    for (let i = 0; i < 4; i++) {
      const cutter = r.state.currentCutterId!;
      const target = r.state.players.find((p) => p.id !== cutter && p.hand.length > 0)!;
      r.handleCommand(cutter, { type: "cut", targetId: target.id, cardIndex: 0 });
    }
    expect(r.state.phase).toBe("round_end");
    expect(r.state.round).toBe(1);
    expect(r.state.cutsThisRound).toBe(4);
    expect(r.state.revealedCards).toHaveLength(4);
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
    readyAll(r, ids);
    for (const id of ids) {
      const snap = r.snapshotFor(id);
      expect(snap.myPlayerId).toBe(id);
      expect(snap.players.every((p) => !("hand" in p))).toBe(true);
      expect(snap.players.every((p) => !("role" in p))).toBe(true);
      expect(snap.revealedRoles).toBeNull();
      expect(JSON.stringify(snap.players)).not.toContain('"hand"');
    }
  });
});
