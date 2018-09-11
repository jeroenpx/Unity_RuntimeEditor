using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using Battlehub.RTSaveLoad2.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    public partial class TypeMap
    {
        public TypeMap()
        {
            m_toPeristentType.Add(typeof(UnityObject), typeof(PersistentObject));
            m_toUnityType.Add(typeof(PersistentObject), typeof(UnityObject));
            m_toGuid.Add(typeof(PersistentObject), new System.Guid("bfa9fba0-03ac-4cd5-bccc-d5f4d90be4e9"));
            m_toGuid.Add(typeof(UnityObject), new System.Guid("bc962d0e-cc38-4a91-81c2-3f4ce91a8e4f"));
            m_toType.Add(new System.Guid("bfa9fba0-03ac-4cd5-bccc-d5f4d90be4e9"), typeof(PersistentObject));
            m_toType.Add(new System.Guid("bc962d0e-cc38-4a91-81c2-3f4ce91a8e4f"), typeof(UnityObject));
            m_toPeristentType.Add(typeof(GameObject), typeof(PersistentGameObject));
            m_toUnityType.Add(typeof(PersistentGameObject), typeof(GameObject));
            m_toGuid.Add(typeof(PersistentGameObject), new System.Guid("f676edeb-4b33-48b6-9966-e427c04fcd07"));
            m_toGuid.Add(typeof(GameObject), new System.Guid("86d6505a-0a73-4f6d-8aec-5d35797dce80"));
            m_toType.Add(new System.Guid("f676edeb-4b33-48b6-9966-e427c04fcd07"), typeof(PersistentGameObject));
            m_toType.Add(new System.Guid("86d6505a-0a73-4f6d-8aec-5d35797dce80"), typeof(GameObject));
            m_toPeristentType.Add(typeof(MeshRenderer), typeof(PersistentMeshRenderer));
            m_toUnityType.Add(typeof(PersistentMeshRenderer), typeof(MeshRenderer));
            m_toGuid.Add(typeof(PersistentMeshRenderer), new System.Guid("cbdc5791-5a58-4966-93f8-e8091b603917"));
            m_toGuid.Add(typeof(MeshRenderer), new System.Guid("df8fbd1a-e1cc-449a-a603-70ad38f1c4cb"));
            m_toType.Add(new System.Guid("cbdc5791-5a58-4966-93f8-e8091b603917"), typeof(PersistentMeshRenderer));
            m_toType.Add(new System.Guid("df8fbd1a-e1cc-449a-a603-70ad38f1c4cb"), typeof(MeshRenderer));
            m_toPeristentType.Add(typeof(MeshFilter), typeof(PersistentMeshFilter));
            m_toUnityType.Add(typeof(PersistentMeshFilter), typeof(MeshFilter));
            m_toGuid.Add(typeof(PersistentMeshFilter), new System.Guid("c7baebb1-3a94-43b0-9040-b38469cfe03e"));
            m_toGuid.Add(typeof(MeshFilter), new System.Guid("11f878a2-c81c-4260-bdb5-95595dc578b4"));
            m_toType.Add(new System.Guid("c7baebb1-3a94-43b0-9040-b38469cfe03e"), typeof(PersistentMeshFilter));
            m_toType.Add(new System.Guid("11f878a2-c81c-4260-bdb5-95595dc578b4"), typeof(MeshFilter));
            m_toPeristentType.Add(typeof(SkinnedMeshRenderer), typeof(PersistentSkinnedMeshRenderer));
            m_toUnityType.Add(typeof(PersistentSkinnedMeshRenderer), typeof(SkinnedMeshRenderer));
            m_toGuid.Add(typeof(PersistentSkinnedMeshRenderer), new System.Guid("1b186c14-a70c-4f26-9e08-df0edaeef1f5"));
            m_toGuid.Add(typeof(SkinnedMeshRenderer), new System.Guid("2b22c03c-6643-4484-8868-01b5aba1d047"));
            m_toType.Add(new System.Guid("1b186c14-a70c-4f26-9e08-df0edaeef1f5"), typeof(PersistentSkinnedMeshRenderer));
            m_toType.Add(new System.Guid("2b22c03c-6643-4484-8868-01b5aba1d047"), typeof(SkinnedMeshRenderer));
            m_toPeristentType.Add(typeof(Mesh), typeof(PersistentMesh));
            m_toUnityType.Add(typeof(PersistentMesh), typeof(Mesh));
            m_toGuid.Add(typeof(PersistentMesh), new System.Guid("a92f5cff-b3de-463c-b616-37448bef09dd"));
            m_toGuid.Add(typeof(Mesh), new System.Guid("a41c3ba8-499c-474a-8708-f0e60748c83f"));
            m_toType.Add(new System.Guid("a92f5cff-b3de-463c-b616-37448bef09dd"), typeof(PersistentMesh));
            m_toType.Add(new System.Guid("a41c3ba8-499c-474a-8708-f0e60748c83f"), typeof(Mesh));
            m_toPeristentType.Add(typeof(Transform), typeof(PersistentTransform));
            m_toUnityType.Add(typeof(PersistentTransform), typeof(Transform));
            m_toGuid.Add(typeof(PersistentTransform), new System.Guid("cda48184-0d5b-42f8-9cd8-f76d46a57094"));
            m_toGuid.Add(typeof(Transform), new System.Guid("8fdc652f-89f0-456d-8eaf-f8424af712a9"));
            m_toType.Add(new System.Guid("cda48184-0d5b-42f8-9cd8-f76d46a57094"), typeof(PersistentTransform));
            m_toType.Add(new System.Guid("8fdc652f-89f0-456d-8eaf-f8424af712a9"), typeof(Transform));
            m_toPeristentType.Add(typeof(RuntimePrefab), typeof(PersistentRuntimePrefab));
            m_toUnityType.Add(typeof(PersistentRuntimePrefab), typeof(RuntimePrefab));
            m_toGuid.Add(typeof(PersistentRuntimePrefab), new System.Guid("c66f920b-d4a3-44a5-83c5-19610e638ac2"));
            m_toGuid.Add(typeof(RuntimePrefab), new System.Guid("fb4c725d-3157-4337-bc3b-d7946e554fa5"));
            m_toType.Add(new System.Guid("c66f920b-d4a3-44a5-83c5-19610e638ac2"), typeof(PersistentRuntimePrefab));
            m_toType.Add(new System.Guid("fb4c725d-3157-4337-bc3b-d7946e554fa5"), typeof(RuntimePrefab));
            m_toPeristentType.Add(typeof(RuntimeScene), typeof(PersistentRuntimeScene));
            m_toUnityType.Add(typeof(PersistentRuntimeScene), typeof(RuntimeScene));
            m_toGuid.Add(typeof(PersistentRuntimeScene), new System.Guid("67e02331-47ab-461e-b5bf-b7605dcb8331"));
            m_toGuid.Add(typeof(RuntimeScene), new System.Guid("bfb37545-dc71-428f-aa9f-edfe3a4b1408"));
            m_toType.Add(new System.Guid("67e02331-47ab-461e-b5bf-b7605dcb8331"), typeof(PersistentRuntimeScene));
            m_toType.Add(new System.Guid("bfb37545-dc71-428f-aa9f-edfe3a4b1408"), typeof(RuntimeScene));
            m_toPeristentType.Add(typeof(Vector3), typeof(PersistentVector3));
            m_toUnityType.Add(typeof(PersistentVector3), typeof(Vector3));
            m_toGuid.Add(typeof(PersistentVector3), new System.Guid("c5b85b21-747e-490a-b8a3-ad7edf4cda34"));
            m_toGuid.Add(typeof(Vector3), new System.Guid("993a554f-9524-4f9d-b9c3-ba76086d6d3d"));
            m_toType.Add(new System.Guid("c5b85b21-747e-490a-b8a3-ad7edf4cda34"), typeof(PersistentVector3));
            m_toType.Add(new System.Guid("993a554f-9524-4f9d-b9c3-ba76086d6d3d"), typeof(Vector3));
            m_toPeristentType.Add(typeof(Quaternion), typeof(PersistentQuaternion));
            m_toUnityType.Add(typeof(PersistentQuaternion), typeof(Quaternion));
            m_toGuid.Add(typeof(PersistentQuaternion), new System.Guid("04e8ba08-f06b-41fd-be90-17134c33c6ea"));
            m_toGuid.Add(typeof(Quaternion), new System.Guid("2dc2e69f-d44a-4f12-8de4-25f509ed12bf"));
            m_toType.Add(new System.Guid("04e8ba08-f06b-41fd-be90-17134c33c6ea"), typeof(PersistentQuaternion));
            m_toType.Add(new System.Guid("2dc2e69f-d44a-4f12-8de4-25f509ed12bf"), typeof(Quaternion));
            
        }
    }
}

