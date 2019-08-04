using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
	This class is used to:
    - Setup DB entries
    - List NPC related things like their quests or phrases

    After making any change/addition on entries saved in db, remember to delete your existing db so changes get populated:
    https://github.com/OpenHogwarts/hogwarts/wiki/Access-game-DB#adding-new-entries

    New entries/changes on other methods like "getTalkerPhrase" don't require deleting the db, as they are not saved there.
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

        item = new ItemData();
        item.id = id++;
        item.price = 1500;
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
        // END -----------------

        npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 40);
        npc.id = i++; //This is 9
        npc.name = "Student";
        npc.subRace = NPCData.creatureSubRace.Talker;
        npc.create();
        // END -----------------

        npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 50);
        npc.id = i++; // 10
        npc.name = "Professor Quirel";
        npc.subRace = NPCData.creatureSubRace.Normal;
        waypoints = new List<Vector3>();
        waypoints.Add(new Vector3(-6.82f, 0.00f, -4f));
        waypoints.Add(new Vector3(-5.96f, 0.00f, -5f));
        waypoints.Add(new Vector3(-1.96f, 0.01f, 4f));
        insertWaypointsTo(npc.id, waypoints);
        npc.create();
        // END -----------------

        npc = NPCTemplate.fillById(NPCData.creatureTemplate.Human, 50);
        npc.id = i++; // 11
        npc.name = "Draco";
        npc.isAggresive = true;
        npc.attackRange = 6;
        npc.attacksPerSecond = 0.25f;
        npc.subRace = NPCData.creatureSubRace.Normal;
        waypoints = new List<Vector3>();
        waypoints.Add(new Vector3(6.65f, 0.01f, -3.00f));
        waypoints.Add(new Vector3(-0.75f, 0.01f, -9.16f));
        waypoints.Add(new Vector3(-4.18f, 0.01f, -3.61f));
        insertWaypointsTo(npc.id, waypoints);
        npc.create();
    }

    public static string getTalkerPhrase (int npcId) {
        switch (npcId) {
            case 5: // regular student
                return LanguageManager.get("RANDOM_STUDENT_PHRASE_1");
                break;
            case 6: // regular student
                return LanguageManager.get("RANDOM_STUDENT_PHRASE_2");
                break;
            case 7: // regular student
                return LanguageManager.get("RANDOM_STUDENT_PHRASE_3");
                break;
            case 9: // regular student
                return LanguageManager.get("RANDOM_STUDENT_PHRASE_4");
                break;
            default:
                return "ERROR_PHRASE_NOT_SET_FOR_NPC_" + npcId;
                break;
        }
    }

    public static void setAllQuests() {
        Quest quest;
        Task task;
        int taskId = 1;

        // -- start quest
        quest = new Quest();
        quest.id = 1;
        quest.assigner = 3; // NPC who assigned it
        QuestManager.Instance.assignToNPC(quest);
        quest.loot.Add(3, 4); // id, quantity

        task = new Task();
        task.quest = quest.id;
        task.taskId = taskId++;
        task.id = (int)NPCData.creatureTemplate.CastleSpider;
        task.idType = Task.IdType.Template;
        task.quantity = 1;
        task.type = Task.ActorType.NPC;
        task.action = Task.ActionType.Kill;

        quest.tasks.Add(task.taskId, task);

        task = new Task();
        task.quest = quest.id;
        task.taskId = taskId++;
        task.id = 1;
        task.idType = Task.IdType.Id;
        task.type = Task.ActorType.NPC;
        task.action = Task.ActionType.Talk;

        quest.tasks.Add(task.taskId, task);

        QuestManager.Instance.allQuests.Add(quest.id, quest);
        // -- end quest

        // -- start quest
        quest = new Quest();
        quest.id = 2;
        quest.assigner = 4; // hagrid
        QuestManager.Instance.assignToNPC(quest);
        quest.loot.Add(3, 4); // id, quantity

        task = new Task();
        task.quest = quest.id;
        task.taskId = taskId++;
        task.id = 26;
        task.idType = Task.IdType.Id;
        task.quantity = 8;
        task.type = Task.ActorType.Item;
        task.action = Task.ActionType.GetItem;

        quest.tasks.Add(task.taskId, task);

        QuestManager.Instance.allQuests.Add(quest.id, quest);
        // -- end quest
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
