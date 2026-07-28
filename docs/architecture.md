# MathVerse Desktop — Architecture Document

> MathVerse is not a collection of mathematical tools.
> MathVerse is a scientific operating environment.
> Every capability must integrate into one persistent workspace.
> Nothing exists outside the workspace.
> The workspace never changes.
> Objects change. Tools change. Selections change. Modes change.
> The workspace remains.

> **Status:** Approved
> **Date:** 2026-07-28
> **Scope:** Frontend architecture only. Backend is complete and untouched.

---

## 1. Product Identity

> **MathVerse is a scientific computing operating environment where mathematical objects are created, edited, visualized, simulated, and published inside one persistent workspace.**

It is NOT a calculator. It is NOT a graphing application. It is NOT a CAS. Those are features. The application is the environment.

It is built like Blender, Unreal Engine, MATLAB, Mathematica, Visual Studio, Adobe After Effects, and Fusion 360.

---

## 2. Core Principles

### 2.1 No Pages

There are no pages. There is no navigation. There is no page switching.

The application opens once. The window persists. The viewport persists. Objects change. Panels change. Selection changes. Tools change.

If any implementation introduces page navigation, page lifecycle management, or viewport replacement, reject it.

### 2.2 Object-Based Workspace

Everything inside the application is an object. Nothing is a page.

The workspace is a persistent container for objects. Objects are created, modified, and deleted. They are never "navigated to."

### 2.3 Selection Drives Everything

Selection is the primary interaction model. The currently selected object determines:
- What the Inspector shows
- What the Viewport highlights
- What context menus apply to
- What keyboard shortcuts do

### 2.4 Panels Never Own Data

Panels are views. They render data. They do not store it.

Objects own data. The Workspace owns objects.

### 2.5 No ViewModel-to-ViewModel Coupling

Panels communicate through the EventBus. Never through direct ViewModel references.

```
Workspace → EventBus → Panels
```

Never:

```
InspectorViewModel → GraphViewModel
```

### 2.6 Every Visible Control Is Real

If a button exists, it works. If a control exists, it is wired to a real operation. If a feature is unfinished, it is hidden — not displayed as a placeholder.

### 2.7 The Workspace Is Permanent

No feature may create a new application window, page, or workflow.

Every capability must integrate into the existing Workspace.

The Workspace is permanent. Objects are temporary. Panels are dynamic. Tools are interchangeable.

Pages do not exist.

### 2.8 Never Implement a Feature — Implement a System

Features are built from systems. Systems produce features.

Don't build "Graph Studio." Build:
- Expression Object → Graph Object → Renderer → Inspector → Viewport

Then Surface, Contour, Heatmap, Parametric, Vector Field become almost free.

Don't build "Evaluate Page." Build:
- Expression Object → ExpressionCompiler → Console → Inspector

Then Simplify, Factor, Expand, Differentiate become almost free.

Systems compose. Features don't.

### 2.9 The UI Never Performs Mathematics

The UI never performs mathematics.
The Workspace never performs mathematics.
The Renderer never performs mathematics.

Only the MathVerse backend performs mathematics.

The frontend visualizes, edits, selects, and orchestrates.

If any ViewModel, View, or Panel calls `Evaluate()`, `Parse()`, `Simplify()`, `Differentiate()`, or any mathematical operation directly — reject it. The backend owns all computation. The frontend owns all interaction.

### 2.10 Dependency Direction

Dependencies only go upward. Never downward.

```
Math Backend (source of truth for all computation)
      ↑
Application Services (compile, generate, transform)
      ↑
Workspace Core (objects, registry, selection, commands)
      ↑
Interaction Layer (tools, mouse, keyboard, viewport interaction)
      ↑
Renderer (pixels, meshes, render passes)
      ↑
UI (panels, menus, inspectors, viewports)
```

Never:
- Renderer → Workspace
- View → Backend
- Graph Object → UI
- Panel → Panel

This single rule keeps the architecture clean.

### 2.11 Build an Editor, Not a Viewer

Most AI code generators accidentally build a Viewer with buttons and graphs.

MathVerse is an Editor with objects, tools, and a workspace.

Blender is an editor. Unreal Engine is an editor. Visual Studio is an editor. CAD applications are editors.

MathVerse is an editor for mathematical objects.

---

## 3. Application Model

```
MathVerse Application
├── Application Shell (window, menu, toolbar, status bar)
├── Service Registry (dependency injection)
├── EventBus (global message bus)
├── Workspace (single, persistent)
│   ├── Object Tree (all workspace objects)
│   ├── Selection State
│   ├── Viewport State (camera, zoom, grid)
│   ├── Panel Layout (which panels are open, their sizes)
│   ├── Undo Stack (per-workspace)
│   └── Expression History
├── Tool System (active tool)
└── Clipboard Manager
```

### 3.1 Single Document

One workspace. One window. No tabs. No multi-document.

If a second workspace is needed in the future, it opens in a new window (like Blender). Workspace tabs can be added later without changing the foundation.

### 3.2 Persistence

The workspace serializes to a single `.mathverse` file (JSON format).

```
WorkspaceFile
├── Metadata (name, version, created, modified)
├── Objects[] (serialized object tree)
├── ViewportState (camera position, zoom, grid)
├── PanelLayout (panel sizes and visibility)
└── ExpressionHistory
```

Auto-save runs every 60 seconds. Crash recovery restores from the last auto-save.

---

## 4. Window Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ Menu Bar                                                        │
├─────────────────────────────────────────────────────────────────┤
│ Toolbar                                                         │
├───────────┬─────────────────────────────────┬───────────────────┤
│ Explorer  │                                 │ Inspector         │
│           │         Viewport                │                   │
│ Objects   │    (persistent, always live)    │ Properties        │
│ Graphs    │                                 │ of selected       │
│ Geometry  │                                 │ object            │
│ Scenes    │                                 │                   │
│           │                                 │ Quick Actions     │
├───────────┤                                 ├───────────────────┤
│ Console   │                                 │                   │
│ > eval    │                                 │                   │
│ > result  │                                 │                   │
├───────────┴─────────────────────────────────┴───────────────────┤
│ Status Bar                                                      │
└─────────────────────────────────────────────────────────────────┘
```

### 4.1 Panel Behavior

Panels are:
- **Resizable** — drag the border to resize
- **Collapsible** — click the header to collapse to a thin strip
- **Hardcoded positions** — Explorer left, Inspector right, Console bottom

Panels are NOT:
- Dockable (no drag-to-dock, no floating panels)
- Removable (they are always present, just collapsible)
- Rearrangeable (positions are fixed)

Real docking can be added in a future phase. The foundation (EventBus + object system) supports it without architectural changes.

### 4.2 Menu Bar

| Menu | Items |
|------|-------|
| File | New Workspace, Open, Save, Save As, Export, Exit |
| Edit | Undo, Redo, Cut, Copy, Paste, Select All, Preferences |
| View | Toggle Explorer, Toggle Inspector, Toggle Console, Zoom In, Zoom Out, Fit All, Home, Toggle Grid |
| Insert | Add Expression, Add Graph, Add Surface, Add Geometry, Add Simulation, Add Dataset |
| Help | About, Keyboard Shortcuts |

### 4.3 Toolbar

```
[+ Expression] [Add Graph] [Add Surface] │ [Zoom+] [Zoom-] [Fit] [Home] [Grid] [3D] │ [Expression: _____] │ [Export] │ [Status]
```

### 4.4 Status Bar

Left: Object count, selection info
Right: Zoom level, render status, time

---

## 5. Object System

### 5.1 Object Interface

Every workspace object inherits from this base. It is large by design — every object needs these capabilities.

```
IWorkspaceObject
├── Id: Guid
├── Name: string
├── Icon: string
├── TypeTag: string
├── IsVisible: bool
├── IsLocked: bool
├── IsPinned: bool
├── IsSelected: bool
├── IsExpanded: bool (for tree views)
├── Tags: List<string>
├── Category: string (for grouping in Explorer)
├── ParentId: Guid?
├── Children: List<Guid>
├── Transform: Matrix4x4 (for spatial objects)
├── Metadata: Dictionary<string, object>
├── BoundingBox: BoundingBox? (for viewport picking)
├── Layer: int (rendering order)
├── Owner: Guid? (which document owns this object)
├── CreatedAt: DateTime
├── ModifiedAt: DateTime
│
├── Methods:
│   ├── Clone(): IWorkspaceObject
│   ├── Serialize(): byte[]
│   ├── Destroy()
│   ├── Duplicate(): IWorkspaceObject
│   ├── Select()
│   └── Deselect()
```

### 5.2 Object Types

```
ObjectType
├── Expression      Mathematical expression
├── Graph           2D/3D plot
├── Surface         Parametric/implicit surface
├── Geometry        Points, lines, circles, polygons
├── Mesh            3D triangle mesh
├── Scene           Collection of objects with camera
├── Simulation      Dynamic system simulation
├── Animation       Keyframe animation
├── Dataset         Tabular data
├── Notebook        Rich text + math cells
├── Publication     Formatted document
├── Camera          Viewpoint definition
├── Light           Scene lighting
├── Material        Surface appearance
├── Image           Raster image
└── Script          User code
```

### 5.3 Object Hierarchy

Objects form a tree. Every object has an optional parent. Children inherit visibility from their parent.

```
Workspace
├── Graph: sin(x)
├── Graph: cos(x)
├── Geometry Group
│   ├── Point: A
│   ├── Line: AB
│   └── Circle: C
├── Simulation: Pendulum
├── Camera: Default
└── Scene: Main
```

### 5.4 Object Lifecycle

```
Create → Active → [Modified] → Deleted
```

- **Create:** Object is added to workspace. `ObjectCreated` event fires.
- **Active:** Object is alive, visible, editable.
- **Modified:** Object properties change. `ObjectPropertyChanged` event fires.
- **Deleted:** Object is removed. `ObjectDeleted` event fires. Undo restores it.

### 5.5 Object Properties

Each object type has specific properties:

| Type | Properties |
|------|-----------|
| Expression | ExpressionString, Result, Variables |
| Graph | Expression, Color, LineWidth, XMin, XMax, YMin, YMax, ShowFill, ShowGrid, Type (Cartesian/Polar/Parametric/Surface/VectorField/Contour/Heatmap/Scatter/Histogram/Fractal), ParameterSliders |
| Surface | Expression, Color, Opacity, XRange, YRange, ZRange, Resolution |
| Geometry | ShapeType, Points, Color, LineWidth, FillColor |
| Mesh | Vertices, Triangles, Material, Transform |
| Simulation | SimulationType, Parameters, Speed, IsRunning |
| Dataset | Data, ColumnNames, DataTypes |
| Camera | Position, Target, Up, FOV, NearClip, FarClip |
| Light | Type, Color, Intensity, Direction, Position |
| Material | Color, Roughness, Metalness, Opacity, EmissiveColor |

### 5.5 Document Model

A Document owns a scene and its objects. The Workspace owns Documents.

```
Workspace
├── ActiveDocument: Document
├── Documents: List<Document>
│
Document
├── Id: Guid
├── Name: string
├── Type: DocumentType (Notebook, Graph, Simulation, Geometry, Scene)
├── Scene: Scene
│   ├── Objects: List<IWorkspaceObject>
│   ├── Camera: Camera
│   └── Lights: List<Light>
├── Metadata: DocumentMetadata
├── CreatedAt: DateTime
├── ModifiedAt: DateTime
│
Workspace
├── ObjectRegistry (flat index of ALL objects across ALL documents)
├── EventBus
├── SelectionManager
├── CommandManager
├── UndoManager
├── ToolManager
├── ModeManager
└── ObjectFactory
```

Later, multi-document tabs (Phase 8+) become:
- Workspace → Documents → active tab → Document → Scene → Objects

For now: single document, single scene.

---

## 6. Object Lifecycle

Every object follows exactly the same lifecycle. No exceptions.

```
Create → Initialize → Compile → Render → Modify → Recompile → Render → Save → Destroy
```

### 6.1 Lifecycle Stages

| Stage | What Happens | Who Owns It |
|-------|-------------|-------------|
| Create | ObjectFactory creates typed object with defaults | ObjectFactory |
| Initialize | Object registers itself, fires ObjectCreated | Workspace + ObjectRegistry |
| Compile | Compiler generates render-ready data | Compiler Layer |
| Render | Renderer draws pixels on screen | Renderer |
| Modify | User changes properties via Inspector/Tool | Command System |
| Recompile | Compiler regenerates render data | Compiler Layer |
| Render | Renderer redraws | Renderer |
| Save | Object serialized to document | ProjectService |
| Destroy | Object removed, resources freed | Workspace + ObjectRegistry |

### 6.2 Lifecycle Rules

1. Every object goes through every stage
2. Modifying display properties (color, visibility) skips Compile
3. Modifying mathematical properties (expression, range) triggers Recompile
4. Destroy cleans up all associated RenderObjects
5. Undo reverses Modify and triggers Recompile

---

## 7. Workspace Modes

Workspace modes change the **layout of tools and panels** — not the viewport, not the objects, not the data.

The viewport stays alive. Objects stay alive. Only the tool configuration changes.

### 6.1 Mode Definitions

```
WorkspaceMode
├── Mathematics       Default. Expression bar, graph tools, CAS console.
├── Visualization     3D viewport focus, scene hierarchy, material inspector.
├── Geometry          2D canvas focus, construction tools, measurement.
├── Simulation        Parameter panel, timeline, real-time graphs.
├── Research          Dataset tools, statistical analysis, plotting.
├── Publication       Document editor, figure insertion, export.
└── Teaching          Presentation mode, step-by-step reveal, annotations.
```

### 6.2 Mode Behavior

When a mode is activated:
1. Viewport remains alive (no re-render, no state loss)
2. Explorer filters to relevant object types
3. Inspector shows mode-relevant panels
4. Toolbar shows mode-specific tools
5. Console shows mode-relevant commands
6. Active tool resets to the mode's default tool

### 6.3 Mode Selection

Mode selector is in the Toolbar — a dropdown or segmented control.

```
[Mathematics] [Visualization] [Geometry] [Simulation] [Research] [Publication]
```

---

## 8. Tool System

Tools are the primary interaction model for the viewport. Like Blender, the active tool determines what happens when the user interacts with the viewport.

No switch statements. No `if(tool==...)`. Every tool implements `ITool`. The workspace calls the active tool's methods. That's it.

### 7.1 Tool Interface

```
ITool
├── Name: string
├── Icon: string
├── Cursor: CursorType
├── Activate()
├── Deactivate()
├── OnMouseDown(point, button, modifierKeys)
├── OnMouseMove(point, button, modifierKeys)
├── OnMouseUp(point, button, modifierKeys)
├── OnWheel(delta)
├── OnKeyDown(key)
├── DrawOverlay()
└── GetGizmos(): List<IGizmo>
```

### 7.2 Tool Implementations

Each tool is a class that implements `ITool`. No enums. No switch statements.

```
PanTool         — middle-mouse drag
ZoomTool        — scroll wheel
RotateTool      — right-mouse drag
SelectTool      — left-click, box-select
MoveTool        — G key, drag handles
RotateGizmoTool — R key, drag rotation
ScaleTool       — S key, drag scale
MeasureTool     — click two points, show distance/angle
AddGraphTool    — click to place expression
AddPointTool    — click to place point
AddLineTool     — click two points
AddCircleTool   — center + radius
CrosshairTool   — coordinate display follows mouse
```

### 7.3 Active Tool State

The workspace maintains:
```
ActiveTool: ITool (currently active tool)
ToolHistory: Stack<ITool> (for tool switching with Esc)
```

Keyboard shortcut: Esc returns to the previous tool. Space opens a tool menu (like Blender).

### 7.4 Tool ↔ Workspace Flow

```
User clicks in viewport
    ↓
ViewportPanel.OnMouseDown()
    ↓
InteractionLayer.OnMouseDown()
    ↓
ActiveTool.OnMouseDown(point, button, keys)
    ↓
Tool calls Workspace methods (create object, select, transform)
    ↓
Workspace fires EventBus events
    ↓
Panels update
```

---

## 9. Viewport Gizmos

Gizmos are visual overlays in the viewport. They are NOT UI controls. They are visual indicators that help the user understand spatial relationships.

### 8.1 Gizmo Types

```
Gizmos
├── Grid              Reference grid (toggleable)
├── Axes              XYZ axes at origin (red/green/blue)
├── Origin Marker     Small crosshair at (0,0,0)
├── Bounding Box      Wireframe around selected object
├── Selection Highlight  Bright outline around selected object
├── Coordinate Cursor   Follows mouse, shows (x,y,z)
├── Measurement Line    When using measurement tool
├── Camera Indicator    Shows camera position and FOV
├── Light Indicator     Shows light position and direction
├── Transform Gizmo     Move/Rotate/Scale handles on selected object
└── Label Overlay       Object names floating near objects
```

### 8.2 Gizmo Rendering

Gizmos render on top of the scene. They are NOT affected by scene lighting or materials. They use the accent color palette (blue for selection, teal for geometry, gold for measurements).

---

## 10. Event System

### 9.1 EventBus

A central publish-subscribe message bus. Zero coupling between publishers and subscribers.

```
EventBus
├── Subscribe<TEvent>(handler: Action<TEvent>)
├── Unsubscribe<TEvent>(handler: Action<TEvent>)
├── Publish<TEvent>(event: TEvent)
└── EventQueue (deferred processing)
```

### 9.2 Event Types

```
Object Events
├── ObjectCreated(objectId)
├── ObjectDeleted(objectId)
├── ObjectRenamed(objectId, newName)
├── ObjectSelectionChanged(objectId?)
├── ObjectPropertyChanged(objectId, propertyName, oldValue, newValue)
├── ObjectVisibilityChanged(objectId, isVisible)
├── ObjectLockChanged(objectId, isLocked)
├── ObjectReparented(objectId, newParentId?)
└── ObjectOrderChanged(objectId, newIndex)

Viewport Events
├── ViewportCameraChanged(cameraState)
├── ViewportModeChanged(mode)
├── ViewportRenderComplete()

Command Events
├── CommandExecuted(commandName, parameters)
├── CommandFailed(commandName, error)
├── UndoPerformed()
├── RedoPerformed()

Workspace Events
├── WorkspaceLoaded(documentPath)
├── WorkspaceSaved(documentPath)
├── WorkspaceModified()
├── WorkspaceModeChanged(mode)

Tool Events
├── ToolActivated(toolName)
├── ToolDeactivated(toolName)
├── ToolProgress(percentage, message)

System Events
├── StatusMessage(text, level)
├── ErrorOccurred(exception)
├── PropertyChanged(propertyName)
```

### 9.3 Event Flow

```
User Action
    ↓
Command.Execute()
    ↓
Workspace modifies object
    ↓
EventBus.Publish(ObjectPropertyChanged)
    ↓
Explorer updates tree
Inspector updates properties
Viewport re-renders
Status bar updates
```

---

## 11. Command System

Every user action is a command. Commands are the only way to modify workspace state.

### 10.1 Command Interface

```
ICommand
├── Name: string
├── Description: string
├── Category: string
├── Icon: string
├── KeyboardShortcut: string?
├── CanExecute(context: CommandContext): bool
├── Execute(context: CommandContext): CommandResult
└── GetUndoData(): UndoData?
```

### 10.2 Command Categories

```
Object Commands
├── CreateGraph(expression, type, color)
├── DeleteObject(objectId)
├── DuplicateObject(objectId)
├── RenameObject(objectId, newName)
├── SetObjectVisibility(objectId, isVisible)
├── SetObjectProperty(objectId, propertyName, value)
├── GroupObjects(objectIds)
├── UngroupObjects(groupId)

Viewport Commands
├── ZoomIn
├── ZoomOut
├── FitAll
├── Home
├── ToggleGrid
├── Toggle3D
├── SetViewportCamera(cameraState)

File Commands
├── NewWorkspace
├── OpenWorkspace(path)
├── SaveWorkspace
├── SaveWorkspaceAs(path)
├── ExportPNG(path)
├── ExportSVG(path)
├── ExportJSON(path)

Edit Commands
├── Undo
├── Redo
├── Cut
├── Copy
├── Paste
├── SelectAll
├── DeselectAll

View Commands
├── ToggleExplorer
├── ToggleInspector
├── ToggleConsole
├── SetWorkspaceMode(mode)

Tool Commands
├── SetActiveTool(tool)
├── PreviousTool

CAS Commands
├── Evaluate(expression)
├── Simplify(expression)
├── Factor(expression)
├── Expand(expression)
├── Differentiate(expression, variable)
├── Integrate(expression, variable)
├── Solve(equation, variable)
├── ComputeLimit(expression, variable, target)
├── TaylorSeries(expression, variable, center, order)
```

### 10.3 Command Context

```
CommandContext
├── Workspace: Workspace
├── SelectedObjects: IReadOnlyList<IWorkspaceObject>
├── ActiveTool: ITool
├── ViewportState: ViewportState
└── UserState: Dictionary<string, object>
```

### 10.4 Command Palette

Ctrl+Shift+P opens a command palette (like VS Code). It searches all registered commands by name and category. Selecting a command executes it.

This is a power-user accelerator. Every command is also accessible through visible UI elements (menus, toolbar buttons, context menus).

---

## 12. Undo System

### 11.1 Architecture

```
UndoManager
├── Per-workspace UndoStack
│   ├── Transaction 1
│   │   ├── Operations: [CreateObject(graphId)]
│   │   └── Reverse: [DeleteObject(graphId)]
│   ├── Transaction 2
│   │   ├── Operations: [ChangeProperty(graphId, "color", "#FF0000")]
│   │   └── Reverse: [ChangeProperty(graphId, "color", "#4A9EFF")]
│   └── ...
├── MaxDepth: 500 transactions
├── MemoryBudget: 100MB
└── Auto-prune: Oldest transactions removed when budget exceeded
```

### 11.2 Transaction Grouping

A transaction is a logical group of operations created by a single user gesture:

| Gesture | Transaction |
|---------|------------|
| Mouse drag (down → up) | One transaction |
| Text input (focus → commit) | One transaction |
| Slider drag (down → up) | One transaction |
| Single click | One transaction |
| Multiple rapid commands (< 200ms, same type) | Merged into one transaction |

### 11.3 Undo Rules

- Undo restores the previous state exactly
- Redo re-applies the undone operation
- Undo after a new operation discards the redo stack
- Undo does NOT affect the viewport camera
- Undo DOES affect object properties, visibility, order, and existence
- Undo is per-workspace (not global)

### 11.4 Serialization

Undo history is NOT serialized. The workspace file saves the final state only. Undo is available only during the current session.

---

## 13. Selection System

### 12.1 Selection Model

```
SelectionManager
├── SelectedObjects: IReadOnlyList<IWorkspaceObject>
├── PrimarySelection: IWorkspaceObject? (last selected)
├── SelectionMode: Single | Multi
└── SelectionHistory: Stack<Guid> (for selection undo)
```

### 12.2 Selection Behavior

| Action | Result |
|--------|--------|
| Left-click on object | Select that object (deselect others) |
| Ctrl+Left-click on object | Toggle selection (add/remove) |
| Left-click on empty space | Deselect all |
| Ctrl+A | Select all objects |
| Escape | Deselect all |
| Tab | Cycle selection to next object |
| Shift+Tab | Cycle selection to previous object |
| Up/Down arrows | Navigate object list in Explorer |

### 12.3 Selection Effects

When selection changes:
1. **Inspector** updates to show selected object's properties
2. **Viewport** highlights selected object (bounding box, outline)
3. **Explorer** highlights selected object in the tree
4. **Toolbar** updates context-sensitive buttons
5. **Status bar** shows selection info

---

## 14. Services

Not everything belongs in ViewModels. Services are stateless or singleton classes that provide capabilities to the workspace.

### 13.1 Service Registry

```
Services
├── ObjectFactory        Creates typed workspace objects with defaults
├── ExpressionCompiler   Parses and evaluates expressions via backend
├── GraphCompiler        Generates render data from graph objects
├── MeshGenerator        Generates triangle meshes from geometry
├── SelectionService     Manages single/multi selection
├── ClipboardService     Copy/paste workspace objects
├── HistoryService       Undo/redo operations
├── ExportService        Export objects to files (PNG, PDF, LaTeX, OBJ, SVG)
├── ImportService        Import objects from files
├── ScreenshotService    Capture viewport as image
├── ProjectService       Save/load workspace state
└── SettingsService      User preferences, themes, keyboard shortcuts
```

### 13.2 Service Rules

1. Services never reference ViewModels
2. Services never own UI state
3. Services communicate via EventBus for side effects
4. ViewModels call services; services don't call ViewModels
5. Services are testable in isolation

---

## 15. Console

The Console is a command-line interface for quick mathematical operations.

### 13.1 Console Features

```
Console
├── Expression Input (monospace, with history navigation)
├── Quick Operations Bar: [Eval] [Simplify] [Factor] [Expand] [d/dx] [∫] [lim] [Series] [Solve]
├── Results List (scrollable, with input/output pairs)
├── Error Display (red text, specific messages)
└── Clear button
```

### 13.2 Console Behavior

- Type an expression, press Enter → evaluates and shows result
- Click a quick operation button → applies that operation to the current expression
- Results accumulate in the list
- Ctrl+L clears the console
- Up/Down arrows navigate expression history
- Expressions from the console can be added to the workspace as Expression objects

### 13.3 Console ↔ Workspace Integration

The console is not isolated. It connects to the workspace:
- "Add to Workspace" button creates an Expression object from the console result
- Console commands can reference workspace objects by name
- Console history is part of the workspace state

---

## 16. Compiler Layer

Between workspace objects and render objects sits a compiler layer. This separates computation from display.

```
Workspace Object (data, properties, expression)
      ↓
Compiled Object (computed samples, evaluated results, generated geometry)
      ↓
Render Object (triangles, colors, transforms)
      ↓
Viewport Renderer (rasterize to screen)
```

### 14.1 Why a Compiler Layer

Without this, changing colors requires recompiling expressions. Changing visibility requires recomputing math. Changing zoom requires evaluating again.

Professional software separates these stages.

### 14.2 Compiler Pipeline

| Stage | Input | Output | Trigger |
|-------|-------|--------|---------|
| Expression Compiler | Expression string | Parsed Expression | Object created/modified |
| Graph Compiler | Parsed Expression + range | Sampled points | Expression changed |
| Mesh Generator | Geometry definition | Triangle mesh | Geometry changed |
| Surface Compiler | Expression + bounds | Vertex grid | Expression changed |
| Render Compiler | Compiled data | RenderObject | Compiled data changed |

### 14.3 Compiler Rules

1. Compilers never draw
2. Compilers never select
3. Compilers never save
4. Compilers only generate render-ready data
5. Changing display properties (color, visibility, line width) skips recompilation
6. Changing mathematical properties (expression, range, resolution) triggers recompilation

### 14.4 Full Pipeline Example

```
User types "sin(x)" in Inspector
      ↓
Expression Compiler parses → Parsed Expression
      ↓
Graph Compiler samples → 1000 points
      ↓
Render Compiler builds → Line segments
      ↓
Viewport Renderer draws → Pixels on screen
```

User changes color to red:
```
RenderObject.Material.Color = Red
      ↓
Viewport Renderer redraws (no recompilation)
```

---

## 17. RenderObject Layer

Workspace objects are never rendered directly. Instead, each object produces a RenderObject that the renderer consumes.

```
WorkspaceObject (data, logic, properties)
    ↓
RenderObject (triangles, colors, transforms)
    ↓
Renderer (rasterize to screen)
```

### 14.1 Why a Separate Render Layer

Without this, the renderer becomes coupled to the workspace:
```
BAD: Renderer → WorkspaceObject → properties → draw
```

With RenderObject:
```
GOOD: WorkspaceObject → RenderObject → Renderer
```

Benefits:
- Workspace objects can change without affecting rendering
- Rendering can change without affecting workspace objects
- GPU data (VBOs, textures) live on RenderObject, not WorkspaceObject
- Multiple RenderObjects can represent one WorkspaceObject (LOD, selection highlight)

### 14.2 RenderObject Interface

```
IRenderObject
├── WorkspaceObjectId: Guid
├── Transform: Matrix4x4
├── IsVisible: bool
├── Layer: int
├── BoundingBox: BoundingBox?
├── MeshData: MeshData? (vertices, indices, normals)
├── Material: RenderMaterial
└── UpdateFrom(IWorkspaceObject)  ← called when workspace object changes
```

### 14.3 RenderObject Mapping

| WorkspaceObject | RenderObject | GPU Data |
|-----------------|--------------|----------|
| GraphObject | GraphRenderObject | Line segments, curve points |
| SurfaceObject | SurfaceRenderObject | Triangle mesh |
| GeometryObject | GeometryRenderObject | Points, lines, polygons |
| MeshObject | MeshRenderObject | Indexed triangle mesh |
| SceneObject | SceneRenderObject | Full scene graph |

### 14.4 Renderer (unchanged architecture)

```
Renderer
├── GridPass (reference grid, axes)
├── ScenePass (RenderObjects → pixels)
├── SelectionPass (outline selected objects)
├── GizmoPass (transform handles)
└── OverlayPass (labels, annotations)
```

Later phases add: ShadowPass, PostProcessingPass, PhysicsOverlayPass, MeasurementOverlayPass.

---

## 18. Rendering Architecture

One renderer. Multiple render passes. Not separate renderers per object type.

### 14.1 Rendering Pipeline

```
UI Thread
    │
    ├── ViewportPanel
    │   └── Displays: RenderResult (Bitmap)
    │
    ├── InteractionLayer
    │   └── Translates input → Tool calls
    │
    └── ViewportRenderer
        ├── SceneGraph (mirrors workspace object tree)
        │   ├── SceneNode (per visible object)
        │   │   ├── Transform
        │   │   ├── Geometry
        │   │   ├── Material
        │   │   └── Children
        │   └── Camera
        │
        ├── RenderPasses (executed in order)
        │   ├── GridPass        (reference grid, axes, tick labels)
        │   ├── ScenePass       (graph curves, surfaces, geometry, meshes)
        │   ├── SelectionPass   (bounding box, highlight on selected object)
        │   ├── GizmoPass       (transform handles, measurement lines, cursors)
        │   └── OverlayPass     (object labels, coordinate display)
        │
        ├── RendererBackend
        │   └── GraphRenderer (existing software renderer)
        │       └── Future: OpenGLRenderer, GPURenderer
        │
        └── RenderThread (background, target 60fps)
            └── Produces: PixelBuffer → Bitmap → ViewportPanel
```

### 14.2 Single Renderer

```
ViewportRenderer
├── Initialize(width, height)
├── SetViewport(centerX, centerY, scale)
├── SetCamera(camera)
├── Clear()
├── RenderAll(sceneGraph, gizmos, selection)
│   ├── ExecutePass(GridPass)
│   ├── ExecutePass(ScenePass, visibleObjects)
│   ├── ExecutePass(SelectionPass, selectedObject)
│   ├── ExecutePass(GizmoPass, activeGizmos)
│   └── ExecutePass(OverlayPass, labels)
├── GetBuffer(): PixelBuffer
└── Resize(width, height)
```

The existing `GraphRenderer` becomes the backend for `ScenePass`. It already renders curves, surfaces, polar, parametric, vector fields, contour, heatmap, scatter, histogram, and fractals. No rewrite needed.

### 14.3 Scene Graph

The scene graph mirrors the workspace object tree but is optimized for rendering:

```
SceneGraph
├── Root
│   ├── GridNode (reference grid)
│   ├── AxesNode (XYZ axes)
│   ├── ObjectNode: sin(x) (from Graph object)
│   │   ├── Geometry: CurveGeometry
│   │   └── Material: LineMaterial(color, width)
│   ├── ObjectNode: cos(x)
│   ├── GroupNode: Geometry Group
│   │   ├── ObjectNode: Point A
│   │   ├── ObjectNode: Line AB
│   │   └── ObjectNode: Circle C
│   └── GizmoNode: Bounding Box (selected object)
└── Camera
```

### 14.4 Render Thread

Rendering runs on a background thread to keep the UI responsive:

1. Workspace changes → `EventBus.Publish(ObjectPropertyChanged)`
2. `RendererService` receives event → marks scene graph dirty
3. Background thread → traverses scene graph → renders to `PixelBuffer`
4. `PixelBuffer` → PNG encode → `Bitmap`
5. `Dispatcher.Post()` → updates `ViewportPanel.Source`

The UI thread never blocks on rendering.

---

## 19. Interaction Layer

Separate from rendering. Separate from the workspace. Sits between the viewport and the tools.

### 15.1 Layer Position

```
Renderer
    ↓
Viewport
    ↓
Interaction Layer
    ↓
Tool
    ↓
Workspace
```

Never:

```
Renderer → Workspace
```

This separation makes professional software possible. The renderer knows nothing about tools. The workspace knows nothing about rendering. The interaction layer is the only thing that connects them.

### 15.2 Interaction Layer Responsibilities

```
InteractionLayer
├── Translates raw input (mouse, keyboard) into tool calls
├── Manages tool state (active tool, tool history)
├── Handles viewport coordinate conversion (screen ↔ math)
├── Manages cursor changes based on active tool
├── Dispatches tool results to workspace
└── Coordinates between tools and renderer (gizmo overlay)
```

### 15.3 Data Flow

```
Mouse/Keyboard Event
    ↓
ViewportPanel (Avalonia control)
    ↓
InteractionLayer.OnInput()
    ↓
ActiveTool.OnMouseXxx() / OnKeyDown()
    ↓
Tool calls Workspace methods
    ↓
Workspace modifies objects
    ↓
EventBus fires events
    ↓
Panels update, Renderer re-renders
```

---

## 20. Threading Model

```
┌──────────────────────────────────────────────────────────┐
│ UI Thread (Avalonia Dispatcher)                           │
│ ├── All UI rendering                                     │
│ ├── Input handling                                       │
│ ├── Inspector property updates                           │
│ ├── Explorer tree updates                                │
│ ├── EventBus dispatch                                    │
│ └── Menu/Toolbar/StatusBar updates                       │
├──────────────────────────────────────────────────────────┤
│ Render Thread (Background, 60fps target)                  │
│ ├── Scene graph traversal                                │
│ ├── Frustum culling                                      │
│ ├── Draw calls to renderer backend                       │
│ ├── Compositing to PixelBuffer                           │
│ └── Post to UI thread → ViewportPanel.Source = bitmap    │
├──────────────────────────────────────────────────────────┤
│ Compute Thread Pool (System.Threading.Tasks)              │
│ ├── CAS operations (simplify, factor, solve)             │
│ ├── Numerical computation (integration, root-finding)    │
│ ├── Simulation stepping                                  │
│ └── Animation evaluation                                 │
├──────────────────────────────────────────────────────────┤
│ I/O Thread (thread pool)                                  │
│ ├── File save/load                                       │
│ ├── Serialization/deserialization                        │
│ └── Export (PNG, SVG, JSON)                              │
└──────────────────────────────────────────────────────────┘

Communication:
├── UI ← Render: Dispatcher.Post(() => bitmap = ...)
├── UI ← Compute: Dispatcher.Post(() => result = ...)
├── UI → Compute: Task.Run(() => CAS.Evaluate(expr))
└── UI → I/O: Task.Run(() => SaveWorkspace(doc))
```

---

## 21. Interaction Model

### 17.1 Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+Enter | Evaluate expression |
| Enter | Add graph from expression bar |
| Delete | Remove selected object |
| Ctrl+Z | Undo |
| Ctrl+Shift+Z | Redo |
| Ctrl+C | Copy |
| Ctrl+V | Paste |
| Ctrl+A | Select all |
| Ctrl+G | Toggle grid |
| Ctrl+F | Fit all |
| H | Home view |
| Ctrl++ | Zoom in |
| Ctrl+- | Zoom out |
| Ctrl+Shift+P | Command palette |
| Ctrl+E | Toggle Explorer |
| Ctrl+I | Toggle Inspector |
| Ctrl+` | Toggle Console |
| Escape | Deselect / cancel tool |
| G | Move tool (Blender-style) |
| R | Rotate tool |
| S | Scale tool |
| Tab | Cycle selection |
| Space | Tool menu |
| 1-7 | Workspace modes |

### 17.2 Mouse Controls

| Input | Action |
|-------|--------|
| Left-click | Select object |
| Left-drag (on object) | Move/transform (depends on active tool) |
| Left-drag (on empty) | Box select |
| Middle-drag | Pan viewport |
| Right-drag | Rotate viewport (3D) |
| Scroll wheel | Zoom |
| Double-click | Fit all / home |
| Right-click | Context menu |

### 17.3 Context Menu

Right-click on an object shows:
```
┌─────────────────────┐
│ Delete              │
│ Duplicate           │
│ Rename              │
│ ─────────────────── │
│ Hide                │
│ Hide Others         │
│ Lock                │
│ ─────────────────── │
│ Add to Favorites    │
│ Copy Expression     │
│ Export...           │
└─────────────────────┘
```

---

## 22. Accessibility

| Requirement | Implementation |
|-------------|---------------|
| Keyboard-only navigation | Tab through panels, arrow keys in lists, Enter to activate |
| Screen reader support | ARIA labels on all controls, live regions for status updates |
| High contrast | Dark theme is default. Future: high-contrast theme |
| Focus indicators | Visible focus rings on all interactive elements |
| Font scaling | UI respects system font scaling |

---

## 23. Error Handling

| Error Type | Presentation |
|-----------|-------------|
| Parse error | Red inline text below expression input |
| Evaluation error | Red text in console result |
| Render error | Status bar notification |
| File I/O error | Toast notification with retry |
| Undo failure | Status bar warning |
| Object creation failure | Toast notification |

No raw exception messages. No stack traces. Always user-friendly text.

---

## 24. What Is NOT in Scope (Phase 1)

The following are explicitly postponed:

| System | Phase |
|--------|-------|
| Plugin architecture | 10+ |
| Scripting (Python, C#) | 10+ |
| Multi-document tabs | 8 |
| Docking framework (floating panels) | 8 |
| OpenGL/Vulkan renderer | 8 |
| GPU compute | 10+ |
| Collaboration (networking, sync) | 12+ |
| Update system | 6 |
| 3D CAD tools | 7 |
| Animation timeline | 5 |
| Notebook cells | 6 |
| Publication editor | 7 |
| Teaching mode | 8 |
| AI integration | 6 |

---

## 25. Scalability Assessment

| Capability | Can this architecture scale? | What's needed? |
|-----------|------------------------------|----------------|
| 3D CAD | Yes | Renderer interface → OpenGL backend |
| Scientific Visualization | Yes | Dataset objects, statistical plotting |
| Symbolic Mathematics | Yes, already built | CAS service wrapper |
| Simulations | Yes | Compute thread integration, timeline |
| GPU Rendering | Yes | GPU renderer backend |
| AI Tools | Yes | AI service integration, chat panel |
| Plugins | Yes | Plugin host, API surface |
| Scripting | Yes | Script objects, script engine |
| Collaboration | Partially | CRDT/OT, networking layer |

---

## 26. Summary

This architecture produces a professional desktop application that feels like Blender, not a website.

**Key decisions:**
1. Single workspace, no pages, no navigation
2. Object-based: everything is an object
3. Selection-driven: selection determines what panels show
4. EventBus communication: no ViewModel-to-ViewModel coupling
5. Hardcoded panel layout: Explorer | Viewport | Inspector + Console
6. Workspace modes: tool configuration changes, not viewport
7. Tool system: active tool determines viewport interaction (polymorphic, no switch statements)
8. Command system: every action is a command with undo
9. Interaction Layer: separates rendering from workspace (Renderer → Viewport → Interaction → Tool → Workspace)
10. Single renderer with render passes: GridPass, ScenePass, SelectionPass, GizmoPass, OverlayPass
11. Software renderer first: existing GraphRenderer, OpenGL later
12. Threading: UI + Render + Compute + I/O

**The foundation supports everything that comes after without architectural changes.**
