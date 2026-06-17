import type { ClientMessage, PlayerViewPublicPlayer, RevealedRole, Role } from "../../shared/types";

const WINNER_LABEL: Record<Role, string> = {
  police: "時空警察",
  bomber: "ボマー",
  spy: "スパイ"
};

interface Props {
  winners: Role[];
  revealedRoles: RevealedRole[];
  players: PlayerViewPublicPlayer[];
  isHost: boolean;
  send: (msg: ClientMessage) => void;
}

export function GameOver({ winners, revealedRoles, players, isHost, send }: Props) {
  const nameById = new Map(players.map((p) => [p.id, p.name]));
  return (
    <section className="game-over">
      <h2>勝者: {winners.map((w) => WINNER_LABEL[w]).join(" / ")}</h2>
      <ul>
        {revealedRoles.map((r) => (
          <li key={r.playerId}>
            {nameById.get(r.playerId) ?? r.playerId}: {WINNER_LABEL[r.role]}
          </li>
        ))}
      </ul>
      {isHost && <button onClick={() => send({ type: "restart" })}>もう一度</button>}
    </section>
  );
}
