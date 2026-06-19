import type { Server as HttpServer } from "node:http";
import { WebSocket, WebSocketServer } from "ws";
import { createSystemShuffle } from "../shared/random.js";
import type { ClientMessage, ServerMessage } from "../shared/types.js";
import { Room } from "./room.js";

interface Session {
  roomCode: string;
  playerId: string;
}

export function attachWebSocketServer(
  server: HttpServer,
  rooms = new Map<string, Room>()
): { wss: WebSocketServer; rooms: Map<string, Room> } {
  const shuffle = createSystemShuffle();
  const wss = new WebSocketServer({ server, path: "/ws" });
  const sessions = new Map<WebSocket, Session>();
  const roomSockets = new Map<string, Set<WebSocket>>();

  wss.on("connection", (socket) => {
    socket.on("message", (data) => {
      const parsed = parseMessage(data);
      if ("error" in parsed) {
        send(socket, { type: "error", message: parsed.error });
        return;
      }
      const msg = parsed.message;

      if (msg.type === "join") {
        const roomCode = msg.roomCode.trim().toUpperCase();
        const room = getOrCreateRoom(rooms, roomCode, shuffle);
        const result = room.handleJoin({ ...msg, roomCode });
        send(socket, result);
        if (result.type === "joined") {
          detachDupSocket(socket, room.code, result.playerId, sessions, roomSockets);
          const session: Session = { roomCode: room.code, playerId: result.playerId };
          bind(socket, session, sessions, roomSockets);
          broadcastRoom(room, sessions, roomSockets);
        }
        return;
      }

      const session = sessions.get(socket);
      if (!session) {
        send(socket, { type: "error", message: "先に join してください" });
        return;
      }
      if (msg.type === "heartbeat") return;

      const room = rooms.get(session.roomCode);
      if (!room) {
        send(socket, { type: "error", message: "ルームが存在しません" });
        return;
      }
      const replies = room.handleCommand(session.playerId, msg);
      for (const r of replies) send(socket, r);
      broadcastRoom(room, sessions, roomSockets);
    });

    socket.on("close", () => {
      const session = sessions.get(socket);
      if (!session) {
        sessions.delete(socket);
        return;
      }
      const room = rooms.get(session.roomCode);
      unbind(socket, session, sessions, roomSockets);
      if (room) {
        room.handleDisconnect(session.playerId);
        broadcastRoom(room, sessions, roomSockets);
      }
    });
  });

  return { wss, rooms };
}

function getOrCreateRoom(
  rooms: Map<string, Room>,
  code: string,
  shuffle: ReturnType<typeof createSystemShuffle>
): Room {
  const normalizedCode = code.trim().toUpperCase();
  let room = rooms.get(normalizedCode);
  if (!room) {
    room = new Room(normalizedCode, shuffle);
    rooms.set(room.code, room);
  }
  return room;
}

function bind(
  socket: WebSocket,
  session: Session,
  sessions: Map<WebSocket, Session>,
  roomSockets: Map<string, Set<WebSocket>>
): void {
  sessions.set(socket, session);
  let set = roomSockets.get(session.roomCode);
  if (!set) {
    set = new Set();
    roomSockets.set(session.roomCode, set);
  }
  set.add(socket);
}

function unbind(
  socket: WebSocket,
  session: Session,
  sessions: Map<WebSocket, Session>,
  roomSockets: Map<string, Set<WebSocket>>
): void {
  sessions.delete(socket);
  const set = roomSockets.get(session.roomCode);
  set?.delete(socket);
  if (set && set.size === 0) roomSockets.delete(session.roomCode);
}

function detachDupSocket(
  current: WebSocket,
  roomCode: string,
  playerId: string,
  sessions: Map<WebSocket, Session>,
  roomSockets: Map<string, Set<WebSocket>>
): void {
  const set = roomSockets.get(roomCode);
  if (!set) return;
  for (const s of [...set]) {
    if (s === current) continue;
    const sess = sessions.get(s);
    if (sess?.playerId === playerId) {
      sessions.delete(s);
      set.delete(s);
      s.close(1000, "replaced");
    }
  }
}

function broadcastRoom(
  room: Room,
  sessions: Map<WebSocket, Session>,
  roomSockets: Map<string, Set<WebSocket>>
): void {
  const set = roomSockets.get(room.code);
  if (!set) return;
  for (const s of set) {
    const sess = sessions.get(s);
    if (!sess) continue;
    send(s, { type: "state", view: room.snapshotFor(sess.playerId) });
  }
}

function send(socket: WebSocket, msg: ServerMessage): void {
  if (socket.readyState !== WebSocket.OPEN) return;
  socket.send(JSON.stringify(msg));
}

function parseMessage(data: WebSocket.RawData): { message: ClientMessage } | { error: string } {
  let parsed: unknown;
  try {
    const text = rawDataToString(data);
    parsed = JSON.parse(text);
  } catch {
    return { error: "JSON 形式が不正です" };
  }
  if (!parsed || typeof parsed !== "object") return { error: "メッセージが不正です" };
  const r = parsed as Record<string, unknown>;
  switch (r.type) {
    case "join":
      if (typeof r.name === "string" && typeof r.roomCode === "string") {
        return {
          message: {
            type: "join",
            name: r.name,
            roomCode: r.roomCode,
            playerId: typeof r.playerId === "string" ? r.playerId : undefined
          }
        };
      }
      return { error: "join のパラメータが不正です" };
    case "setSpy":
      if (typeof r.enabled === "boolean") return { message: { type: "setSpy", enabled: r.enabled } };
      return { error: "setSpy のパラメータが不正です" };
    case "start":
      return { message: { type: "start" } };
    case "ready":
      return { message: { type: "ready" } };
    case "heartbeat":
      return { message: { type: "heartbeat" } };
    case "advanceRound":
      return { message: { type: "advanceRound" } };
    case "cut":
      if (typeof r.targetId === "string" && typeof r.cardIndex === "number" && Number.isInteger(r.cardIndex)) {
        return { message: { type: "cut", targetId: r.targetId, cardIndex: r.cardIndex } };
      }
      return { error: "cut のパラメータが不正です" };
    case "restart":
      return { message: { type: "restart" } };
    default:
      return { error: "未対応のコマンドです" };
  }
}

function rawDataToString(data: WebSocket.RawData): string {
  if (typeof data === "string") return data;
  if (Buffer.isBuffer(data)) return data.toString("utf8");
  if (Array.isArray(data)) {
    return Buffer.concat(data.map((chunk) => toBuffer(chunk))).toString("utf8");
  }
  return toBuffer(data).toString("utf8");
}

function toBuffer(data: Buffer | ArrayBuffer): Buffer {
  return Buffer.isBuffer(data) ? data : Buffer.from(new Uint8Array(data));
}
