using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMProDisplayUpdator : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _targetText;

    public void UpdateText(string newText)
    {
        _targetText.SetText(newText);
    }
    public void UpdateText(ulong value)
    {
        _targetText.SetText(value.ToString());
    }
    
    public void UpdateText(int value)
    {
        _targetText.SetText(value.ToString());
    }

    public void UpdateText(System.Object willBeText)
    {
        _targetText.SetText(willBeText.ToString());
    }
}
