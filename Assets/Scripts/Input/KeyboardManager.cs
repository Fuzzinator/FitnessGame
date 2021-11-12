using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VRKB;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager Instance { get; private set; }

    [SerializeField]
    private MainMenuUIController _uiController;
    
    [SerializeField]
    private KeyboardBehaviour _keyboard;

    [SerializeField]
    private bool _useMallets = false;
    
    [SerializeField]
    private MalletBehaviour _leftMallet;
    
    [SerializeField]
    private MalletBehaviour _rightMallet;
    
    [SerializeField]
    private GameObject[] _disableOnEnabled;
    
    private TMP_InputField _targetText;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void ActivateKeyboard(TMP_InputField ugui, string defaultText)
    {
        _targetText = ugui;
        _keyboard.PlaceholderText = defaultText;
        SetObjectsActive(true);
    }

    public void UpdateTargetText(KeyBehaviour key, Collider col, bool autoRepeat)
    {
        
    }

    public void ConfirmPressed()
    {
        _targetText.text = _keyboard.Text;
        DisableKeyboard();
    }

    public void CancelPressed()
    {
        DisableKeyboard();
    }
    
    private void DisableKeyboard()
    {
        _targetText = null;
        _keyboard.ClearText();
        SetObjectsActive(false);
    }
    
    private void SetObjectsActive(bool on)
    {
        _keyboard.gameObject.SetActive(on);
        if (_useMallets)
        {
            _leftMallet.gameObject.SetActive(on);
            _rightMallet.gameObject.SetActive(on);
            foreach (var obj in _disableOnEnabled)
            {
                if (obj == null)
                {
                    continue;
                }
                obj.SetActive(!on);
            }
        }

        if (on)
        {
            _uiController.RequestDisableUI(this);
        }
        else
        {
            _uiController.RequestEnableUI(this);
        }
    }
}
