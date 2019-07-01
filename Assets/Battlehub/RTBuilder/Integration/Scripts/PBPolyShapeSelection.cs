using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public class PBPolyShapeSelection : MonoBehaviour
    {
        [SerializeField]
        private Color m_edgesColor = Color.green;
        [SerializeField]
        private Color m_vertexColor = new Color(0.33f, 0.33f, 0.33f);
        [SerializeField]
        private Color m_vertexSelectedColor = Color.yellow;
        [SerializeField]
        private Color m_vertexHoverColor = new Color(1, 1, 0, 0.75f);

        private readonly List<Vector3> m_positions = new List<Vector3>();
        public IList<Vector3> Positions
        {
            get { return m_positions; }
        }

        private int m_selectedIndex = -1;
        private int m_hoveredIndex = -1;

        private MeshFilter m_polyShapeSelection;

        private void Awake()
        {
            GameObject go = new GameObject("polyshapeSelection");
            go.transform.SetParent(transform, false);
            m_polyShapeSelection = go.AddComponent<MeshFilter>();
            m_polyShapeSelection.mesh = new Mesh();

            Renderer renderer = go.AddComponent<MeshRenderer>();
            string vertShader = BuiltinMaterials.geometryShadersSupported ?
               BuiltinMaterials.pointShader :
               BuiltinMaterials.dotShader;
            renderer.sharedMaterial = new Material(Shader.Find(vertShader));
        }

        private void OnDestroy()
        {
            Destroy(m_polyShapeSelection.gameObject);
        }

        public void Add(Vector3 position)
        {
            m_positions.Add(position);

            BuildVertexMesh(m_positions, m_polyShapeSelection.sharedMesh);
            SetVertexColors();
        }

        public void Insert(int index, Vector3 position)
        {
            m_positions.Insert(index, position);
            BuildVertexMesh(m_positions, m_polyShapeSelection.sharedMesh);

            if(m_hoveredIndex >= index)
            {
                m_hoveredIndex++;
            }

            if(m_selectedIndex >= index)
            {
                m_selectedIndex++;
            }
            SetVertexColors();
        }

        public void RemoveAt(int index)
        {
            if(index < m_hoveredIndex)
            {                
                m_hoveredIndex--;
            }

           if(index < m_selectedIndex)
            {
                m_selectedIndex--;
            }

            m_positions.RemoveAt(index);
            BuildVertexMesh(m_positions, m_polyShapeSelection.sharedMesh);
            SetVertexColors();
        }

        private void SetVertexColors()
        {
            if (m_hoveredIndex >= 0)
            {
                SetVerticesColor(m_polyShapeSelection, m_vertexHoverColor, new[] { m_hoveredIndex });
            }

            if (m_selectedIndex >= 0)
            {
                SetVerticesColor(m_polyShapeSelection, m_vertexSelectedColor, new[] { m_selectedIndex });
            }
        }


        public void Hover(int index)
        {
            Leave();
            SetVerticesColor(m_polyShapeSelection, m_vertexHoverColor, new[] { index });
        }

        public void Leave()
        {
            if(m_hoveredIndex >= 0)
            {
                if (m_selectedIndex == m_hoveredIndex)
                {
                    SetVerticesColor(m_polyShapeSelection, m_vertexSelectedColor, new[] { m_hoveredIndex });
                }
                else
                {
                    SetVerticesColor(m_polyShapeSelection, m_vertexColor, new[] { m_hoveredIndex });
                }

                m_hoveredIndex = -1;
            }
        }

        public void Select(int index)
        {
            Unselect();
            SetVerticesColor(m_polyShapeSelection, m_vertexSelectedColor, new[] { index });
        }

        public void Unselect()
        {
            if (m_selectedIndex >= 0)
            {
                if (m_selectedIndex == m_hoveredIndex)
                {
                    SetVerticesColor(m_polyShapeSelection, m_vertexHoverColor, new[] { m_selectedIndex });
                }
                else
                {
                    SetVerticesColor(m_polyShapeSelection, m_vertexColor, new[] { m_selectedIndex });
                }

                m_selectedIndex = -1;
            }
        }

        public void Clear()
        {
            m_positions.Clear();
            BuildVertexMesh(m_positions, m_polyShapeSelection.sharedMesh);
        }

        private void SetVerticesColor(MeshFilter vertices, Color color, IEnumerable<int> indices)
        {
            if (BuiltinMaterials.geometryShadersSupported)
            {
                foreach (int index in indices)
                {
                    Color[] colors = vertices.sharedMesh.colors;
                    colors[index] = color;
                    vertices.sharedMesh.colors = colors;
                }
            }
            else
            {
                foreach (int index in indices)
                {
                    Color[] colors = vertices.sharedMesh.colors;

                    colors[index * 4] = color;
                    colors[index * 4 + 1] = color;
                    colors[index * 4 + 2] = color;
                    colors[index * 4 + 3] = color;

                    vertices.sharedMesh.colors = colors;
                }
            }
        }

        private void BuildVertexMesh(IList<Vector3> positions, Mesh target)
        {
            PBUtility.BuildVertexMesh(positions, m_vertexColor, target);
        }

        private void BuildEdgeMesh(IList<Vector3> positions, Mesh target, bool positionsOnly)
        {
            int edgeCount = positions.Count - 1;

            int[] tris;
            if (positionsOnly)
            {
                tris = null;
            }
            else
            {
                tris = new int[edgeCount * 2];
            }

            if (!positionsOnly)
            {
                for (int i = 0; i < edgeCount; ++i)
                {
                    tris[i * 2 + 0] = i + 0;
                    tris[i * 2 + 1] = i + 1;
                }

                target.Clear();
                target.name = "EdgeMesh" + target.GetInstanceID();
                target.vertices = positions.ToArray();
                Color[] colors = new Color[target.vertexCount];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = m_edgesColor;
                }
                target.colors = colors;
                target.subMeshCount = 1;
                target.SetIndices(tris, MeshTopology.Lines, 0);
            }
            else
            {
                target.vertices = positions.ToArray();
            }
        }

    }

}
