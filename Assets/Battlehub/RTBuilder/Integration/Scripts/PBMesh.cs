using System;
using System.Collections;
using System.Collections.Generic;
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

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PBMesh : MonoBehaviour
    {
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
            return result.ToBool();
        }

        public void Subdivide()
        {
            ConnectElements.Connect(m_pbMesh, m_pbMesh.faces);
            m_pbMesh.Refresh();
            m_pbMesh.ToMesh();
        }

        public void Clear()
        {
            m_pbMesh.Clear();
            m_pbMesh.Refresh();
            m_pbMesh.ToMesh();

            MeshFilter filter = m_pbMesh.GetComponent<MeshFilter>();
            filter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.zero);
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

            mesh.ToMesh();
            mesh.Refresh();
        }
    }
}
