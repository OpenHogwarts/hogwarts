#pragma strict
import HighlightingSystem;

class JSHighlighterController extends MonoBehaviour
{
	protected var h : HighlightingSystem.Highlighter;

	// 
	function Awake()
	{
		h = GetComponent(HighlightingSystem.Highlighter);
		if (h == null) { h = gameObject.AddComponent(HighlightingSystem.Highlighter); }
	}

	// 
	function Start()
	{
		h.FlashingOn(new Color(1f, 1f, 0f, 0f), new Color(1f, 1f, 0f, 1f), 2f);
	}
}