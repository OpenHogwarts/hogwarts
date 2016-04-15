using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spell : ScriptableObject {
	
	public enum SpellType
	{
		Single,
		Buff,
		Aoe
	}

	
	public enum BuffType
	{
		Heal,
		MagicalDefense,
		PhysicalDefense
	}
	
	public enum SpellFlag
	{
		Slow,
		DamagePerSecond,
		None
	}

	public enum SpellCategory
	{
		Fire,
		Frost
	}

	public enum SpellDirection
	{
		Directional,
		Follow,
		Point
	}

	public enum SpellPosition
	{
		MyTransform,
		TargetTransform
	}
	
	
	public string spellName = "";
	public string spellInfo = "";
    public int minLevel = 0;

    public GameObject spellPrefab = null;
	public GameObject spellCollisionParticle = null;
	public GameObject dotEffect = null;
	public Texture2D spellIcon = null;
	
	public int spellManaCost = 0;
	public int spellMinDamage = 0;
	public int spellMaxDamage = 0;
	public int projectileSpeed = 0;
	public int dotDamage = 0;
	public int dotSeconds = 0;
	public int dotTick = 0;
	public int minBuffAmount = 0;
	public int maxBuffAmount = 0;
	public int spellCastTime = 0;
	public int slowDuration = 0;
	
	public SpellType spellType = SpellType.Single;
	public SpellDirection spellDirection = SpellDirection.Directional;
	public BuffType buffType = BuffType.Heal;
	public SpellFlag spellFlag = SpellFlag.None;
	public SpellPosition spellPosition = SpellPosition.MyTransform;
	public SpellCategory spellCategory = SpellCategory.Fire;
	
}
