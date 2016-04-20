using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Quest
{
    public int id;
    public int assigner;
    public string name;
    public string pre;
    public string after;
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
    public List<ItemData> loot;
}