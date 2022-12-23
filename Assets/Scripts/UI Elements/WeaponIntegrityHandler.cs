using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WeaponIntegrityHandler : MonoBehaviour
{
    public Slider _weaponBar;
    public void UpdateIntegrity(float amount)
    {
        _weaponBar.value = amount;
    }
}