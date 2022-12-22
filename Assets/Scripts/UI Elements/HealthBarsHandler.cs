using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarsHandler : MonoBehaviour
{
    private static HealthBarsHandler _instance;
    public static HealthBarsHandler Instance { get => _instance; }

    public Vector2 offset;
    public Slider healthbarPrefab;
    Camera _camera;

    Dictionary<Transform, HealthBarData> _entities = new Dictionary<Transform, HealthBarData>();
    Queue<Transform> _incomingEntities = new Queue<Transform>();
    Queue<HealthBarData> _incomingData = new Queue<HealthBarData>();
    Queue<Transform> _outgoingEntities = new Queue<Transform>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_incomingEntities.Count > 0 && _incomingData.Count > 0)
        {
            //IA2-P1
            var incoming = _incomingEntities.Zip(_incomingData, (x, y) => Tuple.Create(x, y));
            foreach (var item in incoming)
            {
                _entities[item.Item1] = item.Item2;
            }
            _incomingEntities.Clear();
            _incomingData.Clear();
        }
        if (_outgoingEntities.Count > 0)
        {
            for (int i = 0; i < _outgoingEntities.Count; i++)
            {
                var tr = _outgoingEntities.Dequeue();
                Destroy(_entities[tr].healthBar.gameObject);
                _entities.Remove(tr);
            }
        }

        foreach (var item in _entities)
        {
            if (item.Key != null)
            {
                var tr = item.Key;
                var hd = item.Value;
                var posInScreen = _camera.WorldToScreenPoint(tr.position);
                hd.sliderTransform.anchoredPosition = posInScreen;
                hd.sliderTransform.position = posInScreen + new Vector3(offset.x, offset.y);
                hd.healthBar.value = hd.lifeGetter();
            }
            else
            {
                Destroy(item.Value.healthBar);
                _entities.Remove(item.Key);
            }
        }
    }

    public void SubscribeHPListener(Transform transform, int min, int max, Func<int> lg)
    {
        var hb = Instantiate(healthbarPrefab, this.transform);
        hb.minValue = min;
        hb.maxValue = max;
        var tr = hb.GetComponent<RectTransform>();
        tr.anchoredPosition = _camera.WorldToScreenPoint(transform.position);
        tr.position = tr.anchoredPosition + offset;
        var hd = new HealthBarData
        {
            healthBar = hb,
            sliderTransform = tr,
            lifeGetter = lg
        };

        _incomingEntities.Enqueue(transform);
        _incomingData.Enqueue(hd);
    }

    public void UnsubscribeHPListener(Transform transform)
    {
        if (_entities.ContainsKey(transform))
        {
            _outgoingEntities.Enqueue(transform);
        }
    }
}

public struct HealthBarData
{
    public Func<int> lifeGetter;

    public Slider healthBar;
    public RectTransform sliderTransform;
}
