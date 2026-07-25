# MathVerse UI Status Audit

## Current State

**Date**: July 25, 2026
**Frontend**: DELETED — No UI exists. Starting from zero.
**Backend**: Feature complete. 65 projects, 5807 tests passing.
**Vision Board**: Complete — 3 screens fully specified.

---

## Audit Summary

| Screen | Quality | Missing Components | Broken Bindings | Placeholder Controls | Backend Connections | Estimated Completion |
|--------|---------|-------------------|-----------------|---------------------|--------------------|--------------------|
| App Shell | None | Everything | N/A | N/A | N/A | Phase 0.1 |
| Home Workspace | None | Everything | N/A | N/A | N/A | Phase 1 |
| Evaluate | None | Everything | N/A | N/A | N/A | Phase 2 |
| Graph Studio | None | Everything | N/A | N/A | N/A | Phase 3 |
| Visualization Studio | None | Everything | N/A | N/A | N/A | Phase 4 |
| Geometry Studio | None | Everything | N/A | N/A | N/A | Phase 5 |
| Simulation Lab | None | Everything | N/A | N/A | N/A | Phase 6 |
| AI Assistant | None | Everything | N/A | N/A | N/A | Phase 7 |
| Data Analysis | None | Everything | N/A | N/A | N/A | Phase 8 |
| Publications | None | Everything | N/A | N/A | N/A | Phase 9 |
| Learning | None | Everything | N/A | N/A | N/A | Phase 10 |
| Settings | None | Everything | N/A | N/A | N/A | Phase 11 |

**Overall UI Completion: 0%**

---

## Detailed Gap Analysis Per Screen

### 1. App Shell

**Current**: No UI exists.
**Target**: Full application shell with sidebar, viewport, status bar.

#### What Must Be Built
- Avalonia Desktop project (`apps/MathVerse.Desktop/`)
- `Program.cs` — Application entry point
- `App.axaml` — Application root with Dark theme, FluentTheme, design system resources
- `App.axaml.cs` — Startup: DI container, MainWindow creation
- `MainWindow.axaml` — Shell layout: sidebar (56px) + viewport (flex) + status bar (24px)
- `MainWindow.axaml.cs` — Window setup
- `Themes/Colors.axaml` — All colors from vision board palette
- `Themes/Brushes.axaml` — SolidColorBrush resources
- `Themes/Controls.axaml` — Global styles for TextBlock, Button, TextBox, etc.
- `Views/Controls/NavButton.cs` + `.axaml` — Custom sidebar button with Glyph, Label, IsActive
- `Views/Controls/ToolbarButton.cs` + `.axaml` — Toolbar button
- `Views/Controls/PageCard.cs` + `.axaml` — Card component
- `Views/Sidebar.cs` + `.axaml` — Navigation sidebar with 10 buttons + settings
- `Views/ViewportHost.cs` + `.axaml` — DataTemplate-driven page router
- `Views/StatusBar.cs` + `.axaml` — Status bar
- `ViewModels/WorkspaceViewModel.cs` — Central VM: CurrentPage, NavigateCommand

#### Backend Dependencies
- None for shell itself (pure UI infrastructure)

#### Gap Size: TOTAL — 17 files to create from scratch

---

### 2. Home Workspace

**Current**: No UI exists.
**Target**: Welcome page with cards, recent items, favorites.

#### What Must Be Built
- `Views/HomeView.axaml` + `.cs` — Welcome section, 8 module cards, recent sections
- `ViewModels/HomeViewModel.cs` — ExpressionInput, RecentProjects, FavoriteVisualizations
- Card hover animations, click navigation, staggered load animation

#### Backend Dependencies
- Project persistence (for recent projects)
- CAS history (for recent equations)
- Visualization export data (for favorite visualizations)

#### Gap Size: TOTAL — Entire page from scratch

---

### 3. Evaluate (Calculator)

**Current**: No UI exists.
**Target**: Expression input with live KaTeX preview, step-by-step results, history.

#### What Must Be Built
- `Views/EvaluateView.axaml` + `.cs` — Input bar, result display, step-by-step, history panel
- `ViewModels/EvaluateViewModel.cs` — ExpressionInput, ExpressionResult, SimplificationSteps, History

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| `Math.CAS.Parsing` | `Parser.Parse(string)` | Parse expression string |
| `Math.CAS.Evaluation` | `Evaluator.Evaluate(parsed)` | Evaluate to result |
| `Math.CAS.Simplification` | `Simplifier.Simplify(result)` | Simplify expression |
| `Math.CAS.Factorization` | `Factorizer.Factor(result)` | Factor expression |
| `Math.CAS.Expansion` | `Expander.Expand(result)` | Expand expression |
| `Math.CAS.Rewriting` | `RuleExecutor.Apply(expression, rules)` | Step-by-step rewrites |
| `Math.CAS.Canonicalization` | `Canonicalizer.Canonicalize(result)` | Normalize form |

#### Gap Size: TOTAL — Entire page from scratch

---

### 4. Graph Studio

**Current**: No UI exists.
**Target**: 2D/3D function plotting with viewport, multiple functions, export.

#### What Must Be Built
- `Views/GraphView.axaml` + `.cs` — Function input, 2D/3D viewport, controls, legend
- `ViewModels/GraphViewModel.cs` — FunctionInput, PlotType, PlotResult, MultipleFunctions

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| `Math.Visualization` | `CartesianPlot.Create(x, y, options)` | 2D line plot |
| `Math.Visualization` | `ScatterPlot.Create(x, y, options)` | 2D scatter |
| `Math.Visualization` | `BarPlot.Create(categories, values, options)` | Bar chart |
| `Math.Visualization` | `HistogramPlot.Create(data, bins, options)` | Histogram |
| `Math.Visualization` | `PolarPlot.Create(theta, r, options)` | Polar plot |
| `Math.Visualization` | `ParametricPlot.Create(x(t), y(t), range, options)` | Parametric |
| `Math.Visualization` | `SurfacePlot.Create(f, xRange, yRange, options)` | 3D surface |
| `Math.Visualization` | `WireframePlot.Create(f, xRange, yRange, options)` | 3D wireframe |
| `Math.Visualization` | `PointCloudPlot.Create(points, options)` | 3D scatter |
| `Math.Visualization` | `SVGExporter.Export(result)` | Export SVG |
| `Math.Visualization` | `PNGExporter.Export(result, width, height)` | Export PNG |
| `Math.Numerics` | `Vector`, `Matrix` | Numerical data |

#### Gap Size: TOTAL — Entire page from scratch

---

### 5. Visualization Studio

**Current**: No UI exists.
**Target**: Three-column layout with 3D viewport, library, properties, timeline. Flagship feature.

#### What Must Be Built
- `Views/VisualizationView.axaml` + `.cs` — Three-column layout
- `Views/VisualizationViewport.cs` + `.axaml` — 3D viewport with OpenGL/WebGPU
- `Views/VisualizationLibrary.cs` + `.axaml` — Left panel: 11 categories
- `Views/VisualizationProperties.cs` + `.axaml` — Right panel: all control sections
- `Views/VisualizationTimeline.cs` + `.axaml` — Bottom timeline
- `ViewModels/VisualizationViewModel.cs` — Scene, Camera, Lighting, Material, Animation state

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| `Math.Visualization` | `VisualizationScene` | Scene container |
| `Math.Visualization` | `VisualizationObject` subtypes | Renderable objects |
| `Math.Visualization` | `Camera` (Orbit/Pan/Zoom) | Camera control |
| `Math.Visualization` | `Light` (Directional/Point/Spot/Ambient) | Lighting |
| `Math.Visualization` | `Material` (PBR) | Material properties |
| `Math.Visualization` | `AnimationTimeline` | Animation playback |
| `Math.Visualization` | `RenderingPipeline` + passes | Multi-pass rendering |
| `Math.Visualization` | `SceneGraph` + `SceneNode` | Scene hierarchy |
| `Math.Visualization` | `HitTester` | Object picking |
| `Math.Visualization` | `OrbitTool`, `PanTool`, `ZoomTool` | Camera tools |
| `Math.Visualization` | `SVGExporter`, `PNGExporter`, `JSONExporter` | Export |
| `Math.Geometry.Advanced` | Serialization (OBJ, STL, glTF) | 3D export |

#### Gap Size: TOTAL — Entire page from scratch. Most complex page.

---

### 6. Geometry Studio

**Current**: No UI exists.
**Target**: Interactive 2D/3D geometry with construction tools.

#### What Must Be Built
- `Views/GeometryView.axaml` + `.cs` — Canvas, tool panel, properties
- `ViewModels/GeometryViewModel.cs` — Objects, Tools, ActiveTool

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| `Math.Geometry` | `GeometryEngine.CreatePoint2D/3D()` | Create points |
| `Math.Geometry` | `GeometryEngine.CreateLine2D/3D()` | Create lines |
| `Math.Geometry` | `GeometryEngine.CreateCircle2D()` | Create circles |
| `Math.Geometry` | `GeometryEngine.CreateSphere()` | Create 3D spheres |
| `Math.Geometry` | `GeometryEngine.CreateMesh()` | Create meshes |
| `Math.Geometry` | `Transform2D`, `Transform3D` | Transforms |
| `Math.Geometry` | `CollisionDetection` | Collision detection |
| `Math.Geometry.Advanced` | `ConvexHull`, `Voronoi`, `Delaunay` | Advanced geometry |
| `Math.Geometry.Advanced` | Serialization (OBJ, STL, SVG) | Export |

#### Gap Size: TOTAL — Entire page from scratch

---

### 7. Simulation Lab

**Current**: No UI exists.
**Target**: Multi-domain simulation with parameter controls and real-time visualization.

#### What Must Be Built
- `Views/SimulateView.axaml` + `.cs` — Category selector, parameters, viewport, playback
- `ViewModels/SimulateViewModel.cs` — SimulationType, Parameters, IsRunning, PlaybackState

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| `Math.Simulation` | `SimulationEngine.SimulatePhysics()` | Physics sim |
| `Math.Simulation` | `SimulationEngine.SimulateThermodynamics()` | Thermal sim |
| `Math.Simulation` | `SimulationEngine.LotkaVolterra()` | Predator-prey |
| `Math.Simulation` | `SimulationEngine.SIRModel()` | Epidemiology |
| `Math.Simulation` | `SimulationEngine.BlackScholesCall()` | Finance |
| `Math.Simulation` | `SimulationEngine.FFT()` | Signal processing |
| `Math.Simulation` | `SimulationEngine.SolveODE()` | ODE solving |
| `Math.Simulation` | `SimulationEngine.PIDControl()` | Control systems |
| `Math.Simulation` | `SimulationEngine.ReynoldsNumber()` | Fluid dynamics |
| `Math.Simulation` | `SimulationEngine.MonteCarloIntegrate()` | Monte Carlo |

#### Gap Size: TOTAL — Entire page from scratch

---

### 8. AI Assistant

**Current**: No UI exists.
**Target**: Chat interface with streaming responses, math rendering, context awareness.

#### What Must Be Built
- `Views/AiView.axaml` + `.cs` — Chat messages, input bar, suggestions
- `ViewModels/AiViewModel.cs` — Messages, InputText, IsStreaming, ConversationHistory

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| `Math.AI` | AI inference services | Generate responses |
| `Math.AI` | Streaming API | Progressive text |
| `Math.CAS` | `Parser.Parse()` | Parse AI-generated expressions |
| `Math.Visualization` | Visualization renderers | Generate visuals from AI |

#### Gap Size: TOTAL — Entire page from scratch

---

### 9. Data Analysis

**Current**: No UI exists.
**Target**: Data import, table view, statistics, charts, filtering.

#### What Must Be Built
- `Views/DataView.axaml` + `.cs` — Import panel, data table, charts, statistics
- `ViewModels/DataViewModel.cs` — DataFrame, Columns, Charts, Filters

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| `Math.DataScience` | `CsvReader.Read()` | Import CSV |
| `Math.DataScience` | `JsonReader.Read()` | Import JSON |
| `Math.DataScience` | `DataFrame` | Data container |
| `Math.DataScience` | `DatasetStatistics` | Summary stats |
| `Math.DataScience` | `DataVisualizer` | Charts |
| `Math.DataScience` | `PCAVisualizer` | PCA projection |
| `Math.DataScience` | `CorrelationMatrixVisualizer` | Correlation heatmap |
| `Math.DataScience` | `CsvWriter.Write()` | Export CSV |
| `Math.DataScience` | `JsonWriter.Write()` | Export JSON |

#### Gap Size: TOTAL — Entire page from scratch

---

### 10. Publications

**Current**: No UI exists.
**Target**: Mathematical document authoring with export.

#### What Must Be Built
- `Views/PublishView.axaml` + `.cs` — Template selector, editor, preview, export
- `ViewModels/PublishViewModel.cs` — Document, Template, ExportFormat

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| Export services | LaTeX generation | Export .tex |
| Export services | PDF generation | Export .pdf |
| Export services | HTML generation | Export .html |
| `Math.CAS` | Expression rendering | Inline math |

#### Gap Size: TOTAL — Entire page from scratch

---

### 11. Learning

**Current**: No UI exists.
**Target**: Course catalog, lesson viewer, exercises, progress tracking.

#### What Must Be Built
- `Views/LearnView.axaml` + `.cs` — Catalog, player, exercises
- `ViewModels/LearnViewModel.cs` — Courses, CurrentCourse, Progress

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| Content service | Course data | Load courses |
| `Math.CAS` | Expression evaluation | Validate exercise answers |
| `Math.Visualization` | Visualization renderers | Interactive examples |

#### Gap Size: TOTAL — Entire page from scratch

---

### 12. Settings

**Current**: No UI exists.
**Target**: Application settings with persistence.

#### What Must Be Built
- `Views/SettingsView.axaml` + `.cs` — Settings categories, controls
- `ViewModels/SettingsViewModel.cs` — All settings properties

#### Backend Connections Required
| Backend API | Method | Purpose |
|-------------|--------|---------|
| Persistence | Settings storage | Load/save settings |
| Kernel | GPU detection | Report rendering backend |

#### Gap Size: TOTAL — Entire page from scratch

---

## Backend Capability Inventory

### Available for UI Consumption

| Domain | Backend Project | Key Facade | Capabilities |
|--------|----------------|------------|--------------|
| CAS | `Math.CAS` | `Evaluator`, `Simplifier`, `Factorizer`, `Expander` | Evaluate, simplify, factor, expand expressions |
| Parsing | `Math.Parsing` | `Parser` | Parse string → expression tree |
| Algebra | `Math.Algebra` | (integrated with CAS) | Polynomial operations, equation solving |
| Calculus | `Math.Calculus` | (integrated with CAS) | Derivatives, integrals, limits |
| 2D Plotting | `Math.Visualization` | `CartesianPlot`, `ScatterPlot`, etc. | 10+ plot types |
| 3D Plotting | `Math.Visualization` | `SurfacePlot`, `WireframePlot`, etc. | 8+ 3D plot types |
| Rendering | `Math.Visualization` | `RenderingPipeline`, `SceneGraph` | Multi-pass rendering, scene management |
| Camera | `Math.Visualization` | `Camera` | Orbit, pan, zoom, perspective/ortho |
| Lighting | `Math.Visualization` | `Light` | Directional, point, spot, ambient |
| Material | `Math.Visualization` | `Material` | PBR: color, metallic, roughness, opacity |
| Animation | `Math.Visualization` | `AnimationTimeline` | Keyframe animation, expression animation |
| Interaction | `Math.Visualization` | `HitTester`, `OrbitTool`, etc. | Picking, camera tools, selection |
| Export (2D) | `Math.Visualization` | `SVGExporter`, `PNGExporter` | SVG, PNG export |
| Export (3D) | `Math.Geometry.Advanced` | Serialization | OBJ, STL, glTF, PLY, OFF |
| Geometry | `Math.Geometry` | `GeometryEngine` | 2D/3D primitives, transforms, meshes |
| Advanced Geo | `Math.Geometry.Advanced` | `GeometryAdvancedEngine` | Convex hull, Voronoi, Delaunay, boolean |
| Physics | `Math.Simulation` | `SimulationEngine` | Particles, rigid bodies, forces, constraints |
| Chemistry | `Math.Simulation` | `SimulationEngine` | Reactions, kinetics, equilibrium |
| Biology | `Math.Simulation` | `SimulationEngine` | Population dynamics, SIR, Lotka-Volterra |
| Finance | `Math.Simulation` | `SimulationEngine` | Black-Scholes, Monte Carlo, bonds |
| Control | `Math.Simulation` | `SimulationEngine` | PID, state-space, transfer functions |
| EM | `Math.Simulation` | `SimulationEngine` | Coulomb, Lorentz, Biot-Savart |
| Fluids | `Math.Simulation` | `SimulationEngine` | Reynolds, Navier-Stokes presets |
| Signals | `Math.Simulation` | `SimulationEngine` | FFT, convolution, filtering |
| ODE | `Math.Simulation` | `SimulationEngine` | RK4, RK45, implicit Euler |
| Statistics | `Math.Simulation` | `SimulationEngine` | Mean, variance, correlation |
| Data | `Math.DataScience` | `DataFrame`, importers | CSV/JSON/XML import, data manipulation |
| Data Viz | `Math.DataScience` | `DataVisualizer` | Distribution, PCA, correlation plots |
| AI | `Math.AI` | AI services | Training, prediction, generation |
| Numerics | `Math.Numerics` | `Vector`, `Matrix` | Linear algebra, root finding, optimization |
| Performance | `Math.Performance` | `PerformanceEngine` | Caching, pooling, parallel execution |

### Skeleton Projects (Not Yet Implemented)

| Project | Status |
|---------|--------|
| `Math.Numerical` | csproj only, no source |
| `Math.Statistics` | csproj only, no source |
| `Rendering.Abstractions` | csproj only, no source |
| `Rendering.OpenGL` | csproj only, no source |
| `Rendering.Metal` | csproj only, no source |
| `Rendering.Vulkan` | csproj only, no source |
| `Rendering.WebGPU` | csproj only, no source |

---

## Completion Timeline Estimate

| Phase | Screen | Effort (days) | Dependencies |
|-------|--------|---------------|-------------|
| 0.1 | App Shell | 3-5 | None |
| 0.2 | Shared Components | 3-4 | Phase 0.1 |
| 1 | Home Workspace | 2-3 | Phase 0.1, 0.2 |
| 2 | Evaluate | 3-4 | Phase 0.1, CAS backend |
| 3 | Graph Studio | 5-7 | Phase 0.1, Visualization backend |
| 4 | Visualization Studio | 7-10 | Phase 0.1, Visualization + Rendering |
| 5 | Geometry Studio | 5-7 | Phase 0.1, Geometry backend |
| 6 | Simulation Lab | 4-6 | Phase 0.1, Simulation backend |
| 7 | AI Assistant | 3-5 | Phase 0.1, AI backend |
| 8 | Data Analysis | 4-6 | Phase 0.1, DataScience backend |
| 9 | Publications | 3-4 | Phase 0.1, CAS + Export |
| 10 | Learning | 3-4 | Phase 0.1, Content service |
| 11 | Settings | 1-2 | Phase 0.1, Persistence |
| **Total** | | **46-67 days** | |

*Estimates assume single developer, include testing and polish.*

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| 3D viewport integration with Avalonia | High | Start early in Phase 0, validate rendering approach |
| KaTeX integration for math rendering | Medium | Evaluate Avalonia-compatible math rendering options |
| AI streaming integration | Medium | Start with non-streaming, add streaming later |
| Performance of real-time simulations | Medium | Offload to background thread, throttle UI updates |
| Cross-platform rendering (OpenGL/Metal/Vulkan) | High | Start with single backend, abstract later |

---

## Decision Record

| Decision | Choice | Rationale |
|----------|--------|-----------|
| UI Framework | Avalonia 11.x | Cross-platform, GPU-accelerated, .NET native |
| MVVM Toolkit | CommunityToolkit.Mvvm 8.2.2 | Source generators, minimal boilerplate |
| Math Rendering | TBD (KaTeX or native) | Needs Avalonia-compatible solution |
| 3D Rendering | OpenGL via Math.Visualization pipeline | Already implemented in backend |
| State Management | Observable properties + reactive streams | Simple, proven pattern |
| DI Container | Microsoft.Extensions.DI | Standard .NET, already in backend |
