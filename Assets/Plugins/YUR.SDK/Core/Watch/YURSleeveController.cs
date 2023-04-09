using UnityEngine;
using YUR.SDK.Core.Enums;
using YUR.Fit.Unity;
using YUR.SDK.Core.Initialization;
using System.Collections;

namespace YUR.SDK.Core.Watch
{
    /// <summary>
    /// Loads widgets, adds them to the sleeve, and updates them via the YUR Service
    /// </summary>
    public class YURSleeveController : MonoBehaviour
    {
        private SleeveState _currentSleeveState = SleeveState.StartSleeve;
        public SleeveState CurrentSleeveState { get => _currentSleeveState; set => _currentSleeveState = value; }

        public SleeveState InitialSleeve = SleeveState.PINSleeve;

        [Header("Sleeve References")]
        [SerializeField]
        private YURSleeve[] _yurSleeves = System.Array.Empty<YURSleeve>();
        [SerializeField] private GameObject[] _sleeves = null;
        [SerializeField] private TriggerSleeveState[] _sleeveStateTriggers = null;

        [Header("Blending References")]
        [SerializeField] private SetBlend _metalBarsBlend = null;
        [SerializeField] private SetBlend _sleeveAnchorBlend = null;

        /// <summary>
        /// Private References
        /// </summary>
        private SleeveState m_previousSleeve = SleeveState.None;
        private Coroutine m_co = null;
        private bool m_wasLoggedIn = false;

        private Coroutine _delayRouteCo;

        private void OnValidate()
        {
            if (_yurSleeves.Length == 0 && _sleeves != null && _sleeves.Length > 0)
            {
                _yurSleeves = new YURSleeve[_sleeves.Length];
                for (var i = 0; i < _yurSleeves.Length; i++)
                {
                    _sleeves[i].TryGetComponent(out _yurSleeves[i]);
                }
            }
        }

        private void Awake()
        {
            foreach (TriggerSleeveState sleeveStateTrigger in _sleeveStateTriggers)
            {
                sleeveStateTrigger.referenceToSleeveController = this;
            }

            CoreServiceManager.OnLoginStateChanged += OnLoginStateChanged;
        }


        private void OnEnable()
        {
            _delayRouteCo = StartCoroutine(DelayInitialSleeveRoute());
        }

        private void OnDisable()
        {
            if (_delayRouteCo != null)
            {
                StopCoroutine(_delayRouteCo);
            }
            m_wasLoggedIn = false;
            m_previousSleeve = SleeveState.StartSleeve;
            CurrentSleeveState = SleeveState.StartSleeve;
            ResetToStartSleeve();
        }

        private IEnumerator DelayInitialSleeveRoute()
        {

            var sleepTime = new WaitForSecondsRealtime(1);
            while (!CoreServiceManager.Initialized)
            {
                yield return sleepTime;
            }
            yield return sleepTime;
            if (!CoreServiceManager.IsLoggedIn)
            {
                CurrentSleeveState = SleeveState.PINSleeve;
            }
            else if (CurrentSleeveState != SleeveState.SetupSleeve && CurrentSleeveState != SleeveState.TileSleeve)
            {
                CurrentSleeveState = SleeveState.SetupSleeve;
            }
            _delayRouteCo = null;
        }


        private void LateUpdate()
        {
            if (gameObject.activeInHierarchy && m_co is null)
                CheckSleeveState(m_previousSleeve, CurrentSleeveState);
        }

        private void OnDestroy()
        {
            CoreServiceManager.OnLoginStateChanged -= OnLoginStateChanged;
        }

        private void OnLoginStateChanged(bool loggedIn)
        {
            YUR_Manager.Instance.Log("Checking Login State");

            try
            {
                //if the GO has gotten disabled
                if (!this.gameObject.activeInHierarchy)
                {
                    YUR_Manager.Instance.Log("Setting Start Sleeve");
                    m_wasLoggedIn = false;
                    m_previousSleeve = SleeveState.StartSleeve;
                    CurrentSleeveState = SleeveState.StartSleeve;
                    ResetToStartSleeve();
                }
                // Going from not logged in to logged in
                else if (loggedIn && !m_wasLoggedIn)
                {
                    if (m_previousSleeve != SleeveState.SetupSleeve)
                    {
                        YUR_Manager.Instance.Log("Going to Setup Sleeve");
                        m_previousSleeve = SleeveState.SetupSleeve;
                        CurrentSleeveState = SleeveState.SetupSleeve;
                        TransitionToSleeve(SleeveState.SetupSleeve);
                    }
                    m_wasLoggedIn = true;
                }
                // Going from logged in to not logged in
                else if (!loggedIn && m_wasLoggedIn)
                {
                    YUR_Manager.Instance.Log($"Going to {InitialSleeve}");
                    m_previousSleeve = InitialSleeve;
                    CurrentSleeveState = InitialSleeve;
                    TransitionToSleeve(InitialSleeve);
                    m_wasLoggedIn = false;
                }
                // Initial state
                else if (!loggedIn && !m_wasLoggedIn)
                {
                    YUR_Manager.Instance.Log($"Setting {InitialSleeve}");
                    m_previousSleeve = InitialSleeve;
                    CurrentSleeveState = InitialSleeve;
                    TransitionToSleeve(InitialSleeve);
                }
            }
            catch (UnityException e)
            {
                YUR_Manager.Instance.Log("Login State Change event couldn't swap. Here's why: " + e.Message);
            }
        }

        private bool IsSleeveExpanded()
        {
            foreach (var sleeve in _yurSleeves)
            {
                if (sleeve.TriggerContent.isActive)
                    return true;
            }
            return false;
        }

        public void ToggleSleeveExpand()
        {
            if (IsSleeveExpanded())
            {
                StartCoroutine(RunSleeveRetract(SleeveState.None));
            }
            else
            {
                StartCoroutine(RunSleeveExpand(m_previousSleeve));
            }
        }

        #region SleeveCode
        /// Constantly check to see if the sleeve needs to change
        private void CheckSleeveState(SleeveState previousSleeve, SleeveState currentSleeve)
        {
            if (previousSleeve != currentSleeve)
            {
                YUR_Manager.Instance.Log("Transition Sleeve to " + currentSleeve + " from " + previousSleeve);
                TransitionToSleeve(currentSleeve);
                m_previousSleeve = currentSleeve;
            }
        }

        /// Transitions watch from one sleeve to the next.
        internal void TransitionToSleeve(SleeveState sleeveToTransitionTo)
        {
            if (gameObject.activeInHierarchy)
            {
                if (m_co != null)
                    StopCoroutine(m_co);

                m_co = StartCoroutine(RunSleeveRetract(sleeveToTransitionTo));
            }
        }

        /// Retracts and turns off content on sleeve.
        private IEnumerator RunSleeveRetract(SleeveState sleeveState)
        {
            /// Tell the objects to start their blend to retract.
            while (_sleeveAnchorBlend.BlendValue >= 0)
            {
                _metalBarsBlend.BlendValue -= 0.025f;
                _sleeveAnchorBlend.BlendValue -= 0.03f;
                yield return null;
            }

            TurnOffAllSleeves();

            /// If we are going to a None Sleeve State, continue blending the Metal Bars to 0.
            /// If we are not, then extend the new sleeve to show.
            if (sleeveState != SleeveState.None)
            {
                m_co = StartCoroutine(RunSleeveExpand(sleeveState));
            }
            else
            {
                m_co = null;

                /// Tell the objects to start their blend to retract.
                while (_metalBarsBlend.BlendValue >= 0.0f)
                {
                    _metalBarsBlend.BlendValue -= 0.025f;
                    yield return null;
                }
            }
        }

        private void TurnOffAllSleeves()
        {
            /// Will always turn off all sleeves as a precautionary measure.
            foreach (var sleeve in _yurSleeves)
            {
                sleeve.TriggerContent.Toggle(false);
            }

            /// Turns off mesh renderer if it is present
            foreach (var sleeve in _yurSleeves)
            {
                if (sleeve.HasMeshRenderer)
                {
                    sleeve.MeshRenderer.enabled = false;
                }
            }
        }

        private void ResetToStartSleeve()
        {
            YURSleeve startSleeve = null;

            // Sets the correct sleeve
            foreach (var sleeve in _yurSleeves)
            {
                if (sleeve.State == SleeveState.StartSleeve)
                {
                    startSleeve = sleeve;
                }
            }
            if (startSleeve == null)
            {
                Debug.LogWarning("Start sleeve could not be found!");
                return;
            }

            TurnOffAllSleeves();
            if (startSleeve.HasMeshRenderer)
            {
                startSleeve.MeshRenderer.enabled = true;
            }
            startSleeve.TriggerContent.Toggle(true);
        }

        private IEnumerator RunSleeveExpand(SleeveState sleeveState)
        {
            yield return null;

            YURSleeve thisSleeve = null;


            // Sets the correct sleeve
            foreach (var sleeve in _yurSleeves)
            {
                if (sleeve.State == sleeveState)
                {
                    thisSleeve = sleeve;
                }
            }

            var turnedOn = false;

            // Tell the objects to start their blend to retract.
            while (_metalBarsBlend.BlendValue < 1.0f)
            {
                _metalBarsBlend.BlendValue += 0.025f;

                if (_metalBarsBlend.BlendValue > 0.25f)
                {
                    if (!turnedOn)
                    {
                        // Turns on the sleeve
                        thisSleeve.TriggerContent.Toggle(true);

                        if (thisSleeve.HasMeshRenderer)
                        {
                            thisSleeve.MeshRenderer.enabled = true;
                        }

                        turnedOn = true;
                    }

                    _sleeveAnchorBlend.BlendValue += 0.035f;
                }

                yield return null;
            }

            _metalBarsBlend.BlendValue = 1.0f;
            _sleeveAnchorBlend.BlendValue = 1.0f;

            m_co = null;
        }
        #endregion
    }
}