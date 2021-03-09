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
    public static Texture2D _questAvailable;
    public static Texture2D QuestAvailable
    {
        get
        {
            if (_questAvailable == null)
            {
                _questAvailable = Resources.Load("2DTextures/Cursor/QuestAvailable") as Texture2D;
            }
            return _questAvailable;
        }
    }
    public static Texture2D _questComplete;
    public static Texture2D QuestComplete
    {
        get
        {
            if (_questComplete == null)
            {
                _questComplete = Resources.Load("2DTextures/Cursor/QuestComplete") as Texture2D;
            }
            return _questComplete;
        }
    }
    public static Texture2D _talk;
    public static Texture2D Talk {
        get {
            if (_talk == null) {
                _talk = Resources.Load("2DTextures/Cursor/Talk") as Texture2D;
            }
            return _talk;
        }
    }
}
