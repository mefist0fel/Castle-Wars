using UnityEngine;

public sealed class ExpandCamera : MonoBehaviour {
    [SerializeField]
    private Camera controlCamera = null; // Set from editor
    [SerializeField]
    private float fieldOfView = 45f;

    private int screenWight;
    private int screenHeight;

    private void Reset()
    {
        controlCamera = GetComponent<Camera>() ?? GetComponentInChildren<Camera>();
        fieldOfView = controlCamera?.fieldOfView ?? fieldOfView;
    }

    private void Start () {
        UpdateAspect();
    }

    private void OnValidate()
    {
        UpdateAspect();
    }

    private void UpdateAspect() {
        screenWight = Screen.width;
        screenHeight = Screen.height;
        var aspect = (float)screenHeight / (float)screenWight;
        if (aspect > 1f) {
            var verticalFOWRadians = fieldOfView * Mathf.Deg2Rad;
            var horisontalFOWRadians = 2f * Mathf.Atan(Mathf.Tan(verticalFOWRadians / 2f) * aspect);
            var horisontalFieldOfView = Mathf.Rad2Deg * horisontalFOWRadians;
            SetCameraFieldOfView(horisontalFieldOfView);
        } else {
            SetCameraFieldOfView(fieldOfView);
        }
    }

    private void SetCameraFieldOfView(float fieldOfView) {
        if (controlCamera == null) {
            Debug.LogError("No camera set on Expand Camera Component");
            return;
        }
        controlCamera.fieldOfView = fieldOfView;
    }

    private void Update () {
        if (screenWight != Screen.width || screenHeight != Screen.height) {
            UpdateAspect();
        }
	}
}
