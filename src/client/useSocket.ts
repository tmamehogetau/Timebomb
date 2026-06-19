import { useCallback, useEffect, useRef, useState } from "react";
import type { ClientMessage, PlayerView, ServerMessage } from "../shared/types";

const MATCH_HEARTBEAT_INTERVAL_MS = 5 * 60 * 1000;

interface SocketState {
  view: PlayerView | null;
  playerId: string | null;
  roomCode: string | null;
  isHost: boolean;
  error: string | null;
  join: (name: string, roomCode: string) => void;
  send: (msg: ClientMessage) => void;
}

export function useSocket(): SocketState {
  const wsRef = useRef<WebSocket | null>(null);
  const roomCodeRef = useRef<string | null>(null);
  const [view, setView] = useState<PlayerView | null>(null);
  const [playerId, setPlayerId] = useState<string | null>(null);
  const [roomCode, setRoomCode] = useState<string | null>(null);
  const [isHost, setIsHost] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const wsUrl = `${window.location.protocol === "https:" ? "wss" : "ws"}://${window.location.host}/ws`;
    const ws = new WebSocket(wsUrl);
    wsRef.current = ws;
    ws.onmessage = (ev) => {
      const msg = JSON.parse(ev.data as string) as ServerMessage;
      switch (msg.type) {
        case "joined":
          setPlayerId(msg.playerId);
          setRoomCode(msg.roomCode);
          roomCodeRef.current = msg.roomCode;
          setIsHost(msg.isHost);
          window.localStorage.setItem(`timebomb:${msg.roomCode}:playerId`, msg.playerId);
          window.localStorage.setItem("timebomb:lastRoomCode", msg.roomCode);
          setError(null);
          break;
        case "state":
          setView(msg.view);
          setPlayerId(msg.view.myPlayerId);
          savePlayerIdentity(roomCodeRef.current, msg.view);
          break;
        case "error":
          setError(msg.message);
          break;
      }
    };
    ws.onclose = () => {
      if (wsRef.current === ws) wsRef.current = null;
    };
    return () => {
      ws.close();
    };
  }, []);

  const send = useCallback((msg: ClientMessage) => {
    wsRef.current?.send(JSON.stringify(msg));
  }, []);

  useEffect(() => {
    if (view?.phase !== "role_reveal" && view?.phase !== "round_play") return;
    const intervalId = window.setInterval(() => send({ type: "heartbeat" }), MATCH_HEARTBEAT_INTERVAL_MS);
    return () => window.clearInterval(intervalId);
  }, [send, view?.phase]);

  const join = useCallback(
    (name: string, roomCodeInput: string) => {
      const normalizedRoomCode = roomCodeInput.trim().toUpperCase();
      const trimmedName = name.trim();
      if (!trimmedName || !normalizedRoomCode) return;
      const savedName = window.localStorage.getItem(`timebomb:${normalizedRoomCode}:playerName`);
      const savedPlayerId =
        savedName === trimmedName
          ? window.localStorage.getItem(`timebomb:${normalizedRoomCode}:playerId`) ?? undefined
          : undefined;
      send({
        type: "join",
        name: trimmedName,
        roomCode: normalizedRoomCode,
        playerId: savedPlayerId
      });
    },
    [send]
  );

  return { view, playerId, roomCode, isHost, error, join, send };
}

function savePlayerIdentity(roomCode: string | null, view: PlayerView): void {
  if (!roomCode) return;
  const me = view.players.find((p) => p.id === view.myPlayerId);
  if (!me) return;
  window.localStorage.setItem(`timebomb:${roomCode}:playerId`, view.myPlayerId);
  window.localStorage.setItem(`timebomb:${roomCode}:playerName`, me.name);
}
