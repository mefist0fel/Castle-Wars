using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public sealed class CylinderMeshFilter: MonoBehaviour {
	[System.Serializable]
	public class Settings {
		[SerializeField] public int edgeCount = 6;
		[SerializeField] public float radius = 1f;
		[SerializeField] public float startAngle = 0;
		[SerializeField] public float depth = 10;
		public override int GetHashCode () {
			return startAngle.GetHashCode() + edgeCount.GetHashCode() + radius.GetHashCode() + depth.GetHashCode();
		}
	}

	[SerializeField] private Settings settings = new Settings();
	[SerializeField] private Vector3 shift = Vector3.zero;
	[SerializeField] private MeshFilter meshFilter = null;

    private void FindComponents() {
		if (meshFilter == null)
			meshFilter = GetComponent<MeshFilter> ();
    }

    private void Start() {
        FindComponents();
        if (meshFilter.sharedMesh == null || meshFilter.sharedMesh.vertexCount == 0) {
            RegenerateMesh();
        }
    }

    [ContextMenu("Regenerate mesh")]
    private void RegenerateMesh () {
		meshFilter.sharedMesh = GenerateMesh();
    }

    private Mesh GenerateMesh () {
		return GeneratePrysmMesh (settings, shift);
	}

    public static Mesh GeneratePrysmMesh(Settings settings, Vector3 shift) {
        var mesh = new MeshGenerator("Prysm");
        int segments = settings.edgeCount;
        Vector3[] sideNormals = new Vector3[segments + 1];
        Vector3[] topPoints = new Vector3[segments + 1];
        Vector3[] bottomPoints = new Vector3[segments + 1];
        for (int i = 0; i < topPoints.Length; i++) {
            float angleInRad = 2f * Mathf.PI / (float)segments * (float)i;
            float angleOfNormalInRad = 2f * Mathf.PI / (float)segments * (float)(i + 0.5f);
            topPoints[i] = new Vector3(Mathf.Sin(angleInRad), 0, Mathf.Cos(angleInRad)) * settings.radius + shift;
            bottomPoints[i] = topPoints[i] + new Vector3(0, -settings.depth);
            sideNormals[i] = new Vector3(Mathf.Sin(angleOfNormalInRad), 0, Mathf.Cos(angleOfNormalInRad));
        }
        Vector3 topCenter = shift;
        Vector3 bottomCenter = shift + new Vector3(0, -settings.depth);
        for (int i = 0; i < segments; i++) {
            mesh.AddTriangle(topCenter, topPoints[i], topPoints[i + 1]);
        }
        if (settings.depth != 0) {
            for (int i = 0; i < segments; i++) {
                mesh.AddQuad(
                    topPoints[i + 1],
                    topPoints[i],
                    bottomPoints[i],
                    bottomPoints[i + 1],
                    sideNormals[i]
                );
            }
        }
		for (int i = 0; i < segments; i++) {
			mesh.AddTriangle (bottomCenter, bottomPoints [i + 1], bottomPoints [i]);
		}
		return mesh.Generate();
	}

	#if UNITY_EDITOR
	private int settingsHash = 0;
	private int shiftHash = 0;
    private void OnDrawGizmos() {
		FindComponents ();
		if (settingsHash != settings.GetHashCode ()) {
			settingsHash = settings.GetHashCode ();
			RegenerateMesh ();
		}
		if (shiftHash != shift.GetHashCode ()) {
			shiftHash = shift.GetHashCode ();
			RegenerateMesh ();
		}
	}
	#endif
}
