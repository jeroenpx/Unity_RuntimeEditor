using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

namespace Battlehub.ProBuilderIntegration
{
    public class PBVertexSelection : MonoBehaviour
    {
        [SerializeField]
        private Color m_color = new Color(0.33f, 0.33f, 0.33f);
        [SerializeField]
        private Color m_selectedColor = Color.yellow;
        [SerializeField]
        private Color m_hoverColor = new Color(1, 1, 0, 0.75f);

        [SerializeField]
        private float m_scale = 3.3f;

        [SerializeField]
        public CompareFunction m_zTest = CompareFunction.LessEqual;
        private Material m_material;
        private readonly Dictionary<ProBuilderMesh, MeshFilter> m_meshToSelection = new Dictionary<ProBuilderMesh, MeshFilter>();
        private readonly Dictionary<ProBuilderMesh, HashSet<int>> m_meshToIndices = new Dictionary<ProBuilderMesh, HashSet<int>>();
        private readonly Dictionary<ProBuilderMesh, List<int>> m_meshToIndicesList = new Dictionary<ProBuilderMesh, List<int>>();
        private readonly List<ProBuilderMesh> m_meshes = new List<ProBuilderMesh>();
        private readonly int[] m_hoveredIndices = new int[] { -1 };
        private ProBuilderMesh m_hoveredMesh;

        private Vector3 m_lastPosition;
        public Vector3 LastPosition
        {
            get { return LastMesh != null ? LastMesh.transform.TransformPoint(m_lastPosition) : Vector3.zero; }
        }

        public Vector3 m_lastNormal = Vector3.forward;
        public Vector3 LastNormal
        {
            get { return LastMesh != null ? LastMesh.transform.TransformDirection(m_lastNormal.normalized) : Vector3.zero; }
        }

        public ProBuilderMesh LastMesh
        {
            get
            {
                if(m_meshes.Count == 0)
                {
                    return null;
                }

                return m_meshes.Last();
            }
        }

        private Vector3 m_centerOfMass;
        public Vector3 CenterOfMass
        {
            get { return m_centerOfMass; }
        }

        private int m_selectedVerticesCount;
        public int VerticesCount
        {
            get { return m_selectedVerticesCount; }
        }

        public int MeshesCount
        {
            get { return m_meshes.Count; }
        }

        public IEnumerable<ProBuilderMesh> Meshes
        {
            get { return m_meshes; }
        }

        private bool IsGeometryShadersSupported
        {
            get { return BuiltinMaterials.geometryShadersSupported; }
        }

        private void Awake()
        {
            string vertShader = IsGeometryShadersSupported ?
                BuiltinMaterials.pointShader :
                BuiltinMaterials.dotShader;
            m_material = new Material(Shader.Find(vertShader));
            m_material.SetColor("_Color", Color.white);
            m_material.SetInt("_HandleZTest", (int)m_zTest);
            m_material.SetFloat("_Scale", m_scale);
        }

        private void OnDestroy()
        {
            if (m_material != null)
            {
                Destroy(m_material);
            }

            m_meshToSelection.Clear();
            m_meshToIndices.Clear();
            m_meshToIndicesList.Clear();
            m_meshes.Clear();
        }

        public bool IsSelected(ProBuilderMesh mesh, int index)
        {
            HashSet<int> indicesHs;
            if(m_meshToIndices.TryGetValue(mesh, out indicesHs))
            {
                return indicesHs.Contains(index);
            }
            return false;
        }

        public int GetVerticesCount(ProBuilderMesh mesh)
        {
            return m_meshToIndicesList[mesh].Count;
        }

        public IList<int> GetVertices(ProBuilderMesh mesh)
        {
            return m_meshToIndicesList[mesh];
        }

        public void Clear()
        {
            for (int i = 0; i < m_meshes.Count; ++i)
            {
                ProBuilderMesh mesh = m_meshes[i];
                MeshFilter filter = m_meshToSelection[mesh];
                Destroy(filter.gameObject);
            }

            m_meshToSelection.Clear();
            m_meshToIndices.Clear();
            m_meshToIndicesList.Clear();
            m_meshes.Clear();

            m_selectedVerticesCount = 0;
            m_centerOfMass = Vector3.zero;
            m_lastNormal = Vector3.forward;
            m_lastPosition = Vector3.zero;
        }

        public void Hover(ProBuilderMesh mesh, int index)
        {
            if(m_hoveredMesh != null)
            {
                Leave();
            }

            HashSet<int> indicesHs;
            MeshFilter vertices;
            m_meshToSelection.TryGetValue(mesh, out vertices);
            if (!m_meshToIndices.TryGetValue(mesh, out indicesHs))
            {
                return;
            }

            m_hoveredMesh = mesh;
            m_hoveredIndices[0] = index;

            if (indicesHs.Contains(m_hoveredIndices[0]))
            {    
                SetVerticesColor(mesh, vertices, m_selectedColor, m_hoveredIndices);
            }
            else
            {
                SetVerticesColor(mesh, vertices, m_hoverColor, m_hoveredIndices);
            }
        }

        public void Leave()
        {
            if (m_hoveredMesh == null) 
            {
                return;
            }

            HashSet<int> indicesHs;
            MeshFilter vertices;
            m_meshToSelection.TryGetValue(m_hoveredMesh, out vertices);
            if (!m_meshToIndices.TryGetValue(m_hoveredMesh, out indicesHs))
            {
                return;
            }

            if (indicesHs.Contains(m_hoveredIndices[0]))
            {
                SetVerticesColor(m_hoveredMesh, vertices, m_selectedColor, m_hoveredIndices);
            }
            else
            {
                SetVerticesColor(m_hoveredMesh, vertices, m_color, m_hoveredIndices);
            }
            m_hoveredIndices[0] = -1;
            m_hoveredMesh = null;
        }

        public void Add(ProBuilderMesh mesh, IEnumerable<int> indices)
        {
            HashSet<int> indicesHs;
            List<int> indicesList;
            MeshFilter vertices;
            m_meshToSelection.TryGetValue(mesh, out vertices);
            m_meshToIndicesList.TryGetValue(mesh, out indicesList);
            if (!m_meshToIndices.TryGetValue(mesh, out indicesHs))
            {
                vertices = CreateVerticesGameObject(mesh, null);
                vertices.transform.SetParent(transform, false);

                indicesHs = new HashSet<int>();
                indicesList = new List<int>();

                m_meshToSelection.Add(mesh, vertices);
                m_meshToIndices.Add(mesh, indicesHs);
                m_meshToIndicesList.Add(mesh, indicesList);
                m_meshes.Add(mesh);
            }

            int[] notSelectedIndices = indices.Where(i => !indicesHs.Contains(i)).ToArray();
            SetVerticesColor(mesh, vertices, m_selectedColor, notSelectedIndices); 
            for(int i = 0; i < notSelectedIndices.Length; ++i)
            {
                indicesHs.Add(notSelectedIndices[i]);
                indicesList.Add(notSelectedIndices[i]);
            }

            if(notSelectedIndices.Length > 0)
            {
                Vertex[] notSelectedVertices = mesh.GetVertices(notSelectedIndices);
                m_lastPosition = notSelectedVertices.Last().position;
                m_lastNormal = notSelectedVertices.Last().normal;

                for(int i = 0; i < notSelectedIndices.Length; i++)
                {
                    m_selectedVerticesCount++;
                    if (m_selectedVerticesCount == 1)
                    {
                        m_centerOfMass = mesh.transform.TransformPoint(notSelectedVertices[i].position);
                    }
                    else
                    {
                        m_centerOfMass *= (m_selectedVerticesCount - 1) / (float)m_selectedVerticesCount;
                        m_centerOfMass += mesh.transform.TransformPoint(notSelectedVertices[i].position) / m_selectedVerticesCount;
                    }
                }
            }
        }

        public void Remove(ProBuilderMesh mesh, IEnumerable<int> indices = null)
        {
            HashSet<int> indicesHs;
            if (m_meshToIndices.TryGetValue(mesh, out indicesHs))
            {
                MeshFilter vertices = m_meshToSelection[mesh];
                List<int> indicesList = m_meshToIndicesList[mesh];
                if (indices != null)
                {
                    int[] selectedIndices = indices.Where(i => indicesHs.Contains(i)).ToArray();
                    SetVerticesColor(mesh, vertices, m_color, selectedIndices);
                    for (int i = 0; i < selectedIndices.Length; ++i)
                    {
                        indicesHs.Remove(selectedIndices[i]);
                        indicesList.Remove(selectedIndices[i]);
                    }

                    Vertex[] selectedVertices = mesh.GetVertices(selectedIndices);
                    UpdateCenterOfMassOnRemove(mesh.transform, selectedVertices);
                }
                else
                {
                    Vertex[] selectedVertices = mesh.GetVertices(indicesList);
                    UpdateCenterOfMassOnRemove(mesh.transform, selectedVertices);

                    indicesHs.Clear();
                    indicesList.Clear();
                }

                if(indicesHs.Count == 0)
                {
                    m_meshToIndices.Remove(mesh);
                    m_meshToIndicesList.Remove(mesh);
                    m_meshToSelection.Remove(mesh);
                    
                    Destroy(vertices.gameObject);
                    m_meshes.Remove(mesh);
                }

                if (indicesList.Count > 0)
                {
                    Vertex[] lastVertex = mesh.GetVertices(new[] { indicesList.Last() });
                    m_lastPosition = lastVertex[0].position;
                    m_lastNormal = lastVertex[0].normal;
                }
                else
                {
                    if(LastMesh != null)
                    {
                        Vertex[] lastVertex = LastMesh.GetVertices(new[] { m_meshToIndicesList[LastMesh].Last() });
                        m_lastPosition = lastVertex[0].position;
                        m_lastNormal = lastVertex[0].normal;
                    }
                    else
                    {
                        m_lastPosition = Vector3.zero;
                        m_lastPosition = Vector3.forward;
                    }
                }
            }
        }

        private void UpdateCenterOfMassOnRemove(Transform mesh, IList<Vertex> selectedVertices)
        {
            for (int i = selectedVertices.Count - 1; i >= 0; --i)
            {
                m_selectedVerticesCount--;
                if (m_selectedVerticesCount == 0)
                {
                    m_centerOfMass = mesh.position;
                }
                else if (m_selectedVerticesCount == 1)
                {
                    m_centerOfMass = mesh.TransformPoint(selectedVertices[0].position);
                }
                else
                {
                    m_centerOfMass -= mesh.TransformPoint(selectedVertices[i].position) / (m_selectedVerticesCount + 1);
                    m_centerOfMass *= (m_selectedVerticesCount + 1) / (float)m_selectedVerticesCount;
                }
            }
        }

        private void SetVerticesColor(ProBuilderMesh mesh, MeshFilter vertices, Color color, IEnumerable<int> indices)
        {
            if (IsGeometryShadersSupported)
            {
                List<int> coincident = new List<int>();
                foreach(int index in indices)
                {
                    mesh.GetCoincidentVertices(index, coincident);
                    Color[] colors = vertices.sharedMesh.colors;
                    for (int i = 0; i < coincident.Count; ++i)
                    {
                        colors[coincident[i]] = color;
                    }
                    vertices.sharedMesh.colors = colors;
                    coincident.Clear();
                }       
            }
            else
            {
                List<int> coincident = new List<int>();
                foreach (int index in indices)
                {
                    mesh.GetCoincidentVertices(index, coincident);
                    Color[] colors = vertices.sharedMesh.colors;
                    for (int i = 0; i < coincident.Count; ++i)
                    {
                        colors[coincident[i] * 4] = color;
                        colors[coincident[i] * 4 + 1] = color;
                        colors[coincident[i] * 4 + 2] = color;
                        colors[coincident[i] * 4 + 3] = color;
                    }
                    vertices.sharedMesh.colors = colors;
                    coincident.Clear();
                }
            }
        }

        private MeshFilter CreateVerticesGameObject(ProBuilderMesh mesh, IList<int> indices)
        {
            GameObject vertices = new GameObject("Vertices");
            MeshFilter meshFilter = vertices.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = vertices.AddComponent<MeshFilter>();
            }
            meshFilter.sharedMesh = new Mesh();

            MeshRenderer renderer = vertices.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = vertices.AddComponent<MeshRenderer>();
            }
            renderer.sharedMaterial = m_material;

            if (indices == null)
            {
                indices = new int[mesh.vertexCount];
                for (int i = 0; i < indices.Count; ++i)
                {
                    indices[i] = i;
                }
            }

            BuildVertexMesh(mesh, meshFilter.sharedMesh, indices);
            CopyTransform(mesh.transform, meshFilter.transform);
            return meshFilter;
        }

        private void BuildVertexMesh(ProBuilderMesh mesh, Mesh target, IList<int> indexes)
        {
            if (IsGeometryShadersSupported)
            {
                BuildVertexMeshInternal(mesh, target, indexes);
            }
            else
            {
                BuildVertexMeshLegacy(mesh, target, indexes);
            }
        }

        private void BuildVertexMeshLegacy(ProBuilderMesh mesh, Mesh target, IList<int> indexes)
        {
            const int k_MaxPointCount = int.MaxValue / 4;

            int billboardCount = indexes == null ? mesh.vertexCount : indexes.Count;

            if (billboardCount > k_MaxPointCount)
            {
                billboardCount = k_MaxPointCount;
            }

            var positions = mesh.positions;

            Vector3[] billboards = new Vector3[billboardCount * 4];
            Vector2[] uvs = new Vector2[billboardCount * 4];
            Vector2[] uv2 = new Vector2[billboardCount * 4];
            Color[] colors = new Color[billboardCount * 4];
            int[] tris = new int[billboardCount * 6];
            
            int n = 0;
            int t = 0;

            Vector3 up = Vector3.up;
            Vector3 right = Vector3.right;

            if (indexes == null)
            {
                for (int i = 0; i < billboardCount; i++)
                {
                    billboards[t + 0] = positions[i];
                    billboards[t + 1] = positions[i];
                    billboards[t + 2] = positions[i];
                    billboards[t + 3] = positions[i];
                }

                target.vertices = billboards;
            }
            else
            {
                for (int i = 0; i < billboardCount; i++)
                {
                    billboards[t + 0] = positions[indexes[i]];
                    billboards[t + 1] = positions[indexes[i]];
                    billboards[t + 2] = positions[indexes[i]];
                    billboards[t + 3] = positions[indexes[i]];

                    uvs[t + 0] = Vector3.zero;
                    uvs[t + 1] = Vector3.right;
                    uvs[t + 2] = Vector3.up;
                    uvs[t + 3] = Vector3.one;

                    uv2[t + 0] = -up - right;
                    uv2[t + 1] = -up + right;
                    uv2[t + 2] = up - right;
                    uv2[t + 3] = up + right;

                    colors[t + 0] = m_color;
                    colors[t + 1] = m_color;
                    colors[t + 2] = m_color;
                    colors[t + 3] = m_color;

                    tris[n + 0] = t + 0;
                    tris[n + 1] = t + 1;
                    tris[n + 2] = t + 2;
                    tris[n + 3] = t + 1;
                    tris[n + 4] = t + 3;
                    tris[n + 5] = t + 2;

                    t += 4;
                    n += 6;
                }

                target.Clear();
                target.vertices = billboards;
                target.uv = uvs;
                target.uv2 = uv2;
                target.colors = colors;
                target.triangles = tris;
            }          
        }

        private void BuildVertexMeshInternal(ProBuilderMesh mesh, Mesh target, IEnumerable<int> indexes)
        {
            if(indexes != null)
            {
                target.Clear();
            }
            
            target.vertices = mesh.positions.ToArray();

            if (indexes != null)
            {
                Color[] colors = new Color[target.vertexCount];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = m_color;
                }
                target.colors = colors;
                target.subMeshCount = 1;
                target.SetIndices(indexes as int[] ?? indexes.ToArray(), MeshTopology.Points, 0);
            }
        }

        private static void CopyTransform(Transform src, Transform dst)
        {
            dst.position = src.position;
            dst.rotation = src.rotation;
            dst.localScale = src.localScale;
        }

        public void Synchronize(Vector3 centerOfMass, Vector3 lastPosition, Vector3 lastNormal)
        {
            foreach (KeyValuePair<ProBuilderMesh, List<int>> kvp in m_meshToIndicesList)
            {
                ProBuilderMesh mesh = kvp.Key;
                List<int> indices = kvp.Value;

                MeshFilter meshFilter = m_meshToSelection[mesh];
                BuildVertexMesh(mesh, meshFilter.sharedMesh, null);

                meshFilter.transform.position = mesh.transform.position;
            }

            m_centerOfMass = centerOfMass;
            m_lastNormal = LastMesh.transform.InverseTransformDirection(lastNormal.normalized);  
            m_lastPosition = LastMesh.transform.InverseTransformPoint(lastPosition);    
            
        }
    }

}
