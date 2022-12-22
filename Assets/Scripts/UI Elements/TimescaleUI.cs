using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimescaleUI : MonoBehaviour
{
    [SerializeField] Text _text;

    public void UpdateText(float val)
    {
        _text.text = string.Format("Timescale {0}", val);
    }
}
