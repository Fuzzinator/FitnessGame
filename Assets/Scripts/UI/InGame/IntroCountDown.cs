using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IntroCountDown : BaseGameStateListener
{
   [SerializeField]
   private Animator _animator;
   [SerializeField]
   private Canvas _canvas;
   [SerializeField]
   private string _startCountDown = "Start CountDown";
   private int _startCode;

   private int delay;

   [SerializeField]
   private UnityEvent<int> _delayChanged = new UnityEvent<int>();

   private void Start()
   {
      _startCode = Animator.StringToHash(_startCountDown);
   }
   
   public void StartCountDown(int delay)
   {
      _animator.enabled = true;
      _canvas.enabled = true;
      this.delay = delay;
      _delayChanged?.Invoke(this.delay);
      _animator.SetInteger(_startCode, delay);
   }

   private void CountDown()
   {
      delay--;
      _delayChanged?.Invoke(delay);
      if (delay <= 0)
      {
         CountDownFinished();
      }
      
   }

   public void CountDownFinished()
   {
      _animator.enabled = false;
      _canvas.enabled = false;
   }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        if(oldState == GameState.Playing && newState == GameState.Paused || newState == GameState.Unfocused)
        {
            CountDownFinished();
        }
        if(oldState == GameState.Paused && newState == GameState.PreparingToPlay)
        {
            StartCountDown(3);
        }
    }
}
