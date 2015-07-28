using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GamePanel : MonoBehaviour, IDragHandler, IDropHandler {

	public static bool isMovingAPanel = false;

	private RectTransform rect;

	public void Start () {
		rect = GetComponent<RectTransform>();
	}

	public void OnEnable () {
		if (rect == null) {
			rect = GetComponent<RectTransform>();
		}
		rect.SetAsLastSibling ();
	}

	/**
		Allows dragging the panel through game screen
		@return void
	*/
	public void OnDrag (PointerEventData eventData) {

		rect.position += new Vector3(eventData.delta.x, eventData.delta.y);
		isMovingAPanel = true;
	}

	public void OnDrop (PointerEventData eventData) {
		isMovingAPanel = false;
	}

	public void closePanel () {
		gameObject.SetActive (false);
	}
}
