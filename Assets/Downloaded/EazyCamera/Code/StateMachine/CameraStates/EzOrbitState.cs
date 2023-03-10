using UnityEngine;
using System.Collections;
using System;
public class EzOrbitState : EzCameraState
{
    private float m_rotY = 0;
    private float m_rotX = 0;
    private float m_horizontalInput = 0;
    private float m_verticalInput = 0;
    private Quaternion m_destRot = Quaternion.identity;
    public EzOrbitState(EzCamera camera, EzCameraSettings settings)
        : base(camera, settings)
    {
    }
    public override void EnterState()
    {
    }
    public override void ExitState()
    {
    }
    public override void UpdateState()
    {
        if (m_controlledCamera.OribtEnabled) HandleInput();
    }
    public override void LateUpdateState()
    {
        OrbitCamera(m_horizontalInput, m_verticalInput);
    }
    public override void UpdateStateFixed()
    {
    }
    private void OrbitCamera(float horz, float vert)
    {
        var step = Time.deltaTime * m_stateSettings.RotateSpeed;
        m_rotY += horz * step;
        m_rotX += vert * step;
        m_rotX = Mathf.Clamp(m_rotX, m_stateSettings.MinRotX, m_stateSettings.MaxRotX);
        var addRot = Quaternion.Euler(0f, m_rotY, 0f);
        m_destRot = addRot * Quaternion.Euler(m_rotX, 0f, 0f);
        m_cameraTransform.rotation = m_destRot;
#if UNITY_EDITOR
        Debug.DrawLine(m_cameraTransform.position, m_cameraTarget.transform.position, Color.red);
        Debug.DrawRay(m_cameraTransform.position, m_cameraTransform.forward, Color.blue);
#endif
        m_controlledCamera.UpdatePosition();
    }
    public override void HandleInput()
    {
        if (m_controlledCamera.OribtEnabled && Input.GetMouseButtonDown(0))
        {
            m_controlledCamera.SetState(State.ORBIT);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_controlledCamera.SetState(State.FOLLOW);
            return;
        }
        var horz = Input.GetAxis(MOUSEX);
        var vert = Input.GetAxis(MOUSEY);
        m_horizontalInput = horz;
        m_verticalInput = vert;
    }
}