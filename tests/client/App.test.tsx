import { act, render, screen } from "@testing-library/react";
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
    send: vi.fn()
  }
}));

vi.mock("../../src/client/useSocket", () => ({
  useSocket: () => socketState.current
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
      isHost: isHost && i === 1
    })),
    lastCut: null,
    revealedCards: [],
  winners: null,
    revealedRoles: null
  };
}

describe("App", () => {
  beforeEach(() => {
    socketState.current.view = null;
    socketState.current.playerId = null;
    socketState.current.roomCode = null;
    socketState.current.isHost = false;
    socketState.current.error = null;
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

  it("カットでゲーム終了した場合は公開演出の後に結果画面を表示する", () => {
    vi.useFakeTimers();
    try {
      socketState.current.playerId = "p0";
      socketState.current.roomCode = "ABCD";
      socketState.current.view = {
        ...lobbyView("p0", true),
        phase: "game_end",
        round: 1,
        currentCutterId: "p0",
        lastCut: { cutterId: "p0", targetId: "p1", cardIndex: 0, revealedType: "bomb" },
        revealedCards: [{ playerId: "p1", cardIndex: 0, type: "bomb" }],
        winners: ["bomber"],
        revealedRoles: [{ playerId: "p0", role: "police" }]
      };
      render(<App />);
      expect(screen.getByTestId("reveal-cinema")).toHaveClass("reveal-cinema-bomb");
      expect(screen.queryByText(/勝者:/)).toBeNull();

      act(() => {
        vi.advanceTimersByTime(6200);
      });
      expect(screen.getByText(/勝者: ボマー/)).toBeInTheDocument();
    } finally {
      vi.useRealTimers();
    }
  });
  it("round_end の公開演出後に次ラウンド進行を送る", () => {
    vi.useFakeTimers();
    try {
      socketState.current.playerId = "p0";
      socketState.current.roomCode = "ABCD";
      socketState.current.view = {
        ...lobbyView("p0", true),
        phase: "round_end",
        round: 1,
        currentCutterId: "p1",
        cutsThisRound: 4,
        lastCut: { cutterId: "p0", targetId: "p1", cardIndex: 0, revealedType: "silence" },
        revealedCards: [{ playerId: "p1", cardIndex: 0, type: "silence" }]
      };
      render(<App />);
      expect(socketState.current.send).not.toHaveBeenCalledWith({ type: "advanceRound" });

      act(() => {
        vi.advanceTimersByTime(6199);
      });
      expect(socketState.current.send).not.toHaveBeenCalledWith({ type: "advanceRound" });

      act(() => {
        vi.advanceTimersByTime(1);
      });
      expect(socketState.current.send).toHaveBeenCalledWith({ type: "advanceRound" });
    } finally {
      vi.useRealTimers();
    }
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
