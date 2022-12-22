using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Planner : MonoBehaviour
{
    private readonly List<Tuple<Vector3, Vector3>> _debugRayList = new List<Tuple<Vector3, Vector3>>();

    PlayerState _state;
    PlayerModel _model;
    PlayerControllerV2 _controller;
    Weapon _weapon;

    [Header("Enemies")]
    [SerializeField] EnemyModel yaku;
    [SerializeField] int yakuCoins;
    [SerializeField] EnemyModel parasite;
    [SerializeField] int parasiteCoins;
    [SerializeField] EnemyModel redYaku;
    [SerializeField] int redYakuCoins;
    [SerializeField] EnemyModel redParasite;
    [SerializeField] int redParasiteCoins;
    [SerializeField] EnemyModel boss;
    [SerializeField] int bossCoins;

    [Header("Experience")]
    [SerializeField] ExpCfg expCfgContainer;

    [Header("Interest points")]
    [SerializeField] Item buyStop;
    [SerializeField] Item repairStop;
    [SerializeField] Item healingStop;
    [SerializeField] Item goalStop;

    [Header("Others")]
    //Cambiar por evento o delegado para obtener valor directamente de las armas
    [SerializeField] int doubleAxeCost = 50;
    [SerializeField] int swordCost = 500;
    [SerializeField] private List<WeaponItem> weapons;


    private EnemySpawner[] _enemySpawners;
    private bool _goalReached;

    private void Awake()
    {
        _state = GetComponent<PlayerState>();
        _model = GetComponent<PlayerModel>();
        _controller = GetComponent<PlayerControllerV2>();
        _enemySpawners = FindObjectsOfType<EnemySpawner>();
        _weapon = GetComponentInChildren<Weapon>();
    }
    private void Start()
    {
        EventsHandler.SubscribeToEvent("EV_WIN", () => _goalReached = true);

        StartCoroutine(Plan());
    }
    public IEnumerator Plan()
    {
        yield return new WaitForSeconds(0.2f);

        var enemies = _enemySpawners.SelectMany(x => x.Enemies)
                                    .Select(x => x.GetComponent<Item>());

        List<Item> targets = new List<Item>() { buyStop, repairStop, healingStop, goalStop };
        var everything = enemies.Concat(targets);

        var actions = CreatePossibleActionsList();

        GoapState initial = new GoapState();
        initial.worldState = new WorldState()
        {
            valuesBool = new Dictionary<string, bool>(),
            valuesInt = new Dictionary<string, int>(),
            valuesFloat = new Dictionary<string, float>(),
            valuesString = new Dictionary<string, string>(),
        };
        initial.worldState.valuesBool["doorOpen"] = goalStop.GetComponent<Goal>().Unlocked;
        initial.worldState.valuesBool["success"] = _goalReached;
        initial.worldState.valuesBool["brokenWeapon"] = _weapon.Broken;
        initial.worldState.valuesInt["coins"] = _state.Coins;
        initial.worldState.valuesFloat["health"] = _model.Hp;
        initial.worldState.valuesString["currentWeapon"] = _weapon.CurrentWeapon;

        foreach (var item in initial.worldState.valuesBool)
            Debug.Log(item.Key + " ---> " + item.Value);
        foreach (var item in initial.worldState.valuesInt)
            Debug.Log(item.Key + " ---> " + item.Value);
        foreach (var item in initial.worldState.valuesFloat)
            Debug.Log(item.Key + " ---> " + item.Value);
        foreach (var item in initial.worldState.valuesString)
            Debug.Log(item.Key + " ---> " + item.Value);


        GoapState goal = new GoapState();
        goal.worldState.valuesBool["success"] = true;
        goal.worldState.valuesBool["doorOpen"] = true;

        string[] keys = new string[] { "success", "doorOpen" };

        Func<GoapState, float> heuristic = (curr) =>
        {
            int count = 0;
            foreach (var k in keys)
            {
                if (!curr.worldState.valuesBool.ContainsKey(k) || !curr.worldState.valuesBool[k])
                    count++;
            }
            return count;
        };

        Func<GoapState, bool> objective = (curr) =>
        {
            bool result = true;

            foreach (var k in keys)
            {
                result = result && curr.worldState.valuesBool.ContainsKey(k) && curr.worldState.valuesBool[k];
            }

            return result;
        };

        var actDict = new Dictionary<string, PlayerInputs>()
        {
            {"kill enemies", PlayerInputs.Kill },
            {"heal", PlayerInputs.Heal },
            {"repair weapon", PlayerInputs.Repair },
            {"buy double axe", PlayerInputs.BuyWeapon },
            {"buy sword", PlayerInputs.BuyWeapon },
            {"go to goal", PlayerInputs.GotoGoal }
        };

        var plan = Goap.Execute(initial, goal, objective, heuristic, actions);

        if (plan == null)
            Debug.Log("Couldn't plan");
        else
        {
            _controller.ExecutePlan(
                plan.Select(a =>
                {
                    Item i2 = everything.FirstOrDefault(i => i.type == a.item);
                    if (actDict.ContainsKey(a.Name) && i2 != null)
                    {
                        return Tuple.Create(actDict[a.Name], i2);
                    }
                    else
                    {
                        return null;
                    }
                }).Where(a => a != null)
                .ToList()
            );
        }
    }
    
    private List<GoapAction> CreatePossibleActionsList()
    {
        return new List<GoapAction>()
        {
            //================ ENEMY KILLS ================
            new GoapAction("kill enemies")
            .SetCost(4)
            .SetItem(ItemType.Enemy)
            .Pre(gs =>
            {
                return gs.worldState.valuesBool.ContainsKey("brokenWeapon") &&
                       gs.worldState.valuesBool.ContainsKey("doorOpen") &&
                      !gs.worldState.valuesBool["brokenWeapon"] &&
                      !gs.worldState.valuesBool["doorOpen"];
            })
            .Effect(gs =>
            {
                gs.worldState.valuesBool["doorOpen"] = true;
                return gs;
            }),

            //================ BUY WEAPONS ================

            new GoapAction("buy double axe")
            .SetCost(0)
            .SetItem(ItemType.Weapon)
            .Pre(gs =>
            {
                return gs.worldState.valuesString.ContainsKey("currentWeapon") &&
                       gs.worldState.valuesInt.ContainsKey("coins") &&
                       gs.worldState.valuesString["currentWeapon"] == "axe" && 
                       gs.worldState.valuesInt["coins"] >= weapons[0].cost;
            })
            .Effect(gs =>
            {
                gs.worldState.valuesString["currentWeapon"] = weapons[0].name;
                gs.worldState.valuesInt["coins"] -= weapons[0].cost;
                return gs;
            }),

            new GoapAction("buy sword")
            .SetCost(0)
            .SetItem(ItemType.Weapon)
            .Pre(gs =>
            {
                return gs.worldState.valuesString.ContainsKey("currentWeapon") &&
                       gs.worldState.valuesInt.ContainsKey("coins") &&
                       gs.worldState.valuesString["currentWeapon"] == weapons[0].name &&
                       gs.worldState.valuesInt["coins"] >= weapons[1].cost;
            })
            .Effect(gs =>
            {
                gs.worldState.valuesString["currentWeapon"] = weapons[1].name;
                gs.worldState.valuesInt["coins"] -= weapons[1].cost;
                return gs;
            }),

            //================ SUPPLIERS ================

            new GoapAction("heal")
            .SetCost(0)
            .SetItem(ItemType.Health)
            .Pre(gs =>
            {
                return gs.worldState.valuesFloat.ContainsKey("health") &&
                       gs.worldState.valuesFloat["health"] <= _model.maxHp * (_model.CritHealthPerc / 100);
            })
            .Effect(gs =>
            {
                gs.worldState.valuesFloat["health"] = _model.maxHp;
                return gs;
            }),

            new GoapAction("repair weapon")
            .SetCost(1)
            .SetItem(ItemType.Repair)
            .Pre(gs =>
            {
                return gs.worldState.valuesBool.ContainsKey("brokenWeapon") &&
                       gs.worldState.valuesBool["brokenWeapon"];
            })
            .Effect(gs =>
            {
                gs.worldState.valuesBool["brokenWeapon"] = false;
                return gs;
            }),

            new GoapAction("go to goal")
            .SetCost(3)
            .SetItem(ItemType.Goal)
            .Pre(gs =>
            {
                return gs.worldState.valuesBool.ContainsKey("doorOpen") &&
                       gs.worldState.valuesBool["doorOpen"];
            })
            .Effect(gs =>
            {
                gs.worldState.valuesBool["success"] = true;
                return gs;
            }),
        };
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var t in _debugRayList)
        {
            Gizmos.DrawRay(t.Item1, (t.Item2 - t.Item1).normalized);
            Gizmos.DrawCube(t.Item2 + Vector3.up, Vector3.one * 0.2f);
        }
    }
}
