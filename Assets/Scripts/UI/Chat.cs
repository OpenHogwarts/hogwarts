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
     public Image background;
	public bool isWritting = false;
	public static Chat Instance;
	public static int MAX_CHARACTERS = 5000;
     private bool hasMouseOver = false;

	void Start () {
		Instance = this;
	}

    // used in Unity UI (when chat input gets clicked)
    public void setWritting () {
        isWritting = true;
        background.enabled = true;
    }

    public void endWritting() {
	   isWritting = false;
	   background.enabled = false;
    }

    public void onPointerEnter () {
        background.enabled = true;
        hasMouseOver = true;
    }
    public void onPointerExit () {
        background.enabled = false;
        hasMouseOver = false;
    }

    public void sendMessage ()
    {
        if (input.text == "") {
		  endWritting();
            return;
        }

        if (input.text.Length < 4) {
            this.GetComponent<PhotonView>().RPC("Msg", PhotonTargets.All, new object[] { "[" + PhotonNetwork.player.name + "] " + input.text });
        } else {
            // We can use this to send special commands, like GM messages, Global Announcements, etc
            if (input.text.Substring(0, 4) == "!gm ") {
                this.GetComponent<PhotonView>().RPC("Msg", PhotonTargets.All, new object[] { "<color=\"#00c0ff\">[GM]</color> " + input.text.Replace("!gm ", "") });
            } else if (input.text.Substring(0, 4) == "!ga ") {
                this.GetComponent<PhotonView>().RPC("Msg", PhotonTargets.All, new object[] { "<color=\"#fe8f00\">[Global Announcement]</color> " + input.text.Replace("!ga ", "") });
            } else {
                this.GetComponent<PhotonView>().RPC("Msg", PhotonTargets.All, new object[] { "[" + PhotonNetwork.player.name + "] " + input.text });
            }
        }
        endWritting();
    }

	// Needs to be RPC to work online
	[PunRPC]
	public void Msg (string msg) {
		AddMsg (msg);

        // if the mouse is not over the chat, scroll down automatically
        if (!hasMouseOver) {
            scroll.value = 0;
        }
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
