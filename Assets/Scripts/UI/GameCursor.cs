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
			if (_buy == null) {
				_buy = Resources.Load("2DTextures/Cursor/Buy") as Texture2D;
			}
			return _buy;
		}
	}
	public static Texture2D _taxi;
	public static Texture2D Taxi {
		get {
			if (_taxi == null) {
				_taxi = Resources.Load("2DTextures/Cursor/Taxi") as Texture2D;
			}
			return _taxi;
		}
	}
}
