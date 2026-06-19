import type { ClientMessage, Role } from "../../shared/types";

const ROLE_LABEL: Record<Role, string> = {
  police: "時空警察",
  bomber: "ボマー",
  spy: "スパイ"
};

const ROLE_ART: Record<Role, { src: string; alt: string }> = {
  police: { src: "/roles/time-police.png", alt: "時空警察のイラスト" },
  bomber: { src: "/roles/time-bomber.png", alt: "ボマーのイラスト" },
  spy: { src: "/roles/time-spy.png", alt: "スパイのイラスト" }
};

interface Props {
  role: Role;
  ready: boolean;
  send: (msg: ClientMessage) => void;
}

export function RoleReveal({ role, ready, send }: Props) {
  const art = ROLE_ART[role];

  return (
    <section className={`role-reveal role-reveal-${role}`}>
      <div className="role-reveal-art">
        <img src={art.src} alt={art.alt} />
      </div>
      <h2>あなたの役職</h2>
      <p className={`role role-${role}`}>{ROLE_LABEL[role]}</p>
      <button disabled={ready} onClick={() => send({ type: "ready" })}>
        確認した
      </button>
    </section>
  );
}