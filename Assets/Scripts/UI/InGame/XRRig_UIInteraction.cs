using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRRig_UIInteraction : BaseGameStateListener
{
    [SerializeField]
    private bool _rightHanded = true;
    [SerializeField]
    private XRRayInteractor _rightInteractor;
    [SerializeField]
    private XRRayInteractor _leftInteractor;

    [SerializeField]
    private LineRenderer _rightLineRenderer;
    [SerializeField]
    private LineRenderer _leftLineRenderer;

    [SerializeField]
    private XRInteractorLineVisual _rightLineVisual;
    [SerializeField]
    private XRInteractorLineVisual _leftLineVisual;
    
    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Paused:
            case GameState.Unfocused:
                //SetInteractionState(true);
                break;
            case GameState.Playing:
            case GameState.InMainMenu:
            case GameState.PreparingToPlay:
                SetInteractionState(false);
                break;
        }
    }

    private void SetInteractionState(bool enabled)
    {
        _rightInteractor.enabled = enabled && _rightHanded;
        _leftInteractor.enabled = enabled && !_rightHanded;
        _rightLineRenderer.enabled = enabled && _rightHanded;
        _leftLineRenderer.enabled = enabled && !_rightHanded;
        _rightLineVisual.enabled = enabled && _rightHanded;
        _leftLineVisual.enabled = enabled && !_rightHanded;
    }
}