using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Task
{
    protected string TABLE_NAME = "tasks";

    public enum ActionType
    {
        Kill = 1,
        Talk = 2,
        GetItem = 3,
        RemoveItem = 4,
        Visit = 5
    }

    public enum ActorType
    {
        NPC = 1,
        Place = 2,
        Item = 3
    }

    public enum IdType
    {
        Id = 1,
        Template = 2,
    }

    public int id;
    public int quest;
    public int taskId; // iBoxDB forces to have a unique id
    public string _phrase;
    public string phrase {
        get {
            if (_phrase == null || _phrase == "") {
                return buildPhrase();
            }
            return _phrase;
        }
        set {
            _phrase = value;
        }
    }

    public int _idType = 2;
    public IdType idType
    {
        get
        {
            return (IdType)_idType;
        }
        set
        {
            _idType = (int)value;
        }
    }

    public int _type;
    public ActorType type
    {
        get
        {
            return (ActorType)_type;
        }
        set
        {
            _type = (int)value;
        }
    }

    public int _action;
    public ActionType action
    {
        get
        {
            return (ActionType)_action;
        }
        set
        {
            _action = (int)value;
        }
    }

    public int quantity = 0;
    public int currentQuantity = 0;
    public bool isCompleted = false;
    public TaskUI ui;

    public string buildPhrase()
    {
        string phrase = "";

        switch (action) {
            case ActionType.Kill:
                phrase += LanguageManager.get("TASK_KILL");
                break;
            case ActionType.Visit:
                phrase += LanguageManager.get("TASK_VISIT");
                break;
            case ActionType.Talk:
                phrase += LanguageManager.get("TASK_TALK");
                break;
            case ActionType.GetItem:
                phrase += LanguageManager.get("TASK_GET_ITEM");
                break;
            default:
                break;
        }
        phrase += " ";

        switch (type) {
            case ActorType.NPC:
                if (idType == IdType.Template) {
                    NPCTemplate data = NPCTemplate.get(id);
                    phrase += data.name;
                } else {
                    NPCData data = NPC.get(id);
                    phrase += data.name;
                }
                break;
            case ActorType.Item:
                ItemData item = Item.get(id);
                phrase += item.name;
                break;
        }
        phrase += " ";

        if (quantity > 1) {
            phrase += currentQuantity + "/" + quantity;
        }
        return phrase;
    }

    public void save()
    {
        // rebuild pharse before saving (as quantity may have changed)
        _phrase = buildPhrase();
        Service.db.Update(TABLE_NAME, this);
    }

    public bool create()
    {
        return Service.db.Insert(TABLE_NAME, this);
    }
}