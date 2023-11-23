using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IPointerClickHandler, IEndDragHandler {

    Item itm;
    bool isDragging = false;
    bool isDeciding = false;
    Vector3 initialPos;
    public Slot currentSlot;

    public Item item {
        get { return itm; }
        set {
            itm = value;
            GetComponent<RawImage>().texture = item.icon;
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        GamePanel.isMovingAPanel = true;
        isDragging = true;
        initialPos = transform.position;
        GetComponent<RectTransform>().SetAsLastSibling();

        Menu.Instance.hideTooltip();
    }

    /**
		Allows dragging the item through the screen
		@return void
	*/
    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    /**
		Checks if user wants to throw the item or just move it in the bag
	*/
    public void OnEndDrag(PointerEventData eventData) {
        GamePanel.isMovingAPanel = false;
        isDragging = false;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        // try to find an slot
        foreach (RaycastResult raycast in raycastResults) {
            try {
                Slot slot = raycast.gameObject.GetComponent<Slot>();

                if (!slot.available) {
                    resetPosition();
                } else {

                    // we need to check if this item can be equiped on this slot
                    if (slot.type == Slot.slotType.equipment && !item.isValidEquipmentPosition(slot.subType)) {
                        break;
                    }
                    // @ToDo switch itemSlot parent to current panel since it could be changed (Inventory or CharacterPanel)
                    transform.position = slot.transform.position;

                    if (slot.type == Slot.slotType.equipment) {
                        transform.SetParent(Menu.Instance.getPanel("InventoryPanel").transform);
                    } else {
                        transform.SetParent(Menu.Instance.getPanel("BagPanel").transform);
                    }

                    slot.available = false;
                    currentSlot.available = true; // free the old slot

                    // update item pos in db
                    item.characterItem.slot = slot.num;
                    item.characterItem.position = slot.subType;
                    item.characterItem.save();

                    if (slot.type == Slot.slotType.equipment || currentSlot.type == Slot.slotType.equipment) {
                        PlayerEquipment.Instance.reload();
                    }
                    currentSlot = slot;
                }
                return;
            } catch (Exception) {
                continue;
            }
        }

        resetPosition();
    }


    /**
		Orders to display the tooltip of this item
	 */
    public void OnPointerEnter(PointerEventData eventData) {
        if (isDragging || isDeciding) {
            return;
        }
        Vector3 pos = new Vector3(transform.position.x + 100, transform.position.y - 50, 0);

        Menu.Instance.showTooltip(pos, item);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Menu.Instance.hideTooltip();
    }

    public void OnPointerClick(PointerEventData data) {
        if (isDragging || data.button != PointerEventData.InputButton.Right) {
            return;
        }

        Vector3 pos = new Vector3(transform.position.x + 100, transform.position.y - 50, 0);

        // check if this slot is in inventory or in characterPanel
        if (item.characterItem.position == 0) {
            Inventory.Instance.showOptions(pos, item);
        }
        isDeciding = true;
    }

    void resetPosition() {
        transform.position = initialPos;
    }
}