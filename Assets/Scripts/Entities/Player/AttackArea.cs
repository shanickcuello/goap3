using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class AttackArea : MonoBehaviour
{
    public float hitTime = .25f;
    public LayerMask targetLayer;
    public Queries[] queries;
    private int _dmg;
    private float[] _currTime;
    private int _layerValue;
    private bool _brokenWeapon;
    private Weapon _weapon;
    private float _weaponDmgMult = 1f;
    public Action<int, int> onEnemyDead;
    public float WeaponDmgMult
    {
        get => _weaponDmgMult;
        set => _weaponDmgMult = value;
    }
    private void Awake()
    {
        _weapon = transform.parent.GetComponentInChildren<Weapon>();
        queries = GetComponentsInChildren<Queries>();
        _currTime = new float[queries.Length];
    }
    private void Start()
    {
        EventsHandler.SubscribeToEvent("EV_BROKENWEAPON", () => _brokenWeapon = true);
        EventsHandler.SubscribeToEvent("EV_FIXEDWEAPON", () => _brokenWeapon = false);
        _layerValue = (int)(Mathf.Log(targetLayer.value) / Mathf.Log(2));
    }
    public void TriggerHit(int dmg)
    {
        TriggerHit(dmg, 0);
    }
    public void TriggerHit(int dmg, int attackType)
    {
        if (_brokenWeapon) return;
        attackType = attackType < queries.Length ? attackType : 0;
        _dmg = (int)(dmg * _weaponDmgMult);
        foreach (var query in queries[attackType].Query()
                     .Where(x => !x.GetComponent<EntityState>().die &&
                                 x.gameObject.layer == _layerValue))
            if (query.TryGetComponent<IHitable>(out var opponent))
            {
                var killed = opponent.GetHit(_dmg, transform.parent.gameObject, out var exp, out var coins);
                if (killed)
                    onEnemyDead?.Invoke(exp, coins);
            }
        _currTime[attackType] = hitTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == _layerValue && other.TryGetComponent<IHitable>(out var opponent) &&
            !_brokenWeapon)
        {
            var killed = opponent.GetHit(_dmg, transform.parent.gameObject, out var exp, out var coins);
            if (killed)
                onEnemyDead?.Invoke(exp, coins);
        }
    }
}