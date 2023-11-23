using System.Collections.Generic;
using UnityEngine;

public class SpellGUI : MonoBehaviour
{
    public bool open;
    public bool showFireSpells;
    public bool showFrostSpells;

    // So basic gui showing spells icons,infos,damages, etc... based on category.
    public List<Spell> spellList = new();

    private void Update()
    {
        if (InputSystemAgent.GetKeyDown("P"))
            open = !open;
    }

    // Update is called once per frame
    private void OnGUI()
    {
        if (open)
            GUI.Window(0, new Rect(200, 200, 400, 400), SpellWindow, "Spells");
    }

    public void SpellWindow(int windowID)
    {
        if (GUILayout.Button("Fire"))
        {
            showFireSpells = true;
            showFrostSpells = false;
        }

        if (GUILayout.Button("Frost"))
        {
            showFireSpells = false;
            showFrostSpells = true;
        }

        if (showFireSpells)
            for (var i = 0; i < spellList.Count; i++)
                if (spellList[i].spellCategory == Spell.SpellCategory.Fire)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(spellList[i].spellIcon, GUILayout.MaxWidth(48), GUILayout.MaxHeight(48));
                    GUILayout.Label(spellList[i].spellName + " - " + spellList[i].spellInfo);
                    GUILayout.Label(spellList[i].spellMinDamage + "," + spellList[i].spellMaxDamage);
                    GUILayout.EndHorizontal();
                }

        if (showFrostSpells)
            for (var i = 0; i < spellList.Count; i++)
                if (spellList[i].spellCategory == Spell.SpellCategory.Frost)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(spellList[i].spellIcon, GUILayout.MaxWidth(48), GUILayout.MaxHeight(48));
                    GUILayout.Label(spellList[i].spellName + " - " + spellList[i].spellInfo);
                    GUILayout.Label(spellList[i].spellMinDamage + "," + spellList[i].spellMaxDamage);
                    GUILayout.EndHorizontal();
                }
    }
}