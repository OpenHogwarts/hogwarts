import UnityEngine
import HighlightingSystem

class BooHighlighterController (MonoBehaviour): 
	
	protected h as Highlighter

	// 
	def Awake():
		h = (gameObject.GetComponent[of Highlighter]() as Highlighter);
		if h == null:
			h = (gameObject.AddComponent[of Highlighter]() as Highlighter);
	
	// 
	def Start():
		h.FlashingOn(Color(0F, 1F, 0F, 0F), Color(0F, 1F, 0F, 1F), 1F);
