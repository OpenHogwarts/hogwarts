using UnityEngine;
using System.Collections;

public class Spell
{
	public int id;
	public string name;
	public float distance;
	public float cooldown;
	public int level;
	public int type;

	public enum Type {
		Attack = 1,
		Heal = 2,
		Defense = 3,
	}
}
