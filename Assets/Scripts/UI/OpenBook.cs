using UnityEngine;
using System.Collections;

public class OpenBook : MonoBehaviour {

	public bool isBook = true;
	public int book;

	void OnMouseDown () {
		if (isBook) {
			GameObject.Find ("Canvas/Books/Panel").SetActive (true);
			GameObject.Find ("Canvas/Books/Base").SetActive (true);
			GameObject.Find ("Canvas/Books/Close").SetActive (true);
			GameObject.Find ("Canvas/Books/Book" + book).SetActive (true);
		}
	}

	public void Close(){
			for(int i=0; i< GameObject.Find ("Canvas/Books").transform.childCount; i++)
			{
				GameObject child =  GameObject.Find ("Canvas/Books").transform.GetChild(i).gameObject;
				if(child != null)
					child.SetActive(false);
			}
	}
}
