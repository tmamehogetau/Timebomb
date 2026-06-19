import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { RoleReveal } from "../../src/client/components/RoleReveal";

describe("RoleReveal", () => {
  it("自分の役職を表示し、確認で ready を送信", async () => {
    const send = vi.fn();
    const user = userEvent.setup();
    render(<RoleReveal role="bomber" ready={false} send={send} />);
    expect(screen.getByText(/ボマー/)).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: /確認/ }));
    expect(send).toHaveBeenCalledWith({ type: "ready" });
  });

  it("ready 済みならボタン無効", () => {
    render(<RoleReveal role="police" ready={true} send={() => {}} />);
    expect(screen.getByRole("button", { name: /確認/ })).toBeDisabled();
  });
  it.each([
    ["police", "時空警察のイラスト", "/roles/time-police.png"],
    ["bomber", "ボマーのイラスト", "/roles/time-bomber.png"],
    ["spy", "スパイのイラスト", "/roles/time-spy.png"]
  ] as const)("%s の役職イラストを表示する", (role, alt, src) => {
    render(<RoleReveal role={role} ready={false} send={() => {}} />);
    expect(screen.getByAltText(alt)).toHaveAttribute("src", src);
  });
});
