using UnityEngine;
using System.Collections;

// Used for fast build debugging

public class Debug2 {

	public static void echo (string text) {
		Chat.Instance.LocalMsg(text);
	}

	public static void Log (object text) {
		echo(text.ToString());
	}
}
