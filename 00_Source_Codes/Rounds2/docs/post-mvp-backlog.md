# Rounds2 Post-MVP Backlog

## Status

Rounds2 MVP is accepted as complete as of 2026-05-18.

The MVP baseline is:

- local server plus two local clients
- 1v1 movement, aiming, shooting, reload, shield, HP, death, set reset
- loser-side draft after round loss
- 8 cumulative MVP cards with visible combat effects
- match end state
- Windows client build
- automated Bot smoke run reaching match end without matched runtime errors

Latest technical smoke result:

- Duration: 10 minutes
- Set winners: 9
- Round winners: 7
- Card drafts: 6
- Card rewards: 6
- Match winners: 1
- Matched errors: 0

## Priority 0: Blocking Fixes

Only use this category for issues that invalidate the MVP loop.

- Crash, exception, or disconnect during local 1v1.
- Match cannot reach `Match winner`.
- Draft cannot be selected or selected cards do not apply.
- Round or match state gets stuck after death, draft, or match end.
- Build or local launch scripts stop working.

## Priority 1: Play Feel

These should be the first post-MVP improvements because they affect whether the prototype is worth replaying.

- Improve hit readability without adding noisy effects.
- Improve shield activation/block readability while keeping it subtle.
- Tune bullet size, speed, trail, and lifetime after human play.
- Tune reload and shield cooldown visibility around the player.
- Review burst and split shot readability when stacked.
- Make self-hit ricochet understandable without requiring large VFX.

## Priority 2: Card Set and Draft Quality

Do this after a few human play sessions, not before.

- Adjust individual card numbers based on observed choices.
- Identify cards that are always picked or always ignored.
- Add card tags or grouping only if draft readability becomes a problem.
- Consider cards that change movement, shield behavior, or arena control.
- Decide whether every card should stay cumulative long-term.
- Add safeguards for extreme stacked values if they break readability.

## Priority 3: Match UX

These are not required for MVP, but they make the game easier to hand to someone else.

- Clearer match end presentation.
- Explicit restart/rematch input.
- Better distinction between set win, round win, and match win.
- Draft result display that confirms what changed.
- Minimal pause or countdown messaging before the next combat starts.

## Priority 4: Production Hardening

Do this before broader external testing.

- Replace the temporary client-as-server flow with a proper server build once Unity server support is installed.
- Add a repeatable automated long-run smoke script.
- Make local logs easier to inspect with one command.
- Document known launch arguments and development shortcuts.
- Reduce manual prefab/project setup risk.
- Clean and split the large MVP worktree into reviewable commits or PRs.

## Priority 5: Future Scope

Keep these out of the immediate post-MVP loop unless the current prototype proves fun.

- Online lobby.
- Remote matchmaking.
- 2v2 or free-for-all.
- Multiple arenas.
- Gamepad support.
- Persistent progression.
- Ranked or serious balance work.

## Recommended Next Step

Run 3 to 5 human-play rounds and record only concrete observations:

- What was unclear?
- What felt unresponsive?
- Which card choice felt obvious?
- Which death felt unfair or unreadable?
- Did the match end and restart path feel understandable?

Do not do broad balance work until these observations exist.
