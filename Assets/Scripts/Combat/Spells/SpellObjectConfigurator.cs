using UnityEngine;
using System.Collections;

public class SpellObjectConfigurator : Photon.MonoBehaviour {
	
	private Transform myTransform = null;
	public Spell spell = null;
	public Transform myTarget = null;
	
	void Start()
	{
		if (!photonView.isMine) {
			return;
		}
		myTransform = transform;
		spell = (Spell)Resources.Load("Spells/" + myTransform.gameObject.name, typeof(Spell));

		if(spell != null)
		{
			if(spell.spellType == Spell.SpellType.Single){

				if(spell.spellDirection == Spell.SpellDirection.Point)
				{
					NPC npc = myTarget.gameObject.GetComponent<NPC>();

					if(spell.spellFlag == Spell.SpellFlag.DamagePerSecond) {

						myTarget.gameObject.GetComponent<NPC>().getHit(Random.Range(spell.spellMinDamage, spell.spellMaxDamage), Player.Instance.gameObject);

						if(npc && npc.check == false)
							npc.StartCoroutine(npc.TakeDamageByFlagType(spell, Player.Instance.gameObject));
						else{
							npc.resetDps = true;
							npc.StartCoroutine(npc.TakeDamageByFlagType(spell, Player.Instance.gameObject));
						}
					}
					else
					{
						npc.StartCoroutine(npc.TakeDamageByFlagType(spell, Player.Instance.gameObject));
					}
				}
			}

			if(spell.spellType == Spell.SpellType.Aoe)
			{

				Collider[] hitColliders = Physics.OverlapSphere(myTransform.position,5.0f);
				
				//for each collider in that radius will take damage
				for(int i = 0; i < hitColliders.Length;i++)
				{
					if(hitColliders[i].tag == "NPC"){
						PhotonNetwork.Instantiate("Particles/" + spell.spellCollisionParticle.name, hitColliders[i].transform.position, Quaternion.identity, 0);

						//You can implement a your own damage script.This is an example.(col) means a player.
						//PlayerDamageScript pds = col.gameObject.GetComponent<PlayerDamageScript>();
						//pds.TakeDamage(); or pds.health -= damage;
						hitColliders[i].gameObject.GetComponent<NPC>().getHit(Random.Range(spell.spellMinDamage,spell.spellMaxDamage), Player.Instance.gameObject);


					}
				}
				//if spell type is aoe and spell flag is a damage over time
				if(spell.spellFlag == Spell.SpellFlag.DamagePerSecond){

					//for each collider in that radius will take damage
					for(int i = 0; i < hitColliders.Length;i++)
					{
						if(hitColliders[i].tag == "NPC"){
							NPC npc = hitColliders[i].gameObject.GetComponent<NPC>();
							
							if(npc && npc.check == false)
								npc.StartCoroutine(npc.TakeDamageByFlagType(spell, Player.Instance.gameObject));
							else{
								npc.resetDps = true;
								npc.StartCoroutine(npc.TakeDamageByFlagType(spell, Player.Instance.gameObject));
							}
						}
					}

				}

				else if(spell.spellFlag == Spell.SpellFlag.Slow)
				{
					for(int i = 0; i < hitColliders.Length;i++)
					{
						if(hitColliders[i].tag == "NPC"){
							NPC npc = hitColliders[i].gameObject.GetComponent<NPC>();

							npc.StartCoroutine(npc.TakeDamageByFlagType(spell, Player.Instance.gameObject));
						}

					}

				}
			}

		}

	}
	
	void Update()
	{
		if (!photonView.isMine) {
			return;
		}

		if(spell != null){
			if(spell.spellType == Spell.SpellType.Single)
			{
				//Instantiated object will move straight forward.
				if(spell.spellDirection == Spell.SpellDirection.Directional)
				{
					MoveStraightForward();
				}
				
				//Instantiated object will follow target.
				if(spell.spellDirection == Spell.SpellDirection.Follow)
				{
					FollowTarget();
				}
			}
		}
	}
	
	
	public void MoveStraightForward()
	{
		myTransform.Translate(new Vector3(0,0,spell.projectileSpeed * Time.deltaTime));
	}
	
	public void FollowTarget()
	{
		myTransform.TransformDirection(Vector3.forward);
		myTransform.Translate(new Vector3(0,0,spell.projectileSpeed * Time.deltaTime));
		myTransform.rotation = Quaternion.Slerp(myTransform.rotation,
											    Quaternion.LookRotation(myTarget.position - myTransform.position),
											    5 * Time.deltaTime);
	}
	
	void OnCollisionEnter(Collision col)
	{
		if (!photonView.isMine) {
			return;
		}

		if(col.gameObject.tag == "NPC")
		{

			ContactPoint cp = col.contacts[0];

			PhotonNetwork.Instantiate("Particles/" + spell.spellCollisionParticle.name, cp.point, Quaternion.identity, 0);
			
			NPC npc = col.gameObject.GetComponent<NPC>();

			if(spell.spellFlag == Spell.SpellFlag.DamagePerSecond){

				myTarget.gameObject.GetComponent<NPC>().getHit(Random.Range(spell.spellMinDamage, spell.spellMaxDamage), Player.Instance.gameObject);

				//This is for dot only.
				if(npc && npc.check == false)
					npc.StartCoroutine(npc.TakeDamageByFlagType(spell, Player.Instance.gameObject));
				else{
					npc.resetDps = true;
					npc.StartCoroutine(npc.TakeDamageByFlagType(spell, Player.Instance.gameObject));
				}
			}
			else
			{
				if(npc)
					npc.StartCoroutine(npc.TakeDamageByFlagType(spell, Player.Instance.gameObject));
				else
					Debug.LogWarning("The DamageByFlag script not found in" + " " + col.gameObject.name + ".Please assign the DamageByFlag script.");
			}

			col.gameObject.GetComponent<NPC>().getHit(Random.Range(spell.spellMinDamage,spell.spellMaxDamage), Player.Instance.gameObject);

			PhotonNetwork.Destroy(this.gameObject);
		}
	}
	
	
	
}
