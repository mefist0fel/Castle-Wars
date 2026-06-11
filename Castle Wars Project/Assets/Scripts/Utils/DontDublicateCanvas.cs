using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Canvas))]
public class DontDublicateCanvas : MonoBehaviour
{
	private void Awake ()
	{
		DontDestroyOnLoad(this.gameObject);
		//if(UILayer.MainCanvas != transform)
		//{
		//	Debug.Log(" BaseUI destroy");
		//	Destroy(gameObject);
		//}
	}
}
