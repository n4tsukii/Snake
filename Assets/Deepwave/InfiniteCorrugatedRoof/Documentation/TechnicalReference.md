---
id: technical-reference
title: Technical Reference
sidebar_position: 4
---

# Technical Architecture

ICR is built using a modular, service-oriented architecture to ensure maintainability and high performance.

## 🧱 Core Modules

- **Core (`Deepwave.ICR.Core`):** Defines the `IRoofGenerator` interface, decoupling the high-level system from the generation logic.
- **Data (`Deepwave.ICR.Data`):** Contains immutable and serializable settings objects (e.g., `BuildSettings`, `BuildContext`).
- **Generation (`Deepwave.ICR.Generation`):** The engine of the tool.
  - `LayoutPlanner`: Calculates the position and attributes of every panel in the grid.
  - `WaveBuilder`: Pre-calculates the shared cross-section (wave profile) for all panels.
  - `PanelBuilder`: Constructs the geometry for a single panel.
  - `MeshBuilder`: Orchestrates the entire process, including LOD management and buffer allocation.
- **Utilities (`Deepwave.ICR.Utilities`):** Optimized helpers for Splines, Noise, and Cutting.

## 🔄 Generation Lifecycle

When `Rebuild()` is called, the system follows these steps:

1. **Initialization:** Set up internal references and renderer components.
2. **Sanitization:** Validate all user parameters to prevent invalid geometry.
3. **Spline Cache Update:** If a spline is assigned, pre-calculate its path for fast lookup.
4. **Layout Planning:** Iterate through rows and columns to determine the `PanelSpec` for each panel (position, random jitter, texture offsets).
5. **Wave Pre-calculation:** Sample the `AnimationCurve` to create a list of `SectionPoints` shared across all panels.
6. **Geometry Pass (x3 LODs):**
   - For each panel:
     - Apply **Global Cuts** (clipping geometry).
     - Apply **Surface Noise** (vertex displacement).
     - Apply **Spline Deformation** (world-space bending).
     - Compute UVs and Vertex Colors (painting and masking).
7. **Buffer Application:** Update the Unity `Mesh` buffers and refresh the `MeshRenderer`.

:::note Memory Optimization
ICR uses pre-allocated buffers and avoids the use of `Linq` or heavy `new` allocations during the main generation loops to minimize GC pressure.
:::

---
*Next: [API Reference](./api-reference.md)*
