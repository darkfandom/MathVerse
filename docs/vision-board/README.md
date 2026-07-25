# MathVerse Vision Board

## Overview

Three concept screens defining the visual identity and interaction model for MathVerse — a next-generation scientific mathematics platform.

---

## Design Philosophy

**Not an IDE. Not a code editor. Not Visual Studio.**

MathVerse is a visual-first mathematics platform where every interaction is spatial, graphical, and intuitive. Think Blender meets Mathematica meets Apple design language.

### Core Principles

1. **Visual First** — Every mathematical concept has a visual representation
2. **Zero Code** — No terminals, no command lines, no syntax
3. **Progressive Disclosure** — Simple by default, powerful on demand
4. **Spatial Computing** — 3D viewport as the primary workspace
5. **AI-Native** — AI explains, suggests, and generates in real-time
6. **Premium Feel** — Apple-quality polish, Blender-level power

### Design DNA

| Influence | What We Take |
|-----------|-------------|
| Blender | 3D viewport, property panels, timeline, keyboard shortcuts |
| MATLAB | Mathematical rigor, visualization depth, numerical precision |
| Wolfram Mathematica | Symbolic computation, notebook-style exploration |
| GeoGebra | Interactive geometry, drag-and-manipulate interface |
| Unreal Engine | Professional viewport, gizmos, material system |
| Apple | Typography, spacing, glassmorphism, attention to detail |
| Adobe CC | Tool panels, workspace management, export workflows |

---

## Screen Specifications

### Screen 1 — Home Workspace
**File**: `01-home-workspace.md`

The landing experience. Clean, inviting, and immediately useful.

- Dark canvas background (`#0B0B12`)
- Left sidebar with icon-only navigation (56px)
- Welcome section with status summary
- 8 module cards in 4x2 grid
- Recent projects, equations, and favorite visualizations
- Global search (Ctrl+K)
- Professional status bar

### Screen 2 — Visualization Studio
**File**: `02-visualization-studio.md`

The flagship feature. A massive 3D viewport dominating the screen.

- Three-column layout: Library (260px) | Viewport (fluid) | Properties (280px)
- Full 3D scene: black hole, gravitational field, particles, vectors
- Blender-style bottom timeline with transport controls
- Left panel: categorized visualization library (11 categories)
- Right panel: Animation, Camera, Lighting, Material, Physics controls
- Export to Blender, OBJ, glTF, STL, Video, GIF, Screenshot

### Screen 3 — Interactive Mathematics Laboratory
**File**: `03-math-lab.md`

Where users explore mathematics visually and interactively.

- Two-column layout: Canvas (fluid) | AI Panel (320px)
- Simultaneously visible: sin(x), tangent, integral, Taylor, Riemann, Newton, derivative, spiral, 3D surface
- All objects animated and interactive
- Real-time AI explanation panel
- Playback controls with frame-by-frame navigation
- Object visibility toggles
- PiP 3D surface preview

---

## Color System

### Primary Palette

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| Canvas | `#0B0B12` | 11, 11, 18 | Main background |
| Surface | `#12121E` | 18, 18, 30 | Cards, panels |
| Surface Elevated | `#1A1A2E` | 26, 26, 46 | Hover states, raised panels |
| Border | `#2A2A3E` | 42, 42, 62 | Separators, card borders |

### Text Palette

| Name | Hex | Usage |
|------|-----|-------|
| Text Primary | `#E8E8F0` | Headings, active text |
| Text Secondary | `#7A7A96` | Descriptions, labels |
| Text Tertiary | `#4A4A64` | Timestamps, metadata |

### Accent Palette

| Name | Hex | Usage |
|------|-----|-------|
| Accent Blue | `#4A9EFF` | Primary actions, math objects |
| Accent Purple | `#8B5CF6` | AI features, tensor fields |
| Accent Teal | `#06D6A0` | Success, geometry, particles |
| Accent Orange | `#FF6B35` | Warnings, simulations, Taylor |
| Accent Red | `#FF4444` | Errors, Newton iteration, playhead |
| Accent Gold | `#FFD700` | Derivatives, highlights |

### Glassmorphism

```css
/* Standard glass panel */
background: rgba(14, 14, 24, 0.85);
backdrop-filter: blur(20px);
border: 1px solid rgba(255, 255, 255, 0.06);
box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
border-radius: 12px;

/* Hover glass panel */
background: rgba(18, 18, 30, 0.9);
border: 1px solid rgba(255, 255, 255, 0.08);
box-shadow: 0 12px 40px rgba(0, 0, 0, 0.5);
```

---

## Typography

### Font Stack

```css
font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
font-feature-settings: 'cv01', 'cv02', 'cv03', 'cv04';
```

### Scale

| Role | Size | Weight | Line Height | Tracking |
|------|------|--------|-------------|----------|
| Display | 32px | 300 | 1.2 | -0.02em |
| H1 | 28px | 300 | 1.3 | -0.01em |
| H2 | 20px | 500 | 1.3 | 0 |
| H3 | 16px | 600 | 1.4 | 0 |
| Body | 14px | 400 | 1.5 | 0 |
| Caption | 12px | 400 | 1.4 | 0.01em |
| Small | 11px | 400 | 1.3 | 0.02em |
| Mono | 12px | 400 | 1.5 | 0.03em |

### Math Typography

```css
/* KaTeX for rendered equations */
font-family: 'KaTeX_Main', 'Latin Modern Math', 'STIX Two Math', serif;
font-size: 14px; /* inline */
font-size: 18px; /* display */
```

---

## Spacing & Layout

### Spacing Scale

| Token | Value |
|-------|-------|
| --space-1 | 4px |
| --space-2 | 8px |
| --space-3 | 12px |
| --space-4 | 16px |
| --space-5 | 20px |
| --space-6 | 24px |
| --space-8 | 32px |
| --space-10 | 40px |
| --space-12 | 48px |

### Border Radius

| Token | Value | Usage |
|-------|-------|-------|
| --radius-sm | 6px | Buttons, inputs |
| --radius-md | 8px | Cards, panels |
| --radius-lg | 12px | Modals, large cards |
| --radius-xl | 16px | Feature cards |
| --radius-full | 9999px | Pills, avatars |

### Shadows

```css
--shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.3);
--shadow-md: 0 4px 12px rgba(0, 0, 0, 0.4);
--shadow-lg: 0 8px 32px rgba(0, 0, 0, 0.5);
--shadow-xl: 0 16px 48px rgba(0, 0, 0, 0.6);
--shadow-glow-blue: 0 0 20px rgba(74, 158, 255, 0.15);
--shadow-glow-purple: 0 0 20px rgba(139, 92, 246, 0.12);
```

---

## Animation System

### Easing Curves

```css
--ease-default: cubic-bezier(0.4, 0, 0.2, 1);
--ease-in: cubic-bezier(0.4, 0, 1, 1);
--ease-out: cubic-bezier(0, 0, 0.2, 1);
--ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
--ease-spring: cubic-bezier(0.34, 1.56, 0.64, 1);
```

### Duration Scale

| Token | Value | Usage |
|-------|-------|-------|
| --duration-fast | 100ms | Micro-interactions |
| --duration-normal | 200ms | Standard transitions |
| --duration-slow | 300ms | Page transitions |
| --duration-slower | 500ms | Complex animations |

---

## Icon System

### Style
- **Weight**: 1.5px stroke (Lucide/Heroicons style)
- **Size**: 20px (sidebar), 16px (buttons), 14px (inline)
- **Color**: Inherit from parent text color
- **Caps**: Round line caps and joins

### Required Icons

| Category | Icons |
|----------|-------|
| Navigation | Home, Calculator, ChartLine, Cube, Triangle, Atom, Sparkles, Book, GraduationCap, Gear |
| Transport | Play, Pause, Stop, SkipBack, SkipForward, Rewind, FastForward, Loop, Record |
| Export | Download, Film, Image, FileDown, Printer |
| View | Grid, Maximize, Minimize, Eye, EyeOff, RotateCcw, Move, ZoomIn, ZoomOut |
| Math | Sigma, Integral, Derivative, Function, Pi, Infinity |
| File | File, Folder, Save, Upload, Trash2, Copy, Clipboard |
| Status | Check, AlertTriangle, Info, X, ChevronDown, ChevronRight |

---

## Interaction Patterns

### Hover Effects
- Cards: `translateY(-2px)` + shadow elevation + border glow
- Buttons: Background brightens + border accent
- Links: Color shifts to accent + underline appears

### Click Effects
- Cards: `scale(0.98)` for 100ms
- Buttons: `scale(0.96)` for 80ms
- Toggles: Spring animation on state change

### Focus Indicators
- Ring: 2px solid `#4A9EFF` with 2px offset
- Visible on keyboard navigation only
- Suppressed on mouse interaction

### Drag & Drop
- Ghost: 80% opacity, slight scale increase
- Drop zone: Dashed border highlight
- Reorder: Smooth 200ms position animation

---

## Rendering Specifications

### Target Resolution
- **Minimum**: 1920 x 1080 (Full HD)
- **Recommended**: 2560 x 1440 (QHD)
- **Maximum**: 3840 x 2160 (4K)

### DPI Awareness
- 1x: Standard displays
- 2x: Retina/HiDPI displays
- SVG-based icons scale cleanly at any DPI

### WebGL Requirements
- **Minimum**: WebGL 2.0
- **Features**: instanced rendering, compute shaders (when available)
- **Anti-aliasing**: MSAA 4x for 3D viewports
- **HDR**: Supported for visualization viewport

---

## File Structure

```
docs/vision-board/
├── 01-home-workspace.md       # Screen 1 specifications
├── 02-visualization-studio.md  # Screen 2 specifications
├── 03-math-lab.md             # Screen 3 specifications
└── README.md                  # This file
```

---

## Implementation Notes

### Technology Stack
- **UI Framework**: Avalonia 11.x (cross-platform, GPU-accelerated)
- **3D Rendering**: OpenGL/WebGPU via native bindings
- **Math Rendering**: KaTeX (JavaScript interop or native port)
- **AI Integration**: Backend LLM service with streaming responses
- **Animation**: Avalonia Composition API + custom render loop

### Architecture
- **Pattern**: MVVM with reactive data binding
- **State Management**: Observable properties + reactive streams
- **3D Scene**: Separate render thread, shared texture with UI
- **AI Panel**: Async streaming with progressive rendering

### Performance Budget
- **UI Thread**: < 2ms per frame (16ms budget at 60fps)
- **3D Render**: < 8ms per frame (16ms budget at 60fps)
- **AI Response**: < 200ms first token, 30 tokens/sec streaming
- **Memory**: < 512MB typical usage
- **Startup**: < 2 seconds cold start
