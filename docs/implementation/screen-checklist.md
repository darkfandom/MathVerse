# MathVerse Screen Checklist

## Quality Gate

A page is **complete** only when ALL checkboxes below are checked. No exceptions. No "good enough." No "we'll fix it later."

**A page that has not passed all checkboxes must not be considered finished.**

---

## Checklist Template

Every screen must satisfy this checklist. Copy this template for each page.

```
### [Page Name]

**Vision Spec**: docs/vision-board/[filename].md
**Backend**: [relevant backend projects]
**Status**: ⬜ Not Started | 🟡 In Progress | ☑ Complete

#### Visual Completion
- [ ] Page layout matches vision board exactly
- [ ] All colors from the palette (no custom colors)
- [ ] All typography from the scale (no arbitrary sizes)
- [ ] All spacing from the spacing scale (no arbitrary values)
- [ ] All border radii correct
- [ ] All shadows correct
- [ ] All icons match style (1.5px stroke, round caps)
- [ ] Sidebar integration works
- [ ] Toolbar integration works
- [ ] Status bar reflects page state
- [ ] Empty state designed and implemented
- [ ] Error state designed and implemented
- [ ] Loading state designed and implemented

#### Interactions Complete
- [ ] All hover states work (50ms response)
- [ ] All click states work (visual feedback)
- [ ] All buttons are functional (no dead buttons)
- [ ] All toggles work with animation
- [ ] All sliders work with value display
- [ ] All dropdowns work with keyboard navigation
- [ ] All form inputs validate and show errors
- [ ] Context menus work where applicable
- [ ] Drag and drop works where applicable
- [ ] No hidden functionality

#### Keyboard Shortcuts
- [ ] All shortcuts from interaction-guidelines.md work
- [ ] Page-specific shortcuts implemented
- [ ] Tab navigation works correctly
- [ ] Focus indicators visible on keyboard nav
- [ ] Focus trap in modals
- [ ] Escape closes modals/menus/search

#### Animations
- [ ] Page transition: fade + slide (300ms)
- [ ] Card hover: translateY(-2px) + glow (200ms)
- [ ] Button press: scale(0.96) (80ms)
- [ ] Panel open/close: width + opacity (200ms)
- [ ] Modal open: scale + opacity spring (200ms)
- [ ] Toast enter: slide from right spring (200ms)
- [ ] Skeleton pulse: 1500ms loop
- [ ] No instant transitions (minimum 80ms)
- [ ] No transitions > 500ms for UI elements

#### Backend Connected
- [ ] Primary action triggers backend operation
- [ ] Backend results render correctly in UI
- [ ] Backend errors display as user-friendly messages
- [ ] Backend loading states show appropriate indicators
- [ ] No mock data in production paths
- [ ] No hardcoded placeholder results

#### Loading State
- [ ] Skeleton screen shown during data fetch
- [ ] Spinner shown during button operations
- [ ] Progress bar shown for operations > 2 seconds
- [ ] Loading state dismissed when data arrives
- [ ] No blank screens during loading

#### Empty State
- [ ] Empty state shown when no data exists
- [ ] Empty state has illustration/icon
- [ ] Empty state has title explaining the page
- [ ] Empty state has description explaining how to start
- [ ] Empty state has primary action button
- [ ] Empty state matches vision board design

#### Error State
- [ ] Backend errors shown as inline messages
- [ ] Network errors shown with retry option
- [ ] Validation errors shown below inputs
- [ ] No raw exception messages shown
- [ ] No stack traces shown
- [ ] Error toasts auto-dismiss or have manual dismiss

#### Responsive
- [ ] Full layout at ≥ 1400px width
- [ ] Side panels collapse at 1000-1399px
- [ ] Minimum window size 1024x768 works
- [ ] No content overflow at any supported size
- [ ] Panel resize handles work

#### Accessibility
- [ ] All interactive elements have aria-labels
- [ ] Minimum hit target 44x44px
- [ ] Contrast ratio ≥ 4.5:1 for text
- [ ] Screen reader announces page changes
- [ ] Keyboard-only navigation works for all features
- [ ] Focus order is logical

#### Performance
- [ ] Page renders in < 500ms
- [ ] Interactions respond in < 16ms (60fps)
- [ ] No jank during animations
- [ ] Memory usage < 100MB for this page
- [ ] No unnecessary re-renders
- [ ] Virtual scrolling for long lists (if applicable)

#### Build & Test
- [ ] Build passes with 0 errors, 0 warnings
- [ ] No TODO comments in code
- [ ] No FIXME comments in code
- [ ] No console.log / Debug.WriteLine in production
- [ ] No dead code paths
- [ ] No unused imports
```

---

## Page Checklists

### App Shell (Phase 0.1)

**Status**: ⬜ Not Started

#### Visual Completion
- [ ] MainWindow layout matches structure: sidebar + viewport + status bar
- [ ] Sidebar is 56px wide with icon-only navigation
- [ ] All 10 nav buttons present with correct icons
- [ ] Settings button separated at bottom of sidebar
- [ ] Status bar shows: ready indicator, version, backend status, GPU status, time
- [ ] Dark theme applied globally (#0B0B12 canvas)
- [ ] All colors match vision board palette
- [ ] All fonts are Inter at correct sizes

#### Interactions Complete
- [ ] Nav buttons show hover state (background brighten)
- [ ] Nav buttons show active state (accent bar + color)
- [ ] Clicking nav button switches page
- [ ] Sidebar scrolls if content overflows
- [ ] Status bar time updates every minute
- [ ] Search bar expands on Ctrl+K

#### Keyboard Shortcuts
- [ ] Ctrl+K opens search
- [ ] Escape closes search
- [ ] Tab navigates sidebar → content → status bar
- [ ] Arrow keys navigate within sidebar

#### Animations
- [ ] Page transition: opacity + translateY (300ms)
- [ ] Nav button hover: background (150ms)
- [ ] Search expand: width (200ms)
- [ ] Active state change: color (150ms)

#### Backend Connected
- [ ] Backend status indicator checks actual backend health
- [ ] Version reads from assembly metadata

#### Loading State
- [ ] Splash screen or loading indicator on startup
- [ ] Startup completes in < 2 seconds

#### Empty State
- [ ] N/A (shell has no empty state)

#### Error State
- [ ] Backend offline shows error in status bar
- [ ] Startup failure shows error dialog

#### Responsive
- [ ] Window resizes correctly
- [ ] Sidebar remains 56px at all sizes
- [ ] Content fills available space
- [ ] Status bar stays at bottom

#### Accessibility
- [ ] All nav buttons have tooltips
- [ ] All nav buttons have aria-labels
- [ ] Tab order is logical
- [ ] Focus ring visible on keyboard nav

#### Performance
- [ ] Startup < 2 seconds
- [ ] Page switch < 100ms
- [ ] No jank during animations

---

### Home Workspace (Phase 1)

**Status**: ⬜ Not Started
**Vision Spec**: `docs/vision-board/01-home-workspace.md`

#### Visual Completion
- [ ] Welcome section with greeting and status
- [ ] 8 module cards in 4x2 grid
- [ ] Each card has icon, title, description
- [ ] Cards use --radius-xl (16px)
- [ ] Recent projects section with horizontal cards
- [ ] Recent equations section with pill-shaped cards
- [ ] Favorite visualizations section with thumbnails
- [ ] All cards use #12121E background, #2A2A3E border
- [ ] Module icons on colored backgrounds (15% opacity accent)

#### Interactions Complete
- [ ] Module cards navigate to correct pages
- [ ] Module cards show hover: translateY(-2px) + glow
- [ ] Module cards show press: scale(0.98)
- [ ] Recent project cards navigate to projects
- [ ] Search in toolbar searches projects/equations/visualizations
- [ ] No dead buttons

#### Keyboard Shortcuts
- [ ] Tab navigates through cards in grid order
- [ ] Enter activates focused card
- [ ] Ctrl+K opens global search

#### Animations
- [ ] Cards hover: translateY(-2px) + shadow (200ms)
- [ ] Page load: cards fade in sequentially (stagger 50ms)
- [ ] Section headers fade in (200ms)

#### Backend Connected
- [ ] Recent projects read from persistence
- [ ] Recent equations read from history
- [ ] Favorites read from storage

#### Loading State
- [ ] Skeleton cards shown while data loads
- [ ] Skeleton shape matches card layout

#### Empty State
- [ ] "Welcome to MathVerse" with get started CTA
- [ ] Shown when no projects exist

#### Error State
- [ ] Failed to load projects shows error with retry
- [ ] Backend offline shows warning

#### Responsive
- [ ] Grid adjusts: 4 cols → 3 cols → 2 cols
- [ ] Sections stack vertically if needed

#### Accessibility
- [ ] Cards have aria-labels
- [ ] Grid navigation with arrow keys
- [ ] Focus ring on cards

#### Performance
- [ ] Page renders < 300ms
- [ ] Cards stagger animation smooth at 60fps

---

### Evaluate (Phase 2)

**Status**: ⬜ Not Started
**Backend**: `Math.CAS` — Evaluator, Simplifier, Parser, Factorizer, Expander

#### Visual Completion
- [ ] Large expression input with KaTeX live preview
- [ ] Evaluate button with keyboard shortcut hint
- [ ] Result area with rendered mathematical output
- [ ] Step-by-step toggle to show simplification steps
- [ ] History panel with previous evaluations
- [ ] Save/favorite button per expression

#### Interactions Complete
- [ ] Type expression → KaTeX preview updates live
- [ ] Ctrl+Enter or click Evaluate → result appears
- [ ] Click history item → loads into input
- [ ] Star expression → saves to favorites
- [ ] Delete history item → removes from list
- [ ] Copy result → copies to clipboard

#### Keyboard Shortcuts
- [ ] Ctrl+Enter: Evaluate
- [ ] Up/Down: Navigate history
- [ ] Ctrl+C: Copy result
- [ ] Ctrl+Shift+C: Copy expression

#### Animations
- [ ] Result fade in (200ms)
- [ ] Step-by-step expand/collapse (200ms)
- [ ] History item enter: slide down (150ms)

#### Backend Connected
- [ ] `Parser.Parse(input)` called on input
- [ ] `Evaluator.Evaluate(parsed)` called on evaluate
- [ ] `Simplifier.Simplify(result)` called for simplification
- [ ] `Factorizer.Factor(result)` available for factoring
- [ ] `Expander.Expand(result)` available for expansion
- [ ] Parse errors caught and displayed
- [ ] Evaluation errors caught and displayed

#### Loading State
- [ ] Spinner on Evaluate button during computation
- [ ] Result area shows skeleton during evaluation

#### Empty State
- [ ] "Type an expression to begin" with math icon
- [ ] Example expressions as clickable chips

#### Error State
- [ ] Parse error: red message below input with position
- [ ] Evaluation error: inline error with description
- [ ] Timeout error: "Expression took too long" with retry

#### Responsive
- [ ] Input expands to fill width
- [ ] History panel collapses below on narrow windows

#### Accessibility
- [ ] Input has aria-label="Mathematical expression"
- [ ] Result announced to screen reader
- [ ] Error messages associated with input via aria-describedby

#### Performance
- [ ] KaTeX preview updates within 100ms of typing
- [ ] Evaluation completes or shows loading within 500ms

---

### Graph Studio (Phase 3)

**Status**: ⬜ Not Started
**Backend**: `Math.Visualization` — CartesianPlot, SurfacePlot, ScatterPlot, PolarPlot

(Same checklist structure as above, with page-specific items for 2D/3D plotting, function input, viewport controls, multiple function overlay, pan/zoom, axes, legend, export)

---

### Visualization Studio (Phase 4)

**Status**: ⬜ Not Started
**Vision Spec**: `docs/vision-board/02-visualization-studio.md`
**Backend**: `Math.Visualization` — VisualizationScene, Camera, Light, Material, RenderingPipeline

(Same checklist structure, with page-specific items for three-column layout, 3D viewport, library tree, property panels, timeline, export)

---

### Geometry Studio (Phase 5)

**Status**: ⬜ Not Started
**Backend**: `Math.Geometry`, `Math.Geometry.Advanced`

(Same checklist structure, with page-specific items for 2D canvas, geometric primitives, construction tools, measurements, transforms)

---

### Simulation Lab (Phase 6)

**Status**: ⬜ Not Started
**Backend**: `Math.Simulation` — SimulationEngine

(Same checklist structure, with page-specific items for simulation categories, parameter controls, real-time visualization, playback)

---

### AI Assistant (Phase 7)

**Status**: ⬜ Not Started
**Backend**: `Math.AI`

(Same checklist structure, with page-specific items for chat interface, streaming, math rendering, context awareness)

---

### Data Analysis (Phase 8)

**Status**: ⬜ Not Started
**Backend**: `Math.DataScience` — DataFrame, importers, visualizers

(Same checklist structure, with page-specific items for data import, table view, statistics, charts, filtering)

---

### Publications (Phase 9)

**Status**: ⬜ Not Started
**Backend**: Export services

(Same checklist structure, with page-specific items for template selection, rich text editing, equation insertion, export)

---

### Learning (Phase 10)

**Status**: ⬜ Not Started

(Same checklist structure, with page-specific items for course catalog, player, exercises, progress)

---

### Settings (Phase 11)

**Status**: ⬜ Not Started

(Same checklist structure, with page-specific items for all settings categories, persistence, reset)

---

## Tracking

| Page | Total Items | Complete | Status |
|------|-------------|----------|--------|
| App Shell | 34 | 0/34 | ⬜ Not Started |
| Home Workspace | 30 | 0/30 | ⬜ Not Started |
| Evaluate | 33 | 0/33 | ⬜ Not Started |
| Graph Studio | ~35 | 0/~35 | ⬜ Not Started |
| Visualization Studio | ~38 | 0/~38 | ⬜ Not Started |
| Geometry Studio | ~32 | 0/~32 | ⬜ Not Started |
| Simulation Lab | ~30 | 0/~30 | ⬜ Not Started |
| AI Assistant | ~32 | 0/~32 | ⬜ Not Started |
| Data Analysis | ~33 | 0/~33 | ⬜ Not Started |
| Publications | ~28 | 0/~28 | ⬜ Not Started |
| Learning | ~26 | 0/~26 | ⬜ Not Started |
| Settings | ~28 | 0/~28 | ⬜ Not Started |
| **Total** | **~379** | **0/379** | **0%** |
