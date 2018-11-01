using ProtoBuf.Meta;
using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;
using Battlehub.RTSaveLoad2.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
   public static partial class TypeModelCreator
   {
       static partial void RegisterAutoTypes(RuntimeTypeModel model)
       {
            model.Add(typeof(PersistentObject), true)
                .AddSubType(1025, typeof(PersistentGameObject))
                .AddSubType(1027, typeof(PersistentMeshFilter))
                .AddSubType(1029, typeof(PersistentTransform))
                .AddSubType(1030, typeof(PersistentRuntimePrefab))
                .AddSubType(1031, typeof(PersistentMesh))
                .AddSubType(1032, typeof(PersistentMaterial))
                .AddSubType(1033, typeof(PersistentRenderer));
            model.Add(typeof(PersistentGameObject), true);
            model.Add(typeof(PersistentMeshRenderer), true);
            model.Add(typeof(PersistentMeshFilter), true);
            model.Add(typeof(PersistentSkinnedMeshRenderer), true);
            model.Add(typeof(PersistentMesh), true);
            model.Add(typeof(PersistentMaterial), true);
            model.Add(typeof(PersistentRenderer), true)
                .AddSubType(1025, typeof(PersistentMeshRenderer))
                .AddSubType(1026, typeof(PersistentSkinnedMeshRenderer));
            model.Add(typeof(PersistentTransform), true);
            model.Add(typeof(PersistentRuntimePrefab), true)
                .AddSubType(1025, typeof(PersistentRuntimeScene));
            model.Add(typeof(PersistentRuntimeScene), true);
            model.Add(typeof(Vector3), false).SetSurrogate(typeof(PersistentVector3));
            model.Add(typeof(Quaternion), false).SetSurrogate(typeof(PersistentQuaternion));
            
       }
   }
}

