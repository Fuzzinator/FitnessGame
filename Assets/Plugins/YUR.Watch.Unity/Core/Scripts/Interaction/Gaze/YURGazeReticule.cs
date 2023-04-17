using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace YUR.Interaction.Gaze {
    public class YURGazeReticule : MonoBehaviour
{
        [SerializeField] private Image progress;
        [SerializeField] private Transform model;
        [SerializeField] private Vector3 defaultScale = new Vector3(0.2f, 0.2f, 0.2f);

        private void Awake()
        {
            if (model)
                model.localScale = defaultScale;

            ResetProgress();
        }

        public void ResetProgress()
        {
            if (progress)
                progress.fillAmount = 0;
        }

        public void SetProgress(float value)
        {
            if (progress)
                progress.fillAmount = value;
        }

    }
}