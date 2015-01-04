using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chat : MonoBehaviour {
	//Declare the SimChat variables.
	public List<ChatMessage> chatMessages = new List<ChatMessage>();
	//GUI Vars
	//scroll view position
	protected Vector2 sp = Vector2.zero,sp2 = Vector2.zero;
	private Rect chatRect = new Rect(0, Screen.height*0.6f, Screen.width*0.4f, Screen.height*0.4f);
	private Color c;
	public string message;
	private int messageCount = 0; // required to auto scroll down the chat
	
	
	public void sendMessage (string message, string sender = "") {
		GetComponent<PhotonView>().RPC ("addMessage", PhotonTargets.All, message, sender);
	}
	
	[RPC]
	public void addMessage (string message, string sender) {
		chatMessages.Add(new ChatMessage(message, sender));
	}
	
	
	void OnGUI()
	{
		string chatName = PhotonNetwork.player.name;
		
		if (PhotonNetwork.offlineMode) { return;} // do not display chat in offline mode
		
		if (GameLogic.showChat)
		{
			Screen.lockCursor = false;
			if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape) {
				GUIUtility.keyboardControl = 0;
				GUI.SetNextControlName("");
				GUI.FocusControl("");
				GameLogic.showChat = false;
				return;
			}
			GUI.skin.textField.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 17;
			GUI.skin.label.wordWrap = false;
			GUILayout.BeginArea(chatRect);
			GUILayout.BeginVertical("box");
			
			GUILayout.BeginVertical("box");
			
			// update scroll pos to bottom
			if(messageCount != chatMessages.Count) {
				sp.y = chatMessages.Count*17;
				messageCount = chatMessages.Count;
			}
			
			sp = GUILayout.BeginScrollView(sp);
			GUILayout.FlexibleSpace();
			c = GUI.contentColor;
			//loop through each of the messages contained in allMessages
			
			// foreach(ChatMessage sm in chatMessages){
			foreach(ChatMessage sm in chatMessages){
				GUILayout.BeginHorizontal();
				//check if the sender had the same name as me, and change the color
				if(sm.sender == chatName){
					GUI.contentColor = Color.white;
					
					GUILayout.Label(sm.sender+": "+sm.message);
					
				}else{
					GUI.contentColor = Color.green;
					
					if (sm.sender == "") { // announce message
						GUILayout.Label(sm.message);
					} else {
						GUILayout.Label(sm.sender+": "+sm.message);
					}
				}
				
				GUILayout.FlexibleSpace();
				
				GUILayout.EndHorizontal();
			}
			GUI.contentColor = c;
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			
			GUILayout.BeginHorizontal();
			//send a new message
			GUI.SetNextControlName("message-field");
			message = GUILayout.TextField(message);
			
			if (GUI.GetNameOfFocusedControl() == string.Empty) {
				GUI.FocusControl("message-field");
			}
			
			if(message != "" && (GUILayout.Button("Send") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) ){
				sendMessage(message, chatName);
				message = "";
			}
			
			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
			GUILayout.EndArea();
		} else {
			GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();
			
			int limit = 5;
			
			if (chatMessages.Count > limit) {
				for (int i = limit; i >= 1; i--) {
					int pos = chatMessages.Count - i;
					
					if (pos < 0) {
						break;
					}
					
					ChatMessage msg = chatMessages[pos];
					
					if (msg.sender == "") { // server announcement, like player join
						GUILayout.Label(msg.message);
					} else {
						GUILayout.Label(msg.sender+": "+msg.message);
					}
				}
			} else {
				int i = 0;
				foreach(ChatMessage msg in chatMessages) {
					if (msg.sender == "") { // server announcement, like player join
						GUILayout.Label(msg.message);
					} else {
						GUILayout.Label(msg.sender+": "+msg.message);
					}
					
					if (i == 5) {
						break;
					}
					
					i++;
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}