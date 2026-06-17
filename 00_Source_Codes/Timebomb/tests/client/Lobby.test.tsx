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
          { id: "p2", name: "C", handSize: 0, connected: true, ready: false, isHost: false }
        ]}
        roomCode="ABCD"
        canStart={false}
        error={null}
        onJoin={() => {}}
        send={send}
      />
    );
    expect(screen.getByTestId("room-code")).toHaveTextContent("ABCD");
    expect(screen.getByText("A（あなた） ★ホスト")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /開始/ })).toBeDisabled();
  });

  it("ホストがスパイトグルを切替可能", async () => {
    const send = vi.fn();
    const user = userEvent.setup();
    render(
      <Lobby
        myId="p0"
        joined={true}
        isHost={true}
        spyEnabled={false}
        players={[]}
        roomCode="X"
        canStart={true}
        error={null}
        onJoin={() => {}}
        send={send}
      />
    );
    await user.click(screen.getByRole("switch", { name: /スパイ/ }));
    expect(send).toHaveBeenCalledWith({ type: "setSpy", enabled: true });
  });

  it("未参加なら名前とルームコードで join できる", async () => {
    const onJoin = vi.fn();
    const user = userEvent.setup();
    render(
      <Lobby
        myId=""
        joined={false}
        isHost={false}
        spyEnabled={false}
        players={[]}
        roomCode="----"
        canStart={false}
        error={null}
        onJoin={onJoin}
        send={() => {}}
      />
    );
    await user.type(screen.getByLabelText("名前"), "Alice");
    await user.type(screen.getByLabelText("ルームコード"), "abcd");
    await user.click(screen.getByRole("button", { name: /参加/ }));
    expect(onJoin).toHaveBeenCalledWith("Alice", "abcd");
  });
});
