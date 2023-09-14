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

    private TMP_InputField _targetInputText;
    private TextMeshProUGUI _targetText;
    public Status status { get; private set; }

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
    private void Start()
    {
        status = Status.Disabled;
    }

    public KeyboardBehaviour ActivateKeyboard(TMP_InputField ugui, string defaultText)
    {
        _targetInputText = ugui;
        _targetText = null;
        _keyboard.PlaceholderText = defaultText;
        status = Status.Visible;
        SetObjectsActive(true);
        UIStateManager.Instance?.RequestEnableInteraction(_keyboard.MyCanvas);
        return _keyboard;
    }
    public KeyboardBehaviour ActivateKeyboard(TextMeshProUGUI ugui, string defaultText)
    {
        _targetInputText = null;
        _targetText = ugui;
        _keyboard.PlaceholderText = defaultText;
        status = Status.Visible;
        SetObjectsActive(true);
        UIStateManager.Instance?.RequestEnableInteraction(_keyboard.MyCanvas);
        return _keyboard;
    }

    public void UpdateTargetText(KeyBehaviour key, Collider col, bool autoRepeat)
    {

    }

    public void ConfirmPressed()
    {
        if (_targetInputText != null)
        {
            _targetInputText.text = _keyboard.Text;
        }
        else if (_targetText != null)
        {
            _targetText.SetTextZeroAlloc(_keyboard.Text, true);
        }
        status = Status.Done;
        DisableKeyboard();
    }

    public void CancelPressed()
    {
        status = Status.Canceled;
        DisableKeyboard();
    }

    private void DisableKeyboard()
    {
        _targetInputText = null;
        _targetText = null;
        _keyboard.ClearText();
        SetObjectsActive(false);
        UIStateManager.Instance?.RequestDisableInteraction(_keyboard.MyCanvas);
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

    public enum Status
    {
        //
        // Summary:
        //     The on-screen keyboard is visible.
        Visible,
        //
        // Summary:
        //     The user has finished providing input.
        Done,
        //
        // Summary:
        //     The on-screen keyboard was canceled.
        Canceled,
        //
        // Summary:
        //     The on-screen keyboard is disabled.
        Disabled
    }
}
