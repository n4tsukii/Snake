---
id: getting-started
title: Getting Started
sidebar_position: 2
---

# Getting Started with ICR

This guide covers the initial setup and basic steps to generate your first procedural roof.

## ⚙️ Installation

ICR is designed for Unity **2021.3 LTS** and above. It depends on several core Unity packages for full functionality.

### 1. Prerequisite Packages
For full feature support, ensure the following are installed via the Unity Package Manager:
- **Splines:** (Highly recommended) For path-deform functionality.
- **Core RP:** For advanced rendering features.

### 2. Importing the Project
Copy the `Assets/Deepwave/InfiniteCorrugatedRoof` folder into your Unity project's `Assets` directory.

## 🏗 Creating Your First Roof

1. **Add to Scene:** Right-click in the Hierarchy or go to **GameObject > 3D Object > Infinite Corrugated Roof**. Alternatively, add the `InfiniteCorrugatedRoof` component to an empty GameObject.
2. **Initial Setup:** A default 1x1 panel roof will be generated immediately.
3. **Adjust Dimensions:**
   - Use **Panel Columns (X)** and **Panel Rows (Z)** to define the overall grid size.
   - Adjust **Panel Width** and **Panel Length** to match your architectural requirements.

:::tip Performance Tip
The tool generates 3 levels of LOD by default. For complex scenes, use the **System & Export** section to bake the geometry to an FBX asset once you are satisfied with the design.
:::

## 🔌 UPM Distribution (Optional)
If you prefer a cleaner project structure, you can add ICR as a local UPM package by pointing the Package Manager to the `package.json` file in the root of the ICR directory.

---
*Next: [Editor Guide](./editor-guide.md)*
