using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Spin : MonoBehaviour
{
    public float speed = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles += new Vector3(0,speed,0);
    }
}
