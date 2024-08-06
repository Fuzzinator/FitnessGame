using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CMeshBounds
{
    //[ExecuteInEditMode]
    public class CMeshBounds : MonoBehaviour
    {
    [SerializeField]
        private                 Material[]              m_Materials;
    [SerializeField]
        private                 string                  m_sProperty              = "_Height_Value";
    [SerializeField]
        private                 Renderer                m_Renderer               = null;

        private      void       OnValidate()
        {
            if( m_Renderer == null ){ GetComponent< Renderer >(); }
            if( m_Renderer != null ){ SetMeshBounds( m_Materials, m_sProperty, m_Renderer ); }
        }

        private     void        Start()
        {
            if( m_Renderer == null ){ GetComponent< Renderer >(); }
            if( m_Renderer != null ){ SetMeshBounds( m_Materials, m_sProperty, m_Renderer ); }
        }

        public      void        SetMeshBounds( Material[]           mMats
                                             , string               sPropertyName   = ""
                                             , Renderer             smRenderer      = null )
        {
            if( mMats.Length > 0 
             && sPropertyName != "" )
            {
                if( smRenderer != null )
                {
                    float fBoundsInY = 0.0f;
                    fBoundsInY = smRenderer.bounds.size.y;

                    for( int i = 0; i < mMats.Length; ++i )
                    {
                        if( mMats[ i ] != null ){ mMats[ i ].SetFloat( sPropertyName, fBoundsInY ); }
                    }
                }
            }
        }
    }
}
