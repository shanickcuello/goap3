using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ItemType
{
    Invalid,
    Weapon,
    Health,
    Repair,
    Enemy,
    Goal
}
public class Item : MonoBehaviour
{
    public ItemType type;
}