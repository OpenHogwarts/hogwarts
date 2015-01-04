using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ExitGames.Client.Photon;
using UnityEngine;
using System.Collections;
using UnityEditor;
using Debug = UnityEngine.Debug;



[CustomEditor(typeof(ServerSettings))]
public class ServerSettingsInspector : Editor
{
    public override void OnInspectorGUI()
    {
        ServerSettings settings = (ServerSettings)this.target;

        #if UNITY_3_5
        EditorGUIUtility.LookLikeInspector();
        #endif


        settings.HostType = (ServerSettings.HostingOption)EditorGUILayout.EnumPopup("Hosting", settings.HostType);
        EditorGUI.indentLevel = 1;

        switch (settings.HostType)
        {
            case ServerSettings.HostingOption.BestRegion:
            case ServerSettings.HostingOption.PhotonCloud:
                if (settings.HostType == ServerSettings.HostingOption.PhotonCloud)
                    settings.PreferredRegion = (CloudRegionCode)EditorGUILayout.EnumPopup("Region", settings.PreferredRegion);
                settings.AppID = EditorGUILayout.TextField("AppId", settings.AppID);
                settings.Protocol = (ConnectionProtocol)EditorGUILayout.EnumPopup("Protocol", settings.Protocol);

                if (string.IsNullOrEmpty(settings.AppID) || settings.AppID.Equals("master"))
                {
                    EditorGUILayout.HelpBox("The Photon Cloud needs an AppId (GUID) set.\nYou can find it online in your Dashboard.", MessageType.Warning);
                }
                break;

            case ServerSettings.HostingOption.SelfHosted:
                bool hidePort = false;
                if (settings.Protocol == ConnectionProtocol.Udp && (settings.ServerPort == 4530 || settings.ServerPort == 0))
                {
                    settings.ServerPort = 5055;
                }
                else if (settings.Protocol == ConnectionProtocol.Tcp && (settings.ServerPort == 5055 || settings.ServerPort == 0))
                {
                    settings.ServerPort = 4530;
                }
                #if RHTTP
                if (settings.Protocol == ConnectionProtocol.RHttp)
                {
                    settings.ServerPort = 0;
                    hidePort = true;
                }
                #endif
                settings.ServerAddress = EditorGUILayout.TextField("Server Address", settings.ServerAddress);
                settings.ServerAddress = settings.ServerAddress.Trim();
                if (!hidePort)
                {
                    settings.ServerPort = EditorGUILayout.IntField("Server Port", settings.ServerPort);
                }
                settings.Protocol = (ConnectionProtocol)EditorGUILayout.EnumPopup("Protocol", settings.Protocol);
                settings.AppID = EditorGUILayout.TextField("AppId", settings.AppID);
                break;

            case ServerSettings.HostingOption.OfflineMode:
                EditorGUI.indentLevel = 0;
                EditorGUILayout.HelpBox("In 'Offline Mode', the client does not communicate with a server.\nAll settings are hidden currently.", MessageType.Info);
                break;

            case ServerSettings.HostingOption.NotSet:
                EditorGUI.indentLevel = 0;
                EditorGUILayout.HelpBox("Hosting is 'Not Set'.\nConnectUsingSettings() will not be able to connect.\nSelect another option or run the PUN Wizard.", MessageType.Info);
                break;

            default:
                DrawDefaultInspector();
                break;
        }

        if (PhotonEditor.CheckPunPlus())
        {
            settings.Protocol = ConnectionProtocol.Udp;
            EditorGUILayout.HelpBox("You seem to use PUN+.\nPUN+ only supports reliable UDP so the protocol is locked.", MessageType.Info);
        }

        settings.AppID = settings.AppID.Trim();

        EditorGUI.indentLevel = 0;
        SerializedObject sObj = new SerializedObject(this.target);
        SerializedProperty sRpcs = sObj.FindProperty("RpcList");
        EditorGUILayout.PropertyField(sRpcs, true);
        sObj.ApplyModifiedProperties();

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        if (GUILayout.Button("Refresh RPCs"))
        {
            PhotonEditor.UpdateRpcList();
            Repaint();
        }
        if (GUILayout.Button("Clear RPCs"))
        {
            PhotonEditor.ClearRpcList();
        }
        if (GUILayout.Button("Log HashCode"))
        {
            Debug.Log("RPC-List HashCode: " + RpcListHashCode() + ". Make sure clients that send each other RPCs have the same RPC-List.");
        }
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        //SerializedProperty sp = serializedObject.FindProperty("RpcList");
        //EditorGUILayout.PropertyField(sp, true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private int RpcListHashCode()
    {
        // this is a hashcode generated to (more) easily compare this Editor's RPC List with some other
        int hashCode = PhotonEditor.Current.RpcList.Count + 1;
        foreach (string s in PhotonEditor.Current.RpcList)
        {
            int h1 = s.GetHashCode();
            hashCode = ((h1 << 5) + h1) ^ hashCode;
        }

        return hashCode;
    }
}
