using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(NPCShape), true)]
public class PlayerEditorTool : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUI.changed)
        {
            Selection.activeGameObject.GetComponent<SpriteRenderer>().color = ((ShapePawn)target).InitColor;
            serializedObject.Update();
        }
    }
}