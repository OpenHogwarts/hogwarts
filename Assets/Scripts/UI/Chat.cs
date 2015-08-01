using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/*
	Provided By Vancete, available in the Unity Asset Store
*/

public class Chat : MonoBehaviour {
	
	public Text chatbox;
	public Text input;
	public InputField input2;
	public Scrollbar scroll;
	public static bool isWritting = false;
	public static Chat Instance;
	public static int MAX_CHARACTERS = 5000;
	
	void Start () {
		Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		//If the mouse position is not over the chat, scroll down
		if ((Input.mousePosition.x > 296)&&(Input.mousePosition.y > 140)) {
			scroll.value = 0;
		}
		
		//If we press Enter, then send a RPC command
		if ((Input.GetKeyDown (KeyCode.Return))&&(input.text != "")) {
			if(input.text.Length < 4){
				this.GetComponent<PhotonView>().RPC("Msg", PhotonTargets.All, new object[] {"[" + PhotonNetwork.player.name + "] " +input.text});
			}else{
				//We can use this to send special commands, like GM messages, Global Announcements, etc
				if(input.text.Substring(0,4) == "!gm "){
					this.GetComponent<PhotonView>().RPC("Msg", PhotonTargets.All, new object[] {"<color=\"#00c0ff\">[GM]</color> " +input.text.Replace("!gm ", "")});
				}else if(input.text.Substring(0,4) == "!ga "){
					this.GetComponent<PhotonView>().RPC("Msg", PhotonTargets.All, new object[] {"<color=\"#fe8f00\">[Global Announcement]</color> " +input.text.Replace("!ga ", "")});
				}else{
					this.GetComponent<PhotonView>().RPC("Msg", PhotonTargets.All, new object[] {"[" + PhotonNetwork.player.name + "] " +input.text});
				}
			}
			isWritting = false;
		}
	}

	public void setWritting (bool isIt) {
		isWritting = isIt;
	}
	
	//Needs to be RPC to work online
	[PunRPC]
	public void Msg (string msg) {
		AddMsg (msg);
	}
	
	//Just add the msg to the chatbox
	void AddMsg (string msg) {
		chatbox.text = chatbox.text + "\n" + msg;
		input.text = "";
		input2.text = "";

		if (chatbox.text.Length > MAX_CHARACTERS) {
			chatbox.text = chatbox.text.Substring(MAX_CHARACTERS - 100);
		}
	}

	public void LocalMsg (string msg) {
		AddMsg (msg);
	}
}
