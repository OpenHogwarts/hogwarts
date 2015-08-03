using UnityEngine;
using System.Collections;

// Cursors resource: http://sethcoder.com/reference/wow/INTERFACE/CURSOR/
public class GameCursor {

	public static Texture2D _attack;
	public static Texture2D Attack {
		get {
			if (_attack == null) {
				_attack = Resources.Load("2DTextures/Cursor/Attack") as Texture2D;
			}
			return _attack;
		}
	}

	public static Texture2D _buy;
	public static Texture2D Buy {
		get {
			if (_attack == null) {
				_attack = Resources.Load("2DTextures/Cursor/Buy") as Texture2D;
			}
			return _attack;
		}
	}
}
