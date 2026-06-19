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
    { id: "p1", name: "Other", handSize: 5, connected: true, ready: true, isHost: false }
  ],
  lastCut: null,
  winners: null,
  revealedRoles: null
};

describe("GameTable", () => {
  it("自分のパネルはオモテ表示、他人は裏向き（枚数のみ）", () => {
    render(<GameTable view={baseView} send={() => {}} />);
    expect(screen.getByText("解除")).toBeInTheDocument();
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

  it("プレイヤーエリアはカード表示を広く取るグリッドで表示する", () => {
    render(<GameTable view={baseView} send={() => {}} />);
    expect(screen.getByTestId("player-grid")).toHaveClass("player-grid-expanded");
  });

  it("直前のカット結果を演出バナーとして表示する", () => {
    const withCut: PlayerView = {
      ...baseView,
      lastCut: {
        cutterId: "p0",
        targetId: "p1",
        cardIndex: 0,
        revealedType: "bomb"
      }
    };
    render(<GameTable view={withCut} send={() => {}} />);
    expect(screen.getByText("導線カット")).toBeInTheDocument();
    expect(screen.getAllByText("ボム").length).toBeGreaterThan(0);
    expect(screen.getByTestId("cut-result-banner")).toHaveClass("cut-result-bomb");
  });

  it("カット結果は裏面から結果面へめくれるカード演出を含む", () => {
    const withCut: PlayerView = {
      ...baseView,
      lastCut: {
        cutterId: "p0",
        targetId: "p1",
        cardIndex: 0,
        revealedType: "defuse"
      }
    };
    render(<GameTable view={withCut} send={() => {}} />);
    expect(screen.getByTestId("cut-flip-card")).toHaveClass("cut-flip-card-defuse");
    expect(screen.getAllByText("解除チップが反転").length).toBeGreaterThan(0);
  });

  it("カット結果は中央のシネマ演出として強調表示する", () => {
    const withCut: PlayerView = {
      ...baseView,
      lastCut: {
        cutterId: "p0",
        targetId: "p1",
        cardIndex: 0,
        revealedType: "bomb"
      }
    };
    render(<GameTable view={withCut} send={() => {}} />);
    expect(screen.getByTestId("reveal-cinema")).toHaveClass("reveal-cinema-bomb");
    expect(screen.getByText("カード公開")).toBeInTheDocument();
  });

  it("ラウンドと現在の手番プレイヤーを大きな告知で表示する", () => {
    const otherTurn = { ...baseView, currentCutterId: "p1" };
    render(<GameTable view={otherTurn} send={() => {}} />);
    expect(screen.getByTestId("round-turn-cue")).toHaveTextContent("ROUND 1");
    expect(screen.getByTestId("round-turn-cue")).toHaveTextContent("Otherの手番");
  });
});
