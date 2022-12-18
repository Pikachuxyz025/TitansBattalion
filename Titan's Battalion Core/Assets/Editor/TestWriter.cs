using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MyScript))]
public class TestWriter : Editor
{
    MyScript myScrip;
    string filePath = "Assets/";
    string fileName = "TestMyEnum";

    private void OnEnable()
    {
        myScrip = (MyScript)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        filePath = EditorGUILayout.TextField("Path", filePath);
        fileName = EditorGUILayout.TextField("Name", fileName);
        if (GUILayout.Button("Save"))
        {
            EditorMethods.WriteToEnum(filePath, fileName, myScrip.days);
        }
    }
}
