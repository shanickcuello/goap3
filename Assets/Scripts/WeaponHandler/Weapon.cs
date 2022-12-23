using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Weapon : MonoBehaviour
{
    public WeaponIntegrityHandler _handler;
    [SerializeField] private float _integrity;
    [SerializeField] private bool _broken;
    [SerializeField] private WeaponType _currentWeapon;
    private Dictionary<string, WeaponType> _weapons = new Dictionary<string, WeaponType>();
    [SerializeField] private GameObject axe;
    [SerializeField] private GameObject doubleAxe;
    [SerializeField] private GameObject sword;
    private float _damageMult;
    public bool Broken => _broken;
    public float Integrity => _integrity;
    public string CurrentWeapon => _currentWeapon.ToString();
    public float DamageMult => _damageMult;
    private void Awake()
    {
        _weapons.Add("axe", WeaponType.axe);
        _weapons.Add("doubleAxe", WeaponType.doubleAxe);
        _weapons.Add("sword", WeaponType.sword);
        ChangeWeapon(CurrentWeapon, 1f);
        _handler.UpdateIntegrity(_integrity);
        if (_integrity <= 0)
        {
            _broken = true;
            EventsHandler.TriggerEvent("EV_BROKENWEAPON");
        }
    }
    public void Hit(int amount)
    {
        _integrity -= amount;
        if (_integrity <= 0)
        {
            _integrity = 0;
            _broken = true;
            EventsHandler.TriggerEvent("EV_BROKENWEAPON");
        }
        _handler.UpdateIntegrity(_integrity);
    }
    public void FixWeapon()
    {
        _integrity = 100;
        _broken = false;
        _handler.UpdateIntegrity(_integrity);
        EventsHandler.TriggerEvent("EV_FIXEDWEAPON");
    }
    public void ChangeWeapon(string weapon, float damage)
    {
        _currentWeapon = _weapons[weapon];
        _damageMult = damage;
        switch (weapon)
        {
            case "axe":
                axe.SetActive(true);
                doubleAxe.SetActive(false);
                sword.SetActive(false);
                break;
            case "doubleAxe":
                axe.SetActive(false);
                doubleAxe.SetActive(true);
                sword.SetActive(false);
                break;
            case "sword":
                axe.SetActive(false);
                doubleAxe.SetActive(false);
                sword.SetActive(true);
                break;
        }
    }
    public void ChangeWeapon(WeaponType weapon, float damage)
    {
        ChangeWeapon(weapon.ToString(), damage);
    }
}
public enum WeaponType
{
    axe,
    doubleAxe,
    sword
}