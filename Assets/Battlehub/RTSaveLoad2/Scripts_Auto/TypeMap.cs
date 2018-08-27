using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
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
            m_toGuid.Add(typeof(PersistentObject), new System.Guid("9b3f1cd7-46e1-46ff-ba5f-67bfc7862fac"));
            m_toGuid.Add(typeof(UnityObject), new System.Guid("d637d7ca-70c4-4911-833f-d0f97c142159"));
            m_toType.Add(new System.Guid("9b3f1cd7-46e1-46ff-ba5f-67bfc7862fac"), typeof(PersistentObject));
            m_toType.Add(new System.Guid("d637d7ca-70c4-4911-833f-d0f97c142159"), typeof(UnityObject));
            m_toPeristentType.Add(typeof(GameObject), typeof(PersistentGameObject));
            m_toUnityType.Add(typeof(PersistentGameObject), typeof(GameObject));
            m_toGuid.Add(typeof(PersistentGameObject), new System.Guid("4034da7d-49a1-452e-8439-4d89416cc6fe"));
            m_toGuid.Add(typeof(GameObject), new System.Guid("01f9f26d-4108-4256-aefb-3de0fc78f6c9"));
            m_toType.Add(new System.Guid("4034da7d-49a1-452e-8439-4d89416cc6fe"), typeof(PersistentGameObject));
            m_toType.Add(new System.Guid("01f9f26d-4108-4256-aefb-3de0fc78f6c9"), typeof(GameObject));
            m_toPeristentType.Add(typeof(MeshRenderer), typeof(PersistentMeshRenderer));
            m_toUnityType.Add(typeof(PersistentMeshRenderer), typeof(MeshRenderer));
            m_toGuid.Add(typeof(PersistentMeshRenderer), new System.Guid("a8b5be48-6753-482e-9682-f573f9ad7aa0"));
            m_toGuid.Add(typeof(MeshRenderer), new System.Guid("d42f86df-e891-4e6c-bba3-b9c1bd6600d0"));
            m_toType.Add(new System.Guid("a8b5be48-6753-482e-9682-f573f9ad7aa0"), typeof(PersistentMeshRenderer));
            m_toType.Add(new System.Guid("d42f86df-e891-4e6c-bba3-b9c1bd6600d0"), typeof(MeshRenderer));
            m_toPeristentType.Add(typeof(MeshFilter), typeof(PersistentMeshFilter));
            m_toUnityType.Add(typeof(PersistentMeshFilter), typeof(MeshFilter));
            m_toGuid.Add(typeof(PersistentMeshFilter), new System.Guid("44f84a76-57db-43f8-a8a2-df45b159f61a"));
            m_toGuid.Add(typeof(MeshFilter), new System.Guid("41c59fc3-ae01-4873-b5ce-edbc287037a7"));
            m_toType.Add(new System.Guid("44f84a76-57db-43f8-a8a2-df45b159f61a"), typeof(PersistentMeshFilter));
            m_toType.Add(new System.Guid("41c59fc3-ae01-4873-b5ce-edbc287037a7"), typeof(MeshFilter));
            m_toPeristentType.Add(typeof(Mesh), typeof(PersistentMesh));
            m_toUnityType.Add(typeof(PersistentMesh), typeof(Mesh));
            m_toGuid.Add(typeof(PersistentMesh), new System.Guid("eed671f9-a1f0-4212-8d2c-8687905ee48d"));
            m_toGuid.Add(typeof(Mesh), new System.Guid("a5eaa044-c521-4e7b-9036-38f6b53a1ca9"));
            m_toType.Add(new System.Guid("eed671f9-a1f0-4212-8d2c-8687905ee48d"), typeof(PersistentMesh));
            m_toType.Add(new System.Guid("a5eaa044-c521-4e7b-9036-38f6b53a1ca9"), typeof(Mesh));
            m_toPeristentType.Add(typeof(Material), typeof(PersistentMaterial));
            m_toUnityType.Add(typeof(PersistentMaterial), typeof(Material));
            m_toGuid.Add(typeof(PersistentMaterial), new System.Guid("19070cbe-8860-4215-a20e-d534502dc238"));
            m_toGuid.Add(typeof(Material), new System.Guid("5cd5086e-c045-483c-b174-f52679ed7bce"));
            m_toType.Add(new System.Guid("19070cbe-8860-4215-a20e-d534502dc238"), typeof(PersistentMaterial));
            m_toType.Add(new System.Guid("5cd5086e-c045-483c-b174-f52679ed7bce"), typeof(Material));
            m_toPeristentType.Add(typeof(Sprite), typeof(PersistentSprite));
            m_toUnityType.Add(typeof(PersistentSprite), typeof(Sprite));
            m_toGuid.Add(typeof(PersistentSprite), new System.Guid("18bd8024-9705-4d66-a152-3d2497ec12dc"));
            m_toGuid.Add(typeof(Sprite), new System.Guid("e030714e-ea73-47ef-9cfd-bd8b8d18fd5c"));
            m_toType.Add(new System.Guid("18bd8024-9705-4d66-a152-3d2497ec12dc"), typeof(PersistentSprite));
            m_toType.Add(new System.Guid("e030714e-ea73-47ef-9cfd-bd8b8d18fd5c"), typeof(Sprite));
            m_toPeristentType.Add(typeof(Transform), typeof(PersistentTransform));
            m_toUnityType.Add(typeof(PersistentTransform), typeof(Transform));
            m_toGuid.Add(typeof(PersistentTransform), new System.Guid("0686f58d-e517-4d32-9e03-55a78f8a1105"));
            m_toGuid.Add(typeof(Transform), new System.Guid("6cd597a6-8cbb-440a-ba82-04efdfcfcd2d"));
            m_toType.Add(new System.Guid("0686f58d-e517-4d32-9e03-55a78f8a1105"), typeof(PersistentTransform));
            m_toType.Add(new System.Guid("6cd597a6-8cbb-440a-ba82-04efdfcfcd2d"), typeof(Transform));
            m_toPeristentType.Add(typeof(Vector2), typeof(PersistentVector2));
            m_toUnityType.Add(typeof(PersistentVector2), typeof(Vector2));
            m_toGuid.Add(typeof(PersistentVector2), new System.Guid("64c8e5c7-ff1d-4c12-8595-52dcc21779f0"));
            m_toGuid.Add(typeof(Vector2), new System.Guid("20ace8eb-e571-43af-aa2f-ce0781258eb7"));
            m_toType.Add(new System.Guid("64c8e5c7-ff1d-4c12-8595-52dcc21779f0"), typeof(PersistentVector2));
            m_toType.Add(new System.Guid("20ace8eb-e571-43af-aa2f-ce0781258eb7"), typeof(Vector2));
            m_toPeristentType.Add(typeof(Vector3), typeof(PersistentVector3));
            m_toUnityType.Add(typeof(PersistentVector3), typeof(Vector3));
            m_toGuid.Add(typeof(PersistentVector3), new System.Guid("98f71be1-228a-4891-8f80-cff7c72322ca"));
            m_toGuid.Add(typeof(Vector3), new System.Guid("6f31166d-2bbe-4aa2-b2d3-cd0f7886d131"));
            m_toType.Add(new System.Guid("98f71be1-228a-4891-8f80-cff7c72322ca"), typeof(PersistentVector3));
            m_toType.Add(new System.Guid("6f31166d-2bbe-4aa2-b2d3-cd0f7886d131"), typeof(Vector3));
            m_toPeristentType.Add(typeof(Quaternion), typeof(PersistentQuaternion));
            m_toUnityType.Add(typeof(PersistentQuaternion), typeof(Quaternion));
            m_toGuid.Add(typeof(PersistentQuaternion), new System.Guid("252577dc-5c3e-45fe-af08-38ee96361d9b"));
            m_toGuid.Add(typeof(Quaternion), new System.Guid("29780e6c-46c3-4625-82b3-e0fdb1581be5"));
            m_toType.Add(new System.Guid("252577dc-5c3e-45fe-af08-38ee96361d9b"), typeof(PersistentQuaternion));
            m_toType.Add(new System.Guid("29780e6c-46c3-4625-82b3-e0fdb1581be5"), typeof(Quaternion));
            
        }
    }
}

