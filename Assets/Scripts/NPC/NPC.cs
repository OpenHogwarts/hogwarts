using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
This NPC brain is based on one-to-many way, which means that players set themself as target.
*/

public class NPC : MonoBehaviour
{
	public int Id;

	public int health
	{
		get {return data.health;}
		set {
			//prevent negative health values
			if (value <  0) {
				value = 0;
			}
			
			data.health = value;

			// @ToDo: Update UI bar
		}
	}

	public GameObject projectilePrefab;
	public AnimationClip idleAnimation;
	public AnimationClip runAnimation;
	public AnimationClip attackAnimation;
	public AnimationClip deathAnimation;
	public Animation anim;

	private bool isRanged;
	private float timeSinceLastAttack;
	private bool inCombat;
	private bool isDead;
	private bool EnableCombat;
	private bool isUseless = false;
	private bool isStunned = false;
	private float amountSlowedBy;
	private bool isKnockedBack = false;
	private bool isAttacking = false;
	private NPCData data;
	private int maxHealth;
	private GameObject target;
	private Vector3 initialPos;
	private float distanceFromIPos = 0;
	private float OriginalAttacksPerSecond;
	private bool backToIPos = false;

	protected NamePlate namePlate;
	
	public void Start()
	{
		if (Id == 0) {
			throw new Exception ("Id not assigned");
		}
		data = NPC.get (Id);
		Color color;

		try {
			anim = transform.FindChild("Model").GetComponent<Animation> ();
		} catch (Exception) {}

		namePlate = transform.FindChild ("NamePlate").GetComponent<NamePlate>();

		this.OriginalAttacksPerSecond = data.attacksPerSecond;
		this.initialPos = this.transform.position;
		this.maxHealth = this.health;

		if (data.isAggresive) {
			color = NamePlate.COLOR_ENEMY;
		} else {
			color = NamePlate.COLOR_NORMAL;
		}
		namePlate.setName (data.name, color);
		namePlate.setLevel (data.level);
	}
	
	private void Update()
	{
		if (data.subType != NPCData.creatureSubType.Normal) {return;} // enable in debug to not verifiy if you are the master
		//if (data.subType != NPCData.creatureSubType.Normal || !PhotonNetwork.isMasterClient) {return;}


		if (this.EnableCombat) {
			this.timeSinceLastAttack += Time.deltaTime;
		}

		if (this.health < 1) {
			this.isDead = true;
		}
		if (this.isDead)
		{
			Player player = this.target.GetComponent<Player>();
			int levelDiff = data.level - player.level;

			// do not give exp if player has > 3 levels than the killed npc
			if (levelDiff > -2) {
				player.exp += data.expValue + levelDiff * 10;
			}

			anim.Play(this.deathAnimation.name);
		}
		else
		{
			float num = 0;
			distanceFromIPos = Vector3.Distance(this.transform.position, this.initialPos);

			if (isTooFar()) {
				backToIPos = true;
			} else if (this.transform.position == initialPos) {
				backToIPos = false;
			}

			if (target != null) {
				num = Vector3.Distance(this.transform.position, target.transform.position);

				if (this.inCombat && !isTooFar() || num < data.distanceToLoseAggro && data.isAggresive && !isTooFar()) {
					this.EnableCombat = true;
				} else {
					this.EnableCombat = false;
					this.inCombat = false;
					this.target = null;
				}
			}


			if (this.isKnockedBack)
			{
				Transform transform = this.transform;
				Vector3 vector3 = transform.position + (this.target.transform.forward * 15f * Time.deltaTime);
				transform.position = vector3;
			}
			else
			{
				if (this.isStunned) {
					this.EnableCombat = false;
				}
					
				if (this.isUseless) {
					return;
				}
					
				if (this.EnableCombat)
				{
					this.transform.eulerAngles = new Vector3(0.0f, Mathf.Atan2((this.target.transform.position.x - this.transform.position.x), (this.target.transform.position.z - this.transform.position.z)) * 57.29578f, 0.0f);
					if (Vector3.Distance(this.transform.position, this.target.transform.position) > data.attackRange)
					{
						anim.Play(this.runAnimation.name);
						transform.position = Vector3.MoveTowards(this.transform.position, this.target.transform.position, data.runSpeed * Time.deltaTime);
					}
					if (this.timeSinceLastAttack > 1.0 / data.attacksPerSecond && !this.isAttacking && num < data.attackRange)
					{
						this.timeSinceLastAttack = 0.0f;
						anim.Play(this.attackAnimation.name);
						this.target.GetComponent<Player>().health -= data.damage;
					}
					else
					{
						if (anim.isPlaying) {
							return;
						}
						anim.Play(this.idleAnimation.name);
					}
				}
				else {
					if (backToIPos) {
						gotoInitialPoint();
					} else {
						anim.Play(this.idleAnimation.name);
					}
				}
					
			}
		}
	}

	public bool isTooFar () {
		if (distanceFromIPos >= data.distanceToLoseAggro) {
			return true;
		}
		return false;
	}

	/**
	 * Points NPC back to initial position
	 *
	 */
	public void gotoInitialPoint () {
		anim.Play(this.runAnimation.name);
		this.transform.position = Vector3.MoveTowards(this.transform.position, this.initialPos, data.runSpeed * Time.deltaTime);
		this.transform.eulerAngles = new Vector3(0.0f, Mathf.Atan2((this.initialPos.x - this.transform.position.x), (this.initialPos.z - this.transform.position.z)) * 57.29578f, 0.0f);

		if (this.health < this.maxHealth) {
			StartCoroutine (restoreHealth());
		}
	}

	public IEnumerator restoreHealth () {
		while (this.health < this.maxHealth) {
			this.health += 10;

			if (this.health > this.maxHealth) {
				this.health = this.maxHealth;
			}

			yield return new WaitForSeconds(1);
		}
	}

	/**
	 * Set NPC target
	 */
	public void setTarget (GameObject gameObject) {

		// trying to set an invalid target?
		if (gameObject.tag != "Player" && gameObject.tag != "NPC" || target != null) {
			return;
		}
		// @ToDo: raycast on target direction to see if we can see it (is behind a wall?, etc..)

		float num = Vector3.Distance(this.transform.position, gameObject.transform.position);

		if ((double) num < data.distanceToLoseAggro && data.isAggresive && !isTooFar()) {
			target = gameObject;
		}
	}

	public void setSelected () {
		if (!data.isAggresive) {
			namePlate.Name.color = NamePlate.COLOR_SELECTED;
		}
	}

	public void OnMouseDown() {
		setSelected ();
	}
	// Cursors resource: http://sethcoder.com/reference/wow/INTERFACE/CURSOR/
	public void OnMouseOver () {

		Texture2D texture = null;

		if (data.isAggresive) {
			texture = Resources.Load("2DTextures/Cursor/Attack") as Texture2D;
		}

		if (texture != null) {
			Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
		}
	}
	public void OnMouseEnter () {
		OnMouseOver();
	}
	public void OnMouseExit () {
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

	public static NPCData get (int id) {
		return Service.db.SelectKey<NPCData> ("npc", id);
	}
	
	public void KnockBackSelf()
	{
		if (this == null)
			return;
		this.StartCoroutine("KnockBack");
	}

	private IEnumerator KnockBack()
	{
		yield return new WaitForSeconds(1);
	}
	
	public void StunSelf(float timeToStunSelf)
	{
		this.StartCoroutine("Stun", (object) timeToStunSelf);
	}

	private IEnumerator Stun(float timeToStun)
	{
		yield return new WaitForSeconds(1);
	}
	
	public void SlowAttackSpeed(float amountToSlow)
	{
		data.attacksPerSecond = this.OriginalAttacksPerSecond;
		this.StartCoroutine("Slow", (object) amountToSlow);
	}

	private IEnumerator Slow(float amountToReduceBy)
	{
		yield return new WaitForSeconds(1);
	}

}
public class ItemSpawner {}