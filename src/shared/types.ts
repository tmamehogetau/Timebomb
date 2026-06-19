export type CardType = "defuse" | "bomb" | "silence";
export type Role = "police" | "bomber" | "spy";
export type Phase =
  | "lobby"
  | "role_reveal"
  | "round_deal"
  | "round_play"
  | "round_end"
  | "game_end";

export interface Player {
  id: string;
  name: string;
  role: Role | null;
  hand: CardType[];
  connected: boolean;
  ready: boolean;
  isHost: boolean;
}

export interface CutEvent {
  cutterId: string;
  targetId: string;
  cardIndex: number;
  revealedType: CardType;
}

export interface RevealedRole {
  playerId: string;
  role: Role;
}

export interface RevealedCard {
  playerId: string;
  cardIndex: number;
  type: CardType;
}

export interface GameState {
  phase: Phase;
  round: number;
  spyEnabled: boolean;
  players: Player[];
  currentCutterId: string | null;
  defuseChipsFlipped: number;
  cutsThisRound: number;
  lastCut: CutEvent | null;
  revealedCards: RevealedCard[];
  winners: Role[] | null;
}

export interface PlayerViewPublicPlayer {
  id: string;
  name: string;
  handSize: number;
  connected: boolean;
  ready: boolean;
  isHost: boolean;
}

export interface PlayerView {
  phase: Phase;
  round: number;
  spyEnabled: boolean;
  myPlayerId: string;
  myRole: Role | null;
  myHand: CardType[];
  currentCutterId: string | null;
  defuseChipsFlipped: number;
  defuseChipsTotal: number;
  cutsThisRound: number;
  cutsPerRound: number;
  players: PlayerViewPublicPlayer[];
  lastCut: CutEvent | null;
  revealedCards: RevealedCard[];
  winners: Role[] | null;
  revealedRoles: RevealedRole[] | null;
}

export type ClientMessage =
  | { type: "join"; name: string; roomCode: string; playerId?: string }
  | { type: "setSpy"; enabled: boolean }
  | { type: "start" }
  | { type: "ready" }
  | { type: "heartbeat" }
  | { type: "cut"; targetId: string; cardIndex: number }
  | { type: "restart" };

export type ServerMessage =
  | { type: "joined"; playerId: string; roomCode: string; isHost: boolean }
  | { type: "state"; view: PlayerView }
  | { type: "error"; message: string };

export const MIN_PLAYERS = 4;
export const MAX_PLAYERS = 6;
export const TOTAL_ROUNDS = 4;
export const HAND_SIZE = 5;
