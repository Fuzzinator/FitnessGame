using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextHolder : MonoBehaviour
{
    [field: SerializeField, TextArea(1, 20)]
    public string Text { get;private set; }
}
