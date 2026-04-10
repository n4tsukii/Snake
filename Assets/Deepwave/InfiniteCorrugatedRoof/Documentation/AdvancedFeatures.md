---
id: advanced-features
title: Advanced Features
sidebar_position: 6
---

# Advanced Features

This section provides in-depth information about the advanced tools and workflows available in ICR.

## 🧬 Spline Path Deformation

The Spline modifier allows you to bend the entire roofing system along a complex path.

### Key Modes:
- **X-axis Follow:** The roof is deformed based on the X-coordinate of its vertices relative to the spline length.
- **Y-axis Follow:** The roof follows the path along its length.

:::tip Path Smoothness
To achieve smooth results on curved paths, ensure that you increase the **Segments per Panel** in the Mesh Settings.
:::

## 🌫 Surface Noise & Randomization

Add organic imperfections and realism with the procedural noise engine.

- **Vertex Displacement:** Noise is applied in local space before spline deformation, ensuring it "follows" the geometry correctly.
- **Panel Jitter:** Small, randomized offsets in position and rotation prevent the "perfect" look of tiled assets.
- **Random Seed:** Any change to the seed will completely refresh all randomized elements while maintaining the overall layout.

## ✂️ Global Cutting System

Define precise clipping regions across your entire roof grid.

- **Non-destructive:** Cuts are calculated during the vertex generation pass. You can change cut boundaries without losing data.
- **Coordinate System:** Boundaries (Min X, Max X, Min Y, Max Y) are expressed in normalized global space (0 to 1). 
  - *Example: Setting Max X to 0.5 will cut the entire roof exactly in half along its width.*

## 🏗 FBX Export & Optimization Workflow

For high-performance production, you should convert your procedural roofs into static FBX assets.

### Workflow:
1. **Design:** Adjust all parameters until the roof looks perfect in the scene.
2. **Create Asset:** In the **System & Export** section, click **New** to create a target FBX file in your project.
3. **Bake:** Click **Bake Geometry to Asset**. This will generate 3 separate meshes (LOD0, LOD1, LOD2) and save them inside the FBX.
4. **Switch to Preview:** Enable **Preview Baked Mesh**. The `InfiniteCorrugatedRoof` component will stop generating procedural geometry and instead display the meshes from the FBX.
5. **Prefabbing:** You can now safely drag your GameObject into a Prefab or move the FBX asset to other scenes.

:::info Baked Mesh Format
The exported FBX includes vertex positions, normals, tangents, and a single set of UVs. It is optimized for standard Unity materials.
:::
