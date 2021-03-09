using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Quest
{
    public int id;
    public int assigner;
    public string name {
        get {
            return LanguageManager.get("QUEST_" + id + "_TITLE");
        }
    }
    public string pre {
        get {
            return LanguageManager.get("QUEST_" + id + "_PRE");
        }
    }
    public string after {
        get {
            return LanguageManager.get("QUEST_" + id + "_AFTER");
        }
    }
    public Dictionary<int, Task> tasks = new Dictionary<int, Task>();
    public Text ui;

    private bool _isCompleted = false;
    public bool isCompleted
    {
        get
        {
            if (!_isCompleted) {
                foreach (Task task in tasks.Values) {
                    if (!task.isCompleted) {
                        return false;
                    }
                }
                // UI effects (tachar la quest)
                _isCompleted = true;
            }
            return _isCompleted;
        }
    }
    public Dictionary<int, int> loot = new Dictionary<int, int>();
}