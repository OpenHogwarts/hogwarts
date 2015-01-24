using UnityEngine;
using System.Collections;

public class KeyCommands : MonoBehaviour {

	public GameObject canvas;
	public Chat chat;
	
	// Update is called once per frame
	void Update () {
		if ((Input.GetKeyDown (KeyCode.Z)) && (Chat.isWritting == false)) {
			if(canvas.GetActive()){
				canvas.SetActive(false);
			}else{
				canvas.SetActive(true);
			}
		} else if (Input.GetKeyDown (KeyCode.F2)) {
			string now = System.DateTime.Now.ToString("_d-MMM-yyyy-HH-mm-ss-f");
			Application.CaptureScreenshot("./Screenshots/"+string.Format("Screenshot{0}.png", now));
			chat.LocalMsg("<color=\"#e8bf00\">[Sistema]</color> Captura guardada como Screenshot"+now+".png");
		} else if (Input.GetKeyDown (KeyCode.P)) {
			UIMenu.Instance.togglePanel("CharacterPanel");
		} else if (Input.GetKeyDown (KeyCode.I)) {
			UIMenu.Instance.togglePanel("BagPanel");
		}

	}
}
