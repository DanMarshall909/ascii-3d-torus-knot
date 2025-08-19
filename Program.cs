using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Spectre.Console;

namespace Ascii3D;

public class Vector3
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public Vector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3 RotateX(double angle)
    {
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);
        return new Vector3(
            X,
            Y * cos - Z * sin,
            Y * sin + Z * cos
        );
    }

    public Vector3 RotateY(double angle)
    {
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);
        return new Vector3(
            X * cos + Z * sin,
            Y,
            -X * sin + Z * cos
        );
    }

    public Vector3 RotateZ(double angle)
    {
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);
        return new Vector3(
            X * cos - Y * sin,
            X * sin + Y * cos,
            Z
        );
    }

    public Vector3 Translate(Vector3 offset)
    {
        return new Vector3(X + offset.X, Y + offset.Y, Z + offset.Z);
    }

    public Vector3 Project(double distance = 12)
    {
        double factor = distance / (distance + Z);
        return new Vector3(X * factor, Y * factor, Z);
    }

    public Vector3 ProjectWithZoom(double distance, double zoom)
    {
        double factor = (distance * zoom) / (distance + Z);
        return new Vector3(X * factor, Y * factor, Z);
    }

    public Vector3 Subtract(Vector3 other)
    {
        return new Vector3(X - other.X, Y - other.Y, Z - other.Z);
    }

    public Vector3 Normalize()
    {
        double length = Math.Sqrt(X * X + Y * Y + Z * Z);
        if (length == 0) return new Vector3(0, 0, 0);
        return new Vector3(X / length, Y / length, Z / length);
    }

    public static Vector3 Cross(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    public static double Dot(Vector3 a, Vector3 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
}

public class Triangle
{
    public Vector3[] Vertices { get; set; }
    public Vector3 Normal { get; set; } = new Vector3(0, 0, 0);

    public Triangle(Vector3[] vertices)
    {
        Vertices = vertices;
        CalculateNormal();
    }

    private void CalculateNormal()
    {
        var v1 = new Vector3(
            Vertices[1].X - Vertices[0].X,
            Vertices[1].Y - Vertices[0].Y,
            Vertices[1].Z - Vertices[0].Z
        );
        var v2 = new Vector3(
            Vertices[2].X - Vertices[0].X,
            Vertices[2].Y - Vertices[0].Y,
            Vertices[2].Z - Vertices[0].Z
        );
        
        Normal = Vector3.Cross(v1, v2).Normalize();
    }

    public double GetAverageZ()
    {
        return Vertices.Average(v => v.Z);
    }
}

public class TorusKnot
{
    private List<Triangle> triangles = new List<Triangle>();
    private int p, q;
    private double radius, tubeRadius;

    public TorusKnot(int p = 3, int q = 2, double radius = 2.0, double tubeRadius = 0.4)
    {
        this.p = p;
        this.q = q;
        this.radius = radius;
        this.tubeRadius = tubeRadius;
        GenerateKnot();
    }

    public void UpdateParameters(int newP, int newQ, double newRadius, double newTubeRadius)
    {
        if (p != newP || q != newQ || Math.Abs(radius - newRadius) > 0.05 || Math.Abs(tubeRadius - newTubeRadius) > 0.02)
        {
            p = newP;
            q = newQ;
            radius = newRadius;
            tubeRadius = newTubeRadius;
            GenerateKnot();
        }
    }

    private void GenerateKnot()
    {
        triangles.Clear(); // Clear previous triangles
        int uSteps = 60; // Reduced from 80 for better performance
        int vSteps = 12;  // Reduced from 16 for better performance
        var vertices = new Vector3[uSteps, vSteps];

        for (int i = 0; i < uSteps; i++)
        {
            double u = 2.0 * Math.PI * i / uSteps;
            
            double knotX = (radius + Math.Cos(q * u)) * Math.Cos(p * u);
            double knotY = (radius + Math.Cos(q * u)) * Math.Sin(p * u);
            double knotZ = Math.Sin(q * u);

            var tangent = new Vector3(
                -(radius + Math.Cos(q * u)) * p * Math.Sin(p * u) - q * Math.Sin(q * u) * Math.Cos(p * u),
                (radius + Math.Cos(q * u)) * p * Math.Cos(p * u) - q * Math.Sin(q * u) * Math.Sin(p * u),
                q * Math.Cos(q * u)
            ).Normalize();

            var binormal = new Vector3(
                -Math.Cos(p * u),
                -Math.Sin(p * u),
                0
            );

            var normal = Vector3.Cross(tangent, binormal).Normalize();
            binormal = Vector3.Cross(normal, tangent).Normalize();

            for (int j = 0; j < vSteps; j++)
            {
                double v = 2.0 * Math.PI * j / vSteps;
                double cosV = Math.Cos(v);
                double sinV = Math.Sin(v);

                vertices[i, j] = new Vector3(
                    knotX + tubeRadius * (cosV * normal.X + sinV * binormal.X),
                    knotY + tubeRadius * (cosV * normal.Y + sinV * binormal.Y),
                    knotZ + tubeRadius * (cosV * normal.Z + sinV * binormal.Z)
                );
            }
        }

        for (int i = 0; i < uSteps; i++)
        {
            int nextI = (i + 1) % uSteps;
            for (int j = 0; j < vSteps; j++)
            {
                int nextJ = (j + 1) % vSteps;

                triangles.Add(new Triangle(new[] {
                    vertices[i, j],
                    vertices[nextI, j],
                    vertices[i, nextJ]
                }));

                triangles.Add(new Triangle(new[] {
                    vertices[nextI, j],
                    vertices[nextI, nextJ],
                    vertices[i, nextJ]
                }));
            }
        }
    }

    public List<Triangle> GetTransformedTriangles(double angleX, double angleY, double angleZ, Vector3 position)
    {
        var transformedTriangles = new List<Triangle>();

        foreach (var triangle in triangles)
        {
            var transformedVertices = triangle.Vertices
                .Select(v => v.RotateX(angleX))
                .Select(v => v.RotateY(angleY))
                .Select(v => v.RotateZ(angleZ))
                .Select(v => v.Translate(position))
                .ToArray();
            
            transformedTriangles.Add(new Triangle(transformedVertices));
        }

        return transformedTriangles.OrderBy(t => -t.GetAverageZ()).ToList();
    }

    public List<Triangle> GetTransformedTrianglesWithCamera(double angleX, double angleY, double angleZ, Vector3 position, Vector3 cameraPos)
    {
        var transformedTriangles = new List<Triangle>();

        foreach (var triangle in triangles)
        {
            var transformedVertices = triangle.Vertices
                .Select(v => v.RotateX(angleX))
                .Select(v => v.RotateY(angleY))
                .Select(v => v.RotateZ(angleZ))
                .Select(v => v.Translate(position))
                .Select(v => v.Subtract(cameraPos))
                .ToArray();
            
            transformedTriangles.Add(new Triangle(transformedVertices));
        }

        return transformedTriangles.OrderBy(t => -t.GetAverageZ()).ToList();
    }
}

public class PixelRenderer
{
    private readonly int width;
    private readonly int height;
    private double[,] zBuffer;
    private Color[,] colorBuffer;

    public PixelRenderer(int width = 120, int height = 60)
    {
        this.width = width;
        this.height = height;
        zBuffer = new double[height, width];
        colorBuffer = new Color[height, width];
    }

    public void Clear()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorBuffer[y, x] = Color.Black;
                zBuffer[y, x] = double.MaxValue;
            }
        }
    }

    public void RenderTriangle(Triangle triangle, Vector3 lightDirection, double zoom = 1.0, double colorShift = 0.0)
    {
        var projectedVertices = triangle.Vertices
            .Select(v => v.ProjectWithZoom(12, zoom))
            .ToArray();

        double brightness = Math.Max(0.1, Vector3.Dot(triangle.Normal, lightDirection));
        
        byte colorValue = (byte)(brightness * 255);
        
        // Skip very dark triangles to avoid black artifacts
        if (colorValue < 20) return;
        
        Color triangleColor;
        var baseHue = (Math.Atan2(triangle.Normal.Y, triangle.Normal.X) + Math.PI) / (2 * Math.PI);
        var hue = (baseHue + colorShift) % 1.0;
        
        // Convert HSV to RGB with dynamic color cycling
        double r, g, b;
        if (hue < 0.166) // Red to Yellow
        {
            r = 1.0;
            g = hue * 6.0;
            b = 0.0;
        }
        else if (hue < 0.333) // Yellow to Green
        {
            r = 1.0 - (hue - 0.166) * 6.0;
            g = 1.0;
            b = 0.0;
        }
        else if (hue < 0.5) // Green to Cyan
        {
            r = 0.0;
            g = 1.0;
            b = (hue - 0.333) * 6.0;
        }
        else if (hue < 0.666) // Cyan to Blue
        {
            r = 0.0;
            g = 1.0 - (hue - 0.5) * 6.0;
            b = 1.0;
        }
        else if (hue < 0.833) // Blue to Magenta
        {
            r = (hue - 0.666) * 6.0;
            g = 0.0;
            b = 1.0;
        }
        else // Magenta to Red
        {
            r = 1.0;
            g = 0.0;
            b = 1.0 - (hue - 0.833) * 6.0;
        }
        
        triangleColor = new Color(
            (byte)(colorValue * r),
            (byte)(colorValue * g),
            (byte)(colorValue * b)
        );

        var minX = projectedVertices.Min(v => v.X);
        var maxX = projectedVertices.Max(v => v.X);
        var minY = projectedVertices.Min(v => v.Y);
        var maxY = projectedVertices.Max(v => v.Y);

        int screenMinX = Math.Max(0, (int)((minX + 6) * width / 12));
        int screenMaxX = Math.Min(width - 1, (int)((maxX + 6) * width / 12));
        int screenMinY = Math.Max(0, (int)((minY + 3) * height / 8));
        int screenMaxY = Math.Min(height - 1, (int)((maxY + 3) * height / 8));

        for (int y = screenMinY; y <= screenMaxY; y++)
        {
            for (int x = screenMinX; x <= screenMaxX; x++)
            {
                double worldX = (x * 12.0 / width) - 6;
                double worldY = (y * 8.0 / height) - 3;

                if (IsPointInTriangle(worldX, worldY, projectedVertices))
                {
                    double z = triangle.GetAverageZ();
                    if (z < zBuffer[y, x])
                    {
                        colorBuffer[y, x] = triangleColor;
                        zBuffer[y, x] = z;
                    }
                }
            }
        }
    }

    private bool IsPointInTriangle(double x, double y, Vector3[] vertices)
    {
        var v0 = vertices[2];
        var v1 = vertices[0];
        var v2 = vertices[1];

        double denom = (v1.Y - v2.Y) * (v0.X - v2.X) + (v2.X - v1.X) * (v0.Y - v2.Y);
        if (Math.Abs(denom) < 1e-10) return false;

        double a = ((v1.Y - v2.Y) * (x - v2.X) + (v2.X - v1.X) * (y - v2.Y)) / denom;
        double b = ((v2.Y - v0.Y) * (x - v2.X) + (v0.X - v2.X) * (y - v2.Y)) / denom;
        double c = 1 - a - b;

        return a >= 0 && b >= 0 && c >= 0;
    }

    public Canvas GetCanvas()
    {
        var canvas = new Canvas(width, height);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                canvas.SetPixel(x, y, colorBuffer[y, x]);
            }
        }
        
        return canvas;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var torusKnot = new TorusKnot(3, 2, 1.5, 0.3);
        var renderer = new PixelRenderer(120, 60);
        
        double angleX = 0;
        double angleY = 0;
        double angleZ = 0;
        double lightAngle = 0;
        
        var cameraPos = new Vector3(0, 0, 0);
        bool autoRotate = true;
        bool autoDynamics = true;

        AnsiConsole.Clear();
        AnsiConsole.Cursor.Hide();

        var initialLayout = new Layout("Root");

        AnsiConsole.Live(initialLayout)
            .Start(ctx =>
            {
                int frameCount = 0;
                
                while (frameCount < 6000)
                {
                    // Dynamic parameters
                    double time = frameCount * 0.01;
                    
                    // Dynamic zoom (0.5x to 2.0x)
                    double zoom = 1.0 + 0.5 * Math.Sin(time * 0.3);
                    
                    // Dynamic knot parameters (slower changes to reduce regeneration)
                    int dynamicP = 3 + (int)(Math.Sin(time * 0.05) * 1.5); // 2-5, slower change
                    int dynamicQ = 2 + (int)(Math.Cos(time * 0.03) * 1); // 1-3, slower change
                    double dynamicRadius = 1.5 + 0.3 * Math.Sin(time * 0.08);
                    double dynamicTubeRadius = 0.3 + 0.1 * Math.Cos(time * 0.06);
                    
                    // Only update geometry every few frames to improve performance
                    if (frameCount % 10 == 0)
                    {
                        torusKnot.UpdateParameters(dynamicP, dynamicQ, dynamicRadius, dynamicTubeRadius);
                    }
                    
                    // Dynamic camera movement
                    double cameraTime = frameCount * 0.005;
                    cameraPos.X = Math.Cos(cameraTime) * (2 + Math.Sin(time * 0.15));
                    cameraPos.Y = Math.Sin(cameraTime * 0.7) * (1 + 0.5 * Math.Cos(time * 0.12));
                    cameraPos.Z = Math.Sin(cameraTime * 0.3) * (1 + 0.3 * Math.Sin(time * 0.18));
                    
                    renderer.Clear();
                    
                    // Dynamic lighting
                    double lightRadius = 3 + Math.Sin(time * 0.4) * 1.5;
                    double lightSpeed = 0.04 + 0.02 * Math.Sin(time * 0.08);
                    lightAngle += lightSpeed;
                    
                    double lightX = Math.Cos(lightAngle) * lightRadius;
                    double lightY = Math.Sin(lightAngle * 0.7) * (2 + Math.Cos(time * 0.3));
                    double lightZ = Math.Sin(lightAngle * 1.3) * 2 - (5 + Math.Sin(time * 0.2));
                    
                    var lightDirection = new Vector3(lightX, lightY, lightZ).Normalize();
                    
                    var position = new Vector3(
                        Math.Sin(frameCount * 0.02) * 0.5,
                        Math.Cos(frameCount * 0.015) * 0.3,
                        Math.Sin(frameCount * 0.01) * 0.2
                    );
                    
                    var triangles = torusKnot.GetTransformedTrianglesWithCamera(angleX, angleY, angleZ, position, cameraPos);
                    
                    // Dynamic color cycling (reduced frequency)
                    double colorShift = (time * 0.2) % 1.0;
                    
                    foreach (var triangle in triangles)
                    {
                        if (triangle.Normal.Z <= 0)
                        {
                            renderer.RenderTriangle(triangle, lightDirection, zoom, colorShift);
                        }
                    }
                    
                    var canvas = renderer.GetCanvas();
                    
                    var layout = new Layout("Root")
                        .SplitRows(
                            new Layout("Main").Update(new Panel(canvas)
                                .Header($"[yellow]Dynamic Torus Knot[/] [dim]|[/] [cyan]({dynamicP},{dynamicQ}) Zoom:{zoom:F1}x[/] [dim]|[/] [magenta]Color Cycling[/]")
                                .BorderStyle(Style.Parse("cyan"))),
                            new Layout("Controls").Size(3).Update(new Panel("[dim]Automatic color cycling, dynamic parameters, zoom & lighting - Press Ctrl+C to exit[/]")
                                .BorderStyle(Style.Parse("grey")))
                        );
                    
                    ctx.UpdateTarget(layout);
                    
                    if (autoRotate)
                    {
                        angleX += 0.02;
                        angleY += 0.03;
                        angleZ += 0.015;
                    }
                    
                    Thread.Sleep(40); // Slightly reduced frame rate for better performance
                    frameCount++;
                }
            });

        AnsiConsole.Cursor.Show();
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[green]Thanks for watching the 3D torus knot![/]");
    }
}