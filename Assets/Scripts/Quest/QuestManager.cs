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
            quests[task.quest].tasks.Add(task.id, task);
        }
        PlayerPanel.Instance.showActiveQuests();
    }

    public void addQuest (Quest quest) {
        quests.Add(quest.id, quest);
        PlayerPanel.Instance.showActiveQuests();
    }

    public void sendAction(int id, Task.ActorType type, Task.ActionType action, int quantity = 0)
    {
        foreach (Quest quest in quests.Values)
        {
            foreach (Task task in quest.tasks.Values)
            {
                if (task.id == id && task.type == type && task.action == action)
                {
                    if (task.quantity > 0)
                    {
                        task.currentQuantity += quantity;
                    }

                    if (task.quantity == 0 || task.quantity == task.currentQuantity)
                    {
                        task.isCompleted = true;
                    }
                    task.save();

                    break;
                }
            }
        }
    }

    public List<int> getByNPC(int id)
    {
        if (npcQuests.ContainsKey(id))
        {
            return npcQuests[id];
        }
        return new List<int>();
    }

    private void assignToNPC(Quest quest)
    {
        if (!npcQuests.ContainsKey(quest.assigner))
        {
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

        task = new Task();
        task.taskId = taskId++;
        task.id = (int)NPCData.creatureTemplate.CastleSpider;
        task.quantity = 1;
        task.type = Task.ActorType.NPC;
        task.action = Task.ActionType.Kill;

        quest.tasks.Add(task.id, task);

        allQuests.Add(quest.id, quest);
        // -- end quest
    }
}