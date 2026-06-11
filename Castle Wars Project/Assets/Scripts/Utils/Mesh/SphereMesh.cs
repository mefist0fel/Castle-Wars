using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof (MeshFilter))]
[RequireComponent(typeof (MeshRenderer))]
public sealed class SphereMesh : MonoBehaviour {
	[SerializeField]
    private float radius = 1;
	[SerializeField]
    private int details = 6;
    [SerializeField]
    private bool generateOnStart = true;

    [SerializeField]
    [HideInInspector]
    private MeshFilter meshFilter;

    private readonly static Quaternion[] sides = new Quaternion[] {
		Quaternion.Euler(0,0,0),
		Quaternion.Euler(180,0,0),
		Quaternion.Euler(0, 90,0),
		Quaternion.Euler(0,-90,0),
		Quaternion.Euler( 90,0,0),
		Quaternion.Euler(-90,0,0)
	};

    public void Regenerate(float newRadius) {
        Regenerate(newRadius, details);
    }

    public void Regenerate(float newRadius, int newDetailsPerUnit) {
        radius = newRadius;
        details = newDetailsPerUnit;
        GenerateMesh();
    }

    private void Awake () {
        FindControlComponents();
	}

    private void FindControlComponents() {
        if (meshFilter == null) {
            meshFilter = Utils.GetOrCreateComponent<MeshFilter>(gameObject);
        }
    }

    private void Start () {
        if (generateOnStart)
            GenerateMesh();
    }

	[ContextMenu("regenerate planet")]
    private void GenerateMesh() {
        FindControlComponents();
        meshFilter.sharedMesh = CreateMesh(radius, details);
    }

    private Mesh CreateMesh(float radius, int details) {
        int h = (details + 1) * (details + 1);
        CombineInstance[] meshes = new CombineInstance[6];
        for (int sideId = 0; sideId < 6; sideId++) {
            Vector3[] normals = new Vector3[h];
            Vector2[] textures = new Vector2[h];
            Vector3[] vertexMeshComponent = new Vector3[h];
            Color[] colorMeshComponent = new Color[h];
            int[] triangles = new int[details * details * 2 * 6];
            for (int i = 0; i <= details; i++) {
                for (int j = 0; j <= details; j++) {
                    normals[i + j * (details + 1)] = (sides[sideId] * new Vector3(1f - 2f * i / (details), 1f - 2f * j / (details), 1f)).normalized;
                }
            }
            for (int i = 0; i <= details; i++) {
                for (int j = 0; j <= details; j++) {
                    vertexMeshComponent[i + j * (details + 1)] = normals[i + j * (details + 1)] * radius;
                    colorMeshComponent[i + j * (details + 1)] = new Color(1, 1, 1);
                    int sideShiftX = sideId % 4;
                    int sideShiftY = sideId / 4;
                    textures[i + j * (details + 1)] = new Vector2((float)i / (details) * 0.25f + sideShiftX * 0.25f, (float)j / (details) * 0.25f + sideShiftY * 0.25f);
                }
            }
            for (int i = 0; i < details; i++) {
                for (int j = 0; j < details; j++) {
                    var isLeftSide = i < (details / 2);
                    var isTopSide = j < (details / 2);
                    if (isLeftSide ^ isTopSide) {
                        triangles[(i + j * details) * 6 + 0] = i + j * (details + 1);
                        triangles[(i + j * details) * 6 + 1] = i + 1 + (j) * (details + 1);
                        triangles[(i + j * details) * 6 + 2] = i + (j + 1) * (details + 1);
                        triangles[(i + j * details) * 6 + 3] = i + (j + 1) * (details + 1);
                        triangles[(i + j * details) * 6 + 4] = i + 1 + (j) * (details + 1);
                        triangles[(i + j * details) * 6 + 5] = i + 1 + (j + 1) * (details + 1);
                    } else {
                        triangles[(i + j * details) * 6 + 0] = i + j * (details + 1);
                        triangles[(i + j * details) * 6 + 1] = i + 1 + (j) * (details + 1);
                        triangles[(i + j * details) * 6 + 2] = i + 1 + (j + 1) * (details + 1);
                        triangles[(i + j * details) * 6 + 3] = i + j * (details + 1);
                        triangles[(i + j * details) * 6 + 4] = i + 1 + (j + 1) * (details + 1);
                        triangles[(i + j * details) * 6 + 5] = i + (j + 1) * (details + 1);
                    }
                }
            }
            meshes[sideId] = new CombineInstance
            {
                transform = Matrix4x4.identity,
                mesh = new Mesh { vertices = vertexMeshComponent, uv = textures, colors = colorMeshComponent, triangles = triangles, normals = normals }
            };
        }

        Mesh combined = new Mesh();
        combined.CombineMeshes(meshes, true);
        return combined;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        GenerateMesh();
    }
#endif
}
