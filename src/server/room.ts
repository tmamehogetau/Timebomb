import { randomUUID } from "node:crypto";
import {
  advanceRound,
  allReady,
  applyCut,
  beginPlay,
  canStart,
  createInitialState,
  restart,
  startGame,
  toPlayerView
} from "../shared/engine.js";
import type { Shuffle } from "../shared/random.js";
import { MAX_PLAYERS } from "../shared/types.js";
import type {
  ClientMessage,
  GameState,
  PlayerView,
  ServerMessage
} from "../shared/types.js";

export class Room {
  readonly code: string;
  readonly state: GameState;
  private shuffle: Shuffle;

  constructor(code: string, shuffle: Shuffle) {
    this.code = code.trim().toUpperCase();
    this.shuffle = shuffle;
    this.state = createInitialState();
  }

  handleJoin(msg: Extract<ClientMessage, { type: "join" }>): ServerMessage {
    const name = msg.name.trim();
    if (!name) return { type: "error", message: "名前を入力してください" };

    if (msg.playerId) {
      const existing = this.state.players.find((p) => p.id === msg.playerId);
      if (existing) {
        if (this.state.players.some((p) => p.id !== existing.id && p.name === name)) {
          return { type: "error", message: "その名前は既に使われています" };
        }
        existing.connected = true;
        existing.name = name;
        return { type: "joined", playerId: existing.id, roomCode: this.code, isHost: existing.isHost };
      }
    }

    if (this.state.players.length >= MAX_PLAYERS) {
      return { type: "error", message: "ルームが満員です" };
    }
    if (this.state.players.some((p) => p.name === name)) {
      return { type: "error", message: "その名前は既に使われています" };
    }
    if (this.state.phase !== "lobby") {
      return { type: "error", message: "ゲーム開始後は新規参加できません" };
    }

    const id = randomUUID();
    const isHost = this.state.players.length === 0;
    this.state.players.push({
      id,
      name,
      role: null,
      hand: [],
      connected: true,
      ready: false,
      isHost
    });
    return { type: "joined", playerId: id, roomCode: this.code, isHost };
  }

  handleCommand(playerId: string, msg: ClientMessage): ServerMessage[] {
    const player = this.state.players.find((p) => p.id === playerId);
    if (!player) return [{ type: "error", message: "プレイヤーが存在しません" }];
    const out: ServerMessage[] = [];

    switch (msg.type) {
      case "setSpy": {
        if (!player.isHost) return [{ type: "error", message: "ホストのみ操作できます" }];
        if (this.state.phase !== "lobby") {
          return [{ type: "error", message: "ロビーでのみ変更できます" }];
        }
        this.state.spyEnabled = msg.enabled;
        break;
      }
      case "start": {
        if (!player.isHost) return [{ type: "error", message: "ホストのみ開始できます" }];
        if (!canStart(this.state)) return [{ type: "error", message: "4〜6人で開始できます" }];
        startGame(this.state, this.shuffle);
        break;
      }
      case "ready": {
        if (this.state.phase !== "role_reveal") {
          return [{ type: "error", message: "役職確認中のみ操作できます" }];
        }
        player.ready = true;
        if (allReady(this.state)) beginPlay(this.state, this.shuffle);
        break;
      }
      case "cut": {
        if (this.state.phase !== "round_play") {
          return [{ type: "error", message: "play 中ではありません" }];
        }
        const result = applyCut(this.state, playerId, {
          targetId: msg.targetId,
          cardIndex: msg.cardIndex
        });
        if (result.kind === "invalid") return [{ type: "error", message: result.reason }];
        break;
      }
      case "advanceRound": {
        if (this.state.phase === "round_end") advanceRound(this.state, this.shuffle);
        break;
      }
      case "restart": {
        if (!player.isHost) return [{ type: "error", message: "ホストのみ再開できます" }];
        if (this.state.phase !== "game_end") {
          return [{ type: "error", message: "終了時のみ再開できます" }];
        }
        restart(this.state);
        break;
      }
      case "join":
        return [{ type: "error", message: "join は handleJoin で処理してください" }];
    }

    out.push({ type: "state", view: toPlayerView(this.state, playerId) });
    return out;
  }

  handleDisconnect(playerId: string): void {
    const player = this.state.players.find((p) => p.id === playerId);
    if (!player) return;
    player.connected = false;
    if (player.isHost) {
      const next = this.state.players.find((p) => p.connected);
      if (next) {
        player.isHost = false;
        next.isHost = true;
      }
    }
  }

  snapshotFor(playerId: string): PlayerView {
    return toPlayerView(this.state, playerId);
  }

  playerIds(): string[] {
    return this.state.players.map((p) => p.id);
  }
}
