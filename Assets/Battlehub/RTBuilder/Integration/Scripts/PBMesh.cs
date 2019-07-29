using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Battlehub.ProBuilderIntegration
{
    public struct PBFace
    {
        public int[] Indexes;
        public int SubmeshIndex;

        public PBFace(Face face)
        {
            Indexes = face.indexes.ToArray();
            SubmeshIndex = face.submeshIndex;
        }

        public Face ToFace()
        {
            Face face = new Face(Indexes);
            face.submeshIndex = SubmeshIndex;
            return face;
        }
    }

    public delegate void PBMeshEvent();
    public delegate void PBMeshEvent<T>(T arg);

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PBMesh : MonoBehaviour
    {
        public event PBMeshEvent<bool> Selected;
        public event PBMeshEvent Unselected;
        public event PBMeshEvent<bool> Changed;

        private ProBuilderMesh m_pbMesh;
        private MeshFilter m_meshFilter;

        private PBFace[] m_faces;
        public PBFace[] Faces
        {
            get
            {
                m_faces = m_pbMesh.faces.Select(f => new PBFace(f)).ToArray();
                return m_faces;
            }
            set { m_faces = value; }
        }

        private Vector3[] m_positions;
        public Vector3[] Positions
        {
            get
            {
                m_positions = m_pbMesh.positions.ToArray();
                return m_positions;
            }
            set { m_positions = value; }
        }

        internal ProBuilderMesh Mesh
        {
            get { return m_pbMesh; }
        }

        private void Awake()
        {
            m_meshFilter = GetComponent<MeshFilter>();
            m_pbMesh = GetComponent<ProBuilderMesh>();
            if (m_pbMesh == null)
            {
                m_pbMesh = gameObject.AddComponent<ProBuilderMesh>();
                if (m_positions != null)
                {
                    Face[] faces = m_faces.Select(f => f.ToFace()).ToArray();
                    m_pbMesh.RebuildWithPositionsAndFaces(m_positions, faces);

                    IList<Face> actualFaces = m_pbMesh.faces;
                    for(int i = 0; i < actualFaces.Count; ++i)
                    {
                        actualFaces[i].submeshIndex = m_faces[i].SubmeshIndex;
                    }

                    m_pbMesh.Refresh();
                    m_pbMesh.ToMesh();
                }
                else
                {
                    ImportMesh(m_meshFilter, m_pbMesh);
                }
            }
        }

        private void OnDestroy()
        {
            if(m_pbMesh != null)
            {
                Destroy(m_pbMesh);
            }
        }

        public bool CreateShapeFromPolygon(IList<Vector3> points, float extrude, bool flipNormals)
        {
            ActionResult result = m_pbMesh.CreateShapeFromPolygon(points, extrude, flipNormals);
            RaiseChanged(false);
            return result.ToBool();
        }

        public void Subdivide()
        {
            ConnectElements.Connect(m_pbMesh, m_pbMesh.faces);
            m_pbMesh.Refresh();
            m_pbMesh.ToMesh();

            RaiseChanged(false);
        }

        public void CenterPivot()
        {
            m_pbMesh.CenterPivot(null);

            RaiseChanged(false);
        }

        public void Clear()
        {
            m_pbMesh.Clear();
            m_pbMesh.Refresh();
            m_pbMesh.ToMesh();

            MeshFilter filter = m_pbMesh.GetComponent<MeshFilter>();
            filter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.zero);

            RaiseChanged(false);
        }

        public void Refresh()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if(filter != null)
            {
                filter.sharedMesh = new Mesh();// filter.mesh;

                m_pbMesh.ToMesh();
                m_pbMesh.Refresh();
            }

            RaiseChanged(false);
        }

        public void RaiseSelected(bool clear)
        {
            if(Selected != null)
            {
                Selected(clear);
            }
        }

        public void RaiseChanged(bool positionsOnly)
        {
            if(Changed != null)
            {
                Changed(positionsOnly);
            }
        }

        public void RaiseUnselected()
        {
            if(Unselected != null)
            {
                Unselected(); 
            }
        }

        public void BuildEdgeMesh(Mesh target, Color color, bool positionsOnly)
        {
            IList<Vector3> positions = m_pbMesh.positions;

            int edgeIndex = 0;
            int edgeCount = 0;
            int faceCount = m_pbMesh.faceCount;

            IList<Face> faces = m_pbMesh.faces;
            for (int i = 0; i < faceCount; i++)
            {
                edgeCount += faces[i].edges.Count;
            }
            edgeCount = System.Math.Min(edgeCount, int.MaxValue / 2 - 1);

            int[] tris;
            Vector3[] vertices;
            if (positionsOnly)
            {
                vertices = target.vertices;
                tris = null;
            }
            else
            {
                tris = new int[edgeCount * 2];
                vertices = new Vector3[edgeCount * 2];
            }

            for (int i = 0; i < faceCount && edgeIndex < edgeCount; i++)
            {
                ReadOnlyCollection<Edge> edges = faces[i].edges;
                for (int n = 0; n < edges.Count && edgeIndex < edgeCount; n++)
                {
                    Edge edge = edges[n];

                    int positionIndex = edgeIndex * 2;

                    vertices[positionIndex + 0] = positions[edge.a];
                    vertices[positionIndex + 1] = positions[edge.b];

                    if (!positionsOnly)
                    {
                        tris[positionIndex + 0] = positionIndex + 0;
                        tris[positionIndex + 1] = positionIndex + 1;
                    }

                    edgeIndex++;
                }
            }

            if (!positionsOnly)
            {
                target.Clear();
                target.name = "EdgeMesh" + target.GetInstanceID();
                target.vertices = vertices.ToArray();
                Color[] colors = new Color[target.vertexCount];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = color;
                }
                target.colors = colors;
                target.subMeshCount = 1;
                target.SetIndices(tris, MeshTopology.Lines, 0);
            }
            else
            {
                target.vertices = vertices.ToArray();
            }
        }


        public static PBMesh ProBuilderize(GameObject gameObject)
        {
            PBMesh mesh = gameObject.GetComponent<PBMesh>();
            if (mesh != null)
            {
                return mesh;
            }

            return gameObject.AddComponent<PBMesh>();
        }

        public static void ImportMesh(ProBuilderMesh mesh)
        {
            MeshFilter filter = mesh.GetComponent<MeshFilter>();
            ImportMesh(filter, mesh);
        }

        private static void ImportMesh(MeshFilter filter, ProBuilderMesh mesh)
        {
            MeshImporter importer = new MeshImporter(mesh);
            Renderer renderer = mesh.GetComponent<Renderer>();
            importer.Import(filter.sharedMesh, renderer.sharedMaterials );

            filter.sharedMesh = new Mesh();

            foreach(Face face in mesh.faces)
            {
                face.manualUV = false;
            }

            mesh.ToMesh();
            mesh.Refresh();
        }

        
    }
}
