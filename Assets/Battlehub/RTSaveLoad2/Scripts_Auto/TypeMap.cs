using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    public partial class TypeMap
    {
        public TypeMap()
        {
            m_toPeristentType.Add(typeof(UnityObject), typeof(PersistentObject));
            m_toUnityType.Add(typeof(PersistentObject), typeof(UnityObject));
            m_toPeristentType.Add(typeof(GameObject), typeof(PersistentGameObject));
            m_toUnityType.Add(typeof(PersistentGameObject), typeof(GameObject));
            m_toPeristentType.Add(typeof(MeshRenderer), typeof(PersistentMeshRenderer));
            m_toUnityType.Add(typeof(PersistentMeshRenderer), typeof(MeshRenderer));
            m_toPeristentType.Add(typeof(MeshFilter), typeof(PersistentMeshFilter));
            m_toUnityType.Add(typeof(PersistentMeshFilter), typeof(MeshFilter));
            m_toPeristentType.Add(typeof(Mesh), typeof(PersistentMesh));
            m_toUnityType.Add(typeof(PersistentMesh), typeof(Mesh));
            m_toPeristentType.Add(typeof(Transform), typeof(PersistentTransform));
            m_toUnityType.Add(typeof(PersistentTransform), typeof(Transform));
            m_toPeristentType.Add(typeof(BoxCollider), typeof(PersistentBoxCollider));
            m_toUnityType.Add(typeof(PersistentBoxCollider), typeof(BoxCollider));
            m_toPeristentType.Add(typeof(Vector3), typeof(PersistentVector3));
            m_toUnityType.Add(typeof(PersistentVector3), typeof(Vector3));
            m_toPeristentType.Add(typeof(Quaternion), typeof(PersistentQuaternion));
            m_toUnityType.Add(typeof(PersistentQuaternion), typeof(Quaternion));
            m_toPeristentType.Add(typeof(Vector4), typeof(PersistentVector4));
            m_toUnityType.Add(typeof(PersistentVector4), typeof(Vector4));
            
        }
    }
}

