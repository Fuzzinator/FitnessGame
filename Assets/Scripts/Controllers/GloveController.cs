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
    [field: SerializeField]
    public Material[] Materials { get; private set; }

    private void OnValidate()
    {
        if(GloveCollider != null)
        {
            return;
        }
        GloveCollider = GetComponent<Collider>();
        Renderers = GetComponentsInChildren<Renderer>();
        if(Renderers != null)
        {
            var uniqueList = new List<Material>();
            foreach(var r in Renderers)
            {
                if(uniqueList.Contains(r.sharedMaterial))
                {
                    continue;
                }
                uniqueList.Add(r.sharedMaterial);
            }
            Materials = uniqueList.ToArray();
        }
    }

    public void SetGloveColor(Color color)
    {
        foreach (var material in Materials)
        {
            material.color = color;
        }
    }
}
