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

type BoardSlot =
  | { kind: "hidden"; cardIndex: number }
  | { kind: "revealed"; card: RevealedCard; isPending: boolean };

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
  const boardSlots = buildBoardSlots(player.handSize, revealedCards, pendingRevealedCard);

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
        {boardSlots.map((slot, index) => {
          if (slot.kind === "hidden") {
            return canCut ? (
              <button
                key={index}
                aria-label={`カード${slot.cardIndex}`}
                className="card card-back"
                onClick={() => onCut(slot.cardIndex)}
              >
                <img src={CARD_BACK_ART} alt="裏向きカード" />
              </button>
            ) : (
              <span key={index} className="card card-back">
                <img src={CARD_BACK_ART} alt="裏向きカード" />
              </span>
            );
          }

          if (slot.isPending) {
            return (
              <span key={index} className="card card-back card-pending-reveal">
                <img src={CARD_BACK_ART} alt="公開待ちカード" />
              </span>
            );
          }

          return (
            <span key={index} className={`card card-revealed card-${slot.card.type}`}>
              <img src={CARD_ART[slot.card.type]} alt={`公開済み${CARD_LABEL[slot.card.type]}カード`} />
            </span>
          );
        })}
      </div>
    </div>
  );
}

function buildBoardSlots(
  handSize: number,
  revealedCards: RevealedCard[],
  pendingRevealedCard: RevealedCard | null
): BoardSlot[] {
  const revealedEntries = [
    ...revealedCards.map((card) => ({ card, isPending: false })),
    ...(pendingRevealedCard ? [{ card: pendingRevealedCard, isPending: true }] : [])
  ];
  const availablePositions = Array.from({ length: handSize + revealedEntries.length }, (_, index) => index);
  const slots: Array<BoardSlot | null> = Array.from({ length: availablePositions.length }, () => null);

  for (const entry of revealedEntries) {
    const position = availablePositions.splice(entry.card.cardIndex, 1)[0];
    if (position !== undefined) {
      slots[position] = { kind: "revealed", card: entry.card, isPending: entry.isPending };
    }
  }

  let hiddenCardIndex = 0;
  return slots.map((slot) => {
    if (slot) return slot;
    return { kind: "hidden", cardIndex: hiddenCardIndex++ };
  });
}