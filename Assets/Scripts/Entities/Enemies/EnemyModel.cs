using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
public class EnemyModel : GridEntity, IHitable
{
    public UnityEvent onDieEvents;
    public float speed;
    public int maxHp;
    private int _hp;
    public int meleeDamage;
    public int expReward;
    public int expLevel;
    public int coinsReward;
    private View _view;
    private Rigidbody _rb;
    private AttackArea _attackArea;
    private Collider _collider;
    private EnemyState _state;
    public Action<GameObject> onDie;
    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
        _view = GetComponent<View>();
        _state = GetComponent<EnemyState>();
        _collider = GetComponent<Collider>();
        _attackArea = GetComponentInChildren<AttackArea>();
    }
    protected override void Start()
    {
        base.Start();
        _hp = maxHp;
        _attackArea.onEnemyDead = (x, c) =>
        {
            _state.target = null;
            _state.alerted = false;
        };
        HealthBarsHandler.Instance.SubscribeHPListener(transform, 0, maxHp, () => _hp);
    }
    public override void Move(Vector3 dir)
    {
        dir.y = 0;
        var newSpeed = dir * speed;
        _rb.velocity = newSpeed;
        transform.forward = dir;
        _view.SetAnimSpeed(newSpeed.magnitude);
        base.Move(dir);
    }
    public void LookTo(Vector3 target)
    {
        var dir = target - transform.position;
        dir.y = 0;
        transform.forward = dir.normalized;
    }
    public void StopMoving()
    {
        _rb.velocity = Vector3.zero;
        _view.SetAnimSpeed(0);
    }
    public void Attack()
    {
        _view.SetAnimAttackType(0);
        _view.TriggerLightAttack();
    }
    public bool GetHit(int damage, GameObject source, out int expReward, out int coinsReward)
    {
        _hp -= damage;
        _state.target = source.transform;
        _state.alerted = true;
        if (_hp <= 0)
        {
            expReward = this.expReward;
            coinsReward = this.coinsReward;
            _collider.enabled = false;
            _view.TriggerDie();
            HealthBarsHandler.Instance.UnsubscribeHPListener(transform);
            _state.die = true;
            onDie?.Invoke(gameObject);
            onDieEvents?.Invoke();
            return true;
        }
        expReward = 0;
        coinsReward = 0;
        return false;
    }
    public void TriggerHit()
    {
        _attackArea.TriggerHit(meleeDamage);
    }
}