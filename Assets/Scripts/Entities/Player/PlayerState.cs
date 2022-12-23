using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerState : EntityState
{
    [SerializeField] private bool run = false;
    [SerializeField] private bool attack = false;
    [SerializeField] private bool specialAttack = false;
    [SerializeField] private Transform _target;
    [SerializeField] private int coins;
    public bool Run
    {
        get => run;
        set => run = value;
    }
    public bool Attack
    {
        get => attack;
        set => attack = value;
    }
    public bool SpecialAttack
    {
        get => specialAttack;
        set => specialAttack = value;
    }
    public Transform Target
    {
        get => _target;
        set => _target = value;
    }
    public int Coins
    {
        get => coins;
        set => coins = value;
    }
}