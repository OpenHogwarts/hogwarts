using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IDropHandler, IBeginDragHandler {
	
	Item itm;
	bool isDragging = false;
	Vector3 initialPos;
	
	public Item item {
		get {return itm;}
		set {
			itm = value;
			GetComponent<RawImage>().texture = item.icon;
		}
	}

	public void OnBeginDrag (PointerEventData eventData) {
		isDragging = true;
		initialPos = transform.position;
		GetComponent<RectTransform> ().SetAsLastSibling ();

		Inventory.Instance.hideTooltip();
	}

	/**
		Allows dragging the item through the screen
		@return void
	*/
	public void OnDrag (PointerEventData eventData) {
		transform.position = Input.mousePosition;
	}

	/**
		Checks if user wants to throw the item or just move it in the bag
	*/
	public void OnDrop (PointerEventData eventData) {
		isDragging = false;

		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, raycastResults);

		if (raycastResults.Count > 1 && raycastResults [1].gameObject.name == "Slot(Clone)") {
			Slot slot = raycastResults [1].gameObject.GetComponent<Slot> ();

			if (!slot.available) {
				// @ToDo: we should move the entire list to free the slot
				restartPosition();
			} else {
				transform.position = slot.transform.position;
				slot.available = false;

				// update item pos in db
				item.characterItem.slot = slot.num;
				item.characterItem.save();
			}
		} else {
			restartPosition();
		}
	}


	/**
		Orders to display the tooltip of this item
	 */
	public void OnPointerEnter (PointerEventData eventData) {
		if (isDragging) {
			return;
		}
		Vector3 pos = new Vector3 (transform.position.x + 100, transform.position.y - 50, 0);
		Inventory.Instance.showTooltip (pos, item);
	}
	
	public void OnPointerExit (PointerEventData eventData) {
		Inventory.Instance.hideTooltip();
	}

	void restartPosition () {
		transform.position = initialPos;
	}
}
