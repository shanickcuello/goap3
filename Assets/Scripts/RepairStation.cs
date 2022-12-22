using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairStation : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 8 && other.TryGetComponent<PlayerModel>(out var model))
        {
            model.GetComponentInChildren<Weapon>().FixWeapon();
        }
    }
}
