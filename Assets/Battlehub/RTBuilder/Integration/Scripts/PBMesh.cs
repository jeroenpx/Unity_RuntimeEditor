using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Battlehub.ProBuilderIntegration
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PBMesh : MonoBehaviour
    {
        private ProBuilderMesh m_pbMesh;
        private MeshFilter m_meshFilter;

        private void Awake()
        {
            m_meshFilter = GetComponent<MeshFilter>();
            m_pbMesh = GetComponent<ProBuilderMesh>();
            if(m_pbMesh == null)
            {
                m_pbMesh = gameObject.AddComponent<ProBuilderMesh>();
                ImportMesh(m_meshFilter, m_pbMesh);
            }
        }

        private void OnDestroy()
        {
            if(m_pbMesh != null)
            {
                Destroy(m_pbMesh);
            }
        }

        public static PBMesh Create()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "PBMesh";

            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.sharedMaterial = Resources.Load<Material>("Materials/PBMeshDefault");

            return ProBuilderize(cube);
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

        private static void ImportMesh(MeshFilter filter, ProBuilderMesh mesh)
        {
            MeshImporter importer = new MeshImporter(mesh);
            importer.Import(filter.sharedMesh);

            filter.sharedMesh = new Mesh();

            mesh.ToMesh();
            mesh.Refresh();
        }
    }
}
