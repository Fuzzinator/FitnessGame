using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;
using YUR.Core.Config;

namespace YUR.Core
{
    public class YURWatch : MonoBehaviour
    {
        public YURSettings Settings;

        internal static Transform HMDAnchor => YURHMD.Instance?.transform;
        internal static Transform RightHandAnchor => YURRightHand.Instance?.transform;
        internal static Transform LeftHandAnchor => YURLeftHand.Instance?.transform;

        private GameObject watch;


        public void Start()
        {
            Begin(GetUniqueEcosystemID());
        }

        private string GetUniqueEcosystemID()
        {
#if YUR_ECO_META
            return Oculus.Platform.Users.GetLoggedInUser().ToString();
# else
            return "";
#endif
        }

        public void Begin(string UserID)
        {
            watch = Instantiate(Settings.WatchSetup, gameObject.transform);
            YURInterface.Instance.Begin(new GameInfo(Settings.GameName, Settings.YurLicense, Settings.GameVersion,Settings.SubPlatform), UserID);
        }

        private void Update()
        {
            if (!watch || Settings == null) return;

            switch (Settings.handInUse)
            {
                case HandSide.Left:
                    SetWatchRelativePosition(LeftHandAnchor.position, LeftHandAnchor.eulerAngles, Settings.LeftHandPositionOffset, Settings.LeftHandEulerOffset);
                    break;

                case HandSide.Right:
                    SetWatchRelativePosition(RightHandAnchor.position, RightHandAnchor.eulerAngles, Settings.RightHandPositionOffset, Settings.RightHandEulerOffset);
                    break;
            }
        }

        private void SetWatchRelativePosition(Vector3 position, Vector3 rotation, Vector3 positionOffset, Vector3 eulerOffset)
        {
            gameObject.transform.position = position;
            gameObject.transform.eulerAngles = rotation;

            if (watch)
            {
                watch.transform.localPosition = positionOffset;
                watch.transform.localEulerAngles = eulerOffset;
            }
        }

        public void SetPreferredWatchHand(HandSide hand)
        {
            Settings.handInUse = hand;
        }

        public void SetTag(string tag)
        {
            YURInterface.Instance.SetTag(tag);
        }

        public static bool IsConnected()
        {
            return YURInterface.Instance._bIsTokenReaded;
        }

        public YUR_SDK.CResults GetResults()
        {
            return YURInterface.Instance.GetResults();
        }
    }
}