using UnityEngine;

public sealed class FPS : MonoBehaviour {

	public enum ScreenCorner {
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	private static FPS instance;

	[SerializeField] private ScreenCorner corner = ScreenCorner.TopLeft;
	[SerializeField] private GUIStyle textStyle = null;

    private float timeA;
    private int fps;
    private int lastFPS;

    private void Awake () {
		if (instance != null) {
			Destroy(this);
		} else {
			instance = this;
		}
	}

    private void Start () {
        timeA = Time.timeSinceLevelLoad;
		textStyle.fontSize = 40;
		DontDestroyOnLoad (this);
    }

    private void Update () {
	    if(Time.timeSinceLevelLoad  - timeA <= 1){
            fps++;
        } else{
            lastFPS = fps + 1;
			timeA = Time.timeSinceLevelLoad;
	        fps = 0;
        }
    }

    private void OnGUI() {
        switch (corner) {
            case ScreenCorner.TopLeft:
                GUI.Label(new Rect(10, 10, 50, 50), lastFPS.ToString(), textStyle);
                break;
            case ScreenCorner.TopRight:
                GUI.Label(new Rect(Screen.width - 60, 10, 50, 50), lastFPS.ToString(), textStyle);
                break;
            case ScreenCorner.BottomLeft:
                GUI.Label(new Rect(10, Screen.height - 60, 50, 50), lastFPS.ToString(), textStyle);
                break;
            case ScreenCorner.BottomRight:
                GUI.Label(new Rect(Screen.width - 60, Screen.height - 60, 50, 50), lastFPS.ToString(), textStyle);
                break;
        }
    }
}