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
        <span>R{view.round}/4</span>
        <span>
          解除 {view.defuseChipsFlipped}/{view.defuseChipsTotal}
        </span>
        <span>
          カット {view.cutsThisRound}/{view.cutsPerRound}
        </span>
        <span>{isMyTurn ? "あなたの手番" : "他プレイヤーの手番"}</span>
      </div>
      <div className="last-cut">
        {view.lastCut && <span>直前: {cardLabel(view.lastCut.revealedType)}</span>}
      </div>
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

function cardLabel(c: CardType): string {
  return c === "defuse" ? "解除" : c === "bomb" ? "ボム" : "しーん";
}
