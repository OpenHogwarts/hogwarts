using UnityEngine;
using System.Collections;

public class OpenBook : MonoBehaviour {


	public int book;

	void OnMouseDown () {
		GameObject.Find ("Canvas/Books/Panel").SetActive (true);
		GameObject.Find ("Canvas/Books/Base").SetActive (true);
		GameObject.Find("Canvas/Books/Book"+book).SetActive (true);
	}
}
