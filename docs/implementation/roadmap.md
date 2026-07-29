# MathVerse Desktop — Implementation Roadmap

> **Architecture:** `docs/architecture.md`
> **Design Rules:** `docs/implementation/design-rules.md`
> **Status:** Ready for implementation
> **Internal Naming:** Application Core (not "Workspace Kernel")
> **Version:** v1.0 (Frozen)

> **Architecture:** 27 sections, ~1,700 lines
> **Design Rules:** 18 mandatory rules
> **Phases:** 15 phases, 35 milestones
> **Current Phase:** 1 — Application Core

---

## Development Strategy

**Incremental, compile-gated milestones.**

Every milestone must:
1. Build with 0 errors, 0 warnings
2. Launch successfully (if visual)
3. Be usable (not broken, not a placeholder)
4. Preserve all previous milestones

No milestone begins until the previous milestone passes the build gate.

---

## Phase 1: Application Core

> Build the engine that the UI will sit on. No visuals except an empty shell.
> No panels. No graphs. No rendering. Just the internal machinery.
> Internally called "Application Core" because Application → Core → Workspace → Renderer → UI.

### Milestone 1.1 — Workspace Core

| Item | Status |
|------|--------|
| `Workspace` class (owns everything) | ⬜ |
| `Document` class (owns a scene and its objects) | ⬜ |
| `IWorkspaceObject` interface (Id, Name, Icon, TypeTag, IsVisible, IsLocked, IsPinned, IsSelected, IsExpanded, Tags, Category, ParentId, Children, Transform, Metadata, BoundingBox, Layer, Owner, CreatedAt, ModifiedAt) | ⬜ |
| `WorkspaceObject` base class with Clone, Serialize, Destroy, Duplicate, Select, Deselect | ⬜ |
| Object property change notification (INotifyPropertyChanged) | ⬜ |
| Object add/remove from workspace | ⬜ |
| Object hierarchy (parent/children) | ⬜ |

### Milestone 1.2 — Object Registry

| Item | Status |
|------|--------|
| `ObjectRegistry` (queryable collection of all objects) | ⬜ |
| `GetById(Guid)`, `GetByType(string)`, `GetByTag(string)` | ⬜ |
| `GetAll()`, `GetVisible()`, `GetSelected()` | ⬜ |
| Object count, type counts | ⬜ |

### Milestone 1.3 — Event Bus

| Item | Status |
|------|--------|
| `EventBus` class (subscribe, unsubscribe, publish) | ⬜ |
| Thread-safe implementation | ⬜ |
| Event types: ObjectCreated, ObjectDeleted, ObjectSelectionChanged, ObjectPropertyChanged, ViewportCameraChanged, CommandExecuted, UndoPerformed, RedoPerformed, WorkspaceModeChanged | ⬜ |
| Unit tests for pub/sub | ⬜ |

### Milestone 1.4 — Selection Manager

| Item | Status |
|------|--------|
| `SelectionManager` (single select, multi select) | ⬜ |
| `Select(objectId)`, `Deselect(objectId)`, `DeselectAll()` | ⬜ |
| `SelectedObjects` (read-only list) | ⬜ |
| `PrimarySelection` (last selected) | ⬜ |
| Fires `ObjectSelectionChanged` via EventBus | ⬜ |

### Milestone 1.5 — Command Manager

| Item | Status |
|------|--------|
| `ICommand` interface (Name, CanExecute, Execute, GetUndoData) | ⬜ |
| `CommandContext` (Workspace, SelectedObjects, ActiveTool) | ⬜ |
| `CommandRegistry` (register, lookup by name) | ⬜ |
| `CommandManager` (execute, track history) | ⬜ |
| 5 initial commands: CreateObject, DeleteObject, SetObjectProperty, SetObjectVisibility, RenameObject | ⬜ |

### Milestone 1.6 — Undo / Redo

| Item | Status |
|------|--------|
| `UndoManager` (per-workspace) | ⬜ |
| `UndoTransaction` (operations + reverse) | ⬜ |
| Gesture grouping (mouse drag = one step) | ⬜ |
| `Undo()`, `Redo()` | ⬜ |
| Max depth (500), memory pruning | ⬜ |
| Fires `UndoPerformed`, `RedoPerformed` via EventBus | ⬜ |

### Milestone 1.7 — Tool Manager

| Item | Status |
|------|--------|
| `ITool` interface (Activate, Deactivate, OnMouseXxx, OnWheel, OnKeyDown, DrawOverlay, Cursor) | ⬜ |
| `ToolManager` (active tool, tool history) | ⬜ |
| `SetTool(ITool)`, `PreviousTool()` | ⬜ |
| Fires `ToolActivated`, `ToolDeactivated` via EventBus | ⬜ |
| Initial tools: PanTool, ZoomTool, SelectTool | ⬜ |

### Milestone 1.8 — Workspace Mode Manager

| Item | Status |
|------|--------|
| `WorkspaceMode` enum (Mathematics, Visualization, Geometry, Simulation, Research, Publication, Teaching) | ⬜ |
| `ModeManager` (active mode, mode-specific tool defaults) | ⬜ |
| `SetMode(WorkspaceMode)` | ⬜ |
| Fires `WorkspaceModeChanged` via EventBus | ⬜ |

### Milestone 1.9 — Object Factory

| Item | Status |
|------|--------|
| `ObjectFactory` (create typed objects with defaults) | ⬜ |
| `CreateGraph(expression, type, color)` | ⬜ |
| `CreateExpression(expression)` | ⬜ |
| `CreateGeometry(type)` | ⬜ |
| `CreateSimulation(type)` | ⬜ |
| All creation goes through ObjectFactory → Command system | ⬜ |

### Milestone 1.10 — Compiler Layer

| Item | Status |
|------|--------|
| `ICompiler` interface (Compile, Recompile) | ⬜ |
| `ExpressionCompiler` (expression string → parsed expression) | ⬜ |
| `GraphCompiler` (parsed expression + range → sampled points) | ⬜ |
| `SurfaceCompiler` (expression + bounds → vertex grid) | ⬜ |
| `MeshGenerator` (geometry definition → triangle mesh) | ⬜ |
| `RenderCompiler` (compiled data → RenderObject) | ⬜ |
| Compiler pipeline: Workspace Object → Compiled Object → Render Object | ⬜ |
| Display property changes skip recompilation | ⬜ |
| Mathematical property changes trigger recompilation | ⬜ |

### Milestone 1.11 — Document Model

| Item | Status |
|------|--------|
| `Document` class (Id, Name, Type, Scene, Metadata, CreatedAt, ModifiedAt) | ⬜ |
| `DocumentType` enum (Notebook, Graph, Simulation, Geometry, Scene) | ⬜ |
| `Scene` class (Objects, Camera, Lights) | ⬜ |
| `Workspace.Documents` collection | ⬜ |
| `Workspace.ActiveDocument` property | ⬜ |
| Single document mode (default) | ⬜ |

### Milestone 1.12 — RenderObject Layer

| Item | Status |
|------|--------|
| `IRenderObject` interface (WorkspaceObjectId, Transform, IsVisible, Layer, BoundingBox, MeshData, Material, UpdateFrom) | ⬜ |
| `RenderObject` base class | ⬜ |
| `GraphRenderObject` (line segments, curve points) | ⬜ |
| `SurfaceRenderObject` (triangle mesh) | ⬜ |
| `GeometryRenderObject` (points, lines, polygons) | ⬜ |
| `MeshRenderObject` (indexed triangle mesh) | ⬜ |
| `RenderMaterial` (Color, Roughness, Metalness, Opacity, EmissiveColor) | ⬜ |

### Milestone 1.13 — Services

| Item | Status |
|------|--------|
| `ObjectFactory` service (create typed objects with defaults) | ⬜ |
| `ExpressionCompiler` service (parse and evaluate via backend) | ⬜ |
| `GraphCompiler` service (generate render data from graph objects) | ⬜ |
| `MeshGenerator` service (generate triangle meshes from geometry) | ⬜ |
| `SelectionService` (single/multi selection management) | ⬜ |
| `ClipboardService` (copy/paste workspace objects) | ⬜ |
| `ExportService` (export objects to files) | ⬜ |
| `ScreenshotService` (capture viewport as image) | ⬜ |

**Build gate:** 0 errors, 0 warnings. All Application Core components compile. Unit tests pass for EventBus, SelectionManager, CommandManager, UndoManager.

---

## Phase 2: Application Shell

> Empty window with panels. No functionality. Just docking.

### Milestone 2.1 — Empty Window

| Item | Status |
|------|--------|
| MainWindow with dark background (#0B0B12) | ⬜ |
| Menu bar (File, Edit, View, Insert, Help) | ⬜ |
| Toolbar (placeholder buttons) | ⬜ |
| Status bar | ⬜ |
| Window resize, minimize, maximize | ⬜ |

**Build gate:** 0 errors, 0 warnings, launches, dark theme.

### Milestone 2.2 — Panel Layout

| Item | Status |
|------|--------|
| Explorer panel (left, 220px) with header | ⬜ |
| Inspector panel (right, 280px) with header | ⬜ |
| Console panel (bottom, 140px) with header | ⬜ |
| Viewport (center, persistent) | ⬜ |
| Panel collapse/expand (click header) | ⬜ |
| Panel resize (drag border) | ⬜ |
| GridSplitter between panels | ⬜ |

**Build gate:** 0 errors, 0 warnings, all panels visible, resize works.

### Milestone 2.3 — Design System

| Item | Status |
|------|--------|
| Colors.axaml | ⬜ |
| Brushes.axaml | ⬜ |
| Controls.axaml (global styles) | ⬜ |
| Typography (Inter font, size scale) | ⬜ |
| Hover/pressed states | ⬜ |
| Focus indicators | ⬜ |

**Build gate:** 0 errors, 0 warnings, consistent dark theme.

---

## Phase 3: Connection Layer

> Wire the kernel to the panels. Selection → Inspector. Object Registry → Explorer. Viewport → Selection.
> No rendering yet. Just data flow.

### Milestone 3.1 — Selection → Inspector

| Item | Status |
|------|--------|
| Inspector subscribes to `ObjectSelectionChanged` via EventBus | ⬜ |
| When selection changes, Inspector shows selected object's properties | ⬜ |
| "No selection" state | ⬜ |
| Inspector renders property controls dynamically (text, checkbox, slider) | ⬜ |

**Build gate:** Selecting an object in code updates the Inspector panel.

### Milestone 3.2 — Object Registry → Explorer

| Item | Status |
|------|--------|
| Explorer subscribes to ObjectCreated, ObjectDeleted, ObjectPropertyChanged via EventBus | ⬜ |
| Explorer displays object tree (grouped by type) | ⬜ |
| Object icon (colored dot per type) | ⬜ |
| Object name (editable on double-click) | ⬜ |
| Click to select (calls SelectionManager) | ⬜ |
| Empty state ("No objects yet") | ⬜ |
| Visibility toggle | ⬜ |
| Delete button | ⬜ |

**Build gate:** Creating objects updates the Explorer. Clicking objects in Explorer selects them.

### Milestone 3.3 — Viewport → Selection

| Item | Status |
|------|--------|
| ViewportPanel receives mouse events | ⬜ |
| InteractionLayer translates input → ActiveTool calls | ⬜ |
| SelectTool calls SelectionManager on click | ⬜ |
| BoxSelectTool selects objects within rectangle | ⬜ |
| Selection highlights in viewport (bounding box) | ⬜ |

**Build gate:** Clicking in the viewport selects objects. Selection shows bounding box.

---

## Phase 4: Rendering

> The viewport renders workspace objects. Leverages existing GraphRenderer.

### Milestone 4.1 — Viewport Core

| Item | Status |
|------|--------|
| ViewportRenderer (wraps existing GraphRenderer) | ⬜ |
| SceneGraph (mirrors workspace object tree) | ⬜ |
| Background render thread | ⬜ |
| Viewport resize → renderer resize | ⬜ |
| PanTool (middle-mouse drag) | ⬜ |
| ZoomTool (scroll wheel) | ⬜ |
| Home (double-click) | ⬜ |
| Coordinate display (follows mouse) | ⬜ |
| Zoom level display | ⬜ |

**Build gate:** 0 errors, 0 warnings, viewport renders, pan/zoom works.

### Milestone 4.2 — Grid Pass

| Item | Status |
|------|--------|
| Reference grid (toggleable) | ⬜ |
| Axis lines (thicker, colored) | ⬜ |
| Tick labels (auto-scaling) | ⬜ |
| Toggle grid (Ctrl+G) | ⬜ |

### Milestone 4.3 — Scene Pass

| Item | Status |
|------|--------|
| Graph objects render in viewport (using existing GraphRenderer) | ⬜ |
| All 15 graph types (Cartesian, Polar, Parametric, Surface, VectorField, Contour, Heatmap, Scatter, Histogram, Fractal) | ⬜ |
| Multiple graphs simultaneously | ⬜ |
| Parameter sliders update in real-time | ⬜ |
| 3D rotation for surface/fractal | ⬜ |
| Fit All after adding graph | ⬜ |

### Milestone 4.4 — Selection Pass

| Item | Status |
|------|--------|
| Bounding box on selected object | ⬜ |
| Selection highlight (outline) | ⬜ |

**Build gate:** 0 errors, 0 warnings, graphs render, selection highlights.

---

## Phase 5: Console

> Expression evaluation. Quick math operations.

### Milestone 5.1 — Console Panel

| Item | Status |
|------|--------|
| Expression input (monospace) | ⬜ |
| Evaluate (Enter) — uses backend CAS | ⬜ |
| Quick operation buttons (Eval, Simplify, Factor, Expand, d/dx, ∫, lim, Series, Solve) | ⬜ |
| Results list (input/output pairs) | ⬜ |
| Error display (red text) | ⬜ |
| Clear button | ⬜ |
| Expression history (Up/Down arrows) | ⬜ |
| "Add to Workspace" creates Expression object | ⬜ |

**Build gate:** 0 errors, 0 warnings, all 9 CAS operations produce real results.

---

## Phase 6: Inspector

> Dynamic property editing. Two-way binding.

### Milestone 6.1 — Inspector Properties

| Item | Status |
|------|--------|
| GraphObject properties: Expression, Type, Color, LineWidth, Domain, Options | ⬜ |
| Color picker (preset circles) | ⬜ |
| Line width slider | ⬜ |
| Domain inputs (X/Y min/max) | ⬜ |
| Checkboxes (ShowFill, ShowGrid) | ⬜ |
| Parameter sliders (dynamic) | ⬜ |
| Two-way binding (change property → update object → re-render) | ⬜ |
| Quick Add presets section | ⬜ |
| Remove button | ⬜ |

**Build gate:** Editing Inspector properties updates objects and re-renders.

---

## Phase 7: Undo/Redo Integration

> Wire undo to all operations.

### Milestone 7.1 — Undo Integration

| Item | Status |
|------|--------|
| All object creation commands produce undo data | ⬜ |
| All object deletion commands produce undo data | ⬜ |
| All property changes produce undo data | ⬜ |
| Ctrl+Z undoes last operation | ⬜ |
| Ctrl+Shift+Z redoes | ⬜ |
| Undo restores exact previous state | ⬜ |

**Build gate:** All operations are undoable.

---

## Phase 8: Persistence

> Save and load workspaces.

### Milestone 8.1 — File I/O

| Item | Status |
|------|--------|
| Workspace serialization (JSON) | ⬜ |
| Workspace deserialization | ⬜ |
| Save (Ctrl+S) | ⬜ |
| Save As (Ctrl+Shift+S) | ⬜ |
| Open (Ctrl+O) | ⬜ |
| New Workspace (Ctrl+N) | ⬜ |
| Auto-save (every 60s) | ⬜ |
| Export PNG (Ctrl+E) | ⬜ |

**Build gate:** Save/load roundtrip works. Objects persist.

---

## Phase 9: Command Palette

> Power-user command discovery.

### Milestone 9.1 — Command Palette

| Item | Status |
|------|--------|
| Ctrl+Shift+P opens palette | ⬜ |
| Search by command name | ⬜ |
| Execute selected command | ⬜ |
| Escape to close | ⬜ |
| All registered commands appear | ⬜ |

**Build gate:** Command palette works, all commands accessible.

---

## Phase 10: Workspace Modes

> Tool configuration changes per mode.

### Milestone 10.1 — Mode System

| Item | Status |
|------|--------|
| Mode selector in toolbar | ⬜ |
| Mode changes panel visibility/content | ⬜ |
| Mathematics mode (default) | ⬜ |
| Visualization mode (3D focus) | ⬜ |
| Geometry mode (2D canvas focus) | ⬜ |
| Mode does not reset viewport | ⬜ |
| Mode does not reset objects | ⬜ |

**Build gate:** Switching modes changes tools, not viewport.

---

## Phase 11: Tool System

> Full tool implementations.

### Milestone 11.1 — Core Tools

| Item | Status |
|------|--------|
| PanTool | ⬜ |
| ZoomTool | ⬜ |
| RotateTool | ⬜ |
| SelectTool | ⬜ |
| BoxSelectTool | ⬜ |
| MoveTool (G key) | ⬜ |
| MeasureTool (distance, angle) | ⬜ |
| AddGraphTool | ⬜ |
| AddPointTool | ⬜ |
| AddLineTool | ⬜ |
| AddCircleTool | ⬜ |
| Tool menu (Space key) | ⬜ |
| Esc to previous tool | ⬜ |

**Build gate:** All tools work. No switch statements. Polymorphic dispatch.

---

## Phase 12: Viewport Gizmos

> Visual overlays.

### Milestone 12.1 — Gizmos

| Item | Status |
|------|--------|
| Origin marker | ⬜ |
| Coordinate cursor (follows mouse) | ⬜ |
| Measurement line (when measuring) | ⬜ |
| Object label overlay | ⬜ |
| Transform gizmo (move handles) | ⬜ |

**Build gate:** Gizmos appear and update.

---

## Phase 13: Geometry Objects

> Interactive 2D geometry.

### Milestone 13.1 — Geometry

| Item | Status |
|------|--------|
| Point creation (click to place) | ⬜ |
| Line creation (click two points) | ⬜ |
| Circle creation (center + radius) | ⬜ |
| Polygon creation (click vertices) | ⬜ |
| Geometry rendering in viewport | ⬜ |
| Geometry properties in Inspector | ⬜ |
| Measurement tools | ⬜ |
| Export geometry (SVG) | ⬜ |

**Build gate:** Geometry creates and renders.

---

## Phase 14: Timeline & Simulation

> Animation and simulation playback.

### Milestone 14.1 — Timeline

| Item | Status |
|------|--------|
| Timeline panel (below viewport) | ⬜ |
| Play/Pause/Stop controls | ⬜ |
| Frame slider | ⬜ |
| Speed control | ⬜ |
| Loop toggle | ⬜ |

### Milestone 14.2 — Simulation Integration

| Item | Status |
|------|--------|
| SimulationObject creation | ⬜ |
| Parameter controls in Inspector | ⬜ |
| Real-time playback in viewport | ⬜ |
| Real-time graphs during simulation | ⬜ |
| Data export (CSV) | ⬜ |

**Build gate:** Simulations run and visualize.

---

## Phase 15: AI Assistant

> AI-powered math assistance.

### Milestone 15.1 — AI Panel

| Item | Status |
|------|--------|
| Chat interface (message bubbles) | ⬜ |
| Expression input | ⬜ |
| AI response display | ⬜ |
| Context awareness (workspace objects) | ⬜ |
| Suggestion chips | ⬜ |
| Conversation history | ⬜ |
| Integration with CAS backend | ⬜ |

**Build gate:** AI responds to math questions.

---

## Completion Summary

| Phase | Description | Milestones | Status |
|-------|------------|------------|--------|
| 1 | Workspace Kernel | 9 | ⬜ |
| 2 | Application Shell | 3 | ⬜ |
| 3 | Connection Layer | 3 | ⬜ |
| 4 | Rendering | 4 | ⬜ |
| 5 | Console | 1 | ⬜ |
| 6 | Inspector | 1 | ⬜ |
| 7 | Undo/Redo Integration | 1 | ⬜ |
| 8 | Persistence | 1 | ⬜ |
| 9 | Command Palette | 1 | ⬜ |
| 10 | Workspace Modes | 1 | ⬜ |
| 11 | Tool System | 1 | ⬜ |
| 12 | Viewport Gizmos | 1 | ⬜ |
| 13 | Geometry | 1 | ⬜ |
| 14 | Timeline & Simulation | 2 | ⬜ |
| 15 | AI Assistant | 1 | ⬜ |
| **Total** | | **32** | **0%** |

---

## Priority Order

1. **Phase 1** — Workspace Kernel (the engine everything plugs into)
2. **Phase 2** — Application Shell (empty window with panels)
3. **Phase 3** — Connection Layer (selection → inspector, registry → explorer, viewport → selection)
4. **Phase 4** — Rendering (viewport renders objects)
5. **Phase 5** — Console (quick math)
6. **Phase 6** — Inspector (property editing)
7. **Phase 7** — Undo/Redo (safety net)
8. **Phase 8** — Persistence (save/load)
9. **Phase 9** — Command Palette (power user)
10. **Phase 10** — Workspace Modes (tool organization)
11. **Phase 11** — Tool System (viewport interaction)
12. **Phase 12** — Viewport Gizmos (visual feedback)
13. **Phase 13** — Geometry (interactive creation)
14. **Phase 14** — Timeline + Simulation (dynamic content)
15. **Phase 15** — AI (intelligence layer)

After Phase 4, the application renders graphs.
After Phase 8, it saves and loads workspaces.
After Phase 11, it feels like Blender for mathematics.
After Phase 15, it is the full product vision.
