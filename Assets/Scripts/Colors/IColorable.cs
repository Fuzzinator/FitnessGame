using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IColorable
{
    Color MyMaterial { get; set; }
    Renderer MyRenderer { get; set; }
    void UpdateMaterial();
}
