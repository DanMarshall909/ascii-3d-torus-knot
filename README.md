# ASCII 3D Torus Knot Renderer

<img width="1822" height="1084" alt="image" src="https://github.com/user-attachments/assets/a3f51fdd-de8e-4828-be23-939538e18f6a" />

A mesmerizing 3D torus knot renderer using Spectre.Console for colorful ASCII/pixel art visualization in the terminal.

## Features

🌈 **Dynamic Color Cycling** - Full spectrum rainbow colors that smoothly transition across the surface

🔄 **Morphing Geometry** - Torus knot parameters (P, Q, radius, tube thickness) change dynamically

📹 **Moving Camera** - Complex orbital camera movement with varying distance and angles

🔍 **Dynamic Zoom** - Smooth zoom in/out from 0.5x to 2.0x

💡 **Dynamic Lighting** - Moving light source with changing intensity and position

⚙️ **6 Degrees of Freedom** - Object rotates and translates in all 3 axes

## Screenshots

The application renders beautiful, colorful torus knots that continuously morph and change:

- Rainbow color cycling creates flowing waves of color across the surface
- Mathematical precision meets artistic beauty
- Real-time parameter display shows current knot configuration
- Smooth 25 FPS animation with optimized performance

## Requirements

- .NET 6.0 or later
- Terminal with true color support (most modern terminals)

## Installation & Usage

1. Clone the repository:
```bash
git clone https://github.com/yourusername/ascii-3d-torus-knot.git
cd ascii-3d-torus-knot
```

2. Run the application:
```bash
dotnet run
```

3. Watch the mesmerizing 3D torus knot animation!

4. Press `Ctrl+C` to exit

## Technical Details

### Mathematics
- **Torus Knot Equation**: Parametric surface generation with P and Q parameters
- **3D Transformations**: Full rotation matrices for all axes
- **Perspective Projection**: 3D to 2D conversion with dynamic zoom
- **Z-Buffer**: Proper depth sorting for realistic 3D rendering

### Rendering
- **HSV Color Space**: Smooth color transitions through the full spectrum
- **Surface Normal Mapping**: Colors based on surface orientation
- **Triangle Rasterization**: Point-in-triangle testing for pixel-perfect rendering
- **Performance Optimization**: Reduced polygon count and smart geometry updates

### Dynamic Parameters
- **P Parameter**: 2-5 (affects major loops)
- **Q Parameter**: 1-3 (affects minor loops)  
- **Radius**: 1.2-1.8 units
- **Tube Radius**: 0.2-0.4 units
- **Zoom**: 0.5x-2.0x
- **Color Shift**: Continuous rainbow cycling

## Architecture

```
Program.cs
├── Vector3 - 3D vector math and transformations
├── Triangle - 3D triangle with normal calculation
├── TorusKnot - Parametric torus knot geometry generation
├── PixelRenderer - 3D to 2D rendering with color mapping
└── Main - Animation loop with dynamic parameters
```

## Dependencies

- [Spectre.Console](https://spectreconsole.net/) - For colorful terminal UI and canvas rendering

## License

MIT License - Feel free to use and modify!

## Contributing

Pull requests welcome! Some ideas for enhancements:

- Add more knot types (trefoil, figure-eight, etc.)
- Interactive controls for real-time parameter adjustment
- Export capabilities (GIF, video)
- Additional lighting models
- Texture mapping support
