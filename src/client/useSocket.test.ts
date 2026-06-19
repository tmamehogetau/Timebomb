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
  }

  send(data: string) {
    this.sent.push(data);
  }

  close() {
    this.onclose?.();
  }
}

describe("useSocket", () => {
  beforeEach(() => {
    FakeSocket.last = null;
    window.localStorage.clear();
    (globalThis as unknown as { WebSocket: typeof WebSocket }).WebSocket =
      FakeSocket as unknown as typeof WebSocket;
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("接続後にメッセージを受信して state を更新", () => {
    const { result } = renderHook(() => useSocket());
    act(() => {
      FakeSocket.last!.onmessage?.({
        data: JSON.stringify({ type: "joined", playerId: "x", roomCode: "ABCD", isHost: true })
      });
    });
    expect(result.current.playerId).toBe("x");
    expect(window.localStorage.getItem("timebomb:ABCD:playerId")).toBe("x");
    expect(window.localStorage.getItem("timebomb:ABCD:playerName")).toBeNull();
  });

  it("send で JSON を送信", () => {
    const { result } = renderHook(() => useSocket());
    act(() => {
      result.current.send({ type: "ready" });
    });
    expect(FakeSocket.last!.sent).toContain(JSON.stringify({ type: "ready" }));
  });

  it("join は roomCode を正規化し、保存済み playerId と同じ名前なら同送する", () => {
    window.localStorage.setItem("timebomb:ABCD:playerId", "saved-player");
    window.localStorage.setItem("timebomb:ABCD:playerName", "Alice");
    const { result } = renderHook(() => useSocket());
    act(() => {
      result.current.join("Alice", "abcd");
    });
    expect(FakeSocket.last!.sent).toContain(
      JSON.stringify({ type: "join", name: "Alice", roomCode: "ABCD", playerId: "saved-player" })
    );
  });

  it("保存済み playerId があれば名前を再入力しても同じ席へ再入室する", () => {
    window.localStorage.setItem("timebomb:ABCD:playerId", "saved-player");
    window.localStorage.setItem("timebomb:ABCD:playerName", "Alice");
    const { result } = renderHook(() => useSocket());
    act(() => {
      result.current.join("Bob", "abcd");
    });
    expect(FakeSocket.last!.sent).toContain(
      JSON.stringify({ type: "join", name: "Bob", roomCode: "ABCD", playerId: "saved-player" })
    );
  });

  it("joined 後の state 受信時に自分の名前を保存する", () => {
    const { result } = renderHook(() => useSocket());
    act(() => {
      FakeSocket.last!.onmessage?.({
        data: JSON.stringify({ type: "joined", playerId: "p1", roomCode: "ABCD", isHost: false })
      });
    });
    act(() => {
      FakeSocket.last!.onmessage?.({
        data: JSON.stringify({
          type: "state",
          view: {
            phase: "lobby",
            round: 0,
            spyEnabled: false,
            myPlayerId: "p1",
            myRole: null,
            myHand: [],
            currentCutterId: null,
            defuseChipsFlipped: 0,
            defuseChipsTotal: 4,
            cutsThisRound: 0,
            cutsPerRound: 4,
            players: [
              { id: "p0", name: "Alice", handSize: 0, connected: true, ready: false, isHost: true },
              { id: "p1", name: "Bob", handSize: 0, connected: true, ready: false, isHost: false }
            ],
            lastCut: null,
            winners: null,
            revealedRoles: null
          }
        })
      });
    });
    expect(result.current.playerId).toBe("p1");
    expect(window.localStorage.getItem("timebomb:ABCD:playerName")).toBe("Bob");
  });
  it("保存済みの参加情報があっても接続時に自動再参加しない", () => {
    window.localStorage.setItem("timebomb:lastRoomCode", "ABCD");
    window.localStorage.setItem("timebomb:ABCD:playerId", "saved-player");
    window.localStorage.setItem("timebomb:ABCD:playerName", "Alice");
    renderHook(() => useSocket());

    act(() => {
      FakeSocket.last!.onopen?.();
    });

    expect(FakeSocket.last!.sent).toEqual([]);
  });
});
