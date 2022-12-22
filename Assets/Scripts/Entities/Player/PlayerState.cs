using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : EntityState
{
    [SerializeField] bool run = false;
    [SerializeField] bool attack = false;
    [SerializeField] bool specialAttack = false;
    [SerializeField] Transform _target;
    [SerializeField] int coins;

    public bool Run { get => run; set => run = value; }
    public bool Attack { get => attack; set => attack = value; }
    public bool SpecialAttack { get => specialAttack; set => specialAttack = value; }
    public Transform Target { get => _target; set => _target = value; }
    public int Coins { get => coins; set => coins = value; }
}
