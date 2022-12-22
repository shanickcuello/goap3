using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System;
using System.Linq;

public class PlayerController : MonoBehaviour
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

    [Header("FSM")]
    private EventFSM<PlayerInputs> _myFsm;
    public enum PlayerInputs { IDLE, MOVE, ATTACK, SPECIAL_ATTACK, DIE }

    private PlayerModel _model;
    private PlayerState _state;
    private AgentTheta _agentTheta;
    private EnemySpawner[] _enemySpawners;
    private Goal _goal;

    private bool _goalReached = false;

    private Roulette<PlayerInputs> _roulette;
    private Dictionary<PlayerInputs, int> _rouletteActions;

    private void Awake()
    {
        _model = GetComponent<PlayerModel>();
        _state = GetComponent<PlayerState>();
        _agentTheta = GetComponent<AgentTheta>();
        _enemySpawners = FindObjectsOfType<EnemySpawner>();
        _goal = FindObjectOfType<Goal>();

        SetupFSM();
    }

    private void Start()
    {
        _roulette = new Roulette<PlayerInputs>();

        _rouletteActions = new Dictionary<PlayerInputs, int>();
        _rouletteActions.Add(PlayerInputs.ATTACK, lightAttackProbability);
        _rouletteActions.Add(PlayerInputs.SPECIAL_ATTACK, specialAttackProbability);

        EventsHandler.SubscribeToEvent("EV_WIN", () => _goalReached = true);
    }

    //IA2-P3
    private void SetupFSM()
    {
        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var idle = new State<PlayerInputs>("idle");
        var moving = new State<PlayerInputs>("moving");
        var attacking = new State<PlayerInputs>("attacking");
        var superAttacking = new State<PlayerInputs>("superAttacking");
        var die = new State<PlayerInputs>("die");

        //creo las transiciones
        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.MOVE, moving)
            .SetTransition(PlayerInputs.ATTACK, attacking)
            .SetTransition(PlayerInputs.SPECIAL_ATTACK, superAttacking)
            .SetTransition(PlayerInputs.DIE, die)
            .Done(); //aplico y asigno

        StateConfigurer.Create(moving)
            .SetTransition(PlayerInputs.IDLE, idle)
            .Done();

        StateConfigurer.Create(attacking)
            .SetTransition(PlayerInputs.IDLE, idle)
            .Done();

        StateConfigurer.Create(superAttacking)
            .SetTransition(PlayerInputs.IDLE, idle)
            .Done();

        //die no va a tener ninguna transición HACIA nada (uno puede morirse, pero no puede pasar de morirse a caminar)
        //entonces solo lo creo e inmediatamente lo aplico asi el diccionario de transiciones no es nulo y no se rompe nada.
        StateConfigurer.Create(die).Done();

        //PARTE 2: SETEO DE LOS ESTADOS
        idle.OnEnter += x =>
        {
            if (_goal.Unlocked && !_goalReached)
            {
                _state.Attack = _state.SpecialAttack = false;
                _state.Run = true;
                _state.Target = _goal.transform;
            }
        };

        idle.OnUpdate += () =>
        {
            if (_goalReached) return;

            if (_state.die)
                SendInputToFSM(PlayerInputs.DIE);
            else if (_state.Run)
                SendInputToFSM(PlayerInputs.MOVE);
            else if(_state.SpecialAttack)
                SendInputToFSM(PlayerInputs.SPECIAL_ATTACK);
            else if (_state.Attack)
            {
                if (_state.Target != null)
                {
                    if (_roulette.Execute(_rouletteActions) == PlayerInputs.ATTACK)
                        SendInputToFSM(PlayerInputs.ATTACK);
                    else
                        _state.SpecialAttack = true;
                }
                else
                    _state.Attack = false;
            }
            else
            {
                //IA2-P1
                //ACA VAN FUNCIONES LINQ PARA OBTENER EL ENEMIGO MAS CERCANO
                if (!_state.Attack)
                {

                    var enemyTarget = _enemySpawners.SelectMany(x => x.Enemies)
                                                    .Where(x => !x.GetComponent<EnemyState>().die)?
                                                    .Select(x => x.GetComponent<EnemyModel>())
                                                    .OrderBy(x => (x.transform.position - transform.position).sqrMagnitude)
                                                    .SkipWhile(x => x.expLevel > _model.expLevel)
                                                    .FirstOrDefault();

                    if (enemyTarget != null)
                    {
                        _state.Target = enemyTarget.transform;
                        _state.Run = true;
                        _state.Attack = true;
                    }
                }
            }
        };

        moving.OnEnter += x =>
        {
            SetupPathfinding(_state.Target);
        };

        moving.OnUpdate += () =>
        {
            Run();

            if (!_state.Run)
                SendInputToFSM(PlayerInputs.IDLE);
        };

        attacking.OnEnter += x =>
        {
            _model.LookTo(_state.Target.position);
            _model.Attack(UnityEngine.Random.Range(0, 5));
        };

        superAttacking.OnEnter += x =>
        {
            _model.LookTo(_state.Target.position);
            _model.SpecialAttack(UnityEngine.Random.Range(0, 3));
        };
        superAttacking.OnExit += x =>
        {
            _state.SpecialAttack = false;
        };

        _myFsm = new EventFSM<PlayerInputs>(idle);
    }

    private void Update()
    {
        _myFsm.Update();
    }

    #region FSM Methods
    private void SendInputToFSM(PlayerInputs inp)
    {
        _myFsm.Feed(inp);
    }
    #endregion

    #region Pathfinding Move
    private List<Node> _waypoints;
    private Vector3 _finalPos;
    private int _nextPoint;
    private ObstacleAvoidance _sb;
    private bool _lastConnection;


    public void Run()
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
    public void SetupPathfinding()
    {
        var wpNodes = _agentTheta.GetPathFinding(_model.transform.position, _state.Target.position);
        SetWayPoints(wpNodes, _state.Target.position);
    }
    public void SetupPathfinding(Transform target)
    {
        _state.Target = target;
        SetupPathfinding();
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
    public void ClickMove()
    {
        SendInputToFSM(PlayerInputs.MOVE);
    }
    public void ReturnToIdle()
    {
        SendInputToFSM(PlayerInputs.IDLE);
    }
    #endregion
}
