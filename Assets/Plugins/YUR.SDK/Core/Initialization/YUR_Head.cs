using UnityEngine;
using YUR.SDK.Core.Watch;

namespace YUR.SDK.Core.Initialization
{
    /// <summary>
    /// Sets/Unsets the Head to the YURWatch component
    /// </summary>
    public class YUR_Head : MonoBehaviour
    {
        private void OnEnable()
        {
            YURWatch.head = gameObject;
        }

        private void OnDisable()
        {
            if (YURWatch.head == gameObject)
            {
                YURWatch.head = null;
            }
        }
    } 
}