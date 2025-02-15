using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Wire : Conductor {
    public const string PREFAB_PATH = "Prefabs/Wire";
    [SerializeField] int resolution;
    [SerializeField] float radius;
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
        polarity = Polarity.NONE;
        meshFilter = GetComponent<MeshFilter>();
    }

    static Mesh GetWireMesh(Vector3 start, Vector3 end, int wireResolution, float radius) {
        List<Vector3> baseCircle = GetCircle(radius, wireResolution, start, end - start).ToList();
        List<Vector3> topCircle = GetCircle(radius, wireResolution, end, end - start).ToList();
        baseCircle.Add(start);
        topCircle.Add(end);

        Vector3[] vertices = baseCircle.Concat(topCircle).ToArray();

        int baseCenterPos = baseCircle.Count - 1;
        int topOffset = baseCircle.Count;
        int topCenterPos = baseCircle.Count + topCircle.Count - 1;

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

            int nextBaseVertex = i + 1;
            if (nextBaseVertex == baseCenterPos) {
                nextBaseVertex = 0;
            }

            int nextTopVertex = topOffset + i + 1;
            if (nextTopVertex == topCenterPos) {
                nextTopVertex = topOffset;
            }

            // Base
            int triangleBaseIndex = triangleBaseOffset + triangleIndex;

            triangles[triangleBaseIndex] = baseCenterPos;
            triangles[triangleBaseIndex + 1] = nextBaseVertex;
            triangles[triangleBaseIndex + 2] = i;

            // Meio
            int triangleMiddleBottomIndex = triangleMiddleOffset + triangleIndex;
            int triangleMiddleTopIndex = triangleMiddleOffset * 2 + triangleIndex;

            triangles[triangleMiddleBottomIndex] = i;
            triangles[triangleMiddleBottomIndex + 1] = nextBaseVertex;
            triangles[triangleMiddleBottomIndex + 2] = topOffset + i;

            triangles[triangleMiddleTopIndex] = topOffset + i;
            triangles[triangleMiddleTopIndex + 1] = nextBaseVertex;
            triangles[triangleMiddleTopIndex + 2] = nextTopVertex;

            // Topo
            int triangleTopIndex = triangleTopOffset + triangleIndex;

            triangles[triangleTopIndex] = topCenterPos;
            triangles[triangleTopIndex + 1] = topOffset + i;
            triangles[triangleTopIndex + 2] = nextTopVertex;
        }

        Mesh mesh = new() {
            vertices = vertices,
            triangles = triangles
        };

        return mesh;
    }

    static Vector3[] GetCircle(float radius, int resolution, Vector3 center, Vector3 direction) {
        Vector2[] circle = GetCircle(radius, resolution);
        Vector3[] vertices = new Vector3[resolution];
        Quaternion rotateToDirection = Quaternion.FromToRotation(Vector3.forward, direction);

        for (int i = 0; i < resolution; i++) {
            vertices[i] = rotateToDirection * new Vector3(circle[i].x, circle[i].y) + center;
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

    public override Conductor[] GetConnectedConductors(Conductor from = null) {
        IEnumerable<Conductor> conductors = new Conductor[] {StartConductor, EndConductor};
        conductors = conductors.Where(x => x != null);

        if (from == null) {
            return conductors.ToArray();
        }

        if (from == StartConductor) {
            return conductors.Where(x => x != StartConductor).ToArray();
        }

        if (from == EndConductor) {
            return conductors.Where(x => x != EndConductor).ToArray();
        }

        return new Conductor[0];
    }

    void OnDestroy()
    {
        StartConductor = null;
        EndConductor = null;
    }
}