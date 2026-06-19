import type { CardType, PlayerViewPublicPlayer, RevealedCard } from "../../shared/types";

const CARD_LABEL: Record<CardType, string> = {
  defuse: "解除",
  bomb: "ボム",
  silence: "しーん"
};

const CARD_ART: Record<CardType, string> = {
  defuse: "/cards/timebomb-card-defuse.png",
  bomb: "/cards/timebomb-card-bomb.png",
  silence: "/cards/timebomb-card-silence.png"
};

const CARD_BACK_ART = "/cards/timebomb-card-back.png";

interface Props {
  player: PlayerViewPublicPlayer;
  isMe: boolean;
  isCurrent: boolean;
  revealedCards: RevealedCard[];
  pendingRevealedCard: RevealedCard | null;
  canCut: boolean;
  onCut: (cardIndex: number) => void;
}

export function PlayerPanel({
  player,
  isMe,
  isCurrent,
  revealedCards,
  pendingRevealedCard,
  canCut,
  onCut
}: Props) {
  return (
    <div
      className={[
        "panel",
        isMe ? "panel-me" : "",
        isCurrent ? "panel-current" : "",
        !player.connected ? "panel-disconnected" : ""
      ].join(" ")}
    >
      <header>
        <span className="name">
          {player.name}
          {isMe ? "（あなた）" : ""}
          {player.isHost ? " ★" : ""}
        </span>
        <span className="status">{!player.connected ? "切断" : ""}</span>
      </header>
      <div className="hand" aria-label="手札">
        {Array.from({ length: player.handSize }, (_, index) =>
          canCut ? (
            <button
              key={index}
              aria-label={`カード${index}`}
              className="card card-back"
              onClick={() => onCut(index)}
            >
              <img src={CARD_BACK_ART} alt="裏向きカード" />
            </button>
          ) : (
            <span key={index} className="card card-back">
              <img src={CARD_BACK_ART} alt="裏向きカード" />
            </span>
          )
        )}
        {pendingRevealedCard ? (
          <span className="card card-back card-pending-reveal">
            <img src={CARD_BACK_ART} alt="公開待ちカード" />
          </span>
        ) : null}
        {revealedCards.map((card, index) => (
          <span key={`${card.cardIndex}-${index}`} className={`card card-revealed card-${card.type}`}>
            <img src={CARD_ART[card.type]} alt={`公開済み${CARD_LABEL[card.type]}カード`} />
          </span>
        ))}
      </div>
    </div>
  );
}