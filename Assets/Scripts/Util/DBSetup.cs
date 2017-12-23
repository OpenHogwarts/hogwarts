using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
	This class is used to setup DB items
    After making any change/addition remember to delete your existing db so changes get populated
    https://github.com/OpenHogwarts/hogwarts/wiki/Access-game-DB#adding-new-entries
 **/

public class DBSetup : MonoBehaviour {

	public static void start () {
		insertItems();
		insertTestItems();

		// NPC Tables
		insertNPCTemplates();
		insertNPCs();
	}

	public static void insertItems () {
		ItemData item;
        int id = 1;

		item = new ItemData ();
		item.id = id++;
		item.price = 40;
		item.health = 20;
		item.type = Item.ItemType.Consumable;
		item.subType = Item.ItemSubType.Health;
		item.create ();
		
		
		item = new ItemData ();
		item.id = id++;
		item.price = 2500;
		item.type = Item.ItemType.Armor;
		item.subType = Item.ItemSubType.Necklace;
		item.create ();

		item = new ItemData ();
		item.id = id++;
		item.price = 40;
		item.type = Item.ItemType.Weapon;
		item.subType = Item.ItemSubType.Wand;
		item.create ();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Ring;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Chest;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Chest;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Necklace;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Necklace;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Necklace;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 4000;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Hands;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.price = 15;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();
    }

	public static void insertTestItems () {
		CharacterItem item = new CharacterItem ();
		item.item = 1;
		item.character = 1;
		item.quantity = 2;
		item.create();
	}

	public static void insertNPCTemplates () {
		NPCTemplate template;

		template = new NPCTemplate();
		template.id = (int)NPCData.creatureTemplate.CastleSpider;
		template.name = "Araña del castillo";
		template.creatureRace = NPCData.creatureRace.Monster;
		template.creatureSubRace = NPCData.creatureSubRace.Normal;
		template.isAgressive = true;
		template.healthBase = 100;
		template.create ();

		template = new NPCTemplate();
		template.id = (int)NPCData.creatureTemplate.Human;
		template.name = "Default name";
		template.creatureRace = NPCData.creatureRace.Human;
		template.creatureSubRace = NPCData.creatureSubRace.Normal;
		template.isAgressive = false;
		template.healthBase = 100;
		template.create ();
    }

	public static void insertNPCs () {

		NPCData npc;
		List<Vector3> waypoints = new List<Vector3>();
		int i = 1;

		npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 25);
		npc.id = i++;
		npc.name = "Mike Ghole";
		npc.subRace = NPCData.creatureSubRace.Seller;
		npc.create ();
		// END -----------------

		npc = NPCTemplate.fillById(NPCData.creatureTemplate.CastleSpider, 5);
		npc.id = i++;
		npc.create ();

        waypoints = new List<Vector3>();
        waypoints.Add(new Vector3(7.56f, 0.00f, -0.81f));
        waypoints.Add(new Vector3(1.08f, 0.00f, -5.30f));
        waypoints.Add(new Vector3(-8.74f, 0.00f, 0.02f));
        waypoints.Add(new Vector3(0.35f, 0.00f, 3.60f));

        insertWaypointsTo(npc.id, waypoints);
        // END -----------------

        npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 25);
        npc.id = i++;
        npc.name = "Instructor";
        npc.subRace = NPCData.creatureSubRace.Quest;
        npc.create();
        // END -----------------
        npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 40);
        npc.id = i++;
        npc.name = "Hagrid";
        npc.subRace = NPCData.creatureSubRace.Quest;
        npc.create();

        // END -----------------
        npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 40);
        npc.id = i++;
        npc.name = "Estudiante";
        npc.subRace = NPCData.creatureSubRace.Talker;
        npc.create();
        // END -----------------
        npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 40);
        npc.id = i++;
        npc.name = "Estudiante";
        npc.subRace = NPCData.creatureSubRace.Talker;
        npc.create();
        // END -----------------
        npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 40);
        npc.id = i++;
        npc.name = "Estudiante";
        npc.subRace = NPCData.creatureSubRace.Talker;
        npc.create();
        // END -----------------
        npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 40);
        npc.id = i++;
        npc.name = "Estudiante";
        npc.subRace = NPCData.creatureSubRace.Normal;
        npc.create();
    }

    public static void insertWaypointsTo (int id, List<Vector3> waypoints) {
		WaypointData wp;

		foreach (Vector3 waypoint in waypoints) {
			wp = new WaypointData();
			wp.npc = id;
			wp.position = waypoint;
			wp.create();
		}
	}
}
