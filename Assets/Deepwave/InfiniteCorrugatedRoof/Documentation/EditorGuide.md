---
id: editor-guide
title: Editor Guide
sidebar_position: 3
---

# Editor & Inspector Guide

The `InfiniteCorrugatedRoof` component provides a custom, organized inspector divided into logical sections.

## 📏 Panel Layout & Dimensions
Define the overall size and grid structure of your roof.

| Property | Description |
| :--- | :--- |
| **Panel Columns (X)** | Number of panels across the width (X-axis). |
| **Panel Rows (Z)** | Number of panels along the length (Z-axis). |
| **Panel Width (m)** | The width of each individual sheet. |
| **Panel Length (m)** | The length of each individual sheet. |
| **Row Overlap (m)** | Distance vertical panels overlap with the row below. |
| **Layer Gap (m)** | Tiny Z-offset between overlapping panels to prevent Z-fighting. |

## 🌊 Wave Configuration
Customize the look and feel of the corrugations.

- **Wave Profile:** Use the `AnimationCurve` to define the cross-section of a single corrugation. The X-axis (0-1) represents one wave cycle.
- **Waves/Panel:** Total number of full wave cycles per panel width.
- **Amplitude:** The peak height of the corrugations.

## 🕸 Mesh Settings
Control the geometry resolution and physical properties.

- **Segments per Panel:** Vertical resolution along the sheet length. Increase for smoother curves on splines or more detailed noise.
- **Thickness:** Adds actual depth to the sheet. 
  - *Note: Setting this to 0 will result in a single-sided plane for maximum performance.*

## ✨ Modifiers & Variations
Add realism and complexity to your roof.

### 🎨 Panel Transform (Jitter)
- **Position Offset:** Randomized XYZ offset per panel.
- **Rotation Offset:** Randomized rotation per panel (useful for "loose" or old roofs).
- **Scale Offset:** Randomized scaling.

### 🌫 Surface Noise
- **Intensity:** Strength of the procedural displacement.
- **Scale:** Frequency of the noise pattern.
- **Seed:** Unique ID for noise randomization.

### 🧬 Spline Path
- **Spline Container:** Assign a Unity `Spline` to bend the roof along a curve.
- **Deform Mode:** Choose how the roof follows the path (e.g., Simple Bend vs. Full Path Follow).

### ✂️ Cut Settings
- **Enable Cuts:** Toggle global cutting logic.
- **Min/Max (X/Y):** Define normalized boundaries (0-1) within the global grid where the roof will be clipped.

## 🛠 System & Export
- **Random Seed:** Controls all randomized variations.
- **Bake Tools:** 
  - Create new FBX assets directly from procedural geometry.
  - Swap between real-time procedural mesh and static baked asset with one click.

---
*Next: [Technical Reference](./technical-reference.md)*
