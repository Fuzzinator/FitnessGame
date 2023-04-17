using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace YUR.Interaction.Gaze
{
    public class YURGazeTargetElement : MonoBehaviour
    {

        [SerializeField] private UnityEvent OnGazeEnterEvent;

        [SerializeField] private UnityEvent OnGazeExitEvent;

        [SerializeField] private UnityEvent OnCompleteEvent;

        public event Action OnGazeEnterAction;

        public event Action OnGazeExitAction;

        public event Action OnCompleteAction;

        private bool _completed;

        public void Enter()
        {
            OnGazeEnterEvent?.Invoke();
            OnGazeEnterAction?.Invoke();

            _completed = false;
        }

        public void Exit()
        {
            OnGazeExitEvent?.Invoke();
            OnGazeExitAction?.Invoke();

            _completed = false;
        }

        public void Complete()
        {
            if (_completed)
                return;

            OnCompleteEvent?.Invoke();
            OnCompleteAction?.Invoke();

            _completed = true;
        }
    }
}