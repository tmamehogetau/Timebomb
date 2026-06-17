import { describe, expect, it } from "vitest";
import {
  cutsPerRound,
  defuseChipsTotal,
  handDistributionFor,
  roleDistributionFor
} from "./config.js";

describe("config", () => {
  it("手札配布: defuse=N, bomb=1, silence=4N-1", () => {
    expect(handDistributionFor(4)).toEqual({ defuse: 4, bomb: 1, silence: 15 });
    expect(handDistributionFor(5)).toEqual({ defuse: 5, bomb: 1, silence: 19 });
    expect(handDistributionFor(6)).toEqual({ defuse: 6, bomb: 1, silence: 23 });
  });

  it("手札合計は常に 5N", () => {
    for (const n of [4, 5, 6]) {
      const d = handDistributionFor(n);
      expect(d.defuse + d.bomb + d.silence).toBe(n * 5);
    }
  });

  it("チップ総数とカット/ラウンドは N に等しい", () => {
    expect(defuseChipsTotal(4)).toBe(4);
    expect(cutsPerRound(4)).toBe(4);
    expect(defuseChipsTotal(6)).toBe(6);
  });

  it("役職: police が常に最多。spyなし", () => {
    expect(roleDistributionFor(4, false)).toEqual({ bomber: 1, police: 3, spy: 0 });
    expect(roleDistributionFor(6, false)).toEqual({ bomber: 2, police: 4, spy: 0 });
  });

  it("役職: spyあり", () => {
    expect(roleDistributionFor(4, true)).toEqual({ bomber: 1, police: 2, spy: 1 });
    expect(roleDistributionFor(5, true)).toEqual({ bomber: 1, police: 3, spy: 1 });
    expect(roleDistributionFor(6, true)).toEqual({ bomber: 2, police: 3, spy: 1 });
  });

  it("未対応人数は明示的に失敗する", () => {
    expect(() => roleDistributionFor(3, false)).toThrow("Unsupported player count");
    expect(() => roleDistributionFor(7, true)).toThrow("Unsupported player count");
  });
});
