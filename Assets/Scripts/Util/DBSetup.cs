using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
	This class is used to setup DB items
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
		item.name = "Queso de cabra";
		item.description = "Hecho en el mejor caserío muggle";
		item.price = 40;
		item.health = 20;
		item.type = Item.ItemType.Consumable;
		item.subType = Item.ItemSubType.Health;
		item.create ();
		
		
		item = new ItemData ();
		item.id = id++;
		item.name = "Colgante de Dermor";
		item.description = "No se me ocurre nada bueno";
		item.price = 2500;
		item.type = Item.ItemType.Armor;
		item.subType = Item.ItemSubType.Necklace;
		item.create ();

		item = new ItemData ();
		item.id = id++;
		item.name = "Varita de principiante";
		item.description = "La varita reglamentaria del centro";
		item.price = 40;
		item.type = Item.ItemType.Weapon;
		item.subType = Item.ItemSubType.Wand;
		item.create ();

        item = new ItemData();
        item.id = id++;
        item.name = "Fibra";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Pieza de mecanismo";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Llave desconocida";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Bufanda de estudiante";
        item.description = "¿Qué habrá sido del resto?";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Carne de araña";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Manuscrito antiguo";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Hoja de algo";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Piel de animal";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Manzana";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Anillo simple";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Ring;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Antigua capa de maga";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Chest;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Antigua capa de mago";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Chest;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Platano";
        item.description = "No tiene mucho misterio";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Mineral de Draconita";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Cosa rara";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Amuleto simple";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Necklace;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Amuleto simple";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Necklace;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Diamante rosa adulterado";
        item.description = "Sín descripción";
        item.price = 4000;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Polvos mágicos";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Useless;
        item.subType = Item.ItemSubType.Scrap;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Guantes de estudiante";
        item.description = "Sín descripción";
        item.price = 40;
        item.type = Item.ItemType.Armor;
        item.subType = Item.ItemSubType.Hands;
        item.create();

        item = new ItemData();
        item.id = id++;
        item.name = "Diamante rosa normal";
        item.description = "Sín descripción";
        item.price = 40;
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
