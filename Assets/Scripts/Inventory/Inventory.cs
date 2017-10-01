using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {

    public GameObject itemSlotPrefab;
    public GameObject slotPrefab;
    public GameObject toolTip;
    public GameObject optionsPanel;
    int slotWidth;
    int slotHeight;
    int margin = 2;

    int x;
    int y;

    public static Inventory _instance;

    public static Inventory Instance {
        get {
            return _instance;
        }
    }

    void OnEnable() {
        _instance = this;

        reload();
        updateMoney();
    }

    public void reload() {

        x = -110;
        y = 110;

        // remove old items
        destroyOldIcons();

        int slotNum = 1; // new items have pos 0

        for (int i = 1; i < 6; i++) {
            for (int k = 1; k < 6; k++) {
                GameObject slot = (GameObject)Instantiate(slotPrefab);
                slot.tag = "TemporalPanel";
                slot.transform.SetParent(this.gameObject.transform, false);
                slot.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
                slot.GetComponent<Slot>().num = slotNum;

                // check if we can fill this slot
                fillSlot(slot.GetComponent<Slot>());

                if (slotWidth == 0) {
                    RectTransform rect = slot.GetComponent<RectTransform>();
                    slotWidth = (int)rect.rect.width + margin;
                    slotHeight = (int)rect.rect.height + margin;
                }

                x = x + slotWidth;
                if (k == 5) {
                    x = -110;
                    y = y - slotHeight;
                }
                slotNum++;
            }
        }
    }

    void destroyOldIcons() {
        var children = new List<GameObject>();
        foreach (Transform child in transform) {
            if (child.tag == "TemporalPanel") {
                children.Add(child.gameObject);
            }
        }
        children.ForEach(child => Destroy(child));
    }

    /**
		Tries to fill the given slot
		@param Slot slot slot to fill
		
		@return void
	 */
    void fillSlot(Slot slot) {

        bool isAssigned = false;
        Item itm = new Item();
        CharacterItem characterItem;
        GameObject itemSlot;

        characterItem = Service.getOne<CharacterItem>("FROM inventory WHERE _position == ? & character == ? & slot == ?", 0, PhotonNetwork.player.customProperties["characterId"], slot.num);

        if (characterItem != null) {
            isAssigned = true;
            
            itemSlot = (GameObject)Instantiate(itemSlotPrefab);
            itemSlot.tag = "TemporalPanel";
            itemSlot.GetComponent<ItemSlot>().item = itm.get(characterItem);
            itemSlot.GetComponent<ItemSlot>().currentSlot = slot;

            itemSlot.transform.SetParent(this.gameObject.transform, false);
            itemSlot.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
        } else {
            characterItem = Service.getOne<CharacterItem>("FROM inventory WHERE _position == ? & character == ? & slot == ?", 0, PhotonNetwork.player.customProperties["characterId"], 0);

            if (characterItem != null) {
                isAssigned = true;

                // assign this slot to the item
                characterItem.slot = slot.num;
                characterItem.save();

                itemSlot = (GameObject)Instantiate(itemSlotPrefab);
                itemSlot.tag = "TemporalPanel";
                itemSlot.GetComponent<ItemSlot>().item = itm.get(characterItem);
                itemSlot.GetComponent<ItemSlot>().currentSlot = slot;

                itemSlot.transform.SetParent(this.gameObject.transform, false);
                itemSlot.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
            }
        }
        slot.available = !isAssigned;
    }

    public void updateMoney() {
        Vector3 money = Util.formatMoney(Player.Instance.money);
        transform.Find("GalleonLabel").GetComponent<Text>().text = money.x.ToString();
        transform.Find("SickleLabel").GetComponent<Text>().text = money.y.ToString();
        transform.Find("KnutLabel").GetComponent<Text>().text = money.z.ToString();
    }

    public void showOptions(Vector3 pos, Item item) {
        bool hasMenu = true;

        Menu.Instance.hideTooltip();
        optionsPanel.SetActive(true);
        optionsPanel.GetComponent<RectTransform>().SetAsLastSibling();
        optionsPanel.transform.position = pos;

        switch (item.type) {
            case Item.ItemType.Consumable:
                optionsPanel.transform.Find("Button1/Text").GetComponent<Text>().text = "Usar";
                optionsPanel.gameObject.transform.Find("Button1").GetComponent<Button>().onClick.AddListener(
                    delegate {
                        item.use();
                        reload();
                    });

                break;
            default:
                hasMenu = false;
                break;
        }

        if (!hasMenu) {
            optionsPanel.SetActive(false);
        }
    }

    /**
		hides the options menu
		@return void
	*/
    public void hideOptions() {
        optionsPanel.SetActive(false);
    }
}