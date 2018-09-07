using Battlehub.RTCommon;
using Battlehub.RTSaveLoad;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class RotationHandleModel : BaseHandleModel
    {
        private const float DefaultMinorRadius = 0.05f;
        private const float DefaultMajorRadius = 1.0f;
        private const float DefaultOuterRadius = 1.11f;
        [SerializeField]
        private float m_minorRadius = DefaultMinorRadius;
        [SerializeField]
        private float m_majorRadius = DefaultMajorRadius;
        [SerializeField]
        private float m_outerRadius = DefaultOuterRadius;

        [SerializeField]
        private MeshFilter m_xyz;
        [SerializeField]
        private MeshFilter m_innerCircle;
        [SerializeField]
        private MeshFilter m_outerCircle;
      
        private Mesh m_xyzMesh;
        private Mesh m_innerCircleMesh;
        private Mesh m_outerCircleMesh;

        [SerializeField]
        private int m_xMatIndex = 0;
        [SerializeField]
        private int m_yMatIndex = 1;
        [SerializeField]
        private int m_zMatIndex = 2;
        [SerializeField]
        private int m_innerCircleBorderMatIndex = 0;
        [SerializeField]
        private int m_innerCircleFillMatIndex = 1;

        private Material[] m_xyzMaterials;
        private Material[] m_innerCircleMaterials;
        private Material m_outerCircleMaterial;

        protected override void Awake()
        {
            base.Awake();

            m_xyzMesh = m_xyz.sharedMesh;
            m_innerCircleMesh = m_innerCircle.sharedMesh;
            m_outerCircleMesh = m_outerCircle.sharedMesh;

            Renderer renderer = m_xyz.GetComponent<Renderer>();
            renderer.sharedMaterials = renderer.materials;
            m_xyzMaterials = renderer.sharedMaterials;

            renderer = m_innerCircle.GetComponent<Renderer>();
            renderer.sharedMaterials = renderer.materials;
            m_innerCircleMaterials = renderer.sharedMaterials;

            renderer = m_outerCircle.GetComponent<Renderer>();
            renderer.sharedMaterials = renderer.materials;
            m_outerCircleMaterial = renderer.sharedMaterial;

            Mesh mesh = m_xyz.mesh;
            m_xyz.sharedMesh = mesh;

            mesh = m_innerCircle.mesh;
            m_innerCircle.sharedMesh = mesh;

            mesh = m_outerCircle.mesh;
            m_outerCircle.sharedMesh = mesh;
        }

        protected override void Start()
        {
            base.Start();

            UpdateXYZ(m_xyz.sharedMesh, m_majorRadius, m_minorRadius);
            UpdateCircle(m_innerCircle.sharedMesh, m_innerCircleMesh, m_innerCircle.transform, m_majorRadius, m_minorRadius);
            UpdateCircle(m_outerCircle.sharedMesh, m_outerCircleMesh, m_outerCircle.transform, m_outerRadius, m_minorRadius);

            SetColors();
        }

        public override void Select(RuntimeHandleAxis axis)
        {
            base.Select(axis);
            SetColors();
        }

        public override void SetLock(LockObject lockObj)
        {
            base.SetLock(lockObj);
            SetColors();
        }

        private void SetDefaultColors()
        {
            if (m_lockObj.RotationX)
            {
                m_xyzMaterials[m_xMatIndex].color = m_disabledColor;
            }
            else
            {
                m_xyzMaterials[m_xMatIndex].color = m_xColor;
            }

            if (m_lockObj.RotationY)
            {
                m_xyzMaterials[m_yMatIndex].color = m_disabledColor;
            }
            else
            {
                m_xyzMaterials[m_yMatIndex].color = m_yColor;
            }

            if (m_lockObj.RotationZ)
            {
                m_xyzMaterials[m_zMatIndex].color = m_disabledColor;
            }
            else
            {
                m_xyzMaterials[m_zMatIndex].color = m_zColor;
            }

            if(m_lockObj.RotationScreen)
            {
                m_outerCircleMaterial.color = m_disabledColor;
            }
            else
            {
                m_outerCircleMaterial.color = m_altColor;
            }
            m_outerCircleMaterial.SetInt("_ZTest", 2);

            if (m_lockObj.IsPositionLocked)
            {
                m_innerCircleMaterials[m_innerCircleBorderMatIndex].color = m_disabledColor;
            }
            else
            {
                m_innerCircleMaterials[m_innerCircleBorderMatIndex].color = m_altColor2;
            }

            m_innerCircleMaterials[m_innerCircleFillMatIndex].color = new Color(0, 0, 0, 0);
        }

        private void SetColors()
        {
            SetDefaultColors();
            switch (m_selectedAxis)
            {
                case RuntimeHandleAxis.X:
                    if (!m_lockObj.PositionX)
                    {
                        m_xyzMaterials[m_xMatIndex].color = m_selectionColor;
                    }
                    break;
                case RuntimeHandleAxis.Y:
                    if (!m_lockObj.PositionY)
                    {
                        m_xyzMaterials[m_yMatIndex].color = m_selectionColor;
                    }
                    break;
                case RuntimeHandleAxis.Z:
                    if (!m_lockObj.PositionZ)
                    {
                        m_xyzMaterials[m_zMatIndex].color = m_selectionColor;
                    }
                    break;
                case RuntimeHandleAxis.Free:
                    if(!m_lockObj.IsPositionLocked)
                    {
                        m_innerCircleMaterials[m_innerCircleFillMatIndex].color = new Color(0, 0, 0, 0.1f);
                    }
                    break;
                case RuntimeHandleAxis.Screen:
                    if(!m_lockObj.RotationScreen)
                    {
                        m_outerCircleMaterial.color = m_selectionColor;
                        m_outerCircleMaterial.SetInt("_ZTest", 0);
                    }
                    break;
            }
        }

        private void UpdateXYZ(Mesh mesh, float majorRadius, float minorRadius)
        {
            m_xyz.transform.localScale = Vector3.one * majorRadius;
            minorRadius /= Mathf.Max(0.01f, majorRadius);

            Vector3[] verts = m_xyzMesh.vertices;
            for(int s = 0; s < m_xyzMesh.subMeshCount; ++s)
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

                    c.Normalize();
                    verts[tri] = c + (v - c).normalized * minorRadius;
                }
            }
            mesh.vertices = verts;
        }

        private void UpdateCircle(Mesh mesh, Mesh originalMesh, Transform circleTransform, float majorRadius, float minorRadius)
        {
            circleTransform.localScale = RuntimeHandles.InvertZAxis ? new Vector3(1, 1, -1) * majorRadius : Vector3.one * majorRadius;
            minorRadius /= Mathf.Max(0.01f, majorRadius);

            Vector3[] verts = originalMesh.vertices;
            int[] tris = mesh.GetTriangles(0);
            for (int t = 0; t < tris.Length; ++t)
            {
                int tri = tris[t];
                Vector3 v = verts[tri];
                Vector3 c = v;
                c.z = 0;

                c.Normalize();

                verts[tri] = c + (v - c).normalized * minorRadius;
            }

            tris = mesh.GetTriangles(1);
            for (int t = 0; t < tris.Length; ++t)
            {
                int tri = tris[t];
                Vector3 v = verts[tri];
                v.Normalize();

                verts[tri] = v * (1 - minorRadius);
            }

            mesh.vertices = verts;
        }

        private float m_prevMinorRadius = DefaultMinorRadius;
        private float m_prevMajorRadius = DefaultMajorRadius;
        private float m_prevOuterRadius = DefaultOuterRadius;
        protected override void Update()
        {
            if(m_prevMinorRadius != m_minorRadius || m_prevMajorRadius != m_majorRadius || m_prevOuterRadius != m_outerRadius)
            {
                m_prevMinorRadius = m_minorRadius;
                m_prevMajorRadius = m_majorRadius;
                m_prevOuterRadius = m_outerRadius;
                UpdateXYZ(m_xyz.sharedMesh, m_majorRadius, m_minorRadius);
                UpdateCircle(m_innerCircle.sharedMesh, m_innerCircleMesh, m_innerCircle.transform, m_majorRadius, m_minorRadius);
                UpdateCircle(m_outerCircle.sharedMesh, m_outerCircleMesh, m_outerCircle.transform, m_outerRadius, m_minorRadius);
            }
        }
    }

}
