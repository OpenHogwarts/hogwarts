using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraTargeting : MonoBehaviour
{
	// Which layers targeting ray must hit (-1 = everything)
	public LayerMask targetingLayerMask = -1;
	
	// Targeting ray length
	private float targetingRayLength = Mathf.Infinity;
	
	// Camera component reference
	private Camera cam;

	// 
	private string info = @"Left Click - switch flashing for object under mouse cursor
Right Click - switch see-through mode for object under mouse cursor
'1' - fade in/out constant highlighting
'2' - turn on/off constant highlighting immediately
'3' - turn off all types of highlighting immediately
";

	// 
	void Awake()
	{
		cam = GetComponent<Camera>();
	}

	// 
	void Update()
	{
		TargetingRaycast();
	}

	// 
	public void TargetingRaycast()
	{
		// Current target object transform component
		Transform targetTransform = null;
		
		// If camera component is available
		if (cam != null)
		{
			RaycastHit hitInfo;
			
			// Create a ray from mouse coords
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);

			// Targeting raycast
			if (Physics.Raycast(ray, out hitInfo, targetingRayLength, targetingLayerMask.value))
			{
				// Cache what we've hit
				targetTransform = hitInfo.collider.transform;
			}
		}
		
		// If we've hit an object during raycast
		if (targetTransform != null)
		{
			// And this object has HighlighterController component
			HighlighterController hc = targetTransform.GetComponentInParent<HighlighterController>();
			if (hc != null)
			{
				// Transfer input information to the found HighlighterController
				if (Input.GetButtonDown("Fire1")) { hc.Fire1(); }
				if (Input.GetButtonUp("Fire2")) { hc.Fire2(); }
				hc.MouseOver();
			}
		}
	}

	// 
	void OnGUI()
	{
		GUI.Label(new Rect(10, Screen.height - 100, 500, 100), info);
	}
}
