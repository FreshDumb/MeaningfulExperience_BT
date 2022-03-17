using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveFormData_Container", menuName = "WaveFormData/WaveFormData_Container", order = 1)]
public class WaveFormContainerContainer_SO : ScriptableObject
{
    public WaveFormData_SO BoopData;
    public WaveFormData_SO BoopResponse;
}
