using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using YUR.Core;
namespace YUR.Watch.Widgets
{
    public enum WidgetType
    {
        YURHeartRate, // Implemented
        YURLevel, // Implemented
        TodaySquatCount, // Implemented
        Clock, // Implemented
        TodaysCalories, // Implemented
        TodaysCaloriesAndTime, // Implemented
        TimePlayedToday, //TODO: Sample Implemented
        SquatsInWorkout,//TODO: Sample Implemented
        WorkoutTime //TODO: Sample Implemented
    }

    public class WidgetBase : MonoBehaviour
    {
        public WidgetType widgetType;

        [Header("References")]
        public TextMeshProUGUI uiText;
        public Image uiBG;

        protected virtual void Show() 
        {
            gameObject.SetActive(true);
        }
        protected virtual void Hide() 
        {
            gameObject.SetActive(false);
        }

        protected virtual void Setup(object data)
        {

        }

        protected virtual void UnSetup()
        {

        }

        private void OnDestroy() {
            UnSetup();
        }
    }
}