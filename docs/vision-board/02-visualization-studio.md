# MathVerse Vision Board — Screen 2

## Advanced Visualization Studio

---

## Application Frame

```
┌──────────────────────────────────────────────────────────────────────────┐
│ ┌──────────────────────────────────────────────────────────────────────┐ │
│ │ ◉ MathVerse   Visualization Studio    [Export] [Theme] [Fullscreen] │ │
│ └──────────────────────────────────────────────────────────────────────┘ │
│ ┌───────┐┌──────────────────────────────────────────┐┌────────────────┐ │
│ │ LIBRARY││                                          ││  PROPERTIES    │ │
│ │        ││                                          ││                │ │
│ │ 📂 Calc ││     ┌──────────────────────────────┐    ││ ┌────────────┐ │ │
│ │ 📂 Alg  ││     │                              │    ││ │ Animation  │ │ │
│ │ 📂 Geo  ││     │                              │    ││ │ ▶ Play     │ │ │
│ │ 📂 DE   ││     │     MASSIVE 3D VIEWPORT      │    ││ │ ⏸ Pause    │ │ │
│ │ 📂 Tens ││     │                              │    ││ │ ⏹ Stop     │ │ │
│ │ 📂 Fluid││     │     ● Black hole             │    ││ │ ────────── │ │ │
│ │ 📂 EM   ││     │     ● Gravitational field    │    ││ │ Timeline   │ │ │
│ │ 📂 Fract││     │     ● Spacetime curvature    │    ││ │ ├─●────────│ │ │
│ │ 📂 Four ││     │     ● Particle trajectories  │    ││ │ 0:00 ─ 2:30│ │ │
│ │ 📂 QM   ││     │     ● Glowing math surface   │    ││ └────────────┘ │ │
│ │ 📂 GR   ││     │     ● Vector field arrows    │    ││                │ │
│ │        ││     │     ● Orbiting camera         │    ││ ┌────────────┐ │ │
│ │ ────── ││     │                              │    ││ │ Camera     │ │ │
│ │ Search ││     │      🔵 ✦ ✦ ✦ ✦ ✦            │    ││ │ Orbit ●    │ │ │
│ │ [____] ││     │     ✦     ◉     ✦            │    ││ │ Pan   ○    │ │ │
│ │        ││     │      ✦ ✦ ✦ ✦ ✦            │    ││ │ Zoom  ○    │ │ │
│ │ ACTIVE ││     │                              │    ││ │ FOV: 45°   │ │ │
│ │ ● GR   ││     │                              │    ││ └────────────┘ │ │
│ │        ││     └──────────────────────────────┘    ││                │ │
│ │        ││                                          ││ ┌────────────┐ │ │
│ │        ││ ┌────────────────────────────────────┐  ││ │ Lighting   │ │ │
│ │        ││ │ ◀ 0:00 ═══════●══════════════ 2:30 │▶ ││ │ Key    ●   │ │ │
│ │        ││ │ ▶ ⏸ ⏹  ⏪ ⏩ 🔴 Loop  Speed: 1.0x  │  ││ │ Fill   ○   │ │ │
│ │        ││ └────────────────────────────────────┘  ││ │ Rim    ○   │ │ │
│ │        ││                                          ││ │ Int: 80%   │ │ │
│ │        ││                                          ││ └────────────┘ │ │
│ │        ││                                          ││                │ │
│ │        ││                                          ││ ┌────────────┐ │ │
│ │        ││                                          ││ │ Material   │ │ │
│ │        ││                                          ││ │ Type  [▼] │ │ │
│ │        ││                                          ││ │ Color  ●●●│ │ │
│ │        ││                                          ││ │ Opacity 85%│ │ │
│ │        ││                                          ││ │ Roughness  │ │ │
│ │        ││                                          ││ │ Metal  60% │ │ │
│ │        ││                                          ││ └────────────┘ │ │
│ │        ││                                          ││                │ │
│ │        ││                                          ││ ┌────────────┐ │ │
│ │        ││                                          ││ │ Physics    │ │ │
│ │        ││                                          ││ │ Speed  1.0 │ │ │
│ │        ││                                          ││ │ Mass   ●●●│ │ │
│ │        ││                                          ││ │ Trail  85% │ │ │
│ │        ││                                          ││ │ Particles  │ │ │
│ │        ││                                          ││ │ Count 5000 │ │ │
│ └───────┘└──────────────────────────────────────────┘└────────────────┘ │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## Layout Structure

### Three-Column Layout
```
┌──────┬────────────────────────────┬──────────────┐
│ 260px│         Fluid              │    280px     │
│ Left │       Viewport             │    Right     │
│Panel │         (flex)             │    Panel     │
│      │                            │              │
│      ├────────────────────────────┤              │
│      │   Bottom Timeline 80px    │              │
└──────┴────────────────────────────┴──────────────┘
```

| Panel | Width | Background |
|-------|-------|------------|
| Left (Library) | 260px fixed | `#0E0E18` |
| Center (Viewport) | Fluid (flex: 1) | `#080810` |
| Right (Properties) | 280px fixed | `#0E0E18` |
| Bottom (Timeline) | Full width, 80px height | `#0B0B14` |

---

## 3D Viewport

### Background
- **Base**: `#080810` (near-black)
- **Grid**: Subtle reference grid, 1px lines, `rgba(74,158,255,0.04)`, fades at distance
- **Origin marker**: Small 3-axis indicator, bottom-left corner, 60x60px
  - X axis: `#FF4444`
  - Y axis: `#44FF44`
  - Z axis: `#4488FF`

### Scene Content — Black Hole Visualization

**Central object: Black hole**
- Core: Pure black sphere with event horizon shimmer
- Accretion disk: Glowing orange-blue ring, `#FF6B35` to `#4A9EFF` gradient
- Gravitational lensing: Background stars distorted around the edge
- Size: ~40% of viewport width

**Gravitational field lines**
- Curved arrows emanating from black hole
- Color: `#8B5CF6` with 60% opacity
- Animated: flowing inward toward event horizon
- 12-16 field lines total

**Particle trajectories**
- Small glowing dots orbiting the black hole
- Trail effect: 80% opacity fading to transparent over 2 seconds
- Colors: `#4A9EFF` (blue particles), `#06D6A0` (green particles), `#FF6B35` (orange particles)
- Count: ~500 visible particles
- Physics-based motion: Kepler orbits with precession

**Spacetime curvature grid**
- Warped mesh plane below the black hole
- Grid deformation increases near the center
- Color: `#4A9EFF` at 20% opacity
- Animated: subtle undulation

**Vector field overlay**
- Small arrows showing gravitational field direction
- Color: `rgba(139,92,246,0.4)`
- Density: one arrow per ~50px

**Animated equations**
- Floating mathematical annotations near key features
- Font: Latin Modern Math or KaTeX, 14px, `#E8E8F0` at 70% opacity
- Examples: `F = GMm/r²`, `ds² = -c²dt² + dx²`, `∇·E = ρ/ε₀`
- Position: anchored to 3D points, billboarding toward camera

**Camera**
- Orbital motion around the black hole
- Speed: one full rotation in ~30 seconds
- FOV: 45 degrees
- Near clip: 0.1, Far clip: 10000

### Viewport Controls (Overlay, Bottom-Left)
```
┌──────────────────────────────────┐
│ 🔲 Perspective  │  FOV: 45°    │
│ Grid: On        │  Axes: On    │
│ ───────────────────────────────  │
│ Render: PBR     │  Samples: 4x │
└──────────────────────────────────┘
```
- Floating glassmorphism panel
- Position: 12px from bottom-left corner
- Background: `rgba(14,14,24,0.8)`, backdrop-blur 16px

---

## Left Panel — Visualization Library

### Header
- **Title**: "LIBRARY" — Inter 11px 700, `#4A4A64`, uppercase, letter-spacing 2px
- **Height**: 44px
- **Border-bottom**: `1px solid #1A1A2E`

### Category Tree
- **Item height**: 36px
- **Left padding**: 16px (base), 28px (children)
- **Font**: Inter 12px 400, `#7A7A96`
- **Active item**: `#4A9EFF` text, left 2px accent bar, bg `rgba(74,158,255,0.06)`
- **Hover**: bg `rgba(255,255,255,0.03)`, text `#E8E8F0`
- **Expand/collapse**: 16px chevron, 150ms rotation animation

| Category | Icon | Items |
|----------|------|-------|
| Calculus | ƒ(x) | Derivatives, Integrals, Limits, Series |
| Algebra | Σ | Polynomials, Matrices, Groups, Rings |
| Geometry | △ | Euclidean, Non-Euclidean, Projective, Differential |
| Differential Equations | ∂ | ODE, PDE, Systems, Boundary Value |
| Tensor Fields | T | Riemann, Ricci, Christoffel, Curvature |
| Fluid Dynamics | ≋ | Navier-Stokes, Turbulence, Laminar, Vortex |
| Electromagnetism | ⚡ | Maxwell, Waveguide, Antenna, Plasma |
| Fractals | ❋ | Mandelbrot, Julia, Sierpinski, Koch |
| Fourier Analysis | ~ | Transform, Series, Spectral, Window |
| Quantum Mechanics | ℏ | Wave Function, Schrodinger, Dirac, QFT |
| General Relativity | ◉ | Schwarzschild, Kerr, Gravitational Waves |

### Search
- **Position**: Below header
- **Height**: 32px
- **Background**: `#12121E`
- **Border**: `1px solid #2A2A3E`
- **Border-radius**: 8px
- **Placeholder**: "Search visualizations..."

### Active State Indicator
- Bottom of panel: "Active: General Relativity" with green dot
- Background: `rgba(6,214,160,0.06)`
- Border: `1px solid rgba(6,214,160,0.15)`

---

## Right Panel — Properties

### Section Headers
- **Font**: Inter 11px 700, `#4A4A64`, uppercase, letter-spacing 1.5px
- **Height**: 40px
- **Border-bottom**: `1px solid #1A1A2E`

### Animation Controls Section

```
┌────────────────────────────┐
│  ANIMATION                 │
│  ───────────────────────── │
│  ┌──┐ ┌──┐ ┌──┐           │
│  │▶ │ │⏸│ │⏹│           │
│  └──┘ └──┘ └──┘           │
│                            │
│  Timeline                  │
│  ├─●═══════════════════┤  │
│  0:00            2:30      │
│                            │
│  Duration: 150s            │
│  Frame: 3750 / 7500       │
│  FPS: 60                   │
└────────────────────────────┘
```

- **Play button**: 32x32px, circle, `#06D6A0` bg when playing
- **Pause button**: 32x32px, circle, `#FF6B35` bg when paused
- **Stop button**: 32x32px, circle, `#FF4444` bg
- **Timeline slider**: 4px track, 12px thumb, `#4A9EFF` fill
- **Time display**: Inter 11px mono, `#7A7A96`

### Camera Controls Section

```
┌────────────────────────────┐
│  CAMERA                    │
│  ───────────────────────── │
│  Mode                      │
│  ┌──────────────────────┐  │
│  │ ● Orbit    ○ Pan     │  │
│  │ ○ Zoom     ○ Free    │  │
│  └──────────────────────┘  │
│                            │
│  Target                    │
│  X: 0.000  Y: 0.000       │
│  Z: 0.000                  │
│                            │
│  Distance: 12.5            │
│  Azimuth: 45.0°            │
│  Elevation: 30.0°          │
│  FOV: 45°                  │
└────────────────────────────┘
```

- **Radio buttons**: Custom styled, 16px outer, 8px inner dot, `#4A9EFF` active
- **Numeric inputs**: 80px wide, right-aligned, monospace, `#12121E` bg

### Lighting Controls Section

```
┌────────────────────────────┐
│  LIGHTING                  │
│  ───────────────────────── │
│  Key Light                 │
│  Color  [●●●]  Int: 80%   │
│  ─────────────────────     │
│  Fill Light                │
│  Color  [●●●]  Int: 40%   │
│  ─────────────────────     │
│  Rim Light                 │
│  Color  [●●●]  Int: 60%   │
│  ─────────────────────     │
│  Ambient: 15%              │
│  Environment: Studio HDRI  │
└────────────────────────────┘
```

- **Color swatches**: 20x20px circles, click opens color picker
- **Intensity sliders**: 120px track, 10px height, rounded 5px

### Material Controls Section

```
┌────────────────────────────┐
│  MATERIAL                  │
│  ───────────────────────── │
│  Surface Type              │
│  ┌──────────────────────┐  │
│  │ PBR Metalness     [▼]│  │
│  └──────────────────────┘  │
│                            │
│  Base Color    [●●●]       │
│  ─────────────────────     │
│  Opacity        ━━━●━━ 85% │
│  ─────────────────────     │
│  Roughness      ━━●━━━ 35% │
│  ─────────────────────     │
│  Metalness      ━━━●━ 60%  │
│  ─────────────────────     │
│  Emission       ━●━━━ 15%  │
│  ─────────────────────     │
│  IOR: 1.45                 │
│  Subsurface: Off           │
└────────────────────────────┘
```

### Physics Controls Section

```
┌────────────────────────────┐
│  PHYSICS                   │
│  ───────────────────────── │
│  Simulation Speed          │
│  ━━━━━●━━━━━━━━━━ 1.0x     │
│  ─────────────────────     │
│  Particle Count            │
│  ━━━━━━━━●━━━━━━━ 5000    │
│  ─────────────────────     │
│  Trail Length              │
│  ━━━━━━━━━●━━━━━━ 85%     │
│  ─────────────────────     │
│  Gravity: 6.67e-11         │
│  Mass: 1.989e30 kg         │
│  Spin: 0.998               │
│  Charge: 0                 │
└────────────────────────────┘
```

---

## Bottom Timeline (Blender-Style)

- **Height**: 80px
- **Background**: `#0B0B14`
- **Top border**: `1px solid #1A1A2E`

### Transport Controls (Left)
```
┌──────────────────────────────────────────────────────┐
│  ⏪  ⏪  ▶  ⏸  ⏹  ⏩  ⏩  │  🔴  Loop  Speed: 1.0x  │
└──────────────────────────────────────────────────────┘
```
- **Buttons**: 24x24px, `#7A7A96` default, `#E8E8F0` hover, `#4A9EFF` active
- **Loop toggle**: Red dot when active, `#FF4444`
- **Speed control**: Dropdown: 0.25x, 0.5x, 1x, 2x, 4x

### Timeline Track (Center)
```
┌──────────────────────────────────────────────────────┐
│  Frame: 3750 / 7500    0:00:00 / 0:02:30            │
│  ├─┬───┬───┬───┬───┬───┬───┬───┬───┬───┤           │
│  0  15s 30s 45s 60s 75s 90s  2m 2m15 2m30           │
│                    ● (playhead)                       │
│  ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░░░░░░░░░░░░░░           │
│  (rendered portion)    (unrendered)                   │
└──────────────────────────────────────────────────────┘
```
- **Playhead**: Vertical line, `#FF4444`, 2px width, with triangle handle at top
- **Frame numbers**: Inter 10px mono, `#4A4A64`
- **Rendered indicator**: `#4A9EFF` at 20% opacity
- **Current frame display**: Inter 11px mono, `#E8E8F0`

### Keyframe Markers
- Diamond shapes at keyframe positions
- Color: `#FF6B35`
- Size: 8x8px
- Hover: Tooltip showing frame number and property

---

## Viewport Overlay Controls

### Top-Right Corner
```
┌──────────────────┐
│ Render: PBR  [▼] │
│ Samples: 4x  [▼] │
│ Resolution: 4K   │
│ FPS: 60          │
└──────────────────┘
```

### Gizmo (Top-Right, 80x80px)
- 3D axis indicator showing camera orientation
- X: Red, Y: Green, Z: Blue
- Labels: 10px, bold
- Interactive: Click axis to snap camera view

---

## Export Button (Top Toolbar)

```
┌──────────────────┐
│ Export        [▼]│
├──────────────────┤
│ Export to Blender│
│ Export OBJ       │
│ Export glTF      │
│ Export STL       │
│ Generate Video   │
│ Generate GIF     │
│ Take Screenshot  │
└──────────────────┘
```
- **Dropdown**: Glassmorphism panel, 200px wide
- **Items**: 36px height, 16px icon + 13px text
- **Hover**: `rgba(255,255,255,0.04)` bg
- **Divider**: Between format types and media types

---

## Animation Details

| Element | Property | Duration | Easing |
|---------|----------|----------|--------|
| Panel resize | width | 200ms | cubic-bezier(0.4, 0, 0.2, 1) |
| Playhead | left | 16ms (60fps) | linear |
| Particle trails | opacity fade | 2000ms | ease-out |
| Camera orbit | rotation | continuous | linear |
| Property slider | value | 50ms | ease-out |
| Dropdown open | height, opacity | 150ms | cubic-bezier(0.4, 0, 0.2, 1) |
| Black hole shimmer | opacity | 3000ms | ease-in-out (loop) |

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Space | Play/Pause |
| R | Reset camera |
| G | Toggle grid |
| H | Toggle HUD |
| F | Frame selected |
| N | Toggle right panel |
| T | Toggle left panel |
| Ctrl+E | Export dialog |
| Ctrl+Shift+S | Screenshot |
| 1/3/7 | Front/Right/Top view |
| Scroll | Zoom |
| Middle drag | Pan |
| Left drag | Orbit |
