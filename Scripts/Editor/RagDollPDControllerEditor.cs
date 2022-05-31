
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(RagDollPDController))]
public class RagDollPDControllerEditor : Editor

{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var c = (RagDollPDController)target;
        if (GUILayout.Button("Activate"))
        {

            c.Activate();
        }
        if (GUILayout.Button("Deactivate"))
        {

            c.Deactivate();
        }
    }
}
