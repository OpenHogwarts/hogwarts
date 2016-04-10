// Needs to be attached to Camera to enable depth rendering in forward lighting (required for some platforms)

using UnityEngine;
using System.Collections;

[AddComponentMenu("AQUAS/AQUAS Camera")]
[RequireComponent(typeof(Camera))]
public class AQUAS_Camera : MonoBehaviour {
#if UNITY_EDITOR
	void OnDrawGizmos(){
		Set();
	}
#endif
	void Start () {
		Set();
	}
	void Set(){
		if(GetComponent<Camera>().depthTextureMode == DepthTextureMode.None)
			GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
	}
}
