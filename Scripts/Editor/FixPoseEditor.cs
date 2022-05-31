using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(FixPose))]
public class FixPoseEditor : Editor

{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        FixPose c = (FixPose)target;
        if (GUILayout.Button("Orient Limbs"))
        {

            c.OrientLimbs();
        }
    }
}
