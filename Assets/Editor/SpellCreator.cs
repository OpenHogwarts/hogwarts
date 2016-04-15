using UnityEngine;
using UnityEditor;
using System.Collections;

public class SpellCreator : EditorWindow {

	[MenuItem("Spell Maker/Create Spell")]
	static void Init()
	{
		SpellCreator spellWindow = (SpellCreator)EditorWindow.CreateInstance(typeof(SpellCreator));
		spellWindow.Show();
	}
	
	Spell tempSpell = null;
	
	void OnGUI()
	{
		// If we have temporary spell.	
		if(tempSpell)
		{
			
			tempSpell.spellName = EditorGUILayout.TextField("Spell Name",tempSpell.spellName);
			tempSpell.spellInfo = EditorGUILayout.TextField("Spell Info",tempSpell.spellInfo);
            tempSpell.minLevel = EditorGUILayout.IntField("Min level", tempSpell.minLevel);
            tempSpell.spellPrefab = (GameObject)EditorGUILayout.ObjectField("Spell prefab",tempSpell.spellPrefab,typeof(GameObject),false);
			tempSpell.spellCollisionParticle = (GameObject)EditorGUILayout.ObjectField("Spell Collision Effect",tempSpell.spellCollisionParticle,typeof(GameObject),false);
			tempSpell.spellManaCost = EditorGUILayout.IntField("Mana cost",tempSpell.spellManaCost);
			tempSpell.spellIcon = (Texture2D)EditorGUILayout.ObjectField("Spell Icon",tempSpell.spellIcon,typeof(Texture2D),false);
			tempSpell.spellCastTime = EditorGUILayout.IntField("Cast Time",tempSpell.spellCastTime);
			tempSpell.spellCategory = (Spell.SpellCategory)EditorGUILayout.EnumPopup("Category",tempSpell.spellCategory);
			tempSpell.spellType = (Spell.SpellType)EditorGUILayout.EnumPopup("Spell Type",tempSpell.spellType);
			
			switch(tempSpell.spellType)
			{
				case Spell.SpellType.Single:
				tempSpell.spellMinDamage = EditorGUILayout.IntField("Minimum Damage",tempSpell.spellMinDamage);
				tempSpell.spellMaxDamage = EditorGUILayout.IntField("Maximum Damage",tempSpell.spellMaxDamage);
				tempSpell.projectileSpeed = EditorGUILayout.IntField("Projectile Speed",tempSpell.projectileSpeed);
				tempSpell.spellDirection = (Spell.SpellDirection)EditorGUILayout.EnumPopup("Direction",tempSpell.spellDirection);
				tempSpell.spellFlag = (Spell.SpellFlag)EditorGUILayout.EnumPopup("Flag",tempSpell.spellFlag);
				switch(tempSpell.spellFlag)
				{
					case Spell.SpellFlag.DamagePerSecond:
					tempSpell.dotDamage = EditorGUILayout.IntField("Damage",tempSpell.dotDamage);
					tempSpell.dotTick = EditorGUILayout.IntField("Over",tempSpell.dotTick);
					tempSpell.dotSeconds = EditorGUILayout.IntField("Time",tempSpell.dotSeconds);
					tempSpell.dotEffect = (GameObject)EditorGUILayout.ObjectField("Effect prefab",tempSpell.dotEffect,typeof(GameObject),false);
					break;
					
					case Spell.SpellFlag.Slow:
					tempSpell.slowDuration = EditorGUILayout.IntField("Slow Duration",tempSpell.slowDuration);
					break;

					case Spell.SpellFlag.None:
					
					break;
					
				}
				break;
				
				case Spell.SpellType.Buff:
				tempSpell.buffType = (Spell.BuffType)EditorGUILayout.EnumPopup("Buff Type",tempSpell.buffType);
				tempSpell.minBuffAmount = EditorGUILayout.IntField("Min Buff Amount",tempSpell.minBuffAmount);
				tempSpell.maxBuffAmount = EditorGUILayout.IntField("Max Buff Amount",tempSpell.maxBuffAmount);
				break;
				
				case Spell.SpellType.Aoe:
				tempSpell.spellMinDamage = EditorGUILayout.IntField("Minimum Damage",tempSpell.spellMinDamage);
				tempSpell.spellMaxDamage = EditorGUILayout.IntField("Maximum Damage",tempSpell.spellMaxDamage);
				tempSpell.spellFlag = (Spell.SpellFlag)EditorGUILayout.EnumPopup("Flag",tempSpell.spellFlag);
				tempSpell.spellPosition = (Spell.SpellPosition)EditorGUILayout.EnumPopup("Position",tempSpell.spellPosition);
				switch(tempSpell.spellFlag)
				{
					case Spell.SpellFlag.DamagePerSecond:
					tempSpell.dotDamage = EditorGUILayout.IntField("Damage",tempSpell.dotDamage);
					tempSpell.dotTick = EditorGUILayout.IntField("Over",tempSpell.dotTick);
					tempSpell.dotSeconds = EditorGUILayout.IntField("Time",tempSpell.dotSeconds);
					tempSpell.dotEffect = (GameObject)EditorGUILayout.ObjectField("Effect prefab",tempSpell.dotEffect,typeof(GameObject),false);
					break;
					
					case Spell.SpellFlag.Slow:
					tempSpell.slowDuration = EditorGUILayout.IntField("Slow Duration",tempSpell.slowDuration);
					break;

					case Spell.SpellFlag.None:
					
					break;
					
				}
				break;
			}
			
			
		}
		
		EditorGUILayout.Space();
		
		
		
		if(tempSpell == null){
			
			if(GUILayout.Button("Create Spell"))
			{
				//This line of code instantiates of our temporary spell.
				tempSpell = (Spell)ScriptableObject.CreateInstance<Spell>();
			}
			
		}
		else
		{
			
			if(GUILayout.Button("Create Scriptable Object"))
			{
				//Creates a scriptable object that contains all of our spell properties.
					AssetDatabase.CreateAsset(tempSpell,
				   	"Assets/Resources/Spells/" + tempSpell.spellName + ".asset");
				 	AssetDatabase.SaveAssets();

				    Selection.activeObject = tempSpell;
					
					tempSpell = null;
				
			}
			
			
			if(GUILayout.Button("Reset"))
			{
				//Reseting spell properties.
				Reset();
				
			}
			
		}
		
	}
	
	void Reset()
	{
		if(tempSpell){
			
			tempSpell.spellName = "";
			tempSpell.spellInfo = "";
            tempSpell.minLevel = 0;
            tempSpell.dotDamage = 0;
			tempSpell.spellCastTime = 0;
			tempSpell.spellIcon = null;
			tempSpell.spellMaxDamage = 0;
			tempSpell.spellMinDamage = 0;
			tempSpell.spellPrefab = null;
			tempSpell.spellType = Spell.SpellType.Single;
			tempSpell.slowDuration = 0;
			tempSpell.spellFlag = Spell.SpellFlag.None;
		
		}
	}
	
}
