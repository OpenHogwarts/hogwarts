using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingPanel : MonoBehaviour {
	
	public RectTransform loadBar;
	public Text text;
	public GameObject self;

	// Use this for initialization
	void Start () {
		//PhotonNetwork.isMessageQueueRunning = false;
		//AsyncOperation async = Application.LoadLevelAsync(Menu.defaultLevel);
		//StartCoroutine (loadLevel(async));
	}

	IEnumerator loadLevel (AsyncOperation async) {
		int progress;
		text.text = "Cargando 0%";


		while (!async.isDone) {
			progress = (int)async.progress * 100;

			text.text = "Cargando "+ progress.ToString() +"%";

			//loadBar.localScale = new Vector2(load.progress, loadBar.localScale.y);
			yield return null;
		}
		if (async.isDone) {
			PhotonNetwork.isMessageQueueRunning = true;
		}
	}
}
