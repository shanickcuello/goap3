using UnityEngine;
using System.Collections;
public class EzAnimator : MonoBehaviour
{
    [SerializeField] private EzMotor m_controlledCharacter = null;
    private Animator m_animator = null;
    private int m_speedHash = -1;
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_speedHash = Animator.StringToHash("Speed");
    }
    private void Start()
    {
        if (m_controlledCharacter == null) m_controlledCharacter = transform.root.GetComponentInChildren<EzMotor>();
    }
    private void Update()
    {
        m_animator.SetFloat(m_speedHash, m_controlledCharacter.GetNormalizedSpeed());
    }
}