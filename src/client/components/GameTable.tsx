import type { CardType, ClientMessage, PlayerView } from "../../shared/types";
import { PlayerPanel } from "./PlayerPanel";

interface Props {
  view: PlayerView;
  send: (msg: ClientMessage) => void;
}

export function GameTable({ view, send }: Props) {
  const isMyTurn = view.currentCutterId === view.myPlayerId;
  return (
    <section className="game-table">
      <div className="hud">
        <span className="hud-chip hud-round">R{view.round}/4</span>
        <span className="hud-chip hud-defuse">
          解除 {view.defuseChipsFlipped}/{view.defuseChipsTotal}
        </span>
        <span className="hud-chip hud-cuts">
          カット {view.cutsThisRound}/{view.cutsPerRound}
        </span>
        <span className={`hud-chip ${isMyTurn ? "hud-turn-mine" : "hud-turn-wait"}`}>
          {isMyTurn ? "あなたの手番" : "他プレイヤーの手番"}
        </span>
      </div>
      {view.lastCut ? <CutResultBanner type={view.lastCut.revealedType} /> : <div className="last-cut idle" />}
      <div className="grid">
        {view.players.map((p) => (
          <PlayerPanel
            key={p.id}
            player={p}
            isMe={p.id === view.myPlayerId}
            isCurrent={p.id === view.currentCutterId}
            myHand={p.id === view.myPlayerId ? view.myHand : null}
            canCut={isMyTurn && p.id !== view.myPlayerId}
            onCut={(cardIndex) => send({ type: "cut", targetId: p.id, cardIndex })}
          />
        ))}
      </div>
    </section>
  );
}

function CutResultBanner({ type }: { type: CardType }) {
  return (
    <div
      className={`last-cut cut-result cut-result-${type}`}
      data-testid="cut-result-banner"
      aria-live="polite"
    >
      <span className="cut-kicker">導線カット</span>
      <strong>{cardLabel(type)}</strong>
      <span className="cut-copy">{cutCopy(type)}</span>
    </div>
  );
}

function cardLabel(c: CardType): string {
  return c === "defuse" ? "解除" : c === "bomb" ? "ボム" : "しーん";
}

function cutCopy(c: CardType): string {
  if (c === "defuse") return "解除チップが反転";
  if (c === "bomb") return "ボマー勝利の導火線";
  return "まだ沈黙が続く";
}
