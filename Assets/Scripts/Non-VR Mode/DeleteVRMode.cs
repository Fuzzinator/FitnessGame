using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteVRMode : MonoBehaviour
{
    [SerializeField]
    private bool _vrMode;

    // Start is called before the first frame update
    void Start()
    {
        if(GameManager.Instance != null && _vrMode == GameManager.Instance.VRMode)
        {
            Destroy(gameObject);
        }
    }
}
