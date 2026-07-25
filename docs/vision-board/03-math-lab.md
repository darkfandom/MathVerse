# MathVerse Vision Board — Screen 3

## Interactive Mathematics Laboratory

---

## Application Frame

```
┌──────────────────────────────────────────────────────────────────────────┐
│ ┌──────────────────────────────────────────────────────────────────────┐ │
│ │ ◉ MathVerse   Math Lab: Calculus Explorer                           │ │
│ │ [Export▼] [Blender] [OBJ] [glTF] [STL] [Video] [GIF] [📷] [⚙] [⛶]│ │
│ └──────────────────────────────────────────────────────────────────────┘ │
│ ┌────────────────────────────────────────────┬─────────────────────────┐ │
│ │                                            │ LIVE EXPLANATION        │ │
│ │     LARGE INTERACTIVE MATHEMATICS CANVAS   │ ─────────────────────── │ │
│ │                                            │                         │ │
│ │     ┌─────────────────────────────────┐    │  🤖 AI Tutor            │ │
│ │     │         y = sin(x)             │    │                         │ │
│ │     │    ∿∿∿∿∿∿∿∿∿∿∿∿∿∿∿∿∿∿∿∿    │    │  "As the tangent line    │ │
│ │     │   ╱     ╲     ╱     ╲         │    │   moves along sin(x),   │ │
│ │  ╱──│──╱───────╲───╱───────╲────────│──  │   the derivative        │ │
│ │ ╱   │ ╱    │    ╲ ╱    │    ╲       │    │   oscillates between    │ │
│ │╱    │╱     │     ╳     │     ╲      │    │   cos(x), which is the  │ │
│ │     │  ●tangent│  ╱│╲   │      ╲     │    │   slope at each point." │ │
│ │─────┼───────┼──╱──┼──╲─┼───────╲────│    │                         │ │
│ │     │       │╱   │   ╲││        ╲   │    │  ───────────────────── │ │
│ │     │      ╱│    │    ╲│         ╲  │    │  Step 3 of 12          │ │
│ │     │     ╱ │    │     │          ╲ │    │                         │ │
│ │     │    ╱  │shaded   │           ╲│    │  Concepts:              │ │
│ │     │   ╱   │integral │            │    │  • Derivative            │ │
│ │     │  ╱    │  area   │            │    │  • Tangent Line          │ │
│ │     │ ╱     │         │            │    │  • Rate of Change        │ │
│ │     └─────────────────────────────────┘    │  • Chain Rule           │ │
│ │                                            │                         │ │
│ │     Annotations:                           │  Next:                  │ │
│ │     • Tangent line (green)                 │  → Second Derivative    │ │
│ │     • Shaded integral area (blue)          │                         │ │
│ │     • Taylor polynomial (orange dashed)    │  Previous:              │ │
│ │     • Riemann rectangles (purple)          │  ← Limits               │ │
│ │     • Newton iteration dots (red)          │                         │ │
│ │     • Derivative graph (yellow)            │                         │ │
│ │                                            │                         │ │
│ ├────────────────────────────────────────────┴─────────────────────────┤ │
│ │ ▶ ⏸ ⏹  ◀◀ ◀ ● ═══════════════════════════════ ▶ ▶▶  Speed: 1x  Loop│ │
│ │ Frame: 3750 / 7500    x = 3.14159    sin(x) = 0.00000              │ │
│ └──────────────────────────────────────────────────────────────────────┘ │
│  Ready  |  GPU: Active  |  Canvas: WebGL 2.0  |  60 FPS  |  4:32 PM    │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## Layout Structure

### Two-Column + Bottom Timeline
```
┌──────────────────────────────────┬──────────────────┐
│                                  │                  │
│         Interactive Canvas       │   AI Explanation │
│            (flex: 1)             │     320px        │
│                                  │                  │
│                                  │                  │
├──────────────────────────────────┴──────────────────┤
│              Playback Controls + Timeline 100px     │
└─────────────────────────────────────────────────────┘
```

| Panel | Size | Background |
|-------|------|------------|
| Canvas | Flex (fills remaining) | `#080810` |
| AI Panel | 320px fixed | `#0E0E18` |
| Timeline | Full width, 100px height | `#0B0B14` |

---

## Interactive Canvas

### Background
- **Base**: `#080810` (near-black)
- **Grid**: Cartesian coordinate grid
  - Major lines: 1px, `rgba(74,158,255,0.08)`
  - Minor lines: 1px, `rgba(74,158,255,0.03)`
  - Axis lines: 2px, `rgba(232,232,240,0.15)`
  - Tick marks: 8px, `rgba(232,232,240,0.2)`
  - Labels: Inter 11px mono, `#4A4A64`

### Mathematical Objects (Simultaneously Visible)

#### 1. Primary Function: y = sin(x)
- **Line**: 2.5px stroke, `#4A9EFF` (blue)
- **Range**: x ∈ [-2π, 4π], y ∈ [-1.5, 1.5]
- **Anti-aliased**: Yes
- **Animated**: Smooth curve drawing animation on entry

#### 2. Tangent Line
- **Line**: 1.5px stroke, `#06D6A0` (green)
- **Length**: Extends ±1.5 units from tangent point
- **Tangent point**: Large circle marker, 8px, `#06D6A0` fill
- **Animated**: Moves continuously along the curve
- **Speed**: One full period in 8 seconds
- **Angle indicator**: Small arc showing angle to x-axis, `#06D6A0` at 40% opacity

#### 3. Shaded Integral Area
- **Fill**: `rgba(74,158,255,0.12)` (blue, transparent)
- **Boundary**: 1px stroke, `#4A9EFF` at 30% opacity
- **Limits**: From x = 0 to x = π (dynamic, follows animation)
- **Animated**: Area grows/shrinks with the integration bounds
- **Label**: "∫₀^π sin(x) dx = 2" — floating above the area, Inter 14px, `#4A9EFF`

#### 4. Taylor Approximation
- **Line**: 1.5px stroke, `#FF6B35` (orange), dashed pattern (6px dash, 4px gap)
- **Order**: Dynamic — increases with animation frame
  - Frame 0-1500: 1st order (linear)
  - Frame 1500-3000: 3rd order
  - Frame 3000-4500: 5th order
  - Frame 4500-6000: 7th order
  - Frame 6000-7500: 9th order
- **Label**: "Tₙ(x) = x - x³/3! + x⁵/5! - ..." — Inter 12px, `#FF6B35`

#### 5. Riemann Rectangles
- **Fill**: `rgba(139,92,246,0.08)` (purple, transparent)
- **Border**: 1px stroke, `#8B5CF6` at 40% opacity
- **Count**: 20 rectangles (adjustable via slider)
- **Type**: Left Riemann sum (toggleable: Left / Right / Midpoint)
- **Animated**: Rectangle count increases with animation, showing convergence

#### 6. Newton Iteration
- **Dots**: Series of points converging to root
- **Color**: `#FF4444` (red)
- **Size**: 6px circles
- **Lines**: Connecting tangent lines, 1px, `#FF4444` at 40% opacity
- **Animated**: Points appear one by one, converging
- **Label**: "x_{n+1} = x_n - f(x_n)/f'(x_n)" — Inter 12px, `#FF4444`

#### 7. Derivative Graph
- **Line**: 1.5px stroke, `#FFD700` (gold)
- **Position**: Slightly offset or overlaid
- **Animated**: Follows the tangent line's slope values
- **Legend**: "f'(x) = cos(x)" — Inter 12px, `#FFD700`

#### 8. Parametric Spiral
- **Line**: 1.5px stroke, `#06D6A0` (teal)
- **Form**: r = θ (Archimedean spiral)
- **Range**: θ ∈ [0, 6π]
- **Animated**: Drawing animation, revealing the spiral
- **Label**: "r = θ" — Inter 12px, `#06D6A0`

#### 9. Rotating 3D Surface (PiP)
- **Position**: Bottom-right corner of canvas, 180x180px
- **Background**: `#0B0B14` with border `#2A2A3E`
- **Content**: Small rotating 3D surface (z = sin(x) * cos(y))
- **Border-radius**: 12px
- **Camera**: Orbiting automatically
- **Shadow**: 0 8px 32px rgba(0,0,0,0.5)

### Canvas Interaction
- **Pan**: Middle mouse drag
- **Zoom**: Scroll wheel
- **Select object**: Left click
- **Context menu**: Right click → properties, hide, export
- **Hover**: Object highlight with glow

### Annotation Labels
- **Font**: Inter 12px, with math rendered via KaTeX
- **Background**: `rgba(14,14,24,0.85)`, backdrop-blur 8px
- **Border-radius**: 6px
- **Padding**: 6px 10px
- **Border**: `1px solid rgba(255,255,255,0.06)`
- **Connector**: Dashed line from label to object, 1px, matching object color

---

## Right Panel — Live AI Explanation

### Header
```
┌────────────────────────────┐
│  🤖 AI EXPLANATION         │
│  ───────────────────────── │
│  Powered by MathVerse AI   │
└────────────────────────────┘
```
- **Title**: Inter 12px 700, `#E8E8F0`
- **Subtitle**: Inter 10px 400, `#4A4A64`
- **Icon**: Sparkle emoji or custom icon, 16px

### Explanation Content
- **Font**: Inter 13px 400, `#E8E8F0`
- **Line height**: 1.6
- **Max width**: 296px (320px - 24px padding)
- **Math rendering**: Inline KaTeX, 13px, `#4A9EFF`
- **Sections**: Bold headers in `#E8E8F0`, body in `#7A7A96`

### Example Content Structure
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━
🤖 AI EXPLANATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━

As the tangent line moves along
y = sin(x), the derivative
changes continuously.

At x = 0:     slope = cos(0) = 1
At x = π/2:   slope = cos(π/2) = 0
At x = π:     slope = cos(π) = -1

━━━━━━━━━━━━━━━━━━━━━━━━━━━
CURRENT OBSERVATIONS
━━━━━━━━━━━━━━━━━━━━━━━━━━━

• Tangent angle: 45°
• Instantaneous rate: 1.0
• Integral accumulated: 2.0
• Taylor order: n = 7
• Newton convergence: 4 iterations

━━━━━━━━━━━━━━━━━━━━━━━━━━━
CONCEPTS
━━━━━━━━━━━━━━━━━━━━━━━━━━━

• Derivative
• Tangent Line
• Rate of Change
• Chain Rule

━━━━━━━━━━━━━━━━━━━━━━━━━━━
NAVIGATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━

Step 3 of 12

  ← Previous: Limits

  Next: Second Derivative →

━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### Step Navigation
- **Previous/Next buttons**: Full-width, 36px height, rounded 8px
- **Previous**: `#1A1A2E` bg, `#7A7A96` text
- **Next**: `#4A9EFF` bg, `#FFFFFF` text
- **Step indicator**: "Step 3 of 12" — Inter 11px, `#4A4A64`

### Concept Tags
- **Style**: Pill-shaped, 24px height
- **Background**: `rgba(74,158,255,0.08)`
- **Border**: `1px solid rgba(74,158,255,0.2)`
- **Text**: Inter 11px 500, `#4A9EFF`
- **Hover**: Brighten background
- **Click**: Highlights related objects on canvas

---

## Bottom Timeline & Playback Controls

### Transport Controls Row
```
┌──────────────────────────────────────────────────────────────────┐
│  ▶  ⏸  ⏹   ◀◀  ◀  ● ═══════════════════════════════ ▶  ▶▶   │
│         Speed: [1x ▼]   [Loop]   Frame: 3750 / 7500            │
│         x = 3.14159   sin(x) = 0.00000   f'(x) = -1.00000      │
└──────────────────────────────────────────────────────────────────┘
```

### Button Specifications

| Button | Size | Icon | Color | Action |
|--------|------|------|-------|--------|
| Play | 32x32 | ▶ | `#06D6A0` | Start animation |
| Pause | 32x32 | ⏸ | `#FF6B35` | Pause animation |
| Stop | 32x32 | ⏹ | `#FF4444` | Reset to frame 0 |
| Step Back | 28x28 | ◀ | `#7A7A96` | Back 1 frame |
| Step Forward | 28x28 | ▶ | `#7A7A96` | Forward 1 frame |
| Rewind | 28x28 | ◀◀ | `#7A7A96` | Back 10 frames |
| Fast Forward | 28x28 | ▶▶ | `#7A7A96` | Forward 10 frames |

### Timeline Slider
- **Track height**: 4px, rounded 2px, `#1A1A2E`
- **Fill**: `#4A9EFF`
- **Thumb**: 12px circle, `#4A9EFF`, white center dot
- **Hover**: Thumb scales to 14px
- **Frame markers**: Every 750 frames (15 seconds), 8px tick, `#2A2A3E`

### Live Readout Bar
- **Background**: `rgba(14,14,24,0.6)`
- **Border-radius**: 6px
- **Padding**: 6px 12px
- **Font**: Inter 11px mono
- **Content**: Current x value, function value, derivative value
- **Color**: `#7A7A96` labels, `#E8E8F0` values

### Speed Control
- **Style**: Dropdown
- **Options**: 0.25x, 0.5x, 1x, 2x, 4x
- **Default**: 1x
- **Font**: Inter 11px, `#7A7A96`
- **Active**: `#4A9EFF`

### Loop Toggle
- **Style**: Toggle button
- **Off**: `#7A7A96` text, transparent bg
- **On**: `#FF4444` dot + text, `rgba(255,68,68,0.08)` bg

---

## Top Toolbar

### Left Section
```
┌──────────────────────────────────────────────────────────┐
│  ◉ MathVerse   Math Lab: Calculus Explorer               │
└──────────────────────────────────────────────────────────┘
```
- App icon: 16x16, teal geometric mark
- App name: Inter 13px 600, `#E8E8F0`
- Separator: 1px vertical, `#2A2A3E`, 16px height
- Page title: Inter 13px 400, `#7A7A96`

### Center Section — Export Buttons
```
┌──────────────────────────────────────────────────────────┐
│  [Export▼] [Blender] [OBJ] [glTF] [STL] [Video] [GIF] [📷] │
└──────────────────────────────────────────────────────────┘
```

| Button | Icon | Tooltip | Accent |
|--------|------|---------|--------|
| Export | ▼ dropdown | Export options | Default |
| Export to Blender | Blender logo | Send to Blender | `#FF6B35` |
| Export OBJ | Cube | 3D mesh export | `#4A9EFF` |
| Export glTF | Globe | Web 3D format | `#06D6A0` |
| Export STL | Layers | 3D printing | `#8B5CF6` |
| Generate Video | Film | MP4 video | `#FF6B35` |
| Generate GIF | Image | Animated GIF | `#4A9EFF` |
| Take Screenshot | Camera | PNG screenshot | `#7A7A96` |

- **Button style**: 32px height, rounded 8px, `#1A1A2E` bg, 1px `#2A2A3E` border
- **Hover**: `#1E1E32` bg, accent border at 30% opacity
- **Active**: Accent bg at 10% opacity

### Right Section
```
┌──────────────────────────────────────────────────────────┐
│  [Theme▼]  [⛶ Fullscreen]                               │
└──────────────────────────────────────────────────────────┘
```

---

## Object Visibility Panel (Collapsible, Bottom-Left of Canvas)

```
┌─────────────────────────────────┐
│ VISIBLE OBJECTS                 │
│ ─────────────────────────────── │
│ ☑ y = sin(x)            [blue] │
│ ☑ Tangent Line         [green] │
│ ☑ Integral Area      [blue⊗]  │
│ ☑ Taylor Approx.     [orange]  │
│ ☑ Riemann Rects.     [purple]  │
│ ☑ Newton Iteration      [red]  │
│ ☑ Derivative Graph     [gold]  │
│ ☑ Parametric Spiral   [teal]   │
│ ☑ 3D Surface (PiP)            │
│ ─────────────────────────────── │
│ Grid: ☑    Axes: ☑    Labels: ☑│
└─────────────────────────────────┘
```

- **Position**: 12px from bottom-left, above timeline
- **Size**: 280px wide, collapsible
- **Background**: `rgba(14,14,24,0.85)`, backdrop-blur 16px
- **Border-radius**: 12px
- **Border**: `1px solid rgba(255,255,255,0.06)`
- **Checkbox**: 14x14px, custom styled, matching object color
- **Eye icon**: Toggle visibility on hover
- **Drag**: Reorder objects (layering order)

---

## Canvas Annotations — Floating Labels

### Label Style
```
┌──────────────────────────┐
│  y = sin(x)              │
│  Period: 2π              │
│  Amplitude: 1            │
└──────────────────────────┘
```

- **Background**: `rgba(14,14,24,0.85)`
- **Backdrop-filter**: blur(8px)
- **Border-radius**: 8px
- **Border**: `1px solid rgba(255,255,255,0.06)`
- **Padding**: 8px 12px
- **Font**: Inter 12px, math in KaTeX
- **Connector**: Dashed 1px line to object, matching color
- **Draggable**: Yes, with snap-to-grid
- **Collapse**: Double-click to minimize to icon

### Example Labels
| Object | Label Content | Color |
|--------|---------------|-------|
| sin(x) | "y = sin(x)" | `#4A9EFF` |
| Tangent | "slope = cos(x₀)" | `#06D6A0` |
| Integral | "∫₀^π sin(x)dx = 2" | `#4A9EFF` |
| Taylor | "T₇(x)" | `#FF6B35` |
| Riemann | "n = 20, Δx = π/20" | `#8B5CF6` |
| Newton | "x₄ ≈ π" | `#FF4444` |
| Derivative | "f'(x) = cos(x)" | `#FFD700` |
| Spiral | "r = θ" | `#06D6A0` |

---

## Animation Timeline Details

### Frame-by-Frame Behavior

| Frame Range | Tangent | Integral | Taylor | Riemann | Newton | 3D Surface |
|-------------|---------|----------|--------|---------|--------|------------|
| 0-750 | x ∈ [0, π/2] | Growing | n=1 | 5 rects | x₀=5.0 | Static |
| 750-1500 | x ∈ [π/2, π] | Full [0,π] | n=1 | 10 rects | x₁=3.1 | Rotating |
| 1500-2250 | x ∈ [π, 3π/2] | Growing [0,3π/2] | n=3 | 15 rects | x₂=3.14 | Rotating |
| 2250-3000 | x ∈ [3π/2, 2π] | Full [0,2π] | n=3 | 20 rects | x₃=π | Rotating |
| 3000-3750 | Second pass | Growing | n=5 | 25 rects | Converged | Rotating |
| 3750-4500 | Second pass | Full | n=5 | 30 rects | Converged | Rotating |
| 4500-5250 | Third pass | Growing | n=7 | 35 rects | Converged | Rotating |
| 5250-6000 | Third pass | Full | n=7 | 40 rects | Converged | Rotating |
| 6000-6750 | Fourth pass | Growing | n=9 | 45 rects | Converged | Rotating |
| 6750-7500 | Fourth pass | Full | n=9 | 50 rects | Converged | Rotating |

---

## Responsive Behavior

| Width | Layout |
|-------|--------|
| ≥ 1400px | Full two-column + timeline |
| 1000-1399px | AI panel collapses to floating overlay |
| < 1000px | AI panel hidden, hamburger menu reveals it |

---

## Accessibility

- All canvas objects have screen reader descriptions
- Keyboard shortcuts for all transport controls
- High contrast mode available
- Color-blind friendly palette option
- Minimum touch target: 44x44px
- Focus indicators on all interactive elements
- Alt+click on objects for detailed properties

---

## Performance Targets

| Metric | Target |
|--------|--------|
| Canvas FPS | 60 fps ( WebGL 2.0 ) |
| Particle count | Up to 10,000 |
| Riemann rectangles | Up to 500 |
| Animation smoothness | No dropped frames |
| AI response time | < 200ms |
| Object selection | < 16ms |
| Pan/Zoom | 60fps continuous |
