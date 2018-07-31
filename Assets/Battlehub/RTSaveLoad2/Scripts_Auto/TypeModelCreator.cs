using ProtoBuf.Meta;
using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
   public static partial class TypeModelCreator
   {
       static partial void RegisterAutoTypes(RuntimeTypeModel model)
       {
            model.Add(typeof(PersistentObject), true)
                .AddSubType(1025, typeof(PersistentGameObject))
                .AddSubType(1026, typeof(PersistentTransform))
                .AddSubType(1027, typeof(PersistentMesh))
                .AddSubType(1028, typeof(PersistentMeshFilter));
            model.Add(typeof(PersistentMesh), true);
            model.Add(typeof(PersistentGameObject), true);
            model.Add(typeof(PersistentMeshFilter), true);
            model.Add(typeof(PersistentTransform), true);
            model.Add(typeof(Vector3), false).SetSurrogate(typeof(PersistentVector3));
            model.Add(typeof(Quaternion), false).SetSurrogate(typeof(PersistentQuaternion));
            
       }
   }
}

