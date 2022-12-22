using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShop : MonoBehaviour
{
    [SerializeField] int index = 0;
    [SerializeField] List<WeaponItem> weapons;

    public bool WeaponInStock { get; private set; }
    public WeaponItem CurrentWeapon { get => weapons[index]; }

    public Action<int> onWeaponBuy;

    private void Start()
    {
        WeaponInStock = index < weapons.Count;

        if(WeaponInStock)
            weapons[index].weaponProjector.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8 && other.TryGetComponent<PlayerModel>(out var model))
        {
            if(index < weapons.Count && model.GetComponent<PlayerState>().Coins >= weapons[index].cost)
            {
                model.GetComponentInChildren<Weapon>().ChangeWeapon(weapons[index].weapon, weapons[index].damageMult);
                model.GetComponentInChildren<Weapon>().FixWeapon();
                model.GetComponentInChildren<AttackArea>().WeaponDmgMult = weapons[index].damageMult;
                model.GetComponent<PlayerControllerV2>().WeaponBought(weapons[index].weapon);
                onWeaponBuy?.Invoke(weapons[index].cost);
                index++;
                ShowNextWeapon(index);
            }
        }
    }

    private void ShowNextWeapon(int i)
    {
        if (!WeaponInStock) return;

        weapons[i - 1].weaponProjector.SetActive(false);

        if (i < weapons.Count)
            weapons[i].weaponProjector.SetActive(true);
        else
            WeaponInStock = false;
    }
}