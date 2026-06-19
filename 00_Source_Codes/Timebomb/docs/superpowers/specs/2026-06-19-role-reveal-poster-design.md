# Role Reveal Poster Design

## Goal

Replace the plain text role-reveal screen with a playful, comic-style role poster that makes the private role assignment feel like an event.

## Scope

- Generate three square illustrations for police, bomber, and spy roles.
- Store the final assets under `public/roles/` so Vite can serve them directly.
- Rework `RoleReveal` around a large role poster, a concise role-specific line, and the existing ready action.
- Add comic-style entrance motion and role-colored visual accents in client CSS.
- Extend client tests to cover role image accessibility and retain the ready-message behavior.

## Visual Direction

The screen uses a pop, comic-poster treatment rather than a dark spy thriller aesthetic.

- Police: cheerful time-travel investigator with a clock-shaped gadget, bright cyan and yellow.
- Bomber: mischievous masked saboteur presenting a cartoon time bomb, hot coral and yellow.
- Spy: playful covert agent with an oversized magnifying glass and disguise props, violet, teal, and pink.
- Assets contain no role names or other text. The application supplies all Japanese labels in HTML for reliable typography and accessibility.

## Component Design

`RoleReveal` owns a static role presentation map containing an asset path, alternate text, and short copy for each `Role`. It renders:

1. A private-role eyebrow and the existing role title.
2. An illustrated poster frame with role-specific classes.
3. A short flavor line below the role title.
4. The existing `ready` button, preserving its disabled state and message payload.

The component receives no new server data. Other players' roles remain absent from the client view, and the server-authoritative role handling is unchanged.

## Motion And Accessibility

- On mount, the poster enters with a brief scale, tilt, and confetti motion.
- `prefers-reduced-motion` displays the final poster state without animated movement.
- The role art has meaningful alternate text; decorative graphic layers are hidden from assistive technology.
- The ready button remains keyboard-accessible and visibly disabled after acknowledgement.

## Verification

- Add a component test that verifies each role renders its matching accessible poster image.
- Preserve the existing tests for ready dispatch and disabled ready state.
- Run the full test suite, TypeScript typecheck, and production build.
- Play through role reveal in the browser and inspect the desktop and mobile presentation.
