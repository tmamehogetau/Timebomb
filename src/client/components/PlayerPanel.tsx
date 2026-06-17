import type { CardType, PlayerViewPublicPlayer } from "../../shared/types";

const CARD_LABEL: Record<CardType, string> = {
  defuse: "解除",
  bomb: "ボム",
  silence: "しーん"
};

interface Props {
  player: PlayerViewPublicPlayer;
  isMe: boolean;
  isCurrent: boolean;
  myHand: CardType[] | null;
  canCut: boolean;
  onCut: (cardIndex: number) => void;
}

export function PlayerPanel({ player, isMe, isCurrent, myHand, canCut, onCut }: Props) {
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
        {isMe && myHand
          ? myHand.map((c, i) => (
              <span key={i} className={`card card-${c}`}>
                {CARD_LABEL[c]}
              </span>
            ))
          : Array.from({ length: player.handSize }, (_, i) =>
              canCut ? (
                <button
                  key={i}
                  aria-label={`カード${i}`}
                  className="card card-back"
                  onClick={() => onCut(i)}
                />
              ) : (
                <span key={i} className="card card-back" />
              )
            )}
      </div>
    </div>
  );
}
