using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public void SetSimulationSpeed(float speed)
    {
        Time.timeScale = speed;
    }
}