---
id: api-reference
title: API Reference
sidebar_position: 5
---

# API Reference

The ICR system is designed to be fully extensible. Use the following classes and methods to integrate the tool into your custom scripts or tools.

## 🚀 Main Entry Point: `InfiniteCorrugatedRoof`

The primary component for managing and generating roofs.

### Public Methods

- `Rebuild()`: Force the tool to recalculate the entire roof and update the meshes for all LODs.
- `GetProceduralMesh(int lodIndex)`: Returns the generated `Mesh` object for the specified LOD index (0-2).
- `ShouldUseBakedMesh()`: Returns `true` if the tool is currently configured to show a baked FBX asset instead of procedural geometry.
- `TryApplyBakedMeshes()`: Attempts to load and apply meshes from the assigned `bakedFbxAsset`.

## 📦 Data Structures

### `BuildSettings`
A snapshot of all parameters required for a single generation pass.
- **Fields:** `PanelWidth`, `PanelLength`, `Rows`, `Columns`, `WaveCountPerPanel`, `WaveAmplitude`, etc.

### `PanelSpec`
Defines the unique attributes of a single panel within the grid.
- **Fields:** `Rotation`, `GapOffset`, `Color`, `TexIndex`, `TexOrientation`.

## 🧬 Core Interface: `IRoofGenerator`
Implement this interface to create your own custom roof generation algorithms.

```csharp
public interface IRoofGenerator
{
    void Generate(Mesh mesh, int lodIndex, BuildContext context);
}
```

### Extending the Generator
1. Create a class that implements `IRoofGenerator`.
2. In `InfiniteCorrugatedRoof.cs`, swap the default `_generator` instance for your custom implementation.

## 🛠 Static Utilities

- **`CutUtility.ApplyCut`**: Calculates global-to-local clipping logic.
- **`NoiseUtility.ApplyNoise`**: Optimized Perlin-based displacement.
- **`SplineUtility.ApplySpline`**: Handles path-deformation and world-space bending.

---
*Next: [Advanced Features](./advanced-features.md)*
