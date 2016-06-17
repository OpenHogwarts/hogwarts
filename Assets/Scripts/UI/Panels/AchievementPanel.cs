using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

public class AchievementPanel : MonoBehaviour
{
    public static AchievementPanel Instance;
    public Texture2D BackgroundTexture;
    public bool visible;

    private AchievementManager achievementManager = new AchievementManager();
    private int iTabSelected = 0; //Actual tab selected


    private Rect backgroundRect = new Rect(Screen.width / 3, Screen.height / 8, 400, 600);



    private GUIStyle mainStyle = new GUIStyle();
    private GUIStyle fontStyle = new GUIStyle();

    // Use this for initialization
    public void Start()
    {
        mainStyle.fontSize = 20;
        fontStyle.fontSize = 10;

        visible = false;
    }

    private void OnGUI()
    {
        if (visible)
        {
            GUI.DrawTexture(backgroundRect, BackgroundTexture);
            GUILayout.BeginArea(backgroundRect);
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        int categoriesCount = 0;
                        foreach (string categorie in Enum.GetNames(typeof(AchievementCategories)))
                        {
                            if (GUILayout.Toggle(iTabSelected == -1, categorie, EditorStyles.toolbarButton))
                            {
                                iTabSelected = categoriesCount;
                            }
                            categoriesCount++;
                        }
                    }
                    GUILayout.EndHorizontal();
                    DoGUI(iTabSelected);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();

        }
    }

    private void DoGUI(int iTabSelected)
    {
        GUI.Label(new Rect(20, 20, 100, 100), "Logros: "+((AchievementCategories)iTabSelected).ToString(), mainStyle);

        List<Achievement> achievementList = achievementManager.AchievementsListForCategorie((AchievementCategories)iTabSelected);

        int achCount=1;
        foreach (Achievement ach in achievementList)
        {
            GUI.Label(new Rect(20, 50*achCount, 100, 100), ach.name, fontStyle);
            GUI.Label(new Rect(20, 15 + 50*achCount, 100, 100), ach.description, fontStyle);
            if(!ach.isCompleted)
                GUI.Label(new Rect(300, 50 * achCount, 100, 100), ach.actualCompleted+"/"+ach.totalCompleted, fontStyle);
            GUI.Box(new Rect(360, 50 * achCount, 20, 20), getCompletedTexture(ach.isCompleted,20,20));

            achCount++;
        }

    }

    private Texture2D getCompletedTexture(bool completed, int width, int height)
    {
        Texture2D rgb_texture = new Texture2D(width, height);
        Color color;

        if (completed)
            color = Color.green;
        else
            color = Color.red;

        for (int y = 0; y < rgb_texture.height; y++)
        {
            for (int x = 0; x < rgb_texture.width; x++)
            {
                rgb_texture.SetPixel(x, y, color);
            }
        }

        return rgb_texture;

    }



}
