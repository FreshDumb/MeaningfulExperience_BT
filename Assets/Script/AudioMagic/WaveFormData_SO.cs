using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveFormData_SO", menuName = "WaveFormData/WaveFormData_SO", order = 1)]
public class WaveFormData_SO : ScriptableObject
{
    public Texture2DArray textureArray;
    public AudioClip AudioClip;
    public float MaterialReso = 0.125f;
}