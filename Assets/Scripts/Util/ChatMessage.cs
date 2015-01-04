using UnityEngine;
using System.Collections;

public class ChatMessage {
	
	public string message = "";
	public string sender = "";
	
	
	public ChatMessage (string message, string sender) {
		this.message = message;
		this.sender = sender;
	}
}