using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GloveController : MonoBehaviour
{
    [field:SerializeField]
    public Collider GloveCollider { get; private set; }
    
    [field:SerializeField]
    public Renderer[] Renderers { get; private set; }
    
    private void OnValidate()
    {
        if(GloveCollider != null)
        {
            return;
        }
        GloveCollider = GetComponent<Collider>();
        Renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetRendersColor(Color color)
    {
        foreach (var renderer in Renderers)
        {
            renderer.sharedMaterial.color = color;
        }
    }
}
