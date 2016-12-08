using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public Dictionary<int, Quest> quests = new Dictionary<int, Quest>();
    public Dictionary<int, Quest> allQuests = new Dictionary<int, Quest>();
    private Dictionary<int, List<int>> npcQuests = new Dictionary<int, List<int>>();

    public void Start()
    {
        Quest quest;
        Instance = this;

        setAllQuests();

        // recover inprocess quests from db
        foreach (Task task in Service.db.Select<Task>("FROM tasks"))
        {
            if (!quests.ContainsKey(task.quest))
            {
                quest = allQuests[task.quest];
                quest.tasks = new Dictionary<int, Task>(); // tasks are begin recovered from db

                quests.Add(task.quest, quest);
            }
            quests[task.quest].tasks.Add(task.taskId, task);
        }
        try {
            PlayerPanel.Instance.showActiveQuests();
        } catch (System.Exception) {
        }
    }

    public void addQuest (Quest quest) {
        quests.Add(quest.id, quest);

        // save the new tasks on db
        foreach (Task task in quest.tasks.Values) {
            task.create();
        }

        PlayerPanel.Instance.showActiveQuests();
    }

    public void completeQuest (Quest quest)
    {
        // remove it from pending quests
        foreach (int id in quest.tasks.Keys) {
            Service.db.Delete("tasks", id);
        }

        // give quest's loot
        foreach (KeyValuePair<int, int> entry in quest.loot) {
            Player.Instance.addItem(entry.Key, entry.Value);
        }

        quests.Remove(quest.id);
        Destroy(quest.ui.gameObject);
    }

    public void sendAction(int id, Task.ActorType type, Task.ActionType action, int quantity = 0, int extraId = 0)
    {
        foreach (Quest quest in quests.Values)
        {
            foreach (Task task in quest.tasks.Values)
            {
                if (task.type == type && task.action == action &&
                    ( (task.type != Task.ActorType.NPC && task.id == id) ||
                      (task.type == Task.ActorType.NPC && ((task.idType == Task.IdType.Id && task.id == id) || (task.idType == Task.IdType.Template && task.id == extraId)))
                    )
                ) {
                    if (task.quantity > 0) {
                        task.currentQuantity += quantity;
                        // update phrase too as it displays the current quantity
                        task.ui.setPhrase(task.buildPhrase());
                        task.ui.setStatus(task.isCompleted);
                    }

                    if (task.quantity == 0 || task.quantity == task.currentQuantity)  {
                        task.isCompleted = true;
                        task.ui.setStatus(task.isCompleted);
                    }
                    task.save();

                    break;
                }
            }
        }
    }

    public List<int> getByNPC(int id)
    {
        if (npcQuests.ContainsKey(id)) {
            return npcQuests[id];
        }
        return new List<int>();
    }

    private void assignToNPC(Quest quest)
    {
        if (!npcQuests.ContainsKey(quest.assigner)) {
            npcQuests.Add(quest.assigner, new List<int>());
        }
        npcQuests[quest.assigner].Add(quest.id);
    }

    private void setAllQuests()
    {
        Quest quest;
        Task task;
        int taskId = 1;

        // -- start quest
        quest = new Quest();
        quest.id = 1;
        quest.assigner = 3; // NPC who assigned it
        assignToNPC(quest);
        quest.name = "Iniciación";
        quest.pre = "¡Bienvenido {{username}} !\nTu iniciación en Hogwarts será matar una de esas horribles <npc id='" + (int)NPCData.creatureTemplate.CastleSpider + "'>arañas</npc> que rondan por las cercanías del castillo.\n ¡Ten cuidado y lleva tu varita!";
        quest.after = "¡Excelente! La próxima vez tendrás recompensa.";
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

        allQuests.Add(quest.id, quest);
        // -- end quest

        // -- start quest
        quest = new Quest();
        quest.id = 2;
        quest.assigner = 4; // hagrid
        assignToNPC(quest);
        quest.name = "Iniciación";
        quest.pre = "{{username}}, necesitamos almacenar leña para poder mantener todas las hogueras encendidas durante el invierno. Ve al busque y tráeme un par.";
        quest.after = "¿Sólo me has traído esto? ... Bueno... Ya es algo.";
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

        allQuests.Add(quest.id, quest);
        // -- end quest
    }
}