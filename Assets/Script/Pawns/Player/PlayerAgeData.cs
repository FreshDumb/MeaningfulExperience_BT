using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAgeData", menuName = "PlayerData/PlayerAgeData", order = 1)]
public class PlayerAgeData : ScriptableObject
{
    [Header("COLOR")]
    public int SelfMixValue = 4;

    [Header("LIGHTS")]
    public int MaxBackLight = 1;
    public int MaxCenterLight = 2;

    [Header("GROWTH")]
    [Range(1, float.MaxValue)]
    public float GrowTime = 1;
    public float GrowScale = 1f;

    [Header("CAMERAGROWTH")]
    [Range(1, float.MaxValue)]
    public float CamGrowTime = 1;
    public float CamSize = 1;

    [Header("MOVEMENT")]
    [Range(1, float.MaxValue)]
    public float LearnToMoveTime = 1;
    public float MovementSpeed = 1;

}