using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CalibrateArmDistanceDisplay : MonoBehaviour, ISaver
{
    private const string LEFTGRIPPRESSED = "LeftGripPressed";
    private const string RIGHTGRIPPRESSED = "RightGripPressed";
    private const string LEFTGRIPRELEASED = "LeftGripReleased";
    private const string RIGHTGRIPRELEASED = "RightGripReleased";
    private const float CalibrateTime = 2.5f;

    private float[] _leftArmLengths = new float[10];
    private float[] _rightArmLengths = new float[10];


    [SerializeField]
    private Image _leftProgressImage;

    [SerializeField]
    private Image _rightProgressImage;

    [SerializeField]
    private Image _leftCompleteImage;

    [SerializeField]
    private Image _rightCompleteImage;

    [SerializeField]
    private UnityEvent _leftGloveCalibrated = new UnityEvent();
    [SerializeField]
    private UnityEvent _rightGloveCalibrated = new UnityEvent();

    private CancellationToken _cancellationToken;

    public bool IsLeftGripPressed { get; private set; }
    public bool IsRightGripPressed { get; private set; }
    public bool SaveRequested { get; set; }

    private bool _leftCalibrated;
    private bool _rightCalibrated;

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    private void OnEnable()
    {
        InputManager.Instance.MainInput[LEFTGRIPPRESSED].performed += LeftGripPressed;
        InputManager.Instance.MainInput[RIGHTGRIPPRESSED].performed += RightGripPressed;
        InputManager.Instance.MainInput[LEFTGRIPRELEASED].performed += LeftGripReleased;
        InputManager.Instance.MainInput[RIGHTGRIPRELEASED].performed += RightGripReleased;
        Revert();
    }

    private void OnDisable()
    {
        InputManager.Instance.MainInput[LEFTGRIPPRESSED].performed -= LeftGripPressed;
        InputManager.Instance.MainInput[RIGHTGRIPPRESSED].performed -= RightGripPressed;
        InputManager.Instance.MainInput[LEFTGRIPRELEASED].performed -= LeftGripReleased;
        InputManager.Instance.MainInput[RIGHTGRIPRELEASED].performed -= RightGripReleased;
    }

    private void LeftGripPressed(InputAction.CallbackContext obj)
    {
        IsLeftGripPressed = true;
        var data = new ArmCalibrationData(
#if UNITY_EDITOR && UNITY_ANDROID
        HandTracker.LeftEditorHand,
#else
        HandTracker.LeftHand,
#endif
        _leftArmLengths, _leftProgressImage, _leftCompleteImage, _leftGloveCalibrated);
        StartTrackingPosition(data).Forget();
    }

    private void RightGripPressed(InputAction.CallbackContext obj)
    {
        IsRightGripPressed = true;
        var data = new ArmCalibrationData(
#if UNITY_EDITOR && UNITY_ANDROID
        HandTracker.RightEditorHand,
#else
        HandTracker.RightHand,
#endif
        _rightArmLengths, _rightProgressImage, _rightCompleteImage, _rightGloveCalibrated);
        StartTrackingPosition(data).Forget();
    }

    private void LeftGripReleased(InputAction.CallbackContext obj)
    {
        IsLeftGripPressed = false;

        if (_leftCalibrated)
        {
            return;
        }

        var leftHand = HandTracker.LeftHand;
#if UNITY_EDITOR && UNITY_ANDROID
        leftHand = HandTracker.LeftEditorHand;
#endif
        ClearProgress(new ArmCalibrationData(leftHand, _leftArmLengths, _leftProgressImage, _leftCompleteImage, _leftGloveCalibrated));
    }

    private void RightGripReleased(InputAction.CallbackContext obj)
    {
        IsRightGripPressed = false;
        if (_rightCalibrated)
        {
            return;
        }

        var rightHand = HandTracker.RightHand;
#if UNITY_EDITOR && UNITY_ANDROID
        rightHand = HandTracker.RightEditorHand;
#endif

        ClearProgress(new ArmCalibrationData(rightHand, _rightArmLengths, _rightProgressImage, _rightCompleteImage, _rightGloveCalibrated));
    }

    private async UniTask StartTrackingPosition(ArmCalibrationData data)
    {
        var timePassed = 0f;
        if (data.CompleteImage == null || data.ProgressImage == null)
        {
            if (this == null || GameStateManager.Instance == null || GameStateManager.Instance.CurrentGameState == GameState.Quitting)
            {
                return;
            }
            Debug.LogError($"Data.CompleteImage is {data.CompleteImage} : Data.ProgressImage is {data.ProgressImage}. Game State is {GameStateManager.Instance.CurrentGameState}");
            return;
        }
        data.CompleteImage.gameObject.SetActive(false);
        while (data.Hand != null && this != null &&
             (data.Hand.AssignedHand == HitSideType.Left && IsLeftGripPressed ||
              data.Hand.AssignedHand == HitSideType.Right && IsRightGripPressed))
        {
            var percent = timePassed / CalibrateTime;
            data.ProgressImage.fillAmount = percent;

            var index = Mathf.FloorToInt(percent * data.Lengths.Length);

            if (index >= data.Lengths.Length)
            {
                data.CompleteImage.gameObject.SetActive(true);
                if (data.Hand.AssignedHand == HitSideType.Left)
                {
                    _leftCalibrated = true;
                }
                else if (data.Hand.AssignedHand == HitSideType.Right)
                {
                    _rightCalibrated = true;
                }

                data.OnComplete.Invoke();
                return;
            }

            if (Mathf.Approximately(data.Lengths[index], 0f))
            {
                data.Lengths[index] = GetDistance(data.Hand);
            }

            await UniTask.NextFrame(cancellationToken: _cancellationToken);

            timePassed += Time.unscaledDeltaTime;
        }
    }

    public void Revert()
    {
        var leftHand = HandTracker.LeftHand;
#if UNITY_EDITOR && UNITY_ANDROID
        leftHand = HandTracker.LeftEditorHand;
#endif
        var rightHand = HandTracker.RightHand;
#if UNITY_EDITOR && UNITY_ANDROID
        rightHand = HandTracker.RightEditorHand;
#endif
        _leftCalibrated = false;
        _rightCalibrated = false;
        ClearProgress(new ArmCalibrationData(leftHand, _leftArmLengths, _leftProgressImage, _leftCompleteImage, _leftGloveCalibrated));
        ClearProgress(new ArmCalibrationData(rightHand, _rightArmLengths, _rightProgressImage, _rightCompleteImage, _rightGloveCalibrated));
    }

    public void Finish()
    {
        if (!_leftCalibrated && !_rightCalibrated)
        {
            SaveRequested = false;
            return;
        }
        SaveRequested = true;
        SettingsDisplay.Instance.ChangeWasMade(this);
    }

    public void CancelChanges()
    {
        SaveRequested = false;
        Revert();
    }

    public void Save(Profile overrideProfile = null)
    {
        var leftAverage = _leftArmLengths.Average();
        var rightAverage = _rightArmLengths.Average();
        var finalAverage = -1f;

        if (_leftCalibrated && _rightCalibrated)
        {
            finalAverage = (leftAverage + rightAverage) * .5f;
        }
        else if (_leftCalibrated)
        {
            finalAverage = leftAverage;
        }
        else if (_rightCalibrated)
        {
            finalAverage = rightAverage;
        }
        else
        {
            SaveRequested = false;
            return;
        }

        SettingsManager.SetCachedFloat(SettingsManager.AverageArmLength, finalAverage);

        SaveRequested = false;
    }

    private float GetDistance(Hand hand)
    {
        var headPos = Head.Instance.transform.position;
        var handPos = new Vector3(headPos.x, headPos.y, hand.transform.position.z);
        return Vector3.Distance(headPos, handPos);
    }

    private void ClearProgress(ArmCalibrationData data)
    {
        for (int i = 0; i < data.Lengths.Length; i++)
        {
            data.Lengths[i] = 0.0f;
        }
        data.ProgressImage.fillAmount = 0.0f;
        data.CompleteImage.gameObject.SetActive(false);
    }

    private struct ArmCalibrationData
    {
        public Hand Hand { get; private set; }
        public float[] Lengths { get; private set; }
        public Image ProgressImage { get; private set; }
        public Image CompleteImage { get; private set; }
        public UnityEvent OnComplete { get; private set; }

        public ArmCalibrationData(Hand hand, float[] lengths, Image progressImage, Image completeImage, UnityEvent onComplete)
        {
            Hand = hand;
            Lengths = lengths;
            ProgressImage = progressImage;
            CompleteImage = completeImage;
            OnComplete = onComplete;
        }
    }
}
