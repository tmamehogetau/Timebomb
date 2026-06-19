# Role Reveal Poster Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (\`- [ ]\`) syntax for tracking.

**Goal:** Turn the private role confirmation screen into an animated pop-comic poster using three generated role illustrations, without changing the ready protocol or exposing hidden roles.

**Architecture:** Vite serves three static files from \`public/roles/\`. \`RoleReveal\` owns a typed map from \`Role\` to presentational metadata and renders semantic HTML. CSS supplies the poster frame, role palettes, and optional motion; no server or shared view type changes are needed.

**Tech Stack:** React 19, TypeScript, Vite public assets, Vitest, Testing Library, CSS animations, built-in ImageGen.

---

## File Structure

- Create: \`public/roles/time-police.png\` — generated square illustration for the police role.
- Create: \`public/roles/time-bomber.png\` — generated square illustration for the bomber role.
- Create: \`public/roles/time-spy.png\` — generated square illustration for the spy role.
- Modify: \`src/client/components/RoleReveal.tsx\` — typed presentation data and semantic poster markup.
- Modify: \`src/client/styles.css\` — pop-comic layout, role colors, responsive and reduced-motion rules.
- Modify: \`tests/client/RoleReveal.test.tsx\` — poster accessibility coverage while retaining ready behavior.

### Task 1: Generate And Install Role Art

**Files:**
- Create: \`public/roles/time-police.png\`
- Create: \`public/roles/time-bomber.png\`
- Create: \`public/roles/time-spy.png\`

- [ ] **Step 1: Generate the police illustration with ImageGen**

Use this prompt:

\`\`\`text
Use case: illustration-story
Asset type: square role-reveal poster artwork for a browser board game
Primary request: a cheerful original time-travel police investigator holding a clock-shaped gadget
Scene/backdrop: floating clock hands, confetti, and comic speed lines on a clean graphic backdrop
Style/medium: polished 2D pop-comic illustration, thick expressive ink lines, playful board-game art
Composition/framing: centered half-body character with generous padding, square composition
Lighting/mood: bright, upbeat, celebratory
Color palette: cyan, electric blue, warm yellow, white
Constraints: no words, letters, numbers, logos, watermarks, or copyrighted characters
Avoid: photorealism, dark horror, guns, UI panels
\`\`\`

- [ ] **Step 2: Save the selected police output**

Save it as \`public/roles/time-police.png\`. Confirm it is square, text-free, and has clear margin for a CSS frame.

- [ ] **Step 3: Generate and save the bomber illustration**

Use this prompt and save the selected result as \`public/roles/time-bomber.png\`:

\`\`\`text
Use case: illustration-story
Asset type: square role-reveal poster artwork for a browser board game
Primary request: a mischievous original masked saboteur presenting a cartoon time bomb with a big round clock face, playful rather than threatening
Scene/backdrop: harmless comic burst, streamers, and zigzag action lines on a clean graphic backdrop
Style/medium: polished 2D pop-comic illustration, thick expressive ink lines, playful board-game art
Composition/framing: centered half-body character with generous padding, square composition
Lighting/mood: energetic, cheeky, celebratory
Color palette: coral red, orange, bright yellow, charcoal accents
Constraints: no words, letters, numbers, logos, watermarks, or copyrighted characters
Avoid: realistic explosives, gore, weapons, horror, UI panels
\`\`\`

- [ ] **Step 4: Generate and save the spy illustration**

Use this prompt and save the selected result as \`public/roles/time-spy.png\`:

\`\`\`text
Use case: illustration-story
Asset type: square role-reveal poster artwork for a browser board game
Primary request: a playful original undercover time spy peeking through an oversized magnifying glass while wearing a whimsical disguise
Scene/backdrop: graphic stars, confetti, and comic speed lines on a clean backdrop
Style/medium: polished 2D pop-comic illustration, thick expressive ink lines, playful board-game art
Composition/framing: centered half-body character with generous padding, square composition
Lighting/mood: curious, clever, celebratory
Color palette: violet, turquoise, hot pink, white
Constraints: no words, letters, numbers, logos, watermarks, or copyrighted characters
Avoid: photorealism, dark espionage, weapons, UI panels
\`\`\`

- [ ] **Step 5: Verify installed assets**

Run:

\`\`\`powershell
Get-ChildItem public/roles | Select-Object Name, Length
\`\`\`

Expected: all three named PNG files have non-zero file sizes.

- [ ] **Step 6: Commit the art**

\`\`\`powershell
git add public/roles/time-police.png public/roles/time-bomber.png public/roles/time-spy.png
git commit -m "Add pop comic role artwork"
\`\`\`

### Task 2: Add The Failing Poster Test

**Files:**
- Modify: \`tests/client/RoleReveal.test.tsx\`

- [ ] **Step 1: Add this test after the ready-state test**

\`\`\`tsx
it.each([
  ["police", "時空警察のポスター", "/roles/time-police.png"],
  ["bomber", "ボマーのポスター", "/roles/time-bomber.png"],
  ["spy", "スパイのポスター", "/roles/time-spy.png"]
] as const)("%s の役職ポスターを表示する", (role, alt, src) => {
  render(<RoleReveal role={role} ready={false} send={() => {}} />);

  expect(screen.getByRole("img", { name: alt })).toHaveAttribute("src", src);
});
\`\`\`

- [ ] **Step 2: Run the test to verify RED**

Run: \`npm.cmd test -- tests/client/RoleReveal.test.tsx\`

Expected: FAIL because \`RoleReveal\` has no accessible image.

### Task 3: Render Typed Role Poster Markup

**Files:**
- Modify: \`src/client/components/RoleReveal.tsx\`
- Modify: \`tests/client/RoleReveal.test.tsx\`

- [ ] **Step 1: Add the presentation type and map**

Place this below \`ROLE_LABEL\`:

\`\`\`tsx
interface RolePresentation {
  art: string;
  alt: string;
  copy: string;
}

const ROLE_PRESENTATION: Record<Role, RolePresentation> = {
  police: {
    art: "/roles/time-police.png",
    alt: "時空警察のポスター",
    copy: "時空を守る捜査官。仲間と協力して爆弾を止めよう。"
  },
  bomber: {
    art: "/roles/time-bomber.png",
    alt: "ボマーのポスター",
    copy: "とびきり怪しい仕掛け人。最後まで正体を隠し通そう。"
  },
  spy: {
    art: "/roles/time-spy.png",
    alt: "スパイのポスター",
    copy: "混乱こそ好機。誰にも読まれず、結末を見届けよう。"
  }
};
\`\`\`

- [ ] **Step 2: Render the role poster and preserve the ready action**

At the beginning of \`RoleReveal\`, add \`const presentation = ROLE_PRESENTATION[role];\`. Replace the section contents with:

\`\`\`tsx
<div className={\`role-poster role-poster-\${role}\`}>
  <div className="role-poster-confetti" aria-hidden="true" />
  <p className="role-reveal-eyebrow">極秘ファイル</p>
  <div className="role-poster-art-frame">
    <img className="role-poster-art" src={presentation.art} alt={presentation.alt} />
  </div>
  <h2>あなたの役職</h2>
  <p className={\`role role-\${role}\`}>{ROLE_LABEL[role]}</p>
  <p className="role-poster-copy">{presentation.copy}</p>
</div>
<button disabled={ready} onClick={() => send({ type: "ready" })}>
  {ready ? "確認済み" : "任務を確認した"}
</button>
\`\`\`

- [ ] **Step 3: Run the focused test to verify GREEN**

Run: \`npm.cmd test -- tests/client/RoleReveal.test.tsx\`

Expected: PASS with three poster cases and the existing ready-state checks.

- [ ] **Step 4: Commit component behavior**

\`\`\`powershell
git add src/client/components/RoleReveal.tsx tests/client/RoleReveal.test.tsx
git commit -m "Show illustrated role posters"
\`\`\`

### Task 4: Style The Pop-Comic Poster

**Files:**
- Modify: \`src/client/styles.css\`

- [ ] **Step 1: Give role reveal a dedicated layout**

Remove \`.role-reveal\` from the generic lobby/game-over panel selector and add:

\`\`\`css
.role-reveal {
  width: min(100% - 2rem, 50rem);
  min-height: calc(100vh - 4rem);
  display: grid;
  place-content: center;
  gap: 1.1rem;
  margin: 2rem auto;
  padding: 1.4rem;
  text-align: center;
}
\`\`\`

- [ ] **Step 2: Add poster rules**

Add styles for \`.role-poster\`, \`.role-poster-art-frame\`, \`.role-poster-art\`, \`.role-poster-confetti\`, \`.role-reveal-eyebrow\`, \`.role-poster-copy\`, and the three \`.role-poster-<role>\` modifiers. Use a solid comic frame, offset dark shadow, clear edges, and role-specific accent colors. Animate only a brief \`role-poster-arrive\` scale-and-tilt entrance.

- [ ] **Step 3: Add responsive and reduced-motion rules**

\`\`\`css
@media (max-width: 520px) {
  .role-reveal {
    width: 100%;
    min-height: 100svh;
    margin: 0;
    padding: 1rem;
  }

  .role-poster-art-frame {
    width: min(78vw, 19rem);
  }
}

@media (prefers-reduced-motion: reduce) {
  .role-poster,
  .role-poster-confetti {
    animation: none;
  }
}
\`\`\`

- [ ] **Step 4: Run focused component tests**

Run: \`npm.cmd test -- tests/client/RoleReveal.test.tsx\`

Expected: PASS.

- [ ] **Step 5: Commit styling**

\`\`\`powershell
git add src/client/styles.css
git commit -m "Style pop comic role reveal"
\`\`\`

### Task 5: Browser Verification And Final Checks

**Files:**
- Verify: \`public/roles/time-police.png\`
- Verify: \`public/roles/time-bomber.png\`
- Verify: \`public/roles/time-spy.png\`
- Verify: \`src/client/components/RoleReveal.tsx\`
- Verify: \`src/client/styles.css\`

- [ ] **Step 1: Play through role reveal in a browser**

Create a room, start a game, and inspect the role reveal. Confirm the generated image appears, the Japanese role name remains legible, and the ready button disables after acknowledgement.

- [ ] **Step 2: Check a 390px viewport**

Confirm the image stays fully visible, the button is reachable, and text neither overlaps nor causes horizontal scrolling.

- [ ] **Step 3: Run final verification**

\`\`\`powershell
npm.cmd test
npm.cmd run typecheck
npm.cmd run build
git diff --check
\`\`\`

Expected: 0 test failures, no TypeScript errors, successful Vite/server build, and no whitespace errors.

- [ ] **Step 4: Inspect commit scope**

\`\`\`powershell
git status --short
git log -3 --oneline
\`\`\`

Expected: only the planned role-art, component/test, and styling changes are present in Timebomb.
