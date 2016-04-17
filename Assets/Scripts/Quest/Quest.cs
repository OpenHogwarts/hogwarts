using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Quest
{
    public int id;
    public int assigner;
    public string name;
    public string pre;
    public string after;
    public List<Task> tasks = new List<Task>();

    private bool _isCompleted = false;
    public bool isCompleted
    {
        get
        {
            return _isCompleted;
        }
        set
        {
            _isCompleted = value;

            if (_isCompleted)
            {
                // UI effects (tachar la quest)
            }
        }
    }
    public List<ItemData> loot;
}