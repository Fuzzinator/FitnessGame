using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingFixingTargets : MonoBehaviour
{
    [SerializeField]
    private Collider _originalCollider;
    [SerializeField]
    private Collider _enlogatedCollider;
    [SerializeField]
    private BaseTarget _target;

    private void OnEnable()
    {
        if(SettingsManager.UseEnlongatedCollider)
        {
            _originalCollider.enabled = false;
            _enlogatedCollider.enabled = true;
        }
        else
        {
            //_target.
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
