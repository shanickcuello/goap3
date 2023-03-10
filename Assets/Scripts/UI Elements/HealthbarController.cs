using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthbarController : MonoBehaviour
{
    public Slider _healthBar;
    public Text _healthText;
    private const string HEALTH_FORMAT = "{0} / {1}";
    private float _maxHP, _hp;
    public void ChangeMaxLimit(float maxHP)
    {
        _healthBar.maxValue = _maxHP = maxHP;
        UpdateText();
    }
    public void UpdateHP(float hp)
    {
        _healthBar.value = _hp = hp;
        UpdateText();
    }
    private void UpdateText()
    {
        _healthText.text = string.Format(HEALTH_FORMAT, _hp, _maxHP);
    }
}