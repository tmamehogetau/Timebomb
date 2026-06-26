import {
  cutsPerRound,
  defuseChipsTotal,
  handDistributionFor,
  roleDistributionFor
} from "./config.js";
import type { Shuffle } from "./random.js";
import {
  MAX_PLAYERS,
  MIN_PLAYERS,
  TOTAL_ROUNDS,
  type CardType,
  type GameState,
  type Player,
  type PlayerView,
  type PlayerViewPublicPlayer,
  type Role,
  type RevealedCard
} from "./types.js";

export type CutInput = { targetId: string; cardIndex: number };
export type CutResult =
  | { kind: "ok"; revealedType: CardType }
  | { kind: "invalid"; reason: string };

export function createInitialState(): GameState {
  return {
    phase: "lobby",
    round: 0,
    spyEnabled: false,
    players: [],
    currentCutterId: null,
    defuseChipsFlipped: 0,
    cutsThisRound: 0,
    lastCut: null,
    revealedCards: [],
    discardedCards: [],
    winners: null
  };
}

export function buildDeck(playerCount: number): CardType[] {
  const d = handDistributionFor(playerCount);
  return [
    ...Array<CardType>(d.defuse).fill("defuse"),
    ...Array<CardType>(d.bomb).fill("bomb"),
    ...Array<CardType>(d.silence).fill("silence")
  ];
}

export function dealRound(state: GameState, shuffle: Shuffle): void {
  const deck = shuffle(buildDeckForDeal(state));
  const handSize = Math.floor(deck.length / state.players.length);
  state.players.forEach((p, i) => {
    p.hand = deck.slice(i * handSize, i * handSize + handSize);
  });
  state.cutsThisRound = 0;
  state.lastCut = null;
  state.revealedCards = [];
}

function buildDeckForDeal(state: GameState): CardType[] {
  const deck = buildDeck(state.players.length);
  state.discardedCards.forEach((discarded) => {
    const index = deck.indexOf(discarded);
    if (index >= 0) deck.splice(index, 1);
  });
  return deck;
}

export function assignRoles(players: Player[], spyEnabled: boolean, shuffle: Shuffle): void {
  const distribution = roleDistributionFor(players.length, spyEnabled);
  const roles: Role[] = [
    ...Array<Role>(distribution.bomber).fill("bomber"),
    ...Array<Role>(distribution.police).fill("police"),
    ...Array<Role>(distribution.spy).fill("spy")
  ];
  const shuffled = shuffle(roles);
  players.forEach((p, i) => {
    p.role = shuffled[i] ?? "police";
  });
}

export function pickRandomFirstCutter(players: Player[], shuffle: Shuffle): string | null {
  return shuffle(players)[0]?.id ?? null;
}

export function canStart(state: GameState): boolean {
  return (
    state.phase === "lobby" &&
    state.players.length >= MIN_PLAYERS &&
    state.players.length <= MAX_PLAYERS
  );
}

export function startGame(state: GameState, shuffle: Shuffle): void {
  if (!canStart(state)) return;
  state.phase = "role_reveal";
  state.round = 1;
  state.defuseChipsFlipped = 0;
  state.cutsThisRound = 0;
  state.lastCut = null;
  state.revealedCards = [];
  state.discardedCards = [];
  state.winners = null;
  state.players.forEach((p) => {
    p.ready = false;
  });
  assignRoles(state.players, state.spyEnabled, shuffle);
  dealRound(state, shuffle);
  state.currentCutterId = pickRandomFirstCutter(state.players, shuffle);
}

export function allReady(state: GameState): boolean {
  return state.players.length > 0 && state.players.every((p) => p.ready);
}

export function beginPlay(state: GameState, shuffle: Shuffle): void {
  if (state.phase !== "role_reveal") return;
  state.players.forEach((player) => {
    player.hand = shuffle(player.hand);
  });
  state.phase = "round_play";
}

export function applyCut(
  state: GameState,
  cutterId: string,
  input: CutInput
): CutResult {
  if (state.phase !== "round_play") return { kind: "invalid", reason: "play 中ではありません" };
  if (state.currentCutterId !== cutterId) {
    return { kind: "invalid", reason: "あなたの手番ではありません" };
  }
  if (input.targetId === cutterId) return { kind: "invalid", reason: "自分は切れません" };
  const target = state.players.find((p) => p.id === input.targetId);
  if (!target) return { kind: "invalid", reason: "対象が存在しません" };
  if (!Number.isInteger(input.cardIndex) || input.cardIndex < 0 || input.cardIndex >= target.hand.length) {
    return { kind: "invalid", reason: "カード位置が不正です" };
  }

  const [revealed] = target.hand.splice(input.cardIndex, 1);
  state.lastCut = {
    cutterId,
    targetId: input.targetId,
    cardIndex: input.cardIndex,
    revealedType: revealed
  };
  state.revealedCards.push({ playerId: input.targetId, cardIndex: input.cardIndex, type: revealed });
  state.discardedCards.push(revealed);

  state.cutsThisRound++;
  if (revealed === "defuse") {
    state.defuseChipsFlipped++;
    if (state.defuseChipsFlipped >= defuseChipsTotal(state.players.length)) {
      endGame(state, ["police"]);
      return { kind: "ok", revealedType: revealed };
    }
  } else if (revealed === "bomb") {
    endGame(state, ["bomber"]);
    return { kind: "ok", revealedType: revealed };
  }

  state.currentCutterId = input.targetId;
  if (state.cutsThisRound >= cutsPerRound(state.players.length)) {
    state.phase = "round_end";
  }
  return { kind: "ok", revealedType: revealed };
}

export function advanceRound(state: GameState, shuffle: Shuffle): void {
  if (state.phase !== "round_end") return;
  if (state.round >= TOTAL_ROUNDS) {
    endGame(state, state.spyEnabled ? ["spy"] : ["bomber"]);
    return;
  }
  state.round++;
  dealRound(state, shuffle);
  state.players.forEach((p) => {
    p.ready = false;
  });
  state.phase = "role_reveal";
}

export function restart(state: GameState): void {
  const hostId = state.players.find((p) => p.isHost)?.id ?? state.players[0]?.id ?? null;
  state.phase = "lobby";
  state.round = 0;
  state.currentCutterId = null;
  state.defuseChipsFlipped = 0;
  state.cutsThisRound = 0;
  state.lastCut = null;
  state.revealedCards = [];
  state.discardedCards = [];
  state.winners = null;
  state.players.forEach((p) => {
    p.role = null;
    p.hand = [];
    p.ready = false;
    p.isHost = p.id === hostId;
  });
}

export function hasAvailableCutTarget(state: GameState): boolean {
  if (!state.currentCutterId) return false;
  return state.players.some((p) => p.id !== state.currentCutterId && p.hand.length > 0);
}

export function toPlayerView(state: GameState, viewerId: string): PlayerView {
  const me = state.players.find((p) => p.id === viewerId);
  const n = state.players.length;
  const players: PlayerViewPublicPlayer[] = state.players.map((p) => ({
    id: p.id,
    name: p.name,
    handSize: p.hand.length,
    connected: p.connected,
    ready: p.ready,
    isHost: p.isHost
  }));
  return {
    phase: state.phase,
    round: state.round,
    spyEnabled: state.spyEnabled,
    myPlayerId: viewerId,
    myRole: me ? me.role : null,
    myHand: me && state.phase === "role_reveal" ? [...me.hand] : [],
    currentCutterId: state.currentCutterId,
    defuseChipsFlipped: state.defuseChipsFlipped,
    defuseChipsTotal: n > 0 ? defuseChipsTotal(n) : 0,
    cutsThisRound: state.cutsThisRound,
    cutsPerRound: n > 0 ? cutsPerRound(n) : 0,
    players,
    lastCut: state.lastCut,
    revealedCards: state.revealedCards.map((card): RevealedCard => ({ ...card })),
    winners: state.winners,
    revealedRoles:
      state.phase === "game_end"
        ? state.players
            .filter((p): p is Player & { role: Role } => p.role !== null)
            .map((p) => ({ playerId: p.id, role: p.role }))
        : null
  };
}

function endGame(state: GameState, winners: Role[]): void {
  state.phase = "game_end";
  state.winners = winners;
}
