import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { useSocket } from "./useSocket";

class FakeSocket {
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
  });

  it("send で JSON を送信", () => {
    const { result } = renderHook(() => useSocket());
    act(() => {
      result.current.send({ type: "ready" });
    });
    expect(FakeSocket.last!.sent).toContain(JSON.stringify({ type: "ready" }));
  });

  it("join は roomCode を正規化し、保存済み playerId があれば同送する", () => {
    window.localStorage.setItem("timebomb:ABCD:playerId", "saved-player");
    const { result } = renderHook(() => useSocket());
    act(() => {
      result.current.join("Alice", "abcd");
    });
    expect(FakeSocket.last!.sent).toContain(
      JSON.stringify({ type: "join", name: "Alice", roomCode: "ABCD", playerId: "saved-player" })
    );
  });
});
