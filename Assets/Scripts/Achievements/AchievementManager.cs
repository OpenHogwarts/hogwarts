using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AchievementManager
{
    protected string TABLE_NAME = "achievements";

    public List<Achievement> achievementsList = new List<Achievement>();

    public AchievementManager()
    {
        CreateAchievements();
    }

    public void CreateAchievements()
    {
        Achievement achievement;
        //ACHIEVEMENT 0
        achievement = new Achievement(0,"Bienvenido",0,AchievementCategories.Miscellany);
        achievement.description = "Te has conectado al juego por primera vez";
        achievementsList.Add(achievement);

        //END ACHIEVEMENT 0  

        //ACHIEVEMENT 1
        achievement = new Achievement(1, "MataArañas", 10, AchievementCategories.Kills);
        achievement.description = "Mata a 10 arañas";
        achievementsList.Add(achievement);


        /*foreach (Achievement ach in achievementsList)
            ach.create();*/
    }

    public List<Achievement> AchievementsListForCategorie(AchievementCategories categorie)
    {
        List<Achievement> achievementsListResult = new List<Achievement>();

        foreach(Achievement ach in achievementsList)
        {
            if (ach.myCategory == categorie)
                achievementsListResult.Add(ach);
        }

        return achievementsListResult;
    }

    /*
    public void saveAchievements()
    {
        foreach (Achievement ach in achievementsList)
            ach.save();
    }

    public void loadAchievements()
    {
        foreach (Achievement ach in achievementsList)
            ach.load();
    }*/

}
