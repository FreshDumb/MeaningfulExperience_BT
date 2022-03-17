using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveFormMagic))]
public class WaveFormMagicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaveFormMagic waveFormMagicRef = (WaveFormMagic)target;

        if(GUILayout.Button("Build Textures"))
        {
            waveFormMagicRef.BuildTextures();
        }
    }
}
