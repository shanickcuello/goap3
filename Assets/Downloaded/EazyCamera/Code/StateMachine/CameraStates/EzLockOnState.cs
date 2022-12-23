using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class EzLockOnState : EzCameraState
{
    public enum LockOnStyle
    {
        TOGGLE,
        HOLD
    }
    public enum TargetSwitchStyle
    {
        CYCLE,
        NEAREST
    }
    public LockOnStyle m_lockOnStyle = LockOnStyle.HOLD;
    public TargetSwitchStyle m_switchStyle = TargetSwitchStyle.NEAREST;
    private EzLockOnTarget m_currentTarget = null;
    private List<EzLockOnTarget> m_nearbyTargets = null;
    private bool m_isActive = false;
    [SerializeField] private float m_snapAngle = 2.5f;
    public EzLockOnState(EzCamera camera, EzCameraSettings settings)
        : base(camera, settings)
    {
        m_nearbyTargets = new List<EzLockOnTarget>();
    }
    public override void EnterState()
    {
    }
    public override void UpdateState()
    {
    }
    public override void ExitState()
    {
        if (m_currentTarget != null)
        {
            m_currentTarget.SetIconActive(false);
            m_currentTarget = null;
        }
    }
    public override void LateUpdateState()
    {
        LockOnTarget();
        m_controlledCamera.UpdatePosition();
    }
    public override void UpdateStateFixed()
    {
    }
    public override void HandleInput()
    {
        if (!m_controlledCamera.LockOnEnabled) return;
        if (m_nearbyTargets.Count == 0) return;
        if (m_isActive)
        {
            if (m_switchStyle == TargetSwitchStyle.NEAREST)
            {
                if (Input.GetKeyDown(KeyCode.E))
                    MoveToNextTarget(m_cameraTransform.right);
                else if (Input.GetKeyDown(KeyCode.Q)) MoveToNextTarget(-m_cameraTransform.right);
            }
            else if (m_switchStyle == TargetSwitchStyle.CYCLE)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q)) CycleTargets();
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!m_controlledCamera.IsLockedOn) m_controlledCamera.SetState(State.LOCKON);
            if (m_lockOnStyle == LockOnStyle.TOGGLE)
                m_isActive = !m_isActive;
            else
                m_isActive = true;
            if (m_isActive)
                SetInitialTarget();
            else
                m_controlledCamera.SetState(m_controlledCamera.DefaultState);
        }
        else if (Input.GetKeyUp(KeyCode.Space) && m_lockOnStyle == LockOnStyle.HOLD)
        {
            m_isActive = false;
            m_controlledCamera.SetState(m_controlledCamera.DefaultState);
        }
    }
    private void LockOnTarget()
    {
        if (m_currentTarget != null)
        {
            var step = Time.deltaTime * m_stateSettings.RotateSpeed;
            var relativePos = m_currentTarget.transform.position - m_cameraTransform.position;
            if (Vector3.Angle(m_cameraTransform.forward, relativePos) > m_snapAngle)
            {
                var nextRot = Quaternion.Lerp(m_cameraTransform.rotation, Quaternion.LookRotation(relativePos), step);
                m_cameraTransform.rotation = nextRot;
            }
            else
            {
                m_cameraTransform.rotation = Quaternion.LookRotation(relativePos);
            }
        }
    }
    private void SetInitialTarget()
    {
        m_currentTarget = m_nearbyTargets[0];
        m_currentTarget.SetIconActive();
    }
    public void MoveToNextTarget(Vector3 direction)
    {
        if (m_nearbyTargets.Count <= 1) return;
        if (m_nearbyTargets.Count == 2)
        {
            m_currentTarget.SetIconActive(false);
            m_currentTarget = m_currentTarget == m_nearbyTargets[0] ? m_nearbyTargets[1] : m_nearbyTargets[0];
            m_currentTarget.SetIconActive(true);
            return;
        }
        var nearestTarget = m_currentTarget;
        EzLockOnTarget nextTarget = null;
        var relativeDirection = direction;
        var currentNearestDistance = float.MaxValue;
        var sqDstance = float.MaxValue;
        for (var i = 0; i < m_nearbyTargets.Count; ++i)
        {
            nextTarget = m_nearbyTargets[i];
            if (nextTarget == m_currentTarget) continue;
            relativeDirection = nextTarget.transform.position - m_cameraTransform.position;
            if (Vector3.Dot(relativeDirection, direction) > 0)
            {
                sqDstance = (m_currentTarget.transform.position - nextTarget.transform.position).sqrMagnitude;
                if (sqDstance < currentNearestDistance)
                {
                    nearestTarget = nextTarget;
                    currentNearestDistance = sqDstance;
                }
            }
        }
        m_currentTarget.SetIconActive(false);
        m_currentTarget = nearestTarget;
        m_currentTarget.SetIconActive(true);
    }
    private void CycleTargets()
    {
        if (m_nearbyTargets.Count <= 1) return;
        if (m_nearbyTargets.Count == 2)
        {
            m_currentTarget = m_currentTarget == m_nearbyTargets[0] ? m_nearbyTargets[1] : m_nearbyTargets[0];
            return;
        }
        m_currentTarget.SetIconActive(false);
        m_currentTarget = m_nearbyTargets[CycleIndex(m_nearbyTargets.IndexOf(m_currentTarget), m_nearbyTargets.Count)];
        m_currentTarget.SetIconActive(true);
    }
    private int CycleIndex(int startIndex, int numElements)
    {
        return (numElements + startIndex + 1) % numElements;
    }
    public void AddTarget(EzLockOnTarget newTarget)
    {
        if (!m_nearbyTargets.Contains(newTarget)) m_nearbyTargets.Add(newTarget);
    }
    public void RemoveTarget(EzLockOnTarget targetToRemove)
    {
        if (m_nearbyTargets.Contains(targetToRemove))
        {
            m_nearbyTargets.Remove(targetToRemove);
            if (m_currentTarget == targetToRemove)
            {
                m_currentTarget.SetIconActive(false);
                m_currentTarget = null;
                if (m_nearbyTargets.Count > 0)
                {
                    m_currentTarget = m_nearbyTargets[0];
                    m_currentTarget.SetIconActive(true);
                }
                else
                {
                    m_isActive = false;
                    m_controlledCamera.SetState(m_controlledCamera.DefaultState);
                }
            }
        }
    }
}