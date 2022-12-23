using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class View : MonoBehaviour
{
    protected Animator _animator;
    [Header("Animator Parameters")] [SerializeField]
    protected string floatSpeedAnim;
    [SerializeField] protected string intLightAttack;
    [SerializeField] protected string intSpecialAttack;
    [SerializeField] protected string triggerLightAttack;
    [SerializeField] protected string triggerSpecialAttack;
    [SerializeField] protected string triggerDie;
    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    public void SetAnimSpeed(float speed)
    {
        _animator.SetFloat(floatSpeedAnim, speed);
    }
    public void SetAnimAttackType(int attack)
    {
        _animator.SetInteger(intLightAttack, attack);
    }
    public void SetAnimSpecialAttackType(int specialAttack)
    {
        _animator.SetInteger(intSpecialAttack, specialAttack);
    }
    public void TriggerLightAttack()
    {
        _animator.SetTrigger(triggerLightAttack);
    }
    public void TriggerSpecialAttack()
    {
        _animator.SetTrigger(triggerSpecialAttack);
    }
    public void TriggerDie()
    {
        _animator.SetTrigger(triggerDie);
    }
}