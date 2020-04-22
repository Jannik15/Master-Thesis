using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventObjectType", menuName = "Event Object Type", order = 8)]
[System.Serializable]
public class EventObjectType : ScriptableObject
{
    public enum ThisType
    {
        PressurePlate,
        WinCondition,
        ShootTarget
    }
    public ThisType thisEventType;
}
