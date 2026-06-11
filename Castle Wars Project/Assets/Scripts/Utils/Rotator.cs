using UnityEngine;

public class Rotator : MonoBehaviour {

    [SerializeField]
    public Vector3 axe = Vector3.up;
    [SerializeField]
    public float speed = 1f; // angles per second

    private Transform thisTransform;

    public static Rotator AddToObject(GameObject gameObject, float newSpeed, Vector3 newAxe) {
        if (gameObject == null)
            return null;
        var rotator = gameObject.GetComponent<Rotator>() ?? gameObject.AddComponent<Rotator>();
        rotator.SetParams(newSpeed, newAxe);
        return rotator;
    }

    public void SetParams(float newSpeed, Vector3 newAxe) {
        speed = newSpeed;
        axe = newAxe.normalized;
    }

    public void SetParams(float newSpeed) {
        speed = newSpeed;
    }

    private void Awake() {
        thisTransform = transform;
        axe.Normalize();
    }

    private void Update() {
        thisTransform.Rotate(axe, speed * Time.deltaTime);
    }
}
