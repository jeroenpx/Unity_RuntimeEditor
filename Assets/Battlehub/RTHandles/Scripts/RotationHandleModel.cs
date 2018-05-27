using UnityEngine;

namespace Battlehub.RTHandles
{
    public class RotationHandleModel : MonoBehaviour
    {
        private const float DefaultMinorRadius = 0.05f;
        [SerializeField]
        private float m_minorRadius = DefaultMinorRadius;

        [SerializeField]
        private MeshFilter m_handles;
        [SerializeField]
        private MeshFilter m_ssCircle1;
        [SerializeField]
        private MeshFilter m_ssCircle2;
      
        private Mesh m_original;
        private Mesh m_ssOriginal1;

        void Start()
        {
            m_original = m_handles.sharedMesh;
            m_ssOriginal1 = m_ssCircle1.sharedMesh;

            Mesh mesh = m_handles.mesh;
            m_handles.sharedMesh = mesh;

            mesh = m_ssCircle1.mesh;
            m_ssCircle1.sharedMesh = mesh;

            mesh = m_ssCircle2.mesh;
            m_ssCircle2.sharedMesh = mesh;
        }

        private void UpdateXYZ(Mesh mesh)
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

        private void UpdateSSCircle(Mesh mesh)
        {
            Vector3[] verts = m_ssOriginal1.vertices;

            int[] tris = mesh.GetTriangles(0);
            for (int t = 0; t < tris.Length; ++t)
            {
                int tri = tris[t];
                Vector3 v = verts[tri];
                Vector3 c = v;
                c.z = 0;

                c.Normalize();

                verts[tri] = c + (v - c).normalized * m_minorRadius;
            }

            tris = mesh.GetTriangles(1);
            for (int t = 0; t < tris.Length; ++t)
            {
                int tri = tris[t];
                Vector3 v = verts[tri];
                v.Normalize();

                verts[tri] = v * (1 - m_minorRadius);
            }

            mesh.vertices = verts;
        }

        private float m_prevMinorRadius = DefaultMinorRadius;
        void Update()
        {
            if(m_prevMinorRadius != m_minorRadius)
            {
                m_prevMinorRadius = m_minorRadius;
                UpdateXYZ(m_handles.sharedMesh);
                UpdateSSCircle(m_ssCircle1.sharedMesh);
                UpdateSSCircle(m_ssCircle2.sharedMesh);

                //m_ssCircle2.transform.localScale = Vector3.one * (1.0f + Mathf.Abs(m_minorRadius * 10));
            }
        }
    }

}
