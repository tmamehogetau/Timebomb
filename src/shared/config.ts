export interface HandDistribution {
  defuse: number;
  bomb: number;
  silence: number;
}

export interface RoleDistribution {
  bomber: number;
  police: number;
  spy: number;
}

export function handDistributionFor(playerCount: number): HandDistribution {
  const defuse = playerCount;
  const bomb = 1;
  const silence = playerCount * 5 - defuse - bomb;
  return { defuse, bomb, silence };
}

export function roleDistributionFor(
  playerCount: number,
  spyEnabled: boolean
): RoleDistribution {
  const table: Record<number, { base: RoleDistribution; spy: RoleDistribution }> = {
    4: { base: { bomber: 1, police: 3, spy: 0 }, spy: { bomber: 1, police: 2, spy: 1 } },
    5: { base: { bomber: 1, police: 4, spy: 0 }, spy: { bomber: 1, police: 3, spy: 1 } },
    6: { base: { bomber: 2, police: 4, spy: 0 }, spy: { bomber: 2, police: 3, spy: 1 } }
  };
  const entry = table[playerCount];
  if (!entry) throw new Error(`Unsupported player count: ${playerCount}`);
  return spyEnabled ? entry.spy : entry.base;
}

export function cutsPerRound(playerCount: number): number {
  return playerCount;
}

export function defuseChipsTotal(playerCount: number): number {
  return playerCount;
}
