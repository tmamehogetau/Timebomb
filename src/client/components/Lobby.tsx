import { useState } from "react";
import type { ClientMessage, PlayerViewPublicPlayer } from "../../shared/types";

interface Props {
  myId: string;
  joined: boolean;
  isHost: boolean;
  spyEnabled: boolean;
  players: PlayerViewPublicPlayer[];
  roomCode: string;
  canStart: boolean;
  error: string | null;
  onJoin: (name: string, roomCode: string) => void;
  send: (msg: ClientMessage) => void;
}

export function Lobby({
  myId,
  joined,
  isHost,
  spyEnabled,
  players,
  roomCode,
  canStart,
  error,
  onJoin,
  send
}: Props) {
  const [name, setName] = useState("");
  const [joinRoomCode, setJoinRoomCode] = useState("");

  if (!joined) {
    return (
      <section className="lobby">
        <h1>タイムボム Online</h1>
        <form
          className="join-form"
          onSubmit={(e) => {
            e.preventDefault();
            onJoin(name, joinRoomCode);
          }}
        >
          <label>
            名前
            <input aria-label="名前" value={name} onChange={(e) => setName(e.target.value)} />
          </label>
          <label>
            ルームコード
            <input
              aria-label="ルームコード"
              value={joinRoomCode}
              onChange={(e) => setJoinRoomCode(e.target.value)}
            />
          </label>
          <button disabled={!name.trim() || !joinRoomCode.trim()}>参加</button>
        </form>
        {error && <p role="alert">{error}</p>}
      </section>
    );
  }

  return (
    <section className="lobby">
      <h1>タイムボム Online</h1>
      <p>
        ルームコード: <strong data-testid="room-code">{roomCode}</strong>
      </p>

      <h2>参加者 ({players.length}/6)</h2>
      <ul>
        {players.map((p) => (
          <li key={p.id}>
            {p.name}
            {p.id === myId ? "（あなた）" : ""}
            {p.isHost ? " ★ホスト" : ""}
            {!p.connected ? "（切断）" : ""}
          </li>
        ))}
      </ul>

      <label className="spy-toggle">
        <input
          type="checkbox"
          role="switch"
          aria-label="スパイ(第三陣営)を有効化"
          checked={spyEnabled}
          disabled={!isHost}
          onChange={(e) => send({ type: "setSpy", enabled: e.target.checked })}
        />
        スパイ（第三陣営）あり
      </label>

      <button disabled={!isHost || !canStart} onClick={() => send({ type: "start" })}>
        開始
      </button>
      {!canStart && <p className="hint">4〜6人で開始できます</p>}
      {error && <p role="alert">{error}</p>}
    </section>
  );
}
