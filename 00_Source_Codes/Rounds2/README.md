# Rounds2

PvP top-down roguelite action shooter.

## Current Status

The first local 1v1 MVP is accepted as complete.

- local dedicated server
- two clients
- movement
- aim
- shooting, reload, and shield
- HP / death
- set, round, and match progression
- loser-side card draft
- 8 MVP cards with visible combat effects
- match end state

Latest Bot smoke run reached match end during a 10 minute run with no matched runtime errors.

## Current Goal

Post-MVP iteration:

- improve play feel after human playtesting
- keep effects readable as cards stack
- harden local launch and logging
- prepare the worktree for reviewable commits or PRs

## Project

- Engine: Unity
- Language: C#
- Networking: FishNet
- Platform: Windows

## Docs

- `docs/run-local.md`: local build, play, and smoke-test commands
- `docs/post-mvp-backlog.md`: post-MVP priorities
