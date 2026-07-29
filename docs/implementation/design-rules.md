# MathVerse — Mandatory Design Rules

These rules are immutable. Prepend them to every prompt. If any implementation violates these rules, reject it and redesign.

---

## Rule 1: MathVerse Is NOT a Website

MathVerse is NOT a calculator.
MathVerse is NOT a website.
MathVerse is NOT a collection of pages.
MathVerse is NOT a ribbon application.
MathVerse is NOT a dashboard.

MathVerse is a professional desktop application.

Think Blender.
Think Unreal Engine.
Think Maya.
Think Fusion 360.
Think Adobe Photoshop.
Think VS Code.
Think MATLAB Desktop.
Think Mathematica Notebook.

---

## Rule 2: The Application Opens ONCE

The application opens ONCE.
The window never changes.
The viewport never changes.
Objects change.
Panels change.
Selection changes.
Tools change.
The workspace changes.

---

## Rule 3: Pages DO NOT Exist

Pages DO NOT exist.
Navigation between pages does NOT exist.
Every feature integrates into the persistent workspace.

If any implementation introduces:
- Page navigation
- UserControls that replace the viewport
- Page lifecycle management
- ContentControl page swapping
- Frame navigation
- Tabbed page application
- Wizard-style UI
- Anything resembling a website

**Reject that implementation and redesign it.**

---

## Rule 4: Everything Is an Object

The workspace contains objects, not pages.

Examples:
- Expression
- Graph
- Surface
- Geometry
- Simulation
- Animation
- Dataset
- Notebook
- Publication
- Camera
- Image
- Mesh
- Scene

Objects remain alive until deleted.
Objects have: ID, Name, Visibility, Selection, Properties, History, Tags, Serialization.

---

## Rule 5: Selection Drives Everything

Selection is a first-class citizen.

The Inspector shows properties of the selected object.
The Viewport highlights the selected object.
The Explorer highlights the selected object.
Context menus apply to the selected object.

Everything depends on selection.

---

## Rule 6: Panels Never Own Data

Panels are views.
Objects own data.
Workspace owns objects.

This distinction must never be broken.

---

## Rule 7: No ViewModel Knows Another ViewModel

Never.

Everything goes:
```
Workspace
    ↓
EventBus
    ↓
Panels
```

Never:
```
InspectorViewModel
    ↓
GraphViewModel
```

---

## Rule 8: Every Visible Control Performs a Real Action

If a button exists, it must do something.
If a control exists, it must be wired to a real backend operation.
If a feature is unfinished, hide it. Do not expose dead controls.
No placeholder text. No placeholder buttons. No dead controls.

---

## Rule 9: The Backend Is Complete

Do NOT modify the backend.
Do NOT rewrite the mathematical engine.
Do NOT redesign the CAS.
Do NOT replace renderers.
Do NOT touch parsing, evaluation, differentiation, integration, simplification, equation solving, geometry, visualization, simulation, animation, or export.

Only the frontend is under discussion.

---

## Rule 10: Professional Desktop Software Patterns

The application should feel like:
- Blender (layout, tools, undo, object hierarchy)
- Unreal Engine (viewport, outliner, details panel)
- Adobe After Effects (timeline, composition, properties)
- Visual Studio (command palette, keyboard workflow)
- MATLAB Desktop (workspace, command window, editor)
- Mathematica Notebook (expression-driven, computational)

NOT like:
- A website
- An MVVM sample
- A dashboard
- A page application
- A mobile app
- A calculator

---

## Rule 11: The Workspace Is Permanent

No feature may create a new application window, page, or workflow.

Every capability must integrate into the existing Workspace.

The Workspace is permanent. Objects are temporary. Panels are dynamic. Tools are interchangeable.

Pages do not exist.

---

## Rule 12: Never Implement a Feature — Implement a System

Features are built from systems. Systems produce features.

Don't build "Graph Studio." Build:
- Expression Object → Graph Object → Renderer → Inspector → Viewport

Then Surface, Contour, Heatmap, Parametric, Vector Field become almost free.

Don't build "Evaluate Page." Build:
- Expression Object → ExpressionCompiler → Console → Inspector

Then Simplify, Factor, Expand, Differentiate become almost free.

Systems compose. Features don't.

---

## Rule 13: Never Duplicate State

Every piece of information has exactly one owner. Everything else references it.

Bad:
```
Explorer stores selection
Inspector stores selection
Viewport stores selection
```

Good:
```
SelectionService owns selection
  Explorer observes
  Inspector observes
  Viewport observes
```

This single rule prevents countless synchronization bugs.

---

## Rule 14: Build an Editor, Not a Viewer

Most AI code generators accidentally build a Viewer with buttons and graphs.

MathVerse is an Editor with objects, tools, and a workspace.

Blender is an editor. Unreal Engine is an editor. Visual Studio is an editor. CAD applications are editors.

MathVerse is an editor for mathematical objects.

---

## Rule 15: The UI Is Only a Projection of the Workspace

The UI must never own application state. The Workspace owns the state. Every panel is only a visual representation of that state.

```
Explorer  projects  Workspace.Objects
Inspector projects  SelectedObject
Timeline  projects  AnimationSystem
Console   projects  CommandSystem
Viewport  projects  Scene
```

If a panel disappears, nothing is lost. If a panel is recreated, it must reconstruct itself entirely from Workspace state.

Panels are disposable. Workspace is permanent.

No panel may become the source of truth. This rule is absolute.

---

## Rule 16: Every User Action Is A Command

Nothing happens directly. Every user interaction becomes a Command.

```
Mouse drag        → MoveObjectCommand
Keyboard Delete   → DeleteObjectCommand
Ctrl+D            → DuplicateObjectCommand
Expression Edit   → ChangeExpressionCommand
Color Change      → ChangeColorCommand
```

Every command must support:
- `Execute()`
- `Undo()`
- `Redo()`

History stores commands. Workspace never stores "previous values."

Commands are the only mutation mechanism. This is how Blender, Unreal, Visual Studio, and Photoshop maintain deterministic undo/redo. No exceptions.

---

## Rule 17: The Workspace Is the Product

The workspace is not a window. The workspace is not a collection of panels. The workspace is not a UI.

The Workspace is the application itself.

Explorer, Inspector, Viewport, Timeline, Console, Toolbar, Properties, Notifications, History, Command Palette — these are all merely different projections of the exact same Workspace.

No panel owns data. No panel owns logic. No panel owns state.

Removing every panel should not destroy a single WorkspaceObject. The Workspace must continue to exist.

---

## Rule 18: Build Infrastructure Before Experience

Never build a user-facing feature first. Always build the infrastructure that makes dozens of future features trivial.

Bad: "Implement Graph."

Good: Implement WorkspaceObject, GraphObject, ObjectFactory, SelectionService, CommandSystem, GraphCompiler, RenderObject, RenderPass, ViewportRenderer. Only after those exist should Graph become possible.

The same rule applies to every subsystem. Do not build features. Build systems that naturally produce features.
