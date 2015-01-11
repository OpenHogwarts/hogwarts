using UnityEngine;
using System.Collections;

public class Player : Photon.MonoBehaviour {

	private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
	private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this

	public int health
	{
		get {return characterData.health;}
		set {
			//prevent negative health values
			if (value <  0) {
				value = 0;
			}

			characterData.health = value;

			if (photonView.isMine) {
				characterData.save();
				healthBar.updateVertical(characterData.health, characterData.maxHealth);
			}
		}
	}

	public int exp
	{
		get {return characterData.exp;}
		set {
			//prevent negative values
			if (value <  0) {
				value = 0;
			}
			
			characterData.exp = value;
			
			if (photonView.isMine) {
				characterData.save();
				expBar.updateHoritzontal(characterData.exp, 100);
			}
		}
	}

	public int mana
	{
		get {return characterData.mana;}
		set {
			//prevent negative values
			if (value <  0) {
				value = 0;
			}
			
			characterData.mana = value;
			
			if (photonView.isMine) {
				characterData.save();
				manaBar.updateHoritzontal(characterData.mana, characterData.maxMana);
			}
		}
	}

	public int knut
	{
		get {return characterData.knut;}
		set {
			//prevent negative values
			if (value <  0) {value = 0;}

			characterData.knut = value;
			
			if (photonView.isMine) {
				characterData.save();
				Inventory.Instance.updateMoney();
			}
		}
	}
	public int sickle
	{
		get {return characterData.sickle;}
		set {
			//prevent negative values
			if (value <  0) {value = 0;}

			characterData.sickle = value;
			
			if (photonView.isMine) {
				characterData.save();
				Inventory.Instance.updateMoney();
			}
		}
	}

	public int galleon
	{
		get {return characterData.galleon;}
		set {
			//prevent negative values
			if (value <  0) {value = 0;}
			
			characterData.galleon = value;
			
			if (photonView.isMine) {
				characterData.save();
				Inventory.Instance.updateMoney();
			}
		}
	}

	public CharacterData characterData;

	Animator anim;
	bool gotFirstUpdate = false;
	public TextMesh nick;

	public UIBar healthBar;
	public UIBar expBar;
	public UIBar manaBar;

	public static Player _instance;
	
	public static Player Instance {
		get
		{
			return _instance;
		}
	}


	void Start () {
		if (photonView.isMine) {
			_instance = this;
		}
		anim = GetComponent<Animator>();
	}

	void Update()
	{
		if (!photonView.isMine) {
			transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
		} else {
			if (!gotFirstUpdate) {
				this.GetComponent<PhotonView>().RPC("setNick", PhotonTargets.OthersBuffered, PhotonNetwork.player.name);

				healthBar = GameObject.Find ("Canvas/HP Orb").GetComponent<UIBar>();
				expBar = GameObject.Find ("Canvas/ExpBar").GetComponent<UIBar>();
				manaBar = GameObject.Find ("Canvas/Mana Semicircle").GetComponent<UIBar>();

				healthBar.updateVertical(characterData.health, characterData.maxHealth);
				expBar.updateHoritzontal(characterData.exp, 100);
				manaBar.updateHoritzontal(characterData.mana, characterData.maxMana);

				gotFirstUpdate = true;
			}

			//looks like player is falling
			if (transform.position.y < -100) {
				transform.position = GameObject.Find("SpawnPoints/FirstJoin").transform.position;
			}
		}
	}

	public bool isDead () {
		if (health < 1) {
			return true;
		}
		return false;
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			// We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(anim.GetFloat("Speed"));
			stream.SendNext(anim.GetBool("Jumping"));
		}
		else
		{
			// Network player, receive data
			this.correctPlayerPos = (Vector3)stream.ReceiveNext();
			this.correctPlayerRot = (Quaternion)stream.ReceiveNext();

			if (anim) {
				anim.SetFloat("Speed", (float)stream.ReceiveNext());
				anim.SetBool("Jumping", (bool)stream.ReceiveNext());
			}

			
			if(gotFirstUpdate == false) {
				transform.position = this.correctPlayerPos;
				transform.rotation = this.correctPlayerRot;
				gotFirstUpdate = true;
			}
			
		}
	}

	[RPC]
	void setNick (string name) {
		nick.text = name;
	}

	private static Vector3 getVector3(string rString){
		string[] temp = rString.Substring(1, rString.Length-2).Split(',');
		float x = float.Parse(temp[0]);
		float y = float.Parse(temp[1]);
		float z = float.Parse(temp[2]);
		Vector3 rValue = new Vector3(x,y,z);
		return rValue;
	}
}
