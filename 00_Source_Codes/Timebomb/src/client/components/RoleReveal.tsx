import type { ClientMessage, Role } from "../../shared/types";

const ROLE_LABEL: Record<Role, string> = {
  police: "時空警察",
  bomber: "ボマー",
  spy: "スパイ"
};

interface Props {
  role: Role;
  ready: boolean;
  send: (msg: ClientMessage) => void;
}

export function RoleReveal({ role, ready, send }: Props) {
  return (
    <section className="role-reveal">
      <h2>あなたの役職</h2>
      <p className={`role role-${role}`}>{ROLE_LABEL[role]}</p>
      <button disabled={ready} onClick={() => send({ type: "ready" })}>
        確認した
      </button>
    </section>
  );
}
