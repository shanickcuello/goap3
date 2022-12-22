using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Vector3 size;
    public Color cubeColor;

    public GameObject door;
    public Vector3 finalPos;
    public float doorOpeningSpeed;

    public GameObject winText;

    public bool _unlocked;
    private bool _opened;

    public bool Unlocked { get => _unlocked; set => _unlocked = value; }

    private void Update()
    {
        if (_unlocked && !_opened)
        {
            MoveDoor();
        }
    }

    private void MoveDoor()
    {
        var dir = (finalPos - door.transform.position).normalized;
        door.transform.position += dir * doorOpeningSpeed * Time.deltaTime;

        if(door.transform.position == finalPos)
        {
            _opened = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 8)
        {
            winText.SetActive(true);
            EventsHandler.TriggerEvent("EV_WIN");
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 cubePos = transform.position + new Vector3(0, size.y / 2, 0);
        Gizmos.color = cubeColor;
        Gizmos.DrawCube(cubePos, size);
    }
}
