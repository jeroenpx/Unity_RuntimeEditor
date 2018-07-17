using ProtoBuf.Meta;
using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
   public static partial class TypeModelCreator
   {
       static partial void RegisterAutoTypes(RuntimeTypeModel model)
       {
            model.Add(typeof(PersistentObject), true)
                .AddSubType(1025, typeof(PersistentGameObject))
                .AddSubType(1026, typeof(PersistentMeshRenderer))
                .AddSubType(1027, typeof(PersistentMesh))
                .AddSubType(1028, typeof(PersistentMaterial))
                .AddSubType(1029, typeof(PersistentShader))
                .AddSubType(1030, typeof(PersistentTexture));
            model.Add(typeof(PersistentMaterial), true);
            model.Add(typeof(PersistentShader), true);
            model.Add(typeof(PersistentTexture), true);
            model.Add(typeof(PersistentGameObject), true);
            model.Add(typeof(PersistentMeshRenderer), true);
            model.Add(typeof(PersistentMesh), true);
            model.Add(typeof(Vector2), false).SetSurrogate(typeof(PersistentVector2));
            model.Add(typeof(Color), false).SetSurrogate(typeof(PersistentColor));
            model.Add(typeof(Vector4), false).SetSurrogate(typeof(PersistentVector4));
            model.Add(typeof(Vector2Int), false).SetSurrogate(typeof(PersistentVector2Int));
            model.Add(typeof(Vector3Int), false).SetSurrogate(typeof(PersistentVector3Int));
            
       }
   }
}

