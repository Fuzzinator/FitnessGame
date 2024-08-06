using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HologramDissolve
{
    //[ExecuteInEditMode]
public class CDissolveExample : MonoBehaviour
{
    public Material[] m_Material;
    public string m_MaterialProperty = "_Dissolve";
    public float m_fSpeed = 1.0f;
    [Range(0,1.0f)]
    public float m_fRange = 0.0f;
    public bool m_bUseTime = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if( m_bUseTime )
        {
            if( m_Material.Length > 0 && m_MaterialProperty != "" )
            {
                for( int i = 0; i < m_Material.Length; ++i )
                {
                    m_Material[ i ].SetFloat( m_MaterialProperty, Mathf.Clamp( Mathf.Abs( Mathf.Sin( Time.time / m_fSpeed ) ), 0.0f, 1.0f ) );
                }
            }
        }
        else
        {
            if( m_Material.Length > 0 && m_MaterialProperty != "" )
            {
                for( int j = 0; j < m_Material.Length; ++j )
                {
                    m_Material[ j ].SetFloat( m_MaterialProperty, m_fRange );
                }
            }
        }
    }
}
}