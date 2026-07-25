# MathVerse Interaction Guidelines

## Interaction Principles

Every interaction in MathVerse must feel responsive, predictable, and physically satisfying. These guidelines define how users interact with the interface.

---

## 1. Hover Feedback

Every interactive element must respond to hover within **50ms**.

| Element | Hover Effect |
|---------|-------------|
| Cards | `translateY(-2px)` + shadow elevation + border glow at accent color 30% |
| Buttons | Background brightens (surface → surface elevated) + border accent at 20% |
| Sidebar items | Background `rgba(255,255,255,0.04)`, icon brightens to primary text |
| Links | Color shifts to accent + 1px underline appears from left |
| Tool icons | Background `rgba(255,255,255,0.04)`, tooltip appears after 400ms delay |
| List items | Background `rgba(255,255,255,0.03)` |
| Toggle switches | Thumb scales to 110% |
| Slider tracks | Track height increases from 4px to 6px |
| Dropdown items | Background `rgba(255,255,255,0.04)` |
| Table rows | Background `rgba(255,255,255,0.02)` |

**Tooltip delay**: 400ms for icon-only buttons, 0ms for text buttons, 0ms for cards.

---

## 2. Click Feedback

Every click must provide immediate visual confirmation.

| Element | Click Effect | Duration |
|---------|-------------|----------|
| Cards | `scale(0.98)` | 100ms |
| Buttons | `scale(0.96)` | 80ms |
| Sidebar items | Brief background flash `rgba(74,158,255,0.1)` | 150ms |
| Toggle switches | Spring animation to new position | 200ms |
| Checkboxes | Checkmark draws in with scale animation | 150ms |
| Radio buttons | Inner dot scales from 0 to 1 with spring | 200ms |
| Sliders | Thumb pulses briefly | 100ms |
| Icons (action) | Brief rotate or scale pulse | 150ms |

**Ripple effect**: Not used. MathVerse does not use Material Design ripple.

---

## 3. Selection Behavior

### Single Selection
- Click to select
- Click elsewhere to deselect
- Only one item selected at a time (unless multi-select is explicitly supported)
- Selected item: accent border (2px), background tint at 6%

### Multi-Selection
- Ctrl+Click to toggle individual items
- Shift+Click to select range
- Selected items: accent border (1px), background tint at 4%
- Selection count shown in toolbar: "3 items selected"

### Selection in Viewports
- Click on object to select
- Click on empty space to deselect all
- Selected object: outline glow at accent color
- Gizmo appears on selected 3D objects
- Rubber band selection for multiple objects (drag on empty space)

---

## 4. Animation Duration

| Category | Duration | Easing |
|----------|----------|--------|
| Micro-interactions (toggle, checkbox) | 100ms | `--ease-default` |
| Button press | 80ms | `ease-in` |
| Hover transforms | 200ms | `--ease-default` |
| Panel open/close | 200ms | `--ease-default` |
| Page transition | 300ms | `--ease-default` |
| Modal open | 200ms | `--ease-spring` |
| Modal close | 150ms | `--ease-out` |
| Notification enter | 200ms | `--ease-spring` |
| Notification exit | 150ms | `--ease-out` |
| Complex animation (3D scene) | 500ms | `--ease-in-out` |
| Skeleton pulse | 1500ms loop | `ease-in-out` |

**Never use instant transitions.** The minimum transition duration is 80ms.

**Never use transitions > 500ms** for UI elements. Long animations are reserved for mathematical visualizations only.

---

## 5. Transition Timing

### Page Transitions
```
Current page: opacity 1→0, translateY 0→-8px (150ms)
[blank gap: 50ms]
New page: opacity 0→1, translateY 8px→0 (200ms)
```
Total perceived transition: ~350ms

### Panel Transitions
```
Close: width/opacity animates to 0 (200ms)
Open: width/opacity animates from 0 (200ms)
Content fades in 50ms after panel opens
```

### Modal Transitions
```
Backdrop: opacity 0→1 (200ms)
Content: scale 0.95→1 + opacity 0→1 (200ms, spring easing)
```

---

## 6. Keyboard Navigation

### Global Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+K` | Open search |
| `Ctrl+Z` | Undo |
| `Ctrl+Shift+Z` / `Ctrl+Y` | Redo |
| `Ctrl+S` | Save current project |
| `Ctrl+N` | New project |
| `Ctrl+O` | Open project |
| `Ctrl+E` | Export |
| `Ctrl+/` | Toggle command palette |
| `Escape` | Close modal / deselect / cancel |
| `Tab` | Move to next interactive element |
| `Shift+Tab` | Move to previous interactive element |
| `Enter` / `Space` | Activate focused element |
| `Arrow keys` | Navigate within lists, grids, or radio groups |

### Page-Specific Shortcuts

| Page | Shortcut | Action |
|------|----------|--------|
| Graph Studio | `Ctrl+Enter` | Evaluate & plot |
| Visualization | `G` | Toggle grid |
| Visualization | `H` | Toggle HUD |
| Visualization | `R` | Reset camera |
| Visualization | `F` | Frame selected |
| Visualization | `1/3/7` | Front/Right/Top view |
| Math Lab | `Space` | Play/Pause |
| Math Lab | `→` | Step forward |
| `←` | Step backward |

### Focus Management

- **Tab order**: Sidebar → Main content (top to bottom, left to right) → Status bar
- **Focus ring**: 2px solid `#4A9EFF` with 2px offset
- **Focus-visible only**: Focus ring appears only on keyboard navigation, hidden on mouse click
- **Focus trap**: Modals trap focus inside until closed
- **Initial focus**: First interactive element in new page or modal

---

## 7. Mouse Wheel Behavior

| Context | Wheel Action |
|---------|-------------|
| Scrollable panels | Vertical scroll |
| 3D viewport | Zoom (cursor position as anchor) |
| Slider controls | Increment/decrement value |
| Numeric inputs | Increment/decrement by 1 (or 0.1 with Shift) |
| Horizontal scroll containers | Horizontal scroll (with Shift held) |
| Over canvas with Ctrl | Zoom in/out |

**Scroll acceleration**: Standard OS scroll acceleration. No custom acceleration.

**Scroll snap**: Not used in main content areas. Used in horizontal carousels (snap to item).

---

## 8. Context Menus

### Rules
- Right-click on any interactive element opens a context menu
- Context menu appears at cursor position
- Context menu closes on: click outside, Escape, or selection
- Maximum 8 items per context menu
- Menus are grouped with dividers between logical groups
- Destructive actions (Delete, Remove) are at the bottom, colored red

### Standard Context Menu Items

| Element | Context Menu |
|---------|-------------|
| Card/project | Open, Rename, Duplicate, Delete |
| Graph object | Hide, Isolate, Properties, Delete |
| Visualization object | Hide, Isolate, Focus, Properties, Export, Delete |
| Canvas background | Reset view, Grid on/off, Axes on/off |
| Text field | Cut, Copy, Paste, Select All |
| Table row | Edit, Duplicate, Delete |

### Visual Style
```css
background: rgba(14, 14, 24, 0.95);
backdrop-filter: blur(20px);
border: 1px solid rgba(255, 255, 255, 0.08);
border-radius: 8px;
box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);
padding: 4px;
```

Menu items: 32px height, 16px icon + 13px text, 12px padding left.

---

## 9. Drag and Drop

### Supported Drag Operations

| Source | Target | Action |
|--------|--------|--------|
| Project card | Sidebar | Add to favorites |
| Visualization object | Visibility panel | Reorder layers |
| Timeline keyframe | Timeline | Reposition |
| File from OS | Import area | Import data |
| Panel tab | Panel area | Reposition panel |

### Visual Feedback

| State | Visual |
|-------|--------|
| Drag start | Ghost element: 80% opacity, slight scale (1.02), elevated shadow |
| Valid drop target | Target highlights with dashed accent border, slight glow |
| Invalid drop target | No change (cursor shows not-allowed) |
| Drop success | Brief flash animation on target (150ms) |
| Drop failure | Ghost snaps back to origin (200ms spring) |

### Drag Threshold
- Minimum 5px movement before drag initiates
- Prevents accidental drags on click

---

## 10. Undo/Redo

### Stack Behavior
- Maximum undo depth: 100 operations
- Undo stack clears on project close
- Redo stack clears on new action after undo
- Each undoable action has a human-readable label

### Undoable Actions
- Expression evaluation
- Visualization parameter changes
- Object creation/deletion
- Property modifications
- Camera position changes (viewport)
- Timeline edits
- Import operations

### Non-Undoable Actions
- File system operations (save, export)
- Settings changes (persisted immediately)
- Navigation between pages

### UI
- `Ctrl+Z`: Undo (shows toast: "Undid: [action name]")
- `Ctrl+Shift+Z`: Redo (shows toast: "Redid: [action name]")
- Undo/Redo buttons in toolbar (when applicable)

---

## 11. Form Controls

### Text Input
- Height: 36px
- Background: `#12121E`
- Border: `1px solid #2A2A3E`
- Border radius: 6px
- Focus: `1px solid #4A9EFF`
- Placeholder: `#4A4A64`
- Text: `#E8E8F0`, Inter 14px
- Padding: 0 12px

### Dropdown / Select
- Same styling as text input
- Dropdown panel: Same as context menu styling
- Selected item: Checkmark icon + accent text
- Keyboard: Arrow up/down to navigate, Enter to select, Escape to close

### Checkbox
- Outer: 16x16px, rounded 4px, `#2A2A3E` border
- Checked: `#4A9EFF` fill, white checkmark
- Hover: Border brightens to `#4A4A64`
- Label: Inter 14px, 8px gap from checkbox

### Radio Button
- Outer: 16x16px, circle, `#2A2A3E` border
- Selected: `#4A9EFF` fill, 8px inner white circle
- Hover: Border brightens

### Toggle Switch
- Track: 40x20px, rounded 10px
- Off: `#2A2A3E` track, `#7A7A96` thumb
- On: `#4A9EFF` track, white thumb
- Thumb: 16x16px circle
- Animation: 200ms spring

### Slider
- Track: 4px height, rounded 2px, `#1A1A2E`
- Fill: `#4A9EFF` from left to thumb
- Thumb: 12px circle, `#4A9EFF`, white center dot
- Hover: Thumb scales to 14px
- Labels: Min/max values at track ends, Inter 11px, `#4A4A64`

### Numeric Input
- Same as text input but right-aligned
- Monospace font for values
- Up/down arrow buttons on hover (right side)
- Mouse wheel increment/decrement
- Shift + wheel for fine control (0.1x)

---

## 12. Notifications / Toasts

### Types

| Type | Color | Icon | Duration |
|------|-------|------|----------|
| Info | `#4A9EFF` | Info circle | 3s |
| Success | `#06D6A0` | Check circle | 3s |
| Warning | `#FF6B35` | Alert triangle | 5s |
| Error | `#FF4444` | X circle | Manual dismiss |

### Position
- Top-right corner
- 12px from edges
- Stack vertically (newest on top)
- Maximum 3 visible at once

### Visual Style
```css
background: rgba(14, 14, 24, 0.95);
backdrop-filter: blur(16px);
border: 1px solid rgba(255, 255, 255, 0.08);
border-radius: 8px;
border-left: 3px solid [type-color];
padding: 12px 16px;
min-width: 300px;
max-width: 420px;
```

### Animation
```
Enter: translateX(100%) → translateX(0) (200ms, spring)
Exit: translateX(0) → translateX(100%) (150ms, ease-out)
Stack shift: translateY(0) → translateY(-52px) (200ms)
```

---

## 13. Modals

### Structure
```
┌──────────────────────────────────────┐
│  Modal Title                    [X]  │
│  ─────────────────────────────────── │
│                                      │
│  Content area                        │
│                                      │
│  ─────────────────────────────────── │
│  [Cancel]              [Confirm]     │
└──────────────────────────────────────┘
```

### Rules
- Maximum width: 560px
- Backdrop: `rgba(0,0,0,0.5)`, click to close
- Focus trapped inside modal
- Escape to close
- Enter to confirm (if confirm button is focused)
- Destructive confirm button: Red background
- Non-destructive confirm button: Blue (`#4A9EFF`) background
- Cancel button: Ghost (transparent bg, `#7A7A96` text)

---

## 14. Scroll Behavior

### Scrollbar Styling
- Width: 6px (hover: 8px)
- Track: Transparent
- Thumb: `#2A2A3E`, rounded 3px
- Thumb hover: `#4A4A64`
- Always visible on scrollable panels (no overlay scrollbars)

### Scroll Physics
- Momentum scrolling on all platforms
- Scroll wheel: 3 lines per tick (OS default)
- Smooth scroll for programmatic navigation

---

## 15. Resize Behavior

### Panel Resizing
- Drag handle: 4px wide, invisible by default
- Hover: 4px wide, `#4A9EFF` at 30% opacity
- Cursor: `col-resize` (vertical panels) or `row-resize` (horizontal panels)
- Minimum panel size enforced (cannot collapse below minimum)
- Double-click handle: Reset to default size

### Window Resize
- Content reflows to fit new window size
- Panels respect minimum widths
- Viewport fills available space
- Status bar remains at bottom

---

## 16. Search Behavior

### Global Search (Ctrl+K)
- Expands search bar to 360px
- Focus moves to search input
- Results appear below as dropdown
- Results grouped by: Projects, Equations, Functions, Visualizations
- Arrow keys to navigate results
- Enter to open selected result
- Escape to close search

### In-Page Search
- Dedicated search input within the page
- Results highlighted in-place
- Result count shown: "3 of 12"
- Enter/Shift+Enter to navigate between results

---

## 17. Export Workflow

### Standard Export Flow
1. Click Export button (toolbar)
2. Dropdown appears with format options
3. Select format
4. Configure options (if applicable) in a small modal
5. Click "Export" in modal
6. Progress indicator appears
7. Success toast with "Open" action button
8. File saved to default location (or last used location)

### Export Formats by Page

| Page | Formats |
|------|---------|
| Visualization | Blender, OBJ, glTF, STL, PNG, JPEG, SVG, Video, GIF, Screenshot |
| Graph | PNG, SVG, JSON, PDF |
| Math Lab | PNG, SVG, Video, GIF, LaTeX |
| Geometry | OBJ, STL, glTF, SVG |
| Data Analysis | CSV, JSON, Excel, HTML |
| Publications | PDF, LaTeX, HTML |

---

## 18. Loading Patterns

### Skeleton Screens
Used for: Page content, card lists, data tables
- Gray rectangles (`#1A1A2E`) pulsing between `#1A1A2E` and `#2A2A3E`
- Pulse duration: 1500ms
- Shape matches expected content (rectangle for text, circle for avatars)

### Spinners
Used for: Buttons, small operations, inline loading
- 20x20px, 2px stroke, `#4A9EFF`
- Rotation: 600ms linear infinite

### Progress Bars
Used for: Operations > 2 seconds with known duration
- Height: 4px
- Track: `#1A1A2E`
- Fill: `#4A9EFF`
- Indeterminate: Gradient animation left-to-right

### Content Placeholder
Used for: Viewport loading, complex content
- Centered spinner with text: "Loading [content name]..."
- Opacity: 0.7
- Background matches page background
