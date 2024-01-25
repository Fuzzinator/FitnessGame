using UnityEngine;
using YUR.Core;

namespace YUR.Core.Config
{
    [CreateAssetMenu(fileName = "YURWatchSettings", menuName = "YUR/YURWatchSettings", order = 0)]
    public class YURSettings : ScriptableObject
    {
        public GameObject WatchSetup;

        [Header("Game Information")]
        public string GameName   = "YourCompany.YourGame";
        public string GameVersion = "0.0.1";
        public string YurLicense = "TemporaireLicense";

        [Header("Left Hand Setup")]
        public Vector3 LeftHandPositionOffset = new Vector3();
        public Vector3 LeftHandEulerOffset = new Vector3();

        [Header("Right Hand Setup")]
        public Vector3 RightHandPositionOffset = new Vector3();
        public Vector3 RightHandEulerOffset = new Vector3();

        public HandSide handInUse;
        [SerializeField]
        private YUR_SDK.SubPlatoform _androidPlatform = YUR_SDK.SubPlatoform.Android_Quest;
        [SerializeField]
        private YUR_SDK.SubPlatoform _pcvrPlatform = YUR_SDK.SubPlatoform.Auto;
        void Reset()
        {
            GameName = Application.identifier;
            LeftHandPositionOffset = new Vector3();
            LeftHandEulerOffset = new Vector3();
            RightHandPositionOffset = new Vector3();
            RightHandEulerOffset = new Vector3();
        }

        public YUR_SDK.SubPlatoform SubPlatform
        {
            get
            {
#if UNITY_ANDROID
                return _androidPlatform;
#elif UNITY_STANDALONE_WIN
                return _pcvrPlatform;
#endif
            }
        }
    }
}