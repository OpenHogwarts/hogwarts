using UnityEngine;
using System.Collections;
using HighlightingSystem;

public class ObjectGlow : MonoBehaviour {

	private Highlighter h;
	public Color c;

	// Use this for initialization
	void Start () {
		gameObject.AddComponent<Highlighter> ();
		h = gameObject.GetComponent<Highlighter> ();
		h.OccluderOn();
	}

	void OnMouseOver () {
		h.On (c);
	}
}
