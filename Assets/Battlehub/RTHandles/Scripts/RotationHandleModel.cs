using UnityEngine;

namespace Battlehub.RTHandles
{
    public class RotationHandleModel : MonoBehaviour
    {
        private const float DefaultMinorRadius = 0.05f;
        [SerializeField]
        private float m_minorRadius = DefaultMinorRadius;

        private MeshFilter m_filter;
        private Mesh m_original;
        // Use this for initialization
        void Start()
        {
            m_filter = GetComponent<MeshFilter>();
            m_original = m_filter.sharedMesh;
            Mesh mesh = m_filter.mesh;
            m_filter.sharedMesh = mesh;
        }

        private void UpdateTransform(Mesh mesh)
        {
            Vector3[] verts = m_original.vertices;
            for(int s = 0; s < m_original.subMeshCount; ++s)
            {
                int[] tris =  mesh.GetTriangles(s);
                for(int t = 0; t < tris.Length; ++t)
                {
                    int tri = tris[t];
                    Vector3 v = verts[tri];
                    Vector3 c = v;
                   
                    if(s == 0)
                    {
                        c.x = 0;
                    }
                    else if(s == 1)
                    {
                        c.y = 0;
                    }
                    else if(s == 2)
                    {
                        c.z = 0;
                    }
                    else
                    {
                      //  throw new System.NotSupportedException();
                    }

                    c.Normalize();

                    verts[tri] = c + (v - c).normalized * m_minorRadius;
                }
            }
            mesh.vertices = verts;
        }

        private float m_prevMinorRadius = DefaultMinorRadius;
        void Update()
        {
            if(m_prevMinorRadius != m_minorRadius)
            {
                m_prevMinorRadius = m_minorRadius;
                UpdateTransform(m_filter.sharedMesh);
            }
        }
    }

}
