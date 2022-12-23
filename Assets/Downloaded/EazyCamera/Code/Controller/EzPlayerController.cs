using UnityEngine;
using System.Collections;
public class EzPlayerController : MonoBehaviour
{
    [SerializeField] private EzCamera m_camera = null;
    [SerializeField] private EzMotor m_controlledPlayer = null;
    private void Start()
    {
        SetUpControlledPlayer();
        SetUpCamera();
    }
    private void Update()
    {
        if (m_controlledPlayer != null && m_camera != null) HandleInput();
    }
    private void SetUpControlledPlayer()
    {
        if (m_controlledPlayer == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) m_controlledPlayer = playerObj.GetComponent<EzMotor>();
        }
    }
    private void SetUpCamera()
    {
        if (m_camera == null)
        {
            m_camera = Camera.main.GetComponent<EzCamera>();
            if (m_camera == null) m_camera = Camera.main.gameObject.AddComponent<EzCamera>();
        }
    }
    private void HandleInput()
    {
        var horz = Input.GetAxis(ExtensionMethods.HORIZONTAL);
        var vert = Input.GetAxis(ExtensionMethods.VERITCAL);
        var moveVector = m_camera.ConvertMoveInputToCameraSpace(horz, vert);
        m_controlledPlayer.MovePlayer(moveVector.x, moveVector.z, Input.GetKey(KeyCode.LeftShift));
    }
}