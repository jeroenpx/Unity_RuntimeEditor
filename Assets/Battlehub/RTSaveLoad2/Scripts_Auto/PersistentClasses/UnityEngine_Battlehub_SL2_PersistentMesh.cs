using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentMesh : PersistentObject
    {
        
        public static implicit operator Mesh(PersistentMesh surrogate)
        {
            return (Mesh)surrogate.WriteTo(new Mesh());
        }
        
        public static implicit operator PersistentMesh(Mesh obj)
        {
            PersistentMesh surrogate = new PersistentMesh();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

