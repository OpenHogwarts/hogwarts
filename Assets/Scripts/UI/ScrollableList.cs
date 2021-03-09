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
                rectTransform.SetParent(gameObject.GetComponent<RectTransform>(), false);
                    
			} catch (Exception) { //looks like there are no more results to show
				break;
			}
        }
        
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
