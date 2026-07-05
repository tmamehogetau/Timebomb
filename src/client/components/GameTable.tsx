import { useEffect, useState } from "react";
import type { CardType, ClientMessage, PlayerView } from "../../shared/types";
import { PlayerPanel } from "./PlayerPanel";

const CARD_ART: Record<CardType, string> = {
  defuse: "/cards/timebomb-card-defuse.png",
  bomb: "/cards/timebomb-card-bomb.png",
  silence: "/cards/timebomb-card-silence.png"
};

const CARD_BACK_ART = "/cards/timebomb-card-back.png";
const BOARD_REVEAL_DELAY_MS = 2500;
const REVEAL_CINEMA_DURATION_MS = 6200;

interface Props {
  view: PlayerView;
  send: (msg: ClientMessage) => void;
}

export function GameTable({ view, send }: Props) {
  const isMyTurn = view.currentCutterId === view.myPlayerId;
  const currentPlayer = view.players.find((p) => p.id === view.currentCutterId);
  const turnName = currentPlayer ? playerDisplayName(currentPlayer.name, currentPlayer.id === view.myPlayerId) : "手番不明";
  const revealKey = view.lastCut
    ? `${view.lastCut.targetId}-${view.lastCut.cardIndex}-${view.cutsThisRound}`
    : "";
  const [completedRevealKey, setCompletedRevealKey] = useState<string | null>(null);
  const [activeRevealKey, setActiveRevealKey] = useState<string | null>(view.lastCut ? revealKey : null);
  const allRevealedCards = view.revealedCards ?? [];
  const isLatestRevealVisible = !view.lastCut || completedRevealKey === revealKey;
  const pendingRevealedCard = isLatestRevealVisible
    ? null
    : allRevealedCards[allRevealedCards.length - 1] ?? null;
  const visibleRevealedCards = pendingRevealedCard ? allRevealedCards.slice(0, -1) : allRevealedCards;

  useEffect(() => {
    if (!view.lastCut) {
      setCompletedRevealKey(null);
      return;
    }
    const timerId = window.setTimeout(() => setCompletedRevealKey(revealKey), BOARD_REVEAL_DELAY_MS);
    return () => window.clearTimeout(timerId);
  }, [revealKey, view.lastCut]);

  useEffect(() => {
    if (!view.lastCut) {
      setActiveRevealKey(null);
      return;
    }
    setActiveRevealKey(revealKey);
    const timerId = window.setTimeout(() => {
      setActiveRevealKey((current) => (current === revealKey ? null : current));
    }, REVEAL_CINEMA_DURATION_MS);
    return () => window.clearTimeout(timerId);
  }, [revealKey, view.lastCut]);

  const isRevealActive = Boolean(view.lastCut && activeRevealKey === revealKey);
  const isCardSelectionOpen = view.phase === "round_play" && Boolean(view.currentCutterId) && !isRevealActive;
  const [turnElapsedSeconds, setTurnElapsedSeconds] = useState(0);

  useEffect(() => {
    setTurnElapsedSeconds(0);
    if (!isCardSelectionOpen) return;
    const timerId = window.setInterval(() => {
      setTurnElapsedSeconds((seconds) => seconds + 1);
    }, 1000);
    return () => window.clearInterval(timerId);
  }, [isCardSelectionOpen, view.currentCutterId]);

  return (
    <section className={`game-table ${view.lastCut ? "game-table-has-reveal" : ""}`.trim()}>
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
        <span className="hud-chip hud-timer" data-testid="turn-elapsed" aria-label="相談時間">
          相談 {turnElapsedSeconds}秒
        </span>
      </div>
      <RoundTurnCue
        key={`${view.round}-${view.currentCutterId}-${view.cutsThisRound}`}
        round={view.round}
        turnName={turnName}
        isMyTurn={isMyTurn}
        elapsedSeconds={turnElapsedSeconds}
      />
      {view.lastCut ? (
        <CutResultBanner
          key={`banner-${revealKey}`}
          type={view.lastCut.revealedType}
        />
      ) : null}
      {isRevealActive && view.lastCut ? <RevealCinema key={`cinema-${revealKey}`} type={view.lastCut.revealedType} /> : null}
      <div className="grid player-grid-expanded" data-testid="player-grid">
        {view.players.map((p) => (
          <PlayerPanel
            key={p.id}
            player={p}
            isMe={p.id === view.myPlayerId}
            isCurrent={p.id === view.currentCutterId}
            revealedCards={visibleRevealedCards.filter((card) => card.playerId === p.id)}
            pendingRevealedCard={pendingRevealedCard?.playerId === p.id ? pendingRevealedCard : null}
            canCut={!isRevealActive && isMyTurn && p.id !== view.myPlayerId}
            onCut={(cardIndex) => send({ type: "cut", targetId: p.id, cardIndex })}
          />
        ))}
      </div>
    </section>
  );
}

function RoundTurnCue({
  round,
  turnName,
  isMyTurn,
  elapsedSeconds
}: {
  round: number;
  turnName: string;
  isMyTurn: boolean;
  elapsedSeconds: number;
}) {
  return (
    <div
      className={`round-turn-cue ${isMyTurn ? "round-turn-cue-mine" : "round-turn-cue-other"}`}
      data-testid="round-turn-cue"
      aria-live="polite"
    >
      <span>ROUND {round}</span>
      <strong>{turnName}</strong>
      <span className="round-turn-cue-time">会話時間 {elapsedSeconds}秒</span>
    </div>
  );
}

function CutResultBanner({ type }: { type: CardType }) {
  return (
    <div
      className={`last-cut cut-result cut-result-${type}`}
      data-testid="cut-result-banner"
      aria-live="polite"
    >
      <div className={`cut-flip-card cut-flip-card-${type}`} data-testid="cut-flip-card" aria-hidden="true">
        <span className="cut-flip-face cut-flip-back">
          <img src={CARD_BACK_ART} alt="裏向きカード" />
        </span>
        <span className="cut-flip-face cut-flip-front">
          <img src={CARD_ART[type]} alt={`${cardLabel(type)}カード`} />
        </span>
      </div>
      <div className="cut-result-copy">
        <span className="cut-kicker">導線カット</span>
        <strong>{cardLabel(type)}</strong>
        <span className="cut-copy">{cutCopy(type)}</span>
      </div>
    </div>
  );
}

function RevealCinema({ type }: { type: CardType }) {
  return (
    <div
      className={`reveal-cinema reveal-cinema-${type}`}
      data-testid="reveal-cinema"
      aria-live="polite"
    >
      <div className="reveal-cinema-backdrop" />
      <div className="reveal-cinema-stage">
        <span className="reveal-cinema-kicker">カード公開</span>
        <div className="reveal-cinema-card" aria-hidden="true">
          <span className="reveal-cinema-face reveal-cinema-back">
            <img src={CARD_BACK_ART} alt="裏向きカード" />
          </span>
          <span className="reveal-cinema-face reveal-cinema-front">
            <img src={CARD_ART[type]} alt={`${cardLabel(type)}カード`} />
          </span>
        </div>
        <strong>{cardLabel(type)}</strong>
        <span>{cutCopy(type)}</span>
      </div>
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

function playerDisplayName(name: string, isMe: boolean): string {
  return isMe ? "あなたの手番" : `${name}の手番`;
}
