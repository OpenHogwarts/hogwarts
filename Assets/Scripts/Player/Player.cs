using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class Player : Photon.MonoBehaviour {

    const int XP_BASE = 400;
    const int HEALTH_REGEN_BASE = 5;
    const int MANA_REGEN_BASE = 2;

	public int health
	{
		get {return characterData.health;}
		set {
			//prevent negative health values
			if (value <  0) {
				value = 0;
			}

            // We dont check if health > maxHealth here, because player may have some temporal buff
			characterData.health = value;

			if (photonView.isMine) {
				characterData.save();
				healthBar.updateVertical(characterData.health, characterData.maxHealth);
                startHealthRegeneration();
            }
		}
	}

	public int maxHealth {
		get {
			return  characterData.maxHealth;
		}
	}

    public int maxMana {
        get {
            return characterData.maxMana;
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
			
			if (photonView.isMine)
            {
                int nextLevelExp = XP_BASE * level;

                if (exp >= nextLevelExp) {
                    characterData.exp = exp - nextLevelExp;
                    level += 1;
                }
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

            // We dont check if mana > maxMana here, because player may have some temporal buff
            characterData.mana = value;
			
			if (photonView.isMine) {
				characterData.save();
				manaBar.updateHoritzontal(characterData.mana, characterData.maxMana);
                startManaRegeneration();
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
    private bool healthRegenStarted = false;
    private bool manaRegenStarted = false;
	public bool isFlying = false;
    public float lastHitTime;
    public bool isInCombat {
        get {
            if (Time.time > (lastHitTime + 10)) {
                return false;
            }
            return true;
        }
        set {
            if (value) {
                lastHitTime = Time.time;
            }
        }
    }
    public bool isDead {
        get {
            if (health < 1) {
                return true;
            }
            return false;
        }
    }

    public Animator anim;
	private bool gotFirstUpdate = false;
	public TextMesh nick;

	public UIBar healthBar;
	public Image healthBar2;
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
		startHealthRegeneration ();
		startManaRegeneration ();
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
				healthBar2 = GameObject.Find ("Canvas/PlayerPanel/HP Orb 2").GetComponent<Image>();
				expBar = GameObject.Find ("Canvas/PlayerPanel/ExpBar").GetComponent<UIBar>();
				manaBar = GameObject.Find ("Canvas/PlayerPanel/Mana Semicircle").GetComponent<UIBar>();

				healthBar.updateVertical(characterData.health, characterData.maxHealth);
				expBar.updateHoritzontal(characterData.exp, 100);
				manaBar.updateHoritzontal(characterData.mana, characterData.maxMana);

				gotFirstUpdate = true;
			}

			healthBar.GetComponent<Image> ().fillAmount = Mathf.Lerp (healthBar.GetComponent<Image> ().fillAmount, health / 270f, 4f*Time.deltaTime);
			healthBar2.fillAmount = Mathf.Lerp (healthBar.GetComponent<Image> ().fillAmount, health / 270f, 0.5f*Time.deltaTime);


			//looks like player is falling
			if (transform.position.y < -100) {
				transform.position = GameObject.Find("SpawnPoints/FirstJoin").transform.position;
			}
		}
	}

    private void startHealthRegeneration()
    {
        if (healthRegenStarted) {
            return;
        }
        StartCoroutine("regenerateHealth");
    }

    private void startManaRegeneration()
    {
        if (manaRegenStarted) {
            return;
        }
        StartCoroutine("regenerateMana");
    }
    

    IEnumerator regenerateHealth()
    {
        if (isDead) {
            yield break;
        }
        healthRegenStarted = true;

        while (health != maxHealth)
        {
            if (!isInCombat) {
                health += 1;
            }
            if (health > maxHealth) {
                health = maxHealth;
            }
			yield return new WaitForSeconds(1f/(HEALTH_REGEN_BASE + (level / 15)));
        }
        healthRegenStarted = false;
    }

    IEnumerator regenerateMana()
    {
        if (isDead) {
            yield break;
        }
        manaRegenStarted = true;

        while (mana != maxMana)
        {
            if (!isInCombat) {
                mana += 1;
            }
            if (mana > maxMana) {
                mana = maxMana;
            }
			yield return new WaitForSeconds(1f/(MANA_REGEN_BASE + (level / 15)));
        }
        manaRegenStarted = false;
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

		bool success;
		CharacterItem characterItem = Service.getOne<CharacterItem>("FROM inventory WHERE item == ?", id);
		
		if (characterItem != null) {
			characterItem.quantity += amount;
			characterItem.save();
			success = true;
		} else {
			CharacterItem item = new CharacterItem {
				item = id,
				quantity = amount
			};
			success = item.create ();
		}
		Debug.Log(success);
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
        isInCombat = true;
        health -= amount;

        if (target == null) {
			PhotonView.Find(attacker).gameObject.GetComponent<NPC>().setSelected();
		}
	}
}
