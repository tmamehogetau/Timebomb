import { GameOver } from "./components/GameOver";
import { GameTable } from "./components/GameTable";
import { Lobby } from "./components/Lobby";
import { RoleReveal } from "./components/RoleReveal";
import { useSocket } from "./useSocket";

export function App() {
  const sock = useSocket();
  const v = sock.view;
  const me = v?.players.find((p) => p.id === v.myPlayerId);
  const isHost = me?.isHost ?? sock.isHost;

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
        ready={v.players.find((p) => p.id === v.myPlayerId)?.ready ?? false}
        send={sock.send}
      />
    );
  }

  if (v.phase === "game_end") {
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
