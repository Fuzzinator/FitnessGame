using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSelectionLoader : MonoBehaviour
{
    public void LoadSelectedEnvironmentContainer()
    {
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.LoadSelection();
        }
    }
}
