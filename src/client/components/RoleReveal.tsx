import type { CardType, ClientMessage, Role } from "../../shared/types";

const ROLE_LABEL: Record<Role, string> = {
  police: "時空警察",
  bomber: "ボマー",
  spy: "スパイ"
};

const CARD_ART: Record<CardType, string> = {
  defuse: "/cards/timebomb-card-defuse.png",
  bomb: "/cards/timebomb-card-bomb.png",
  silence: "/cards/timebomb-card-silence.png"
};

const CARD_LABEL: Record<CardType, string> = {
  defuse: "解除",
  bomb: "ボム",
  silence: "しーん"
};

const ROLE_ART: Record<Role, { src: string; alt: string }> = {
  police: { src: "/roles/time-police.png", alt: "時空警察のイラスト" },
  bomber: { src: "/roles/time-bomber.png", alt: "ボマーのイラスト" },
  spy: { src: "/roles/time-spy.png", alt: "スパイのイラスト" }
};

interface Props {
  role: Role;
  myHand?: CardType[];
  ready: boolean;
  send: (msg: ClientMessage) => void;
}

export function RoleReveal({ role, myHand = [], ready, send }: Props) {
  const art = ROLE_ART[role];

  return (
    <section className={`role-reveal role-reveal-${role}`}>
      <div className="role-reveal-art">
        <img src={art.src} alt={art.alt} />
      </div>
      <h2>あなたの役職</h2>
      <p className={`role role-${role}`}>{ROLE_LABEL[role]}</p>
      <div className="hand role-reveal-hand" aria-label="確認中の手札">
        {myHand.map((card, index) => (
          <span key={index} className={`card card-${card}`}>
            <img src={CARD_ART[card]} alt={`${CARD_LABEL[card]}カード`} />
          </span>
        ))}
      </div>
      <button disabled={ready} onClick={() => send({ type: "ready" })}>
        確認した
      </button>
    </section>
  );
}