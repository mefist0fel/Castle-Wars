using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public sealed class TubeMeshFilter : MonoBehaviour {
	[System.Serializable]
	public class Settings {
		[SerializeField] public int edgeCount = 6;
		[SerializeField] public float outerRadius = 1f;
        [SerializeField] public float innerRadius = 0.5f;
        [SerializeField] public float startAngle = 0;
		[SerializeField] public float depth = 10;
		public override int GetHashCode () {
			return startAngle.GetHashCode() + edgeCount.GetHashCode() + outerRadius.GetHashCode() + innerRadius.GetHashCode() + depth.GetHashCode();
		}
	}
	[SerializeField] private Settings settings = new Settings();
	[SerializeField] private Vector3 shift = Vector3.zero;

	[SerializeField] private MeshFilter meshFilter = null;

	private Mesh controlMesh = null;

    private void FindComponents() {
		if (meshFilter == null)
			meshFilter = GetComponent<MeshFilter> ();
    }

    private void Awake() {
        FindComponents();
        if (meshFilter.sharedMesh == null || meshFilter.sharedMesh.vertexCount == 0) {
            RegenerateMesh();
        }
    }

    [ContextMenu("Regenerate mesh")]
    private void RegenerateMesh () {
		controlMesh = GenerateMesh ();
		meshFilter.sharedMesh = controlMesh;
	}

    private Mesh GenerateMesh () {
		return GenerateTubeMesh (settings, shift);
	}

    public static Mesh GenerateTubeMesh(Settings settings, Vector3 shift) {
        var mesh = new MeshGenerator("Tube");
        int segments = settings.edgeCount;
        Vector3[] sidePoints = new Vector3[segments + 1];
        for (int i = 0; i < sidePoints.Length; i++) {
            float angleInRad = 2f * Mathf.PI / (float)segments * (float)i;
            sidePoints[i] = new Vector3(Mathf.Sin(angleInRad), 0, Mathf.Cos(angleInRad));
        }
        Vector3 topCenter = shift;
        Vector3 bottomCenter = shift + new Vector3(0, -settings.depth);
        // top
        for (int i = 0; i < segments; i++) {
            mesh.AddQuad(
                sidePoints[i + 1] * settings.innerRadius + topCenter,
                sidePoints[i] * settings.innerRadius + topCenter,
                sidePoints[i] * settings.outerRadius + topCenter,
                sidePoints[i + 1] * settings.outerRadius + topCenter
            );
        }
        if (settings.depth != 0) {
            for (int i = 0; i < segments; i++) {
                // outer
                mesh.AddQuad(
                    sidePoints[i + 1] * settings.outerRadius + topCenter,
                    sidePoints[i] * settings.outerRadius + topCenter,
                    sidePoints[i] * settings.outerRadius + bottomCenter,
                    sidePoints[i + 1] * settings.outerRadius + bottomCenter
                );
                // inner
                mesh.AddQuad(
                    sidePoints[i + 1] * settings.innerRadius + bottomCenter,
                    sidePoints[i] * settings.innerRadius + bottomCenter,
                    sidePoints[i] * settings.innerRadius + topCenter,
                    sidePoints[i + 1] * settings.innerRadius + topCenter
                );
            }
        }
        for (int i = 0; i < segments; i++) {
            // bottom
            mesh.AddQuad(
                sidePoints[i + 1] * settings.outerRadius + bottomCenter,
                sidePoints[i] * settings.outerRadius + bottomCenter,
                sidePoints[i] * settings.innerRadius + bottomCenter,
                sidePoints[i + 1] * settings.innerRadius + bottomCenter
            );
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
