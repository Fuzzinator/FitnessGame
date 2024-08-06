using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace HologramDissolve
{
public class CRotatingGoo : MonoBehaviour
{
[SerializeField]
    private          float           m_fTime;
[SerializeField]
    private          TimeSpan        m_CurrTime;
[SerializeField]
    private          Transform       m_Transform;
[SerializeField]
    private          float           m_fSpeed           =   6144.0f;

    void Start()
    {
        
    }

    private     void    ChangeTime()
    {
        m_fTime +=  Time.deltaTime * m_fSpeed;
        m_CurrTime                      =   TimeSpan.FromSeconds( m_fTime );
        m_Transform.rotation            =   Quaternion.Euler( new Vector3( m_Transform.rotation.eulerAngles.x, ( m_fTime - 21600 ) / 86400 * 360, m_Transform.rotation.eulerAngles.z ) );
    }

    // Update is called once per frame
    void Update()
    {
        ChangeTime();
    }
}
}