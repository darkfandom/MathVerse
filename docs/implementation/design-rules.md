# MathVerse Design Rules

## Permanent UI Rules

These rules are immutable. Every screen, every component, every interaction must follow them. They derive directly from the vision board and the product philosophy.

---

## 1. Layout Hierarchy

**Maximum 3 levels of visual hierarchy on any screen.**

| Level | Example | Purpose |
|-------|---------|---------|
| 1 | Page title, main content | What am I looking at? |
| 2 | Panels, sections, cards | What are the parts? |
| 3 | Controls, labels, values | What can I do? |

If a screen requires a 4th level, the layout is too complex. Flatten it.

---

## 2. One Primary Action Per Page

Every page has exactly one primary action — the thing the user is most likely to do.

| Page | Primary Action |
|------|---------------|
| Home | Open a module or continue a project |
| Evaluate | Type and evaluate an expression |
| Graph Studio | Plot a function |
| Visualization Studio | Load or create a visualization |
| Geometry Studio | Create a geometric object |
| Simulation Lab | Run a simulation |
| AI Assistant | Ask a question |
| Data Analysis | Import or create a dataset |
| Publications | Create a new publication |
| Settings | Adjust a setting |

Secondary actions exist but are visually subordinate.

---

## 3. Maximum 3 Clicks to Any Function

Every user-facing function must be reachable within 3 clicks from any screen:

- **Click 1**: Navigate to the relevant page (sidebar)
- **Click 2**: Open the relevant panel or tool
- **Click 3**: Execute the action

If a function requires more than 3 clicks, the information architecture is wrong.

---

## 4. No Dead Buttons

If a button exists, it must do something. Specifically:

- Every button must have a working click handler
- Every button must have a visible hover state
- Every button must have a visible pressed state
- Every button must have a tooltip if its purpose isn't self-evident from its icon/label
- Disabled buttons must show why they're disabled (tooltip or inline message)
- No button may navigate to a placeholder page without disclosure

---

## 5. No Hidden Functionality

All major functions must be discoverable through the UI without documentation:

- Visible in panels, toolbars, or context menus
- Reachable via keyboard shortcuts (shown in tooltips)
- Present in the navigation sidebar if it's a top-level feature
- Present in a toolbar if it's a page-level action
- Present in a context menu if it's an object-level action

No feature may exist only as a keyboard shortcut with no visible trigger.

---

## 6. No Command Palette as Primary Interaction

Keyboard-driven command palettes are acceptable as a power-user accelerator (Ctrl+K search), but must never be the primary way to discover or access features.

Every function accessible via command palette must also be accessible via visible UI elements.

---

## 7. One Toolbar Per Page

Each page may have at most one toolbar, positioned at the top of the page content area (below the global navigation bar).

The toolbar contains:
- Page title (left)
- Primary action button(s) (right)
- Context-specific toggles or filters (center, if needed)

Multiple toolbars, floating toolbars, and stacked toolbars are prohibited.

---

## 8. Consistent Spacing

Use the spacing scale from the vision board exclusively:

| Token | Value | When to Use |
|-------|-------|-------------|
| `--space-1` | 4px | Inline element gaps |
| `--space-2` | 8px | Tight grouping (icon + label) |
| `--space-3` | 12px | Form field gaps |
| `--space-4` | 16px | Card padding, list item gaps |
| `--space-5` | 20px | Section internal spacing |
| `--space-6` | 24px | Panel padding |
| `--space-8` | 32px | Page margin, major sections |
| `--space-10` | 40px | Hero sections |
| `--space-12` | 48px | Maximum spacing |

Never use arbitrary pixel values. Never use `margin: 13px` or `gap: 7px`.

---

## 9. Consistent Border Radius

| Element | Radius |
|---------|--------|
| Buttons, inputs | `--radius-sm` (6px) |
| Cards, panels | `--radius-md` (8px) |
| Modals, large cards | `--radius-lg` (12px) |
| Feature cards (Home) | `--radius-xl` (16px) |
| Pills, avatars | `--radius-full` (9999px) |

All corners of the same element type use the same radius. Mixed radii (e.g., 8px top, 4px bottom) are prohibited unless for a specific design pattern like notch/chip.

---

## 10. Color Palette Compliance

Only colors from the vision board palette may be used:

### Backgrounds
- `#0B0B12` — Canvas
- `#12121E` — Surface
- `#1A1A2E` — Surface Elevated
- `#080810` — Viewport (3D)
- `#0E0E18` — Side panels
- `#0B0B14` — Timeline

### Borders
- `#2A2A3E` — Default border
- `rgba(255,255,255,0.06)` — Glass border
- `rgba(255,255,255,0.04)` — Subtle separator

### Text
- `#E8E8F0` — Primary
- `#7A7A96` — Secondary
- `#4A4A64` — Tertiary

### Accents
- `#4A9EFF` — Blue (primary action, math objects)
- `#8B5CF6` — Purple (AI, tensor fields)
- `#06D6A0` — Teal (success, geometry)
- `#FF6B35` — Orange (warnings, simulations)
- `#FF4444` — Red (errors, destructive)
- `#FFD700` — Gold (derivatives, highlights)

No other colors. No custom brand colors. No theme-dependent palettes. The dark theme is the only theme.

---

## 11. Typography Scale Compliance

Use only the defined type roles:

| Role | Size | Weight | Font |
|------|------|--------|------|
| Display | 32px | 300 | Inter |
| H1 | 28px | 300 | Inter |
| H2 | 20px | 500 | Inter |
| H3 | 16px | 600 | Inter |
| Body | 14px | 400 | Inter |
| Caption | 12px | 400 | Inter |
| Small | 11px | 400 | Inter |
| Mono | 12px | 400 | JetBrains Mono / Consolas |

Never use font sizes outside this scale. Never use non-Inter fonts for UI text.

---

## 12. Icon Consistency

- All icons use 1.5px stroke weight
- All icons use round line caps and joins
- Sidebar icons: 20px
- Button icons: 16px
- Inline icons: 14px
- Icon color always inherits from parent text color
- No filled icons unless representing an active/selected state

---

## 13. Glassmorphism Rules

Glassmorphism is used for:
- Floating panels over 3D viewports
- Search results dropdown
- Context menus
- Tooltips with rich content

Glassmorphism is NOT used for:
- Sidebars
- Main content areas
- Cards in lists
- Form elements

Standard glass:
```css
background: rgba(14, 14, 24, 0.85);
backdrop-filter: blur(20px);
border: 1px solid rgba(255, 255, 255, 0.06);
box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
```

---

## 14. Status Indicators

Every long-running operation must show status:

| Status | Visual |
|--------|--------|
| Ready | Green dot + "Ready" in status bar |
| Loading | Spinner + "Loading..." in the relevant area |
| Processing | Progress bar or percentage |
| Error | Red banner with description and retry button |
| Success | Teal checkmark, auto-dismiss after 3s |
| Warning | Orange triangle with description |

---

## 15. Empty States

Every page with dynamic content must have a designed empty state:

- **Illustration**: Subtle, abstract, non-distracting (line art or geometric)
- **Title**: What this page does (1 sentence)
- **Description**: How to get started (1-2 sentences)
- **Primary action**: Button to create the first item
- **No content**: Never show a blank white/dark area

---

## 16. Error States

Every operation that can fail must have a designed error state:

- **Inline error**: Below the failing control, red text, specific message
- **Page error**: Centered, icon + title + description + retry button
- **Toast notification**: Non-blocking, auto-dismiss after 5s, with action button
- **Never**: Raw exception messages, stack traces, or debug output

---

## 17. Loading States

Every data-fetching or computation must show loading:

- **Skeleton screens**: Preferred for content areas (gray pulsing placeholders)
- **Spinners**: For buttons and small operations
- **Progress bars**: For operations > 2 seconds with known duration
- **Never**: Blank screens with no feedback

---

## 18. Responsive Behavior

| Width | Behavior |
|-------|----------|
| ≥ 1400px | Full layout with all panels visible |
| 1000-1399px | Side panels collapse to overlays |
| < 1000px | Side panels hidden, hamburger menu |

This applies to the application window, not web responsive design. The minimum supported window size is 1024x768.

---

## 19. No IDE Patterns

The following patterns are explicitly forbidden:

- Solution explorer / file tree as primary navigation
- Tab-based document interface (like VS Code tabs)
- Terminal / console panel
- Debug console
- Output window
- IntelliSense / autocomplete popups as primary interaction
- Property grid (like Visual Studio Properties window)
- Error list / task list panel
- Find and replace dialog as primary search

---

## 20. Card Design Standard

All cards follow this template:

```
┌────────────────────────────────┐
│  [Icon]  Title                  │
│          Subtitle               │
│                                 │
│          Description text       │
│          that may span two      │
│          lines maximum          │
│                                 │
└────────────────────────────────┘
```

- Padding: 20px
- Border radius: 16px (feature cards) or 12px (list cards)
- Border: `1px solid #2A2A3E`
- Background: `#12121E`
- Hover: `translateY(-2px)`, shadow elevation, border glow
- Click: `scale(0.98)` for 100ms
