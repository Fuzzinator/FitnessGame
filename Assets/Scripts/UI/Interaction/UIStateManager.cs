using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateManager : MonoBehaviour
{
    public static UIStateManager Instance { get; private set; }

    private List<Canvas> _activeUI = new ();

    private UIInteractionRegister _leftHand;
    private UIInteractionRegister _rightHand;

    public static bool InteractionEnabled { get; private set; }

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

    public void RegisterController(UIInteractionRegister hand, bool isRightHand)
    {
        if (isRightHand)
        {
            if (_rightHand != hand)
            {
                DeRegisterController(_rightHand, true);
            }

            _rightHand = hand;
        }
        else
        {
            if (_leftHand != hand)
            {
                DeRegisterController(_leftHand, false);
            }

            _leftHand = hand;
        }

        hand.SetInteractionState(InteractionEnabled);
    }

    public void DeRegisterController(UIInteractionRegister hand, bool rightHand)
    {
        if (rightHand && _rightHand == hand)
        {
            if (_rightHand == null)
            {
                return;
            }

            hand.SetInteractionState(false);
            _rightHand = null;
        }
        else if (_leftHand == hand)
        {
            if (_leftHand == null)
            {
                return;
            }

            hand.SetInteractionState(false);
            _leftHand = null;
        }
    }

    public void RequestEnableInteraction(Canvas canvas)
    {
        if (_activeUI.Contains(canvas))
        {
            return;
        }
        else
        {
            _activeUI.Add(canvas);
            SetInteractionState(true);
        }
    }

    public void RequestDisableInteraction(Canvas canvas)
    {
        if (_activeUI.Contains(canvas))
        {
            _activeUI.Remove(canvas);
        }

        if (_activeUI.Count == 0)
        {
            SetInteractionState(false);
        }
    }

    private void SetInteractionState(bool on)
    {
        if (_leftHand != null)
        {
            _leftHand.SetInteractionState(on);
        }

        if (_rightHand != null)
        {
            _rightHand.SetInteractionState(on);
        }

        InteractionEnabled = on;
    }
}