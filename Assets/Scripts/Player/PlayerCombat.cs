using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour {

	public static PlayerCombat Instance;
	public bool castingSpell = false;
	
	public List<Spell> spellList = new List<Spell> ();

	void Start () {
		Instance = this;

		// TMP TEMPORAL THIS SHOULD COME FROM DB
		spellList.Add((Spell)Resources.Load("Spells/Fireball"));
		spellList.Add((Spell)Resources.Load("Spells/Frostbolt"));
		spellList.Add((Spell)Resources.Load("Spells/Area test"));
	}

	// for numeric hotkeys
	public void spellCast(int key)
	{
		if (Player.Instance.target == null) {
			return;
		}
		StartCoroutine(SpellCast(spellList[key]));
	}

	public IEnumerator SpellCast(Spell spell)
	{
		//If not out of mana.
		if (spell.spellManaCost <= Player.Instance.mana)
		{
			SkillsUI.Instance.disableSkill(1);// TMP TEMPORAL
			castingSpell = true;
			Player.Instance.mana -= spell.spellManaCost;
			//Wait for choosen spell cast time.
			yield return new WaitForSeconds(spell.spellCastTime);

			// @ToDo: Play the spell cast animation

			//Set up a spell and cast it.
			SpellSetUp(spell);
			SkillsUI.Instance.enableSkill(1);// TMP TEMPORAL
		}
		
		castingSpell = false;
		yield break;
	}

	void SpellSetUp(Spell spell)
	{
		
		if(spell.spellPrefab == null)
		{
			Debug.LogWarning("Spell prefab is null.Assign a spell prefab.");
			return;
			
		}
		
		GameObject spellObject = null;
		
		//We will find what type of spell we are using
		//
		//********************************SINGLE*********************************************
		if(spell.spellType == Spell.SpellType.Single)
		{
			//Instantiated object will move straight forward.
			if(spell.spellDirection == Spell.SpellDirection.Directional)
			{
				spellObject = (GameObject)Instantiate(spell.spellPrefab, transform.position + transform.up, transform.rotation);
				spellObject.name = spell.spellName;
				
			}
			
			//Instantiated object will follow target.
			if(spell.spellDirection == Spell.SpellDirection.Follow)
			{
				spellObject = (GameObject)Instantiate(spell.spellPrefab,transform.position + transform.up, Quaternion.identity);
				spellObject.name = spell.spellName;
				spellObject.GetComponent<SpellObjectConfigurator>().myTarget = Player.Instance.target.transform;
			}
			
			//Instantiating to target's position.
			if(spell.spellDirection == Spell.SpellDirection.Point)
			{
				spellObject = (GameObject)Instantiate(spell.spellPrefab,Player.Instance.target.transform.position,Quaternion.identity);
				spellObject.name = spell.spellName;
				spellObject.GetComponent<SpellObjectConfigurator>().myTarget = Player.Instance.target.transform;
			}
			
			
		}
		
		//********************************AOE*********************************************
		else if(spell.spellType == Spell.SpellType.Aoe)
		{
			if(spell.spellPosition == Spell.SpellPosition.TargetTransform)
				spellObject = (GameObject)Instantiate(spell.spellPrefab,Player.Instance.target.transform.position,Quaternion.identity);
			else
				spellObject = (GameObject)Instantiate(spell.spellPrefab,transform.position,Quaternion.identity);
			
			spellObject.name = spell.spellName;
			
			
		}
		
		//********************************BUFF*********************************************
		else
		{
			//Spell type is a buff.And we are checking what type of buff spell is used.
			if(spell.buffType == Spell.BuffType.Heal)
			{
				spellObject = (GameObject)Instantiate(spell.spellPrefab,transform.position,Quaternion.identity);
				spellObject.name = spell.spellName;
				//currentHealth += (Random.Range(spell.minBuffAmount,spell.maxBuffAmount));	
				
			}
			else if(spell.buffType == Spell.BuffType.MagicalDefense)
			{
				spellObject = (GameObject)Instantiate(spell.spellPrefab,transform.position,Quaternion.identity);
				spellObject.name = spell.spellName;
				//magicalDefense += (Random.Range(spell.minBuffAmount,spell.maxBuffAmount));	
				
			}
			else
			{
				//Physical Defense
				spellObject = (GameObject)Instantiate(spell.spellPrefab,transform.position,Quaternion.identity);
				spellObject.name = spell.spellName;
				//physicalDefense += (Random.Range(spell.minBuffAmount,spell.maxBuffAmount));	
			}
		}
	}
}
