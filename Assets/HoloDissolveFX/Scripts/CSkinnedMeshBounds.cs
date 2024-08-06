using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CSkinnedMeshBounds
{
    //[ExecuteInEditMode]
    public class CSkinnedMeshBounds : MonoBehaviour
    {
    [SerializeField]
        private                 Material[]              m_Materials              = new Material[ 1 ];
    [SerializeField]
        private                 string                  m_sProperty              = "_Height_Value";
    [SerializeField]
        private                 SkinnedMeshRenderer     m_Renderer               = null;
    [SerializeField]
        private                 bool                    m_bSharedMesh            = true;

        private      void       OnValidate()
        {
            if( m_Renderer == null ){ GetComponent< SkinnedMeshRenderer >(); }
            if( m_Renderer != null ){ SetMeshBounds( m_Materials, m_sProperty, m_Renderer, m_bSharedMesh ); }
        }

        private     void        Start()
        {
            if( m_Renderer == null ){ GetComponent< SkinnedMeshRenderer >(); }
            if( m_Renderer != null ){ SetMeshBounds( m_Materials, m_sProperty, m_Renderer, m_bSharedMesh ); }
        }

        public      void        SetMeshBounds( Material[]           mMats
                                             , string               sPropertyName   = ""
                                             , SkinnedMeshRenderer  smRenderer      = null
                                             , bool                 bShared         = true )
        {
            if( mMats.Length > 0 
             && sPropertyName != "" )
            {
                if( smRenderer != null )
                {
                    float fBoundsInY = 0.0f;
                    if( bShared ) { fBoundsInY = smRenderer.sharedMesh.bounds.size.y; }
                    else { fBoundsInY = smRenderer.bounds.size.y; }

                    for( int i = 0; i < mMats.Length; ++i )
                    {
                        if( mMats[ i ] != null ){ mMats[ i ].SetFloat( sPropertyName, fBoundsInY ); }
                    }
                }
            }
        }
    }
}
