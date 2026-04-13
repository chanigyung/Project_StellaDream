using UnityEngine;

public abstract class WeaponControlData : ScriptableObject
{
    public abstract WeaponControlType ControlType { get; }
}
