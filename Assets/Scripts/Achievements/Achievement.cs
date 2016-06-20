using UnityEngine;
using System.Collections;

public class Achievement {

    protected string TABLE_NAME = "achievements";

    public int id; //Id of achievement in DB
    public string name; //Name of achievement
    public string description; //Description of achievement

    public int actualCompleted = 0; //Actual elements for this achievement
    public int totalCompleted; //Total elements needed for complete this achievement

    public AchievementCategories myCategory;

    public bool isCompleted
    {
        get
        {
            if (actualCompleted >= totalCompleted)
                return true;
            else
                return false;
        }
    }

    public Achievement(int id, string name, int totalCompleted, AchievementCategories category)
    {
        this.id = id;
        this.name = name;
        this.totalCompleted = totalCompleted;
        this.myCategory = category;
    }

    /*
    public void save()
    {
        Service.db.Update(TABLE_NAME, this);
    }

    public void load()
    {
        Service.db.Select(TABLE_NAME, this);
    }

    public bool create()
    {
        return Service.db.Insert(TABLE_NAME, this);
    }*/

}
