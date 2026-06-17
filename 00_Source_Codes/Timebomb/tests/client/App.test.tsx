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

  it("ホスト状態は joined 応答ではなく最新 view.players から導出する", () => {
    socketState.current.playerId = "p1";
    socketState.current.roomCode = "ABCD";
    socketState.current.isHost = false;
    socketState.current.view = lobbyView("p1", true);
    render(<App />);
    expect(screen.getByRole("button", { name: /開始/ })).toBeEnabled();
  });
});
