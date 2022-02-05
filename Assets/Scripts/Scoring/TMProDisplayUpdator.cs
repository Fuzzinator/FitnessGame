using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMProDisplayUpdator : MonoBehaviour
{
    [SerializeField]
    private string _prefix = string.Empty;

    [SerializeField]
    private string _suffix = string.Empty;
    
    [SerializeField]
    private TextMeshProUGUI _targetText;

    public void UpdateText(string newText)
    {
        _targetText.SetText($"{_prefix}{newText}{_suffix}");
    }
    public void UpdateText(ulong value)
    {
        _targetText.SetText($"{_prefix}{value}{_suffix}");
    }
    
    public void UpdateText(int value)
    {
        _targetText.SetText(value.TryGetCachedIntString());
    }

    public void UpdateText(System.Object willBeText)
    {
        _targetText.SetText($"{_prefix}{willBeText}{_suffix}");
    }
}
