using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using IA2;

public enum PlayerInputs { NextStep, FailedStep, Kill, GotoGoal, Heal, Repair, BuyWeapon, Success, Idle }

public class PlayerControllerV2 : MonoBehaviour
{
    [Header("Pathfinding")]
    [Tooltip("Distancia requerida para que la entidad se dirija al siguiente waypoint.")] public float nextWaypointDistance;
    [Tooltip("Distancia de detencion respecto al objetivo")]
    [Header("Obstacle avoidance")]
    public float obstacleDistance;
    public float avoidWeight;
    public LayerMask avoidLayer;

    [Header("Attack Settings")]
    [Range(0, 100)] public int lightAttackProbability = 90;
    [Range(0, 100)] public int specialAttackProbability = 10;

    private PlayerModel _model;

    private PlayerState _state;
    private AgentTheta _agentTheta;
    private EnemySpawner[] _enemySpawners;
    private Goal _goal;
    private Planner _planner;
    private WeaponShop _weaponShop;
    private bool _weaponBought = false;

    private EventFSM<PlayerInputs> _fsm;

    IEnumerable<Tuple<PlayerInputs, Item>> _plan;
    private Item _target;
    private bool _goalReached;
    //private bool _firstPlan = false;
    [SerializeField] bool _weaponBroken = true;

    private Roulette<string> _roulette;
    private Dictionary<string, int> _rouletteActions;

    private void Awake()
    {
        _model = GetComponent<PlayerModel>();
        _state = GetComponent<PlayerState>();
        _agentTheta = GetComponent<AgentTheta>();
        _enemySpawners = FindObjectsOfType<EnemySpawner>();
        _goal = FindObjectOfType<Goal>();
        _planner = GetComponent<Planner>();
        _weaponShop = FindObjectOfType<WeaponShop>();

        SetupFSM();
    }
    private void Start()
    {
        EventsHandler.SubscribeToEvent("EV_WIN", () => _goalReached = true);
        EventsHandler.SubscribeToEvent("EV_BROKENWEAPON", () => _weaponBroken = true);
        EventsHandler.SubscribeToEvent("EV_FIXEDWEAPON", () => _weaponBroken = false);
    }
    private void Update()
    {
        _fsm.Update();
    }
    private void SetupFSM()
    {
        var any = new State<PlayerInputs>("any");

        var idle = new State<PlayerInputs>("idle");
        var planStep = new State<PlayerInputs>("planStep");
        var failStep = new State<PlayerInputs>("failStep");
        var kill = new State<PlayerInputs>("kill");
        var toGoal = new State<PlayerInputs>("toGoal");
        var heal = new State<PlayerInputs>("heal");
        var repair = new State<PlayerInputs>("repair");
        var buyWeapon = new State<PlayerInputs>("buyWeapon");
        var success = new State<PlayerInputs>("success");

        idle.OnEnter += a => _state.Run = true;

        idle.OnUpdate += () =>
        {
            if (_weaponBroken || WeaponAvailableToBuy())
                _fsm.Feed(PlayerInputs.FailedStep);
            if (_goal.Unlocked)
                _fsm.Feed(PlayerInputs.NextStep);
            else
                _fsm.Feed(PlayerInputs.Kill);
        };

        kill.OnEnter += a =>
        {
            if (_state.Target != null && _state.Target.TryGetComponent<EnemyState>(out var es) && !es.die) return;

            var enemyTarget = _enemySpawners.SelectMany(x => x.Enemies)
                                                    .Where(x => !x.GetComponent<EnemyState>().die)?
                                                    .Select(x => x.GetComponent<EnemyModel>())
                                                    .OrderByDescending(x => x.expLevel)
                                                    .ThenBy(x => (x.transform.position - transform.position).sqrMagnitude)
                                                    .SkipWhile(x => x.expLevel > _model.expLevel)
                                                    .FirstOrDefault();

            if (enemyTarget != null)
            {
                _state.Target = enemyTarget.transform;
                _state.Run = true;
                SetupPathfinding(_state.Target);
            }
            else
                _fsm.Feed(PlayerInputs.Idle);
        };

        kill.OnUpdate += () =>
        {
            if (_state.Run)
                Run();
            else
            {
                if (_state.Target != null && !_state.Attack)
                {
                    _state.Attack = true;
                    _model.LookTo(_state.Target.position);
                    _model.Attack(UnityEngine.Random.Range(0, 8));
                }
            }
        };

        kill.OnExit += a => _state.Attack = false;

        failStep.OnEnter += a =>
        {
            _model.StopMoving(); Debug.Log("Plan failed");

            if (!_state.die)
                _planner.StartCoroutine(_planner.Plan());
        };

        planStep.OnEnter += a =>
        {
            var step = _plan.FirstOrDefault();
            if (step != null)
            {
                _plan = _plan.Skip(1);
                var oldTarget = _target;
                _target = step.Item2;
                if (!_fsm.Feed(step.Item1))
                {
                    _target = oldTarget;
                }
            }
            else
            {
                _fsm.Feed(PlayerInputs.Success);
            }
        };

        repair.OnEnter += a =>
        {
            _state.Target = null;
            SetupPathfinding(_target.transform);
        };
        repair.OnUpdate += () =>
        {
            if (_weaponBroken)
                Run();
            else
                NextStep();
        };

        heal.OnEnter += a =>
        {
            _state.Target = null;
            SetupPathfinding(_target.transform);
        };
        heal.OnUpdate += () =>
        {
            if (HealthCritical())
                Run();
            else
                NextStep();
        };

        buyWeapon.OnEnter += a =>
        {
            _state.Target = null;
            SetupPathfinding(_target.transform);
        };
        buyWeapon.OnUpdate += () =>
        {
            if (!_weaponBought)
                Run();
            else
                NextStep();
        };
        buyWeapon.OnExit += a =>
        {
            _weaponBought = false;
        };


        toGoal.OnEnter += a =>
        {
            SetupPathfinding(_target.transform);
        };
        toGoal.OnUpdate += () =>
        {
            if (!_goalReached)
                Run();
            else
                NextStep();
        };

        success.OnEnter += a =>
        {
            Debug.Log("Success");
        };

        StateConfigurer.Create(any)
            .SetTransition(PlayerInputs.NextStep, planStep)
            .SetTransition(PlayerInputs.FailedStep, failStep)
            .Done();
        StateConfigurer.Create(planStep)
            .SetTransition(PlayerInputs.Kill, kill)
            .SetTransition(PlayerInputs.GotoGoal, toGoal)
            .SetTransition(PlayerInputs.Heal, heal)
            .SetTransition(PlayerInputs.Repair, repair)
            .SetTransition(PlayerInputs.BuyWeapon, buyWeapon)
            .SetTransition(PlayerInputs.Success, success)
            .Done();

        StateConfigurer.Create(kill)
            .SetTransition(PlayerInputs.Idle, idle)
            .Done();
        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.Kill, kill)
            .Done();

        _fsm = new EventFSM<PlayerInputs>(idle, any);
    }

    private bool HealthCritical() => _model.Hp < _model.maxHp * (_model.CritHealthPerc / 100);
    private bool WeaponAvailableToBuy()
    {
        return _weaponShop.WeaponInStock &&
               _weaponShop.CurrentWeapon.cost <= _state.Coins;
    }
    public void ExecutePlan(List<Tuple<PlayerInputs, Item>> plan)
    {
        _plan = plan;
        _fsm.Feed(PlayerInputs.NextStep);
    }

    #region FSM Methods
    private void SendInputToFSM(PlayerInputs inp)
    {
        _fsm.Feed(inp);
    }
    #endregion

    #region Pathfinding Move
    private List<Node> _waypoints;
    private Vector3 _finalPos;
    private int _nextPoint;
    private ObstacleAvoidance _sb;
    private bool _lastConnection;

    private void Run()
    {
        var point = _waypoints[_nextPoint];
        var posPoint = point.transform.position;
        posPoint.y = transform.position.y;

        bool stopNext = false;

        Vector3 dir;
        if (!_lastConnection)
            dir = posPoint - transform.position;
        else
        {
            dir = _finalPos - transform.position;
            stopNext = true;
        }

        if (dir.magnitude < nextWaypointDistance)
        {
            if (!_lastConnection)
            {
                if (_nextPoint + 1 < _waypoints.Count)
                {
                    _nextPoint++;
                    _sb = new ObstacleAvoidance(transform, _waypoints[_nextPoint].transform, obstacleDistance, avoidWeight, avoidLayer);
                }

                else if (_nextPoint + 1 >= _waypoints.Count)
                {
                    _lastConnection = true;
                    _sb = new ObstacleAvoidance(transform, _finalPos, obstacleDistance, avoidWeight, avoidLayer);
                }
            }
        }

        if (stopNext && dir.magnitude < nextWaypointDistance)
        {
            _state.Run = false;
            _model.StopMoving();
            return;
        }

        _model.Move(dir.normalized);
    }

    public void SetupPathfinding(Transform target)
    {
        var wpNodes = _agentTheta.GetPathFinding(_model.transform.position, target.position);
        SetWayPoints(wpNodes, target.position);
    }
    public void SetWayPoints(List<Node> newPoints, Vector3 finalPos)
    {
        if (newPoints.Count == 0) return;

        _waypoints = newPoints;
        _finalPos = finalPos;
        _nextPoint = 0;
        var pos = _waypoints[_nextPoint].transform.position;
        pos.y = transform.position.y;

        _lastConnection = false;
    }
    #endregion

    #region Event Methods
    public void NextStep()
    {
        SendInputToFSM(PlayerInputs.NextStep);
    }
    public void CriticalHealth()
    {
        SendInputToFSM(PlayerInputs.FailedStep);
    }
    
    public void WeaponBought(WeaponType weapon)
    {
        _weaponBought = true;
    }
    public void ReturnToIdle()
    {
        SendInputToFSM(PlayerInputs.Idle);
    }
    #endregion
}