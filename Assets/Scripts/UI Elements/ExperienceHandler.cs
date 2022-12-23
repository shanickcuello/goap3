using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class ExperienceHandler : MonoBehaviour
{
    public Slider _expBar;
    public Text _expText;
    [Header("NO TOCAR")] [SerializeField] private int _exp;
    [SerializeField] private int _maxHP;
    [SerializeField] private int _currentLevel;
    [SerializeField] private int _startLevelExp;
    [SerializeField] private int _nextLevelTarget;
    [SerializeField] private float _damageBuff;
    [SerializeField] private List<ExpLevelSettings> _expSettings;
    private const string EXP_FORMAT = "Lvl {3}\n{0} / {1} - {2}%";
    private readonly string _expSettingsInResources = "Experience/Exp Settings";
    public int Exp => _exp;
    public int MaxHP => _maxHP;
    public float DamageBuff => _damageBuff;
    public Action<ExpLevelSettings> updateExpSettings;
    private void Awake()
    {
        _expSettings = Resources.Load<ExpCfg>(_expSettingsInResources).expLevelSettings
            .Aggregate(new List<ExpLevelSettings>(), (x, y) =>
            {
                y.level = x.Count + 1;
                x.Add(y);
                return x;
            });
        UpdateNewValues(0);
    }
    public void UpdateExperience(int exp)
    {
        _exp = exp;
        if (_exp >= _nextLevelTarget && _currentLevel + 1 < _expSettings.Count)
        {
            _currentLevel++;
            UpdateNewValues(_currentLevel);
        }
        else
        {
            UpdateUI();
        }
    }
    private void UpdateNewValues(int level)
    {
        _maxHP = _expSettings[level].maxHP;
        _damageBuff = _expSettings[level].damageBuff;
        if (level + 1 < _expSettings.Count)
        {
            _startLevelExp = _expSettings[level].startAmount;
            _nextLevelTarget = _expSettings[level + 1].startAmount;
        }
        updateExpSettings?.Invoke(_expSettings[level]);
        UpdateUI();
    }
    private void UpdateUI()
    {
        _expBar.minValue = _startLevelExp;
        _expBar.maxValue = _nextLevelTarget;
        _expBar.value = _exp;
        _expText.text = string.Format(EXP_FORMAT, _exp, _nextLevelTarget,
            Mathf.InverseLerp(_startLevelExp, _nextLevelTarget, _exp) * 100, _currentLevel + 1);
    }
}