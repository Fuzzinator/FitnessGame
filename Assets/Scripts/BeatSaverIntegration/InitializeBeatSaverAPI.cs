using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeBeatSaverAPI : MonoBehaviour
{
    [SerializeField]
    private BeatSaverPageController _controller;

    // Start is called before the first frame update
    void Start()
    {
        _controller.Initialize();
        if (NetworkConnectionValidator.HasNetworkConnection())
        {
            _controller.RequestHighestRated();
        }
    }
}
