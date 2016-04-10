using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

/*
Base http://forum.unity3d.com/threads/editor-scripting-how-to-add-objects-in-editor-on-click.42097/
 */

[CustomEditor(typeof(WaypointContainer))]
public class WaypointEditor : Editor
{
	
	private static bool m_editMode = false;
	private static int m_count = 0;
	private GameObject m_container;

	private string nothing = "";
	public static string coords = "";
    public static Vector3 npcPos;

	void OnSceneGUI()
	{
		if (m_editMode)
		{
			// on middle mouse (wheel button) click: create a waypoint
			if (Event.current.type == EventType.MouseUp && Event.current.button == 2)
			{
				Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				RaycastHit hitInfo;
				
				if (Physics.Raycast(worldRay, out hitInfo)) {
					createWaypoint(hitInfo.point);
				}
				Event.current.Use();
			}
		}
	}
	override public void OnInspectorGUI()
	{
		// Do not enable the editor ingame
        if (Application.isPlaying) {
            return;
        }
            
        NPC npc = Selection.activeGameObject.GetComponent<NPC>();
		npc.Start(); // we load npcData
        npcPos = npc.transform.position;

        if (m_editMode)
		{
			GUILayout.Label("To add a Waypoint: wheel/middle click in the desired position.\n To change the order, edit Waypoint's name.\n");
			if (GUILayout.Button("Stop Editing"))
			{
				WaypointData wp;
				m_editMode = false;
				checkContainer();

				// destroy old waypoints
				foreach (WaypointData waypoint in Service.db.Select<WaypointData>("FROM " + WaypointData.TABLE_NAME + " WHERE npc==?", npc.Id)) {
					waypoint.delete();
				}

				// save new waypoints
				foreach (Transform child in m_container.transform)
				{
					wp = new WaypointData();
					wp.npc = npc.Id;
					wp.position = npcPos - child.transform.position; // save the relative pos
					wp.create();
				}
				DestroyImmediate(m_container);
				m_count = 0;


				// copy waypoints to clipboard
				TextEditor te = new TextEditor();
				te.content = new GUIContent(coords);
				te.SelectAll();
				te.Copy();
			}
		}
		else
		{
			if (GUILayout.Button("Start Editing"))
			{
				m_editMode = true;
				checkContainer();
				resetList();

                // show existing waypoints
                foreach (WaypointData waypoint in npc.waypoints) {
					createWaypoint(npcPos + waypoint.position);
				}
			}
			if (coords.Length > 0) {
				GUILayout.Label("\nCurrent Waypoints:");
				nothing = GUILayout.TextArea(coords);
			}
		}
		
		if (GUILayout.Button("Reset"))
		{
			m_count = 0;
			nothing = "";
			coords = "";
			checkContainer();
			DestroyImmediate(m_container);
		}
		
	}

	void checkContainer () {
		m_container = GameObject.Find("tmpWaypointContainer");
		
		// create the temporary container
		if (m_container == null) {
			m_container = new GameObject();
			m_container.name = "tmpWaypointContainer";
			m_container.hideFlags = HideFlags.DontSave;
		}
	}

	public static void AssignLabel(GameObject g)
	{
		Texture2D tex = EditorGUIUtility.IconContent("sv_label_0").image as Texture2D;
		Type editorGUIUtilityType  = typeof(EditorGUIUtility);
		BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
		object[] args = new object[] {g, tex};
		editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
	}

	private void resetList () {
		coords = "waypoints = new List<Vector3>();\n";
	}

	private void addToList (Vector3 point) {
		if (coords.Length == 0) {
			resetList();
		}
        point = npcPos - point; // get the relative pos

        coords += "waypoints.Add(new Vector3(" + point.x.ToString("0.00") + "f," + point.y.ToString("0.00") + "f," + point.z.ToString("0.00") + "f));\n";
	}

	
	void createWaypoint (Vector3 position)
	{
		checkContainer ();
		GameObject waypoint = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Waypoint.prefab", typeof(GameObject)) as GameObject;
		GameObject waypointInstance = Instantiate(waypoint) as GameObject;
		AssignLabel(waypointInstance);
		waypointInstance.hideFlags = HideFlags.DontSave;
		waypointInstance.transform.position = position;
		waypointInstance.name = m_count.ToString("00");
		waypointInstance.transform.parent = m_container.transform;

		EditorUtility.SetDirty(waypointInstance);
		
		m_count++;

		addToList(position);
	}
}
