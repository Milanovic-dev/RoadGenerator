using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NimiMobilePack;
[CustomEditor(typeof(NimiManager))]
public class NimiManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        NimiManager nm = target as NimiManager;

        if(!AssetDatabase.IsValidFolder("Assets/Shaders"))
        {
            if(GUILayout.Button("Create Project Folders"))
            {
                nm._CreateFolders();
            }
        }
        
    }
}
