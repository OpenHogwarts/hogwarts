using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ScrollableList : MonoBehaviour
{
    public GameObject itemPrefab;
    public int itemCount = 10,
			columnCount = 1;

	public delegate GameObject CallBack(GameObject newItem, int num);

	public void load(CallBack callback)
    {
        RectTransform rowRectTransform = itemPrefab.GetComponent<RectTransform>();
        RectTransform containerRectTransform = gameObject.GetComponent<RectTransform>();

        //calculate the width and height of each child item.
        float width = containerRectTransform.rect.width / columnCount;
        float ratio = width / rowRectTransform.rect.width;
        float height = rowRectTransform.rect.height * ratio;
        int rowCount = itemCount / columnCount;
        if (itemCount % rowCount > 0)
            rowCount++;

        //adjust the height of the container so that it will just barely fit all its children
        float scrollHeight = height * rowCount;
        containerRectTransform.offsetMin = new Vector2(containerRectTransform.offsetMin.x, -scrollHeight / 2);
        containerRectTransform.offsetMax = new Vector2(containerRectTransform.offsetMax.x, scrollHeight / 2);

        int j = 0;
        for (int i = 0; i < itemCount; i++)
        {
            //this is used instead of a double for loop because itemCount may not fit perfectly into the rows/columns
			if (i % columnCount == 0) {
				j++;
			}

			try {
				GameObject newItem = createNewItem(i, j, callback);

				//move and size the new item
				RectTransform rectTransform = newItem.GetComponent<RectTransform>();
				
				float x = -containerRectTransform.rect.width / 2 + width * (i % columnCount);
				float y = containerRectTransform.rect.height / 2 - height * j;
				rectTransform.offsetMin = new Vector2(x, y);
				
				x = rectTransform.offsetMin.x + width;
				y = rectTransform.offsetMin.y + height;
				rectTransform.offsetMax = new Vector2(x, y);
			} catch (Exception) { //looks like there are no more results to show
				break;
			}
        }

		//GameObject.Find ("MainPanel/Scrollbar").GetComponent<Scrollbar> ().value = 1;
    }

	//create a new item, name it, and set the parent
	private GameObject createNewItem(int num, int j, CallBack callback)
	{
		GameObject newItem = Instantiate(itemPrefab) as GameObject;

		newItem.tag = "TemporalPanel";
		newItem.name = gameObject.name + " item at (" + num + "," + j + ")";
		newItem.transform.SetParent (gameObject.transform);

		return callback (newItem, num);
	}
	
}
