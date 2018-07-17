using UnityEngine;
using UnityEngine.Battlehub.SL2;
using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    public partial class TypeMap
    {
        public TypeMap()
        {
            m_toPeristentType.Add(typeof(UnityObject), typeof(PersistentObject));
            m_toUnityType.Add(typeof(PersistentObject), typeof(UnityObject));
            m_toPeristentType.Add(typeof(Material), typeof(PersistentMaterial));
            m_toUnityType.Add(typeof(PersistentMaterial), typeof(Material));
            m_toPeristentType.Add(typeof(Shader), typeof(PersistentShader));
            m_toUnityType.Add(typeof(PersistentShader), typeof(Shader));
            m_toPeristentType.Add(typeof(Texture), typeof(PersistentTexture));
            m_toUnityType.Add(typeof(PersistentTexture), typeof(Texture));
            m_toPeristentType.Add(typeof(GameObject), typeof(PersistentGameObject));
            m_toUnityType.Add(typeof(PersistentGameObject), typeof(GameObject));
            m_toPeristentType.Add(typeof(MeshRenderer), typeof(PersistentMeshRenderer));
            m_toUnityType.Add(typeof(PersistentMeshRenderer), typeof(MeshRenderer));
            m_toPeristentType.Add(typeof(Mesh), typeof(PersistentMesh));
            m_toUnityType.Add(typeof(PersistentMesh), typeof(Mesh));
            m_toPeristentType.Add(typeof(Vector2), typeof(PersistentVector2));
            m_toUnityType.Add(typeof(PersistentVector2), typeof(Vector2));
            m_toPeristentType.Add(typeof(Color), typeof(PersistentColor));
            m_toUnityType.Add(typeof(PersistentColor), typeof(Color));
            m_toPeristentType.Add(typeof(Vector4), typeof(PersistentVector4));
            m_toUnityType.Add(typeof(PersistentVector4), typeof(Vector4));
            m_toPeristentType.Add(typeof(Vector2Int), typeof(PersistentVector2Int));
            m_toUnityType.Add(typeof(PersistentVector2Int), typeof(Vector2Int));
            m_toPeristentType.Add(typeof(Vector3Int), typeof(PersistentVector3Int));
            m_toUnityType.Add(typeof(PersistentVector3Int), typeof(Vector3Int));
            
        }
    }
}

