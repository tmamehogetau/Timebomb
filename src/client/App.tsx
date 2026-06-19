import { useEffect, useState } from "react";
import { GameOver } from "./components/GameOver";
import { GameTable } from "./components/GameTable";
import { Lobby } from "./components/Lobby";
import { RoleReveal } from "./components/RoleReveal";
import { useSocket } from "./useSocket";

const GAME_END_REVEAL_DURATION_MS = 6200;

export function App() {
  const sock = useSocket();
  const v = sock.view;
  const me = v?.players.find((p) => p.id === v.myPlayerId);
  const isHost = me?.isHost ?? sock.isHost;
  const [showGameOver, setShowGameOver] = useState(false);
  const gameEndRevealKey =
    v?.phase === "game_end" && v.lastCut
      ? `${v.lastCut.targetId}-${v.lastCut.cardIndex}-${v.cutsThisRound}`
      : null;

  useEffect(() => {
    if (v?.phase !== "game_end") {
      setShowGameOver(false);
      return;
    }
    if (!gameEndRevealKey) {
      setShowGameOver(true);
      return;
    }
    setShowGameOver(false);
    const timerId = window.setTimeout(() => setShowGameOver(true), GAME_END_REVEAL_DURATION_MS);
    return () => window.clearTimeout(timerId);
  }, [gameEndRevealKey, v?.phase]);
  if (!sock.playerId || !v || v.phase === "lobby") {
    return (
      <Lobby
        myId={sock.playerId ?? ""}
        joined={Boolean(sock.playerId)}
        isHost={isHost}
        spyEnabled={v?.spyEnabled ?? false}
        players={v?.players ?? []}
        roomCode={sock.roomCode ?? "----"}
        canStart={(v?.players.length ?? 0) >= 4 && (v?.players.length ?? 0) <= 6}
        error={sock.error}
        onJoin={sock.join}
        send={sock.send}
      />
    );
  }

  if (v.phase === "role_reveal") {
    return (
      <RoleReveal
        role={v.myRole ?? "police"}
        myHand={v.myHand}
        ready={v.players.find((p) => p.id === v.myPlayerId)?.ready ?? false}
        send={sock.send}
      />
    );
  }

  if (v.phase === "game_end" && showGameOver) {
    return (
      <GameOver
        winners={v.winners ?? []}
        revealedRoles={v.revealedRoles ?? []}
        players={v.players}
        isHost={isHost}
        send={sock.send}
      />
    );
  }

  return <GameTable view={v} send={sock.send} />;
}
