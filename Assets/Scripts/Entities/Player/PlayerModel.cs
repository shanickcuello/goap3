using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]

[RequireComponent(typeof(Rigidbody))]
public class PlayerModel : GridEntity, IHitable
{
    PlayerView _view;
    Rigidbody _rb;
    PlayerState _state;
    AttackArea _attackArea;
    Weapon _weapon;
    WeaponShop _weaponShop;

    public float speed;
    public HealthbarController _hpBar;
    public float maxHp;
    [SerializeField] float _hp;
    public ExperienceHandler _expHandler;
    public int expLevel;
    private int _xp;
    public int meleeDamage;
    public float damageBuff = 1;
    public CoinsUIHandler coinsUIHandler;
    private int _coins;

    [SerializeField] [Range(0, 100)] float critHealthPerc = 10;

    public float Hp { get => _hp; }
    public int Xp { get => _xp; }
    public float CritHealthPerc { get => critHealthPerc; }

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
        _view = GetComponent<PlayerView>();
        _state = GetComponent<PlayerState>();
        _attackArea = GetComponentInChildren<AttackArea>();
        _weapon = GetComponentInChildren<Weapon>();
        _weaponShop = FindObjectOfType<WeaponShop>();
    }

    protected override void Start()
    {
        base.Start();

        _hp = maxHp;
        _hpBar.ChangeMaxLimit(maxHp);
        UpdateHPBar();

        _expHandler.updateExpSettings += x =>
        {
            expLevel = x.level;
            maxHp = _hp = x.maxHP;
            damageBuff = x.damageBuff;
            _hpBar.ChangeMaxLimit(maxHp);
            UpdateHPBar();
        };

        _attackArea.onEnemyDead = (x,c) =>
        {
            _xp += x;
            _coins += c;
            _expHandler.UpdateExperience(_xp);
            coinsUIHandler.UpdateCoins(_coins);
            _state.Target = null;
            _state.Coins = _coins;
        };

        _weaponShop.onWeaponBuy = c =>
        {
            _coins -= c;
            coinsUIHandler.UpdateCoins(_coins);
            _state.Coins = _coins;
        };
    }

    private void UpdateHPBar()
    {
        _hpBar.UpdateHP(_hp);
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

    public void Attack(int attackType)
    {
        if(attackType >= 0 && attackType < 5)
        {
            _view.SetAnimAttackType(attackType);
            _view.TriggerLightAttack();
        }
        else if(attackType >= 5)
            SpecialAttack(attackType - 5);
    }
    public void SpecialAttack(int specialAttackType)
    {
        _view.SetAnimSpecialAttackType(specialAttackType);
        _view.TriggerSpecialAttack();
    }

    public void TriggerHit(int type)
    {
        _attackArea.TriggerHit((int)(meleeDamage * damageBuff), type);
    }

    public bool GetHit(int damage, GameObject source, out int expReward, out int coinsReward)
    {
        _hp -= damage;
        UpdateHPBar();
        expReward = 0;
        coinsReward = 0;

        if (_hp <= 0)
        {
            _view.TriggerDie();
            _state.die = true;
            return true;
        }
        else if(HealthCritical())
            GetComponent<PlayerControllerV2>().CriticalHealth();

        return false;
    }

    public void Heal()
    {
        _hp = maxHp;
        UpdateHPBar();
    }

    public void Hit(int amount) => _weapon.Hit(amount);

    private bool HealthCritical() => _hp < maxHp * (critHealthPerc / 100);
}
