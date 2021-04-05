using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PickupType
{
    Health,
    Ammo,
    Key
}
public class Pickup : MonoBehaviour
{
    [SerializeField] protected float value; //Amount to restore
    [SerializeField] protected PickupType type; //Stat to restore

    public float Value
    {
        get { return value; }
    }

    public PickupType Type
    {
        get { return type; }
    }
}
