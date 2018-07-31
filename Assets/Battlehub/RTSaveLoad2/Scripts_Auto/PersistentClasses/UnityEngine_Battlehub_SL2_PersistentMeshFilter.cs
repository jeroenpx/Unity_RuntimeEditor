using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentMeshFilter : PersistentObject
    {
        
        public static implicit operator MeshFilter(PersistentMeshFilter surrogate)
        {
            return (MeshFilter)surrogate.WriteTo(new MeshFilter());
        }
        
        public static implicit operator PersistentMeshFilter(MeshFilter obj)
        {
            PersistentMeshFilter surrogate = new PersistentMeshFilter();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

