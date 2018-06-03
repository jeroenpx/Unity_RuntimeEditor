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
            model.Add(typeof(PersistentMaterial), true);
            model.Add(typeof(PersistentShader), true);
            model.Add(typeof(PersistentTexture), true);
            model.Add(typeof(Vector2), false).SetSurrogate(typeof(PersistentVector2));
            model.Add(typeof(Color), false).SetSurrogate(typeof(PersistentColor));
            
       }
   }
}

