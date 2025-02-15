using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Wire : Conductor {
    public const string PREFAB_PATH = "Prefabs/Wire";
    [SerializeField] int resolution;
    [SerializeField] float radius;
    public new Polarity polarity = Polarity.NONE;
    MeshFilter meshFilter;
    Vector3 startPosition;
    public Vector3 StartPosition {
        get => startPosition;
        set {
            if (value == startPosition) {
                return;
            }

            startPosition = value;

            meshFilter.mesh = GetWireMesh(startPosition, endPosition, resolution, radius);
        }
    }

    Vector3 endPosition;
    public Vector3 EndPosition {
        get => endPosition;
        set {
            if (value == endPosition) {
                return;
            }

            endPosition = value;
            meshFilter.mesh = GetWireMesh(startPosition, endPosition, resolution, radius);
        }
    }

    Conductor startConductor = null;
    public Conductor StartConductor {
        get => startConductor;
        set {
            if (value == startConductor) {
                return;
            }
            startConductor = value;
            GeneratorManager.Singleton.RefreshConductors();
        }
    }
    Conductor endConductor = null;
    public Conductor EndConductor {
        get => endConductor;
        set {
            if (value == endConductor) {
                return;
            }
            endConductor = value;
            GeneratorManager.Singleton.RefreshConductors();
        }
    }

    void Awake() {
        meshFilter = GetComponent<MeshFilter>();
    }

    static Mesh GetWireMesh(Vector3 start, Vector3 end, int wireResolution, float radius) {
        List<Vector3> startCircle = GetCircle(radius, wireResolution, start).ToList();
        List<Vector3> endCircle = GetCircle(radius, wireResolution, end).ToList();
        startCircle.Add(start);
        endCircle.Add(end);

        Vector3[] vertices = startCircle.Concat(endCircle).ToArray();

        int startCenterPos = startCircle.Count - 1;
        int endOffset = startCircle.Count;
        int endCenterPos = endCircle.Count - 1;

        // Quantidade de arestas da base em polígolo regular = quantidade de vértices da base
        // Quantidade de faces = Quantidade de arestas (juntam no meio)
        // Faces entre as bases = Quantidade de arestas da base * 2
        // Total: Quantidade de vértices da base * 4
        int triangleAmount = wireResolution * 4;
        int triangleBaseOffset = 0;
        int triangleMiddleOffset = wireResolution * 3;
        int triangleTopOffset = wireResolution * 9;
        
        int[] triangles = new int[triangleAmount * 3];

        for (int i = 0; i < wireResolution; i++) {
            int triangleIndex = i * 3;
            // Base
            int triangleBaseIndex = triangleBaseOffset + triangleIndex;

            triangles[triangleBaseIndex] = startCenterPos;
            triangles[triangleBaseIndex + 1] = i;
            triangles[triangleBaseIndex + 2] = i + 1;

            // Meio
            int triangleMiddleIndex = triangleMiddleOffset + triangleIndex;

            triangles[triangleMiddleIndex] = i;
            triangles[triangleMiddleIndex + 1] = i + 1;
            triangles[triangleMiddleIndex + 2] = endOffset + i;

            triangles[triangleMiddleIndex + 3] = endOffset + i;
            triangles[triangleMiddleIndex + 4] = endOffset + i + 1;
            triangles[triangleMiddleIndex + 5] = i + 1;

            // Topo
            int triangleTopIndex = triangleTopOffset + triangleIndex;

            triangles[triangleTopIndex] = endCenterPos;
            triangles[triangleTopIndex + 1] = endOffset + i;
            triangles[triangleTopIndex + 2] = endOffset + i + 1;
        }

        Mesh mesh = new() {
            vertices = vertices,
            triangles = triangles
        };

        return mesh;
    }

    static Vector3[] GetCircle(float radius, int resolution, Vector3 center) {
        Vector2[] circle = GetCircle(radius, resolution);
        Vector3[] vertices = new Vector3[resolution];

        for (int i = 0; i < resolution; i++) {
            vertices[i] = new Vector3(circle[i].x, circle[i].y) + center;
        }

        return vertices;
    }

    static Vector2[] GetCircle(float radius, int resolution) {
        Vector2[] vertices = new Vector2[resolution];
        for (int i = 0; i < resolution; i++) {
            float angleInRad = (float)i / resolution * 2 * MathF.PI;
            vertices[i] = new(
                (float) Math.Cos(angleInRad) * radius,
                (float) Math.Sin(angleInRad) * radius
            );
        }

        return vertices;
    }

    public override Conductor[] GetConnectedConductors(Conductor from) {
        if (from == StartConductor) {
            return new Conductor[] {EndConductor};
        }

        if (from == EndConductor) {
            return new Conductor[] {StartConductor};
        }

        return new Conductor[0];
    }

    void OnDestroy()
    {
        StartConductor = null;
        EndConductor = null;
    }
}