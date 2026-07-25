# MathVerse Implementation Roadmap

## Development Strategy

**Screen-by-screen. One page at a time.**

No page begins until the previous page passes the quality gate defined in `screen-checklist.md`. The vision board (`docs/vision-board/`) is the authoritative specification.

---

## Phase 0: Foundation Infrastructure

> Before any screen can be built, the shared infrastructure must exist.

### Milestone 0.1 — Application Shell

| Item | Status | Notes |
|------|--------|-------|
| Avalonia project setup | ⬜ Not Started | `apps/MathVerse.Desktop/` |
| Program.cs entry point | ⬜ Not Started | |
| App.axaml with Dark theme | ⬜ Not Started | FluentTheme, RequestedThemeVariant=Dark |
| Design system resources | ⬜ Not Started | Colors.axaml, Brushes.axaml, Controls.axaml |
| Global styles | ⬜ Not Started | Typography, buttons, inputs per design-rules.md |
| Custom controls library | ⬜ Not Started | NavButton, ToolbarButton, PageCard |
| MainWindow shell | ⬜ Not Started | Sidebar + ViewportHost + StatusBar |
| WorkspaceViewModel | ⬜ Not Started | Single source of truth, page routing |
| ViewportHost with DataTemplates | ⬜ Not Started | VM → View resolution |
| Sidebar navigation | ⬜ Not Started | 10 nav buttons + settings, active state |
| Status bar | ⬜ Not Started | Ready, version, backend status, GPU, time |
| Global keyboard shortcuts | ⬜ Not Started | Ctrl+K, Escape, Tab navigation |
| DI container setup | ⬜ Not Started | Register all ViewModels and services |

### Milestone 0.2 — Shared Components

| Item | Status | Notes |
|------|--------|-------|
| Search overlay (Ctrl+K) | ⬜ Not Started | Global search with results dropdown |
| Toast notification system | ⬜ Not Started | Info/Success/Warning/Error |
| Modal dialog system | ⬜ Not Started | Centered modal with backdrop |
| Context menu system | ⬜ Not Started | Right-click menus |
| Loading/skeleton system | ⬜ Not Started | Skeleton screens, spinners, progress bars |
| Empty state component | ⬜ Not Started | Illustration + title + CTA |
| Error state component | ⬜ Not Started | Icon + message + retry |
| Property panel controls | ⬜ Not Started | Sliders, toggles, color pickers, dropdowns |
| Timeline control | ⬜ Not Started | Playback transport + frame slider |

---

## Phase 1: Home Workspace

> Vision spec: `docs/vision-board/01-home-workspace.md`

### Milestone 1.1 — Home Page

| Item | Status | Notes |
|------|--------|-------|
| Welcome section | ⬜ Not Started | Greeting + status summary |
| Module cards grid (4x2) | ⬜ Not Started | 8 cards with icons, titles, descriptions |
| Card hover animations | ⬜ Not Started | translateY + glow + shadow |
| Card click navigation | ⬜ Not Started | Navigate to module page |
| Recent projects section | ⬜ Not Started | Horizontal card row |
| Recent equations section | ⬜ Not Started | Scrollable equation pills |
| Favorite visualizations section | ⬜ Not Started | Thumbnail grid |
| Search integration | ⬜ Not Started | Search bar in toolbar |
| Page transition animations | ⬜ Not Started | Fade + slide |

**Quality Gate**: All 9 items complete + vision board match + keyboard nav works.

---

## Phase 2: Evaluate (Calculator)

> Backend: `Math.CAS` (Evaluator, Simplifier, Parser, Factorizer, Expander)

### Milestone 2.1 — Evaluate Page

| Item | Status | Notes |
|------|--------|-------|
| Expression input bar | ⬜ Not Started | Large text input with KaTeX preview |
| Evaluate button + Ctrl+Enter | ⬜ Not Started | Triggers CAS evaluation |
| Result display | ⬜ Not Started | Rendered mathematical output |
| Step-by-step expansion | ⬜ Not Started | Toggle to show simplification steps |
| History panel | ⬜ Not Started | List of previous evaluations |
| Save to favorites | ⬜ Not Started | Star/bookmark expression |
| Error handling | ⬜ Not Started | Parse errors, evaluation errors |
| Empty state | ⬜ Not Started | "Type an expression to begin" |
| Loading state | ⬜ Not Started | Spinner during evaluation |

**Backend connections**: `Evaluator.Evaluate()`, `Simplifier.Simplify()`, `Parser.Parse()`, `Factorizer.Factor()`, `Expander.Expand()`

**Quality Gate**: All 9 items complete + CAS produces correct results + no placeholders.

---

## Phase 3: Graph Studio

> Backend: `Math.Visualization` (CartesianPlot, SurfacePlot, 2D/3D plotters)

### Milestone 3.1 — 2D Graphing

| Item | Status | Notes |
|------|--------|-------|
| Function input | ⬜ Not Started | y = f(x) input with parsing |
| 2D viewport with grid | ⬜ Not Started | Cartesian plane, axes, labels |
| Plot rendering | ⬜ Not Started | Line, scatter, bar, polar |
| Multiple functions | ⬜ Not Started | Add/remove/overlay |
| Pan and zoom | ⬜ Not Started | Mouse wheel + drag |
| Axis labels and ticks | ⬜ Not Started | Auto-scaling |
| Legend | ⬜ Not Started | Color-coded function list |
| Export 2D | ⬜ Not Started | PNG, SVG |
| Parametric plots | ⬜ Not Started | (x(t), y(t)) |
| Polar plots | ⬜ Not Started | r = f(θ) |

### Milestone 3.2 — 3D Graphing

| Item | Status | Notes |
|------|--------|-------|
| 3D viewport | ⬜ Not Started | WebGL/OpenGL canvas |
| Surface plot | ⬜ Not Started | z = f(x,y) |
| Wireframe mode | ⬜ Not Started | Toggle wireframe/solid |
| Camera orbit/pan/zoom | ⬜ Not Started | Orbit tool |
| Lighting | ⬜ Not Started | Directional + ambient |
| Material properties | ⬜ Not Started | Color, opacity |
| Export 3D | ⬜ Not Started | OBJ, glTF |

**Backend connections**: `CartesianPlot.Create()`, `SurfacePlot.Create()`, `ScatterPlot`, `PolarPlot`, `ParametricPlot`

**Quality Gate**: All 17 items complete + plots render correctly + no placeholders.

---

## Phase 4: Visualization Studio

> Vision spec: `docs/vision-board/02-visualization-studio.md`

### Milestone 4.1 — Visualization Shell

| Item | Status | Notes |
|------|--------|-------|
| Three-column layout | ⬜ Not Started | Library (260px) | Viewport | Properties (280px) |
| 3D viewport with grid | ⬜ Not Started | Reference grid + axes |
| Library panel | ⬜ Not Started | 11 categories, expandable tree |
| Properties panel | ⬜ Not Started | Collapsible sections |
| Bottom timeline | ⬜ Not Started | Blender-style transport controls |
| Panel resize handles | ⬜ Not Started | Drag to resize panels |

### Milestone 4.2 — Visualization Content

| Item | Status | Notes |
|------|--------|-------|
| Black hole scene | ⬜ Not Started | Core visualization per vision board |
| Gravitational field lines | ⬜ Not Started | Animated curves |
| Particle trajectories | ⬜ Not Started | Orbiting particles with trails |
| Spacetime curvature grid | ⬜ Not Started | Warped mesh |
| Vector field overlay | ⬜ Not Started | Directional arrows |
| Animated equations | ⬜ Not Started | Floating labels |
| Orbiting camera | ⬜ Not Started | Auto-orbit animation |

### Milestone 4.3 — Properties Controls

| Item | Status | Notes |
|------|--------|-------|
| Animation controls | ⬜ Not Started | Play/Pause/Stop + timeline |
| Camera controls | ⬜ Not Started | Mode, target, distance, FOV |
| Lighting controls | ⬜ Not Started | Key/Fill/Rim + intensity |
| Material controls | ⬜ Not Started | PBR: color, opacity, roughness, metalness |
| Physics controls | ⬜ Not Started | Speed, particle count, trail, mass |
| Export controls | ⬜ Not Started | Blender, OBJ, glTF, STL, Video, GIF |

**Backend connections**: `VisualizationScene`, `SurfacePlot`, `PointCloud`, `Camera`, `Light`, `Material`, `AnimationTimeline`, `RenderingPipeline`, `SVGExporter`, `PNGExporter`

**Quality Gate**: All 18 items complete + 3D scene renders + animations play + no placeholders.

---

## Phase 5: Geometry Studio

> Backend: `Math.Geometry`, `Math.Geometry.Advanced`

### Milestone 5.1 — Geometry Page

| Item | Status | Notes |
|------|--------|-------|
| Interactive 2D canvas | ⬜ Not Started | Cartesian plane with grid |
| Point creation | ⬜ Not Started | Click to place points |
| Line/segment creation | ⬜ Not Started | Click two points |
| Circle creation | ⬜ Not Started | Center + radius |
| Polygon creation | ⬜ Not Started | Click vertices |
| Measurement tools | ⬜ Not Started | Distance, angle, area |
| Transform tools | ⬜ Not Started | Move, rotate, scale |
| Construction tools | ⬜ Not Started | Perpendicular, parallel, bisector |
| Properties panel | ⬜ Not Started | Object properties |
| 3D geometry view | ⬜ Not Started | Toggle 3D view |
| Export | ⬜ Not Started | SVG, OBJ, STL |

**Backend connections**: `GeometryEngine` facade (CreatePoint2D/3D, CreateLine2D/3D, CreateCircle2D, CreateSphere, CreateMesh, etc.), `GeometryAdvancedEngine`

**Quality Gate**: All 11 items complete + geometric operations work + no placeholders.

---

## Phase 6: Simulation Lab

> Backend: `Math.Simulation` (SimulationEngine facade)

### Milestone 6.1 — Simulation Page

| Item | Status | Notes |
|------|--------|-------|
| Simulation category selector | ⬜ Not Started | Physics, Chemistry, Biology, Finance, etc. |
| Parameter input panel | ⬜ Not Started | Domain-specific parameter controls |
| 2D/3D visualization viewport | ⬜ Not Started | Real-time simulation rendering |
| Play/Pause/Reset controls | ⬜ Not Started | Transport controls |
| Timeline with speed control | ⬜ Not Started | Variable speed playback |
| Real-time graphs | ⬜ Not Started | Live plots during simulation |
| Data export | ⬜ Not Started | CSV, JSON |
| Preset simulations | ⬜ Not Started | Pre-configured scenarios |

**Backend connections**: `SimulationEngine.SimulatePhysics()`, `SimulateThermodynamics()`, `LotkaVolterra()`, `SIRModel()`, `BlackScholesCall()`, `FFT()`, `SolveODE()`, `PIDControl()`, `ReynoldsNumber()`, etc.

**Quality Gate**: All 8 items complete + simulations run + visualizations update + no placeholders.

---

## Phase 7: AI Assistant

> Backend: `Math.AI` + streaming LLM integration

### Milestone 7.1 — AI Page

| Item | Status | Notes |
|------|--------|-------|
| Chat interface | ⬜ Not Started | Message bubbles, input bar |
| Streaming response display | ⬜ Not Started | Progressive text rendering |
| Math rendering in responses | ⬜ Not Started | KaTeX in AI output |
| Code/equation copy | ⬜ Not Started | One-click copy |
| Context awareness | ⬜ Not Started | AI knows current page/objects |
| Suggestion chips | ⬜ Not Started | Quick action suggestions |
| Conversation history | ⬜ Not Started | Previous conversations list |
| Visual generation | ⬜ Not Started | AI creates visualizations |
| Error handling | ⬜ Not Started | API failures, rate limits |
| Loading state | ⬜ Not Started | Thinking indicator |

**Backend connections**: `Math.AI` services, streaming LLM API

**Quality Gate**: All 10 items complete + AI responds + math renders + no placeholders.

---

## Phase 8: Data Analysis

> Backend: `Math.DataScience` (DataFrame, importers, exporters, visualizers)

### Milestone 8.1 — Data Analysis Page

| Item | Status | Notes |
|------|--------|-------|
| Data import panel | ⬜ Not Started | CSV, JSON, XML, Excel drag-and-drop |
| Data table view | ⬜ Not Started | Spreadsheet-like data display |
| Column statistics | ⬜ Not Started | Summary stats per column |
| Visualization panel | ⬜ Not Started | Charts: scatter, bar, histogram, heatmap, etc. |
| Filtering controls | ⬜ Not Started | Filter rows by conditions |
| Sorting controls | ⬜ Not Started | Sort by columns |
| Missing value handling | ⬜ Not Started | Visual indicators + options |
| Export | ⬜ Not Started | CSV, JSON, HTML, Markdown |
| Correlation matrix | ⬜ Not Started | Heatmap visualization |
| PCA / dimensionality reduction | ⬜ Not Started | 2D/3D projection |

**Backend connections**: `DataFrame`, `DatasetImporter`, `CsvReader`, `JsonReader`, `DataVisualizer`, `DistributionVisualizer`, `PCAVisualizer`, `CorrelationMatrixVisualizer`

**Quality Gate**: All 10 items complete + data imports/exports + visualizations render + no placeholders.

---

## Phase 9: Publications

> Backend: `Math.DataScience` export, LaTeX generation

### Milestone 9.1 — Publications Page

| Item | Status | Notes |
|------|--------|-------|
| Publication template selector | ⬜ Not Started | Article, report, presentation |
| Rich text editor | ⬜ Not Started | Mathematical content authoring |
| Inline equation editor | ⬜ Not Started | KaTeX input |
| Figure insertion | ⬜ Not Started | Embed visualizations |
| Bibliography manager | ⬜ Not Started | Citations and references |
| Preview panel | ⬜ Not Started | Live preview of output |
| Export formats | ⬜ Not Started | PDF, LaTeX, HTML |
| Template customization | ⬜ Not Started | Styles, fonts, layout |

**Quality Gate**: All 8 items complete + publications generate + no placeholders.

---

## Phase 10: Learning

> No specific backend — curated content and guided experiences

### Milestone 10.1 — Learning Page

| Item | Status | Notes |
|------|--------|-------|
| Course catalog | ⬜ Not Started | Browse available courses |
| Course player | ⬜ Not Started | Step-by-step lesson viewer |
| Interactive exercises | ⬜ Not Started | Math problems with visual feedback |
| Progress tracking | ⬜ Not Started | Completion indicators |
| Bookmarking | ⬜ Not Started | Save courses for later |
| Search courses | ⬜ Not Started | Find by topic |

**Quality Gate**: All 6 items complete + courses load + progress saves + no placeholders.

---

## Phase 11: Settings

### Milestone 11.1 — Settings Page

| Item | Status | Notes |
|------|--------|-------|
| Theme settings | ⬜ Not Started | Dark theme (only option initially) |
| Font size | ⬜ Not Started | UI scaling |
| Default export location | ⬜ Not Started | File path configuration |
| GPU settings | ⬜ Not Started | Rendering backend selection |
| Performance settings | ⬜ Not Started | Quality vs performance |
| Keyboard shortcuts | ⬜ Not Started | View and customize |
| About | ⬜ Not Started | Version, credits, licenses |
| Reset to defaults | ⬜ Not Started | Restore all settings |

**Quality Gate**: All 8 items complete + settings persist + no placeholders.

---

## Completion Summary

| Phase | Screen | Items | Status |
|-------|--------|-------|--------|
| 0.1 | App Shell | 12 | ⬜ Not Started |
| 0.2 | Shared Components | 9 | ⬜ Not Started |
| 1 | Home Workspace | 9 | ⬜ Not Started |
| 2 | Evaluate | 9 | ⬜ Not Started |
| 3 | Graph Studio | 17 | ⬜ Not Started |
| 4 | Visualization Studio | 18 | ⬜ Not Started |
| 5 | Geometry Studio | 11 | ⬜ Not Started |
| 6 | Simulation Lab | 8 | ⬜ Not Started |
| 7 | AI Assistant | 10 | ⬜ Not Started |
| 8 | Data Analysis | 10 | ⬜ Not Started |
| 9 | Publications | 8 | ⬜ Not Started |
| 10 | Learning | 6 | ⬜ Not Started |
| 11 | Settings | 8 | ⬜ Not Started |
| **Total** | | **135** | **0%** |

---

## Priority Order

The phases are ordered by user impact and dependency:

1. **Phase 0** (Foundation) — Required before anything else
2. **Phase 1** (Home) — First thing users see
3. **Phase 2** (Evaluate) — Core math interaction, simplest to implement
4. **Phase 3** (Graph) — Core visualization, high user value
5. **Phase 4** (Visualization) — Flagship feature, highest complexity
6. **Phase 5** (Geometry) — Interactive, high engagement
7. **Phase 6** (Simulation) — High value, leverages existing backend
8. **Phase 7** (AI) — Differentiator, leverages existing backend
9. **Phase 8** (Data) — Professional use case
10. **Phase 9** (Publications) — Output-focused
11. **Phase 10** (Learning) — Content-focused
12. **Phase 11** (Settings) — Can be minimal initially
