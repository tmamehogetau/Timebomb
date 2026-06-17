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
        revealedRoles={[
          { playerId: "p0", role: "bomber" },
          { playerId: "p1", role: "police" }
        ]}
        players={[
          { id: "p0", name: "A", handSize: 0, connected: true, ready: true, isHost: true },
          { id: "p1", name: "B", handSize: 0, connected: true, ready: true, isHost: false }
        ]}
        isHost={true}
        send={send}
      />
    );
    expect(screen.getByRole("heading", { name: "勝者: ボマー" })).toBeInTheDocument();
    expect(screen.getByText(/A: ボマー/)).toBeInTheDocument();
    expect(screen.getByText(/B: 時空警察/)).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: /もう一度/ }));
    expect(send).toHaveBeenCalledWith({ type: "restart" });
  });
});
