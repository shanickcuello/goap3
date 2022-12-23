using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
[CreateAssetMenu(fileName = "WeaponItem", menuName = "WeaponItem")]
public class WeaponItem : ScriptableObject
{
    public string name;
    public WeaponType weapon;
    public int cost;
    public float damageMult;
    public GameObject weaponProjector;
    private void OnValidate()
    {
        var nameSeparated = name.Split(' ');
        name = string.Empty;
        for (var i = 0; i < nameSeparated.Length; i++)
        {
            if (i > 0)
                nameSeparated[i] = string.Format("{0}{1}",
                    char.ToUpper(nameSeparated[i][0]), nameSeparated[i].Remove(0, 1));
            name += nameSeparated[i];
        }
    }
}