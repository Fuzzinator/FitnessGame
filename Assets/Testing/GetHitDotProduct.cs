using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetHitDotProduct : MonoBehaviour
{
    public float dotProduct;
    public Vector3 targetHitDirection;
    public Vector3 actualHitDirection;
    private void OnCollisionEnter(Collision other)
    {
        actualHitDirection = Vector3.Normalize(other.contacts[0].point - transform.position);
        Debug.Log(actualHitDirection);
        dotProduct = Vector3.Dot(targetHitDirection, this.actualHitDirection);
        Debug.Log(dotProduct);
    }
}
