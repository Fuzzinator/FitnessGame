using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.SDK.Core.Initialization;

public class NoWatchMeshSelector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        if (YUR_Manager.Instance.YURSettings.DisableWatchModel)
        {
            if (WithoutWatchMesh != null)
            {
                var meshFilter = this.gameObject.GetComponent<MeshFilter>();
                meshFilter.mesh = WithoutWatchMesh;
            } else
            {
                var renderer = this.gameObject.GetComponent<MeshRenderer>();
                renderer.enabled = false;
            }
        }
    }

    public Mesh WithoutWatchMesh;
}
