using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnvironmentSelectionLoader : MonoBehaviour
{
    public void LoadSelectedEnvironmentContainer()
    {
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.LoadSelection();
        }
    }

    public void RevertToDefaultEnvironment()
    {
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.RevertToDefaultEnvironment();
        }
    }
}
