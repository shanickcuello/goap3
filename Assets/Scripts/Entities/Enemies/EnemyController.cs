using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System;
public class EnemyController : MonoBehaviour
{
    [Header("Obstacle avoidance")] public float obstacleDistance;
    public float avoidWeight;
    public LayerMask avoidLayer;
    [Header("Attack Settings")] [Tooltip("Distancia de detencion respecto al objetivo")]
    public float maximumAttackDistance = 0.5f;
    [Header("NO TOCAR")] public string currentState = "";
    private EventFSM<EnemyInputs> _myFsm;
    public enum EnemyInputs
    {
        IDLE,
        MOVE,
        ATTACK,
        SPECIAL_ATTACK,
        DIE
    }
    private EnemyModel _model;
    private EnemyState _state;
    private AgentTheta _agentTheta;
    private void Awake()
    {
        _model = GetComponent<EnemyModel>();
        _state = GetComponent<EnemyState>();
        _agentTheta = GetComponent<AgentTheta>();
    }
    private void OnEnable()
    {
        SetupFSM();
    }
    private void SetupFSM()
    {
        var idle = new State<EnemyInputs>("idle");
        var moving = new State<EnemyInputs>("moving");
        var attacking = new State<EnemyInputs>("attacking");
        var die = new State<EnemyInputs>("die");
        StateConfigurer.Create(idle)
            .SetTransition(EnemyInputs.MOVE, moving)
            .SetTransition(EnemyInputs.ATTACK, attacking)
            .SetTransition(EnemyInputs.DIE, die)
            .Done();
        StateConfigurer.Create(moving)
            .SetTransition(EnemyInputs.IDLE, idle)
            .SetTransition(EnemyInputs.DIE, die)
            .Done();
        StateConfigurer.Create(attacking)
            .SetTransition(EnemyInputs.IDLE, idle)
            .SetTransition(EnemyInputs.DIE, die)
            .Done();
        StateConfigurer.Create(die).Done();
        idle.OnUpdate += () =>
        {
            if (_state.die)
            {
                SendInputToFSM(EnemyInputs.DIE);
            }
            else if (_state.attack)
            {
                if (_state.target != null)
                    SendInputToFSM(EnemyInputs.ATTACK);
                else
                    _state.attack = false;
            }
            else if (_state.alerted)
            {
                SendInputToFSM(EnemyInputs.MOVE);
            }
        };
        moving.OnUpdate += () =>
        {
            var distance = (_state.target.position - transform.position).magnitude;
            if (_state.target != null && distance < maximumAttackDistance)
            {
                _state.attack = true;
                SendInputToFSM(EnemyInputs.IDLE);
            }
        };
        attacking.OnEnter += x =>
        {
            _model.LookTo(_state.target.position);
            _model.Attack();
        };
        attacking.OnUpdate += () =>
        {
            if (_state.die)
            {
                _state.attack = false;
                _state.alerted = false;
                SendInputToFSM(EnemyInputs.DIE);
            }
        };
        attacking.OnExit += x =>
        {
            if (_state.target != null &&
                (_state.target.position - transform.position).magnitude > maximumAttackDistance) _state.attack = false;
        };
        die.OnEnter += x => { Invoke("Disable", 5); };
        _myFsm = new EventFSM<EnemyInputs>(idle);
    }
    private void Update()
    {
        _myFsm.Update();
    }
    private void SendInputToFSM(EnemyInputs inp)
    {
        _myFsm.Feed(inp);
    }
    private void Disable()
    {
        gameObject.SetActive(false);
    }
    public void ReturnToIdle()
    {
        SendInputToFSM(EnemyInputs.IDLE);
    }
}