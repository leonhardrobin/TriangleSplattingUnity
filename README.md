# UnityTriangleSplatting

## Introduction

**UnityTriangleSplatting** is a Unity package for visualizing triangle splats from COFF files, inspired by the Triangle Splatting project. It supports chunked loading, frustum culling, and optional mesh collider generation for interactive exploration of large triangle datasets.

## Features

- ✅ Load and render `.off` (COFF) triangle files
- ✅ Chunk-based loading for performance
- ✅ Frustum culling for real-time optimization
- ✅ Optional mesh collider generation
- ✅ Vertex color support for per-vertex shading

## Example

!Triangle Splatting Example

> 💡 You can drag and drop your own `.off` files into the scene and assign a material with vertex color support.

## Installation

1. Clone this repository:
   ```bash
   git clone https://github.com/YourUsername/UnityTriangleSplatting.git
   ```
   Open your Unity project.
Drag the TriangleSplatRenderer.cs script and the Shaders folder into your project.
Assign a material using the included VertexColor.shader to your triangle splats.
Usage
Add the TriangleSplatRenderer component to an empty GameObject.
Set the path to your .off file.
Assign a material that supports vertex colors.
Press Play to visualize the triangle splats in the scene.
Requirements
Unity 2022.3+ (tested with Unity 6 beta)
A COFF file with triangle and color data
To-Do
 Runtime .off file loader via UI
 Support for animated splats
 Support for HDRP/URP out of the box
License
This project is licensed under the MIT License.

Contributing
Contributions are welcome! Feel free to:

Open issues for bugs or suggestions
Submit pull requests with improvements
Share your use cases and feedback
