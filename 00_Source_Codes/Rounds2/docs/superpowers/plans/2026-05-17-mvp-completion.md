# Rounds2 MVP Completion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Finish the first credible 1v1 MVP: a local dedicated server match that can be played from start to match end, with draft cards that visibly change combat.

**Architecture:** Keep server-authoritative combat and match state. Card identity, text, category, stacking policy, and numeric effects stay centralized in `CardCatalog`; gameplay systems consume `CombatCardStats` so new cards do not require scattered switch statements.

**Tech Stack:** Unity 6000.3, C#, FishNet, Unity EditMode tests, Windows dedicated server/client builds.

---

## Completion Record

MVP accepted as complete on 2026-05-18.

Technical verification:

- EditMode: 190/190 passed.
- Windows client build succeeded.
- 10 minute Bot smoke run reached match end.
- Smoke run result: 9 set winners, 7 round winners, 6 card drafts, 6 card rewards, 1 match winner, 0 matched runtime errors.

Final product decisions made during implementation:

- Card rewards happen only on round loss, not every set loss.
- All MVP cards are cumulative, including `Ricochet`.
- `Burst Shot` and `Split Shot` increase ammo cost by fired projectile count and also increase magazine capacity.
- Ricocheted bullets can hit the shooter after bouncing.

Post-MVP work is tracked in `docs/post-mvp-backlog.md`.

---

## MVP Definition

MVP is complete when two local clients can play a full 1v1 match on `Arena01` with:

- movement, aiming, shooting, reload, shield, HP, death, and set reset working
- loser-side draft between sets/rounds, with selected cards applied next combat
- at least 8 cards, including 4 or more cards that change combat feel beyond simple number tuning
- clear but restrained feedback for fire, hit, shield block, reload, and draft choice
- match end screen/state and a clean restart path
- local play scripts and logs showing no runtime exceptions during a 10 minute smoke test

Out of scope for this MVP:

- 2v2, FFA, lobby settings, remote matchmaking, cloud servers
- gamepad support
- multiple maps
- ranked balance
- persistent progression outside a match

---

## Current Baseline

Already present:

- `Assets/Scripts/Networking/ConnectionBootstrap.cs`
- `Assets/Scripts/Match/SetManager.cs`
- `Assets/Scripts/Match/RoundScoreState.cs`
- `Assets/Scripts/Match/RoundCombatGate.cs`
- `Assets/Scripts/Cards/CardCatalog.cs`
- `Assets/Scripts/Cards/CardDefinition.cs`
- `Assets/Scripts/Cards/PlayerCardCollection.cs`
- `Assets/Scripts/Combat/WeaponController.cs`
- `Assets/Scripts/Combat/WeaponAmmoState.cs`
- `Assets/Scripts/Player/PlayerShieldController.cs`
- `Assets/Scripts/UI/DraftHud.cs`
- `Assets/Scripts/UI/PlayerStatusDisplay.cs`
- local scripts: `scripts/play-local.ps1`, `scripts/stop-local.ps1`

---

## Phase 1: Lock the Match Loop

**Goal:** A set ends, the correct player drafts, cards apply, and the next set starts without manual intervention.

**Files:**

- Modify: `Assets/Scripts/Match/SetManager.cs`
- Modify: `Assets/Scripts/Match/RoundScoreState.cs`
- Modify: `Assets/Scripts/Match/RoundCombatGate.cs`
- Modify: `Assets/Scripts/Cards/PlayerCardLoadout.cs`
- Modify: `Assets/Scripts/UI/DraftHud.cs`
- Test: `Assets/Tests/EditMode/SetManagerRulesTests.cs`
- Test: `Assets/Tests/EditMode/RoundScoreStateTests.cs`
- Test: `Assets/Tests/EditMode/CardLoadoutTests.cs`

Tasks:

- [ ] Add tests for: set win, round win after 2 set wins, draw set does not increment score, loser draft is assigned to the correct player.
- [ ] Ensure `RoundCombatGate` blocks shooting/movement during countdown and draft, then releases both players together.
- [ ] Ensure selected card IDs persist in the losing player's `PlayerCardLoadout` before the next set spawns.
- [ ] Add a simple match-end state when a player reaches the MVP match win target.
- [ ] Run EditMode tests and local two-client play.

Acceptance:

- A player can lose a set, pick a card, and immediately feel it in the next set.
- No player can shoot during draft or between-set reset.
- Match state does not silently continue after match win.

---

## Phase 2: Expand the Card Set

**Goal:** Reach a minimum card set that makes drafts interesting without pretending final balance is done.

**Files:**

- Modify: `Assets/Scripts/Cards/CardId.cs`
- Modify: `Assets/Scripts/Cards/CardCategory.cs`
- Modify: `Assets/Scripts/Cards/CardDefinition.cs`
- Modify: `Assets/Scripts/Cards/CardCatalog.cs`
- Modify: `Assets/Scripts/Cards/CombatCardStats.cs`
- Modify: `Assets/Scripts/Cards/PlayerCardCollection.cs`
- Modify: `Assets/Scripts/Combat/WeaponController.cs`
- Modify: `Assets/Scripts/Combat/Bullet.cs`
- Modify: `Assets/Scripts/Player/PlayerShieldState.cs`
- Modify: `Assets/Scripts/Player/PlayerShieldController.cs`
- Test: `Assets/Tests/EditMode/CardDefinitionTests.cs`
- Test: `Assets/Tests/EditMode/CardLoadoutTests.cs`
- Test: `Assets/Tests/EditMode/WeaponControllerLaunchOrderTests.cs`
- Test: `Assets/Tests/EditMode/PlayerShieldStateTests.cs`

Card target:

- `Burst Shot`: cumulative, `ShotCount`, +1 burst shot, +1 magazine capacity
- `Split Shot`: cumulative, `ShotCount`, +1 projectile, +1 magazine capacity
- `Fast Reload`: cumulative, `Reload`, reload duration x0.85
- `Long Shield`: cumulative, `Defense`, shield active duration +0.15s
- `Shield Coolant`: cumulative, `Defense`, shield cooldown x0.85
- `Quick Rounds`: cumulative, `Projectile`, projectile speed x1.15
- `Heavy Rounds`: cumulative, `Projectile`, damage +1 or knockback +small, projectile speed x0.92
- `Ricochet`: cumulative, `Projectile`, +1 bullet bounce

Tasks:

- [ ] Add failing tests that `CardCatalog.All` contains the full MVP card list in reward order.
- [ ] Add failing tests for cumulative cards stacking, including `Ricochet`.
- [ ] Extend `CombatCardStats` with only fields needed by the target cards.
- [ ] Implement `PlayerCardCollection.BuildStats()` from catalog definitions.
- [ ] Wire projectile speed, damage/knockback, shield cooldown, and one-bounce behavior into runtime systems.
- [ ] Run EditMode tests, then local play to verify every card changes behavior.

Acceptance:

- Draft choices are no longer mostly cosmetic.
- Firing pattern cards also add magazine capacity.
- Ricochet stacks by adding one bounce per pickup.

---

## Phase 3: Make Draft Choice Readable

**Goal:** The player understands what they are choosing and what they already have.

**Files:**

- Modify: `Assets/Scripts/UI/DraftHud.cs`
- Modify: `Assets/Scripts/UI/DraftHudText.cs`
- Modify: `Assets/Scripts/Cards/CardText.cs`
- Test: `Assets/Tests/EditMode/DraftHudTextTests.cs`

Tasks:

- [ ] Show card name, short effect, category, and stack status.
- [ ] Show owned count for cumulative cards.
- [ ] Show `Owned` or disabled state for non-cumulative cards already held.
- [ ] Keep layout compact so the playfield remains dominant.
- [ ] Verify with local client screenshots or direct play.

Acceptance:

- The player can tell why `Burst Shot` and `Split Shot` stack.
- The player can tell that `Ricochet` stacks.
- Draft UI does not obscure combat after selection.

---

## Phase 4: Combat Feel Pass

**Goal:** Improve the minimum fun of shooting, blocking, and getting hit while keeping effects restrained.

**Files:**

- Modify: `Assets/Scripts/Combat/WeaponController.cs`
- Modify: `Assets/Scripts/Combat/Bullet.cs`
- Modify: `Assets/Scripts/Combat/HealthVisuals.cs`
- Modify: `Assets/Scripts/Player/PlayerShieldController.cs`
- Modify: `Assets/Scripts/UI/PlayerStatusDisplay.cs`
- Test: `Assets/Tests/EditMode/BulletVisualTests.cs`
- Test: `Assets/Tests/EditMode/HealthVisualsTests.cs`
- Test: `Assets/Tests/EditMode/PlayerStatusGaugeStateTests.cs`

Tasks:

- [ ] Tune bullet size, speed, and lifetime for readability at 1-screen scale.
- [ ] Add restrained hit confirmation: small flash or pulse, not a large effect.
- [ ] Add restrained shield block confirmation distinct from shield activation.
- [ ] Ensure ammo, reload, and shield cooldown indicators remain readable while moving.
- [ ] Run a 5 minute local play pass focused only on feel notes.

Acceptance:

- It is clear when a shot fired, hit, or got blocked.
- The playfield remains readable after spread/burst cards stack.

---

## Phase 5: End-to-End MVP Hardening

**Goal:** Make the prototype reliable enough to hand to someone else for one local play session.

**Files:**

- Modify: `docs/run-local.md`
- Modify: `README.md`
- Modify as needed: `scripts/play-local.ps1`
- Modify as needed: `scripts/stop-local.ps1`
- Test: existing EditMode test suite

Tasks:

- [ ] Document start/stop commands and expected logs.
- [ ] Run full EditMode test suite.
- [ ] Build Windows client.
- [ ] Start local server and two clients.
- [ ] Play for 10 minutes: at least 3 set transitions, 2 drafts, and 1 match end.
- [ ] Inspect server and client logs for `Exception`, `InvalidOperationException`, `Cannot complete action`, `error CS`, and `Error`.
- [ ] Fix any blocking issue found during the smoke test.

Acceptance:

- `EditMode` passes.
- Windows client build succeeds.
- Local play starts with server + 2 clients.
- Logs show no blocking runtime exceptions.
- A full match can end and restart.

---

## Suggested Order

1. Phase 1: Match loop
2. Phase 2: Card set expansion
3. Phase 3: Draft readability
4. Phase 4: Combat feel pass
5. Phase 5: Hardening

This order keeps the loop playable at every checkpoint. Card balance should wait until after Phase 2 and Phase 4, because before then the game does not have enough expressive surface to balance.
