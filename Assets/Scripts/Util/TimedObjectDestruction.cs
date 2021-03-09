using UnityEngine;
using System.Collections;

public class TimedObjectDestruction : MonoBehaviour {

	public float timeOut = 1.0f;
	public bool detachChildren = false;
	
	public void Awake ()
	{
		Invoke ("DestroyNow", timeOut);
	}
	
	public void DestroyNow ()
	{
		if (detachChildren) {
			transform.DetachChildren ();
		}
		Destroy(gameObject);
	}
}
