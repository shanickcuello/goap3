using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Exp Settings", menuName = "Exp Settings")]
public class ExpCfg : ScriptableObject
{
    public List<ExpLevelSettings> expLevelSettings;
}

[Serializable]
public struct ExpLevelSettings
{
    [HideInInspector] public int level;
    [Tooltip("Puntos de experiencia necesarios para llegar a este nivel de experiencia")]
    public int startAmount;
    [Tooltip("Salud máxima")]
    public int maxHP;
    [Tooltip("Porcentaje de mejora en daño")]
    public float damageBuff;
}