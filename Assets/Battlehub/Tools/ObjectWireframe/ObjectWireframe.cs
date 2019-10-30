using Battlehub.RTSL;
using UnityEngine;

namespace Battlehub.Wireframe
{
    [RequireComponent(typeof(MeshFilter))]
    public class ObjectWireframe : MonoBehaviour
    {
        private static Material m_wMaterial;
        private GameObject m_wireframe;

        [SerializeField]
        private bool m_showWireframeOnly = false;

        [SerializeField]
        private bool m_isOn = true;
        public bool IsOn
        {
            get { return m_isOn; }
            set
            {
                if (m_isOn != value)
                {
                    m_isOn = value;

                    if (m_showWireframeOnly)
                    {
                        MeshRenderer renderer = GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            renderer.enabled = !m_isOn;
                        }
                    }

                    if (m_wireframe != null)
                    {
                        m_wireframe.SetActive(m_isOn);
                    }
                }
            }
        }

        private void Awake()
        {
            if(m_wMaterial == null)
            {
                m_wMaterial = Resources.Load<Material>("ObjectWireframe");
            }

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh != null)
            {
                m_wireframe = new GameObject("Wireframe");
                m_wireframe.transform.SetParent(transform, false);
                m_wireframe.gameObject.AddComponent<RTSLIgnore>();

                meshFilter = m_wireframe.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = CreateWirefameMesh(mesh);

                MeshRenderer meshRenderer = m_wireframe.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = m_wMaterial;

                m_wireframe.SetActive(m_isOn);
                if (m_showWireframeOnly)
                {
                    meshRenderer = GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        meshRenderer.enabled = !m_isOn;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (m_wireframe)
            {
                Destroy(m_wireframe);
            }
        }


        private static Mesh CreateWirefameMesh(Mesh mesh)
        {
            int[] triangles = mesh.triangles;
            int[] wIndices = new int[triangles.Length * 2];

            Vector3[] vertices = mesh.vertices;
            Color[] colors = new Color[vertices.Length];
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Color.white;
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v0 = triangles[i];
                int v1 = triangles[i + 1];
                int v2 = triangles[i + 2];

                wIndices[i * 2] = v0;
                wIndices[i * 2 + 1] = v1;

                wIndices[(i + 1) * 2] = v1;
                wIndices[(i + 1) * 2 + 1] = v2;

                wIndices[(i + 2) * 2] = v2;
                wIndices[(i + 2) * 2 + 1] = v0;
            }

            Mesh wMesh = new Mesh();
            wMesh.name = mesh.name + " Wireframe";
            wMesh.vertices = vertices;
            wMesh.colors = colors;
            wMesh.subMeshCount = 1;
            wMesh.SetIndices(wIndices, MeshTopology.Lines, 0);

            return wMesh;
        }
    }
}

