using UnityEngine;
using System;
using System.Collections;

public class Player : Photon.MonoBehaviour {

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

	public int maxHealth {
		get {
			return  characterData.maxHealth;
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

	public int money
	{
		get {return characterData.money;}
		set {
			//prevent negative values
			if (value <  0) {value = 0;}

			characterData.money = value;
			
			if (photonView.isMine) {
				characterData.save();
				try {
					Inventory.Instance.updateMoney();
				} catch (Exception) {}
			}
		}
	}

	public int level
	{
		get {return characterData.level;}
		set {
			//prevent negative values
			if (value <  0) {value = 0;}
			
			characterData.level = value;
			
			if (photonView.isMine) {
				characterData.save();
			}
		}
	}

	public bool isMine {
		get {
			return photonView.isMine;
		}
	}

	public bool isFriendly {
		get {
			return true; // this will change when we have multiple classes
		}
	}

	public new string name;
	public CharacterData characterData;
	private NPC _target;
	public NPC target {
		get {
			return _target;
		}
		set {
			_target = value;

			if (value == null) {
				PlayerPanel.Instance.hideTargetPanel();
			} else {
				PlayerPanel.Instance.showTargetPanel(value.transform);
			}

			SkillsUI.Instance.updateStatus();
		}
	}
	public bool isFlying = false;

	public Animator anim;
	private bool gotFirstUpdate = false;
	public TextMesh nick;

	public UIBar healthBar;
	public UIBar expBar;
	public UIBar manaBar;

	public NamePlate namePlate;

	public static Player _instance;
	
	public static Player Instance {
		get
		{
			return _instance;
		}
	}

	private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
	private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this


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

				photonView.RPC("setNick", PhotonTargets.OthersBuffered, PhotonNetwork.player.name);

				healthBar = GameObject.Find ("Canvas/PlayerPanel/HP Orb").GetComponent<UIBar>();
				expBar = GameObject.Find ("Canvas/PlayerPanel/ExpBar").GetComponent<UIBar>();
				manaBar = GameObject.Find ("Canvas/PlayerPanel/Mana Semicircle").GetComponent<UIBar>();

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

	[PunRPC]
	void setNick (string nick) {
		name = nick;
		namePlate.setName (nick, NamePlate.COLOR_NORMAL);
	}

	private static Vector3 getVector3(string rString){
		string[] temp = rString.Substring(1, rString.Length-2).Split(',');
		float x = float.Parse(temp[0]);
		float y = float.Parse(temp[1]);
		float z = float.Parse(temp[2]);
		Vector3 rValue = new Vector3(x,y,z);
		return rValue;
	}

	/**
	* Adds an item to current user
	*
	* @param int id item id
	* @param int amount item quantity
	* 
	* @return bool
	 */
	public bool addItem (int id, int amount = 1) {

		CharacterItem item = new CharacterItem {
			item = id,
			quantity = amount
		};
		bool success = item.create ();

		// reload inventory
		if (success) {
			try {
				Inventory.Instance.reload();
			} catch (Exception) {}
		}

		return success;
	}

	public void freeze () {
		transform.GetComponent<Motor>().enabled = false;
		anim.SetBool("Jumping", false);
		anim.SetFloat("Speed", 0);

		Rigidbody rigidbody = GetComponent<Rigidbody>();
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
	}

	public void unfreeze () {
		transform.GetComponent<Motor>().enabled = true;
	}

	[PunRPC]
	public void getDamage (int amount, int attacker) {
		health -= amount;

		if (target == null) {
			PhotonView.Find(attacker).gameObject.GetComponent<NPC>().setSelected();
		}
	}
}
