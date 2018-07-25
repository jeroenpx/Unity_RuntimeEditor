using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentBoxCollider : PersistentObject
    {
        
        public static implicit operator BoxCollider(PersistentBoxCollider surrogate)
        {
            return (BoxCollider)surrogate.WriteTo(new BoxCollider());
        }
        
        public static implicit operator PersistentBoxCollider(BoxCollider obj)
        {
            PersistentBoxCollider surrogate = new PersistentBoxCollider();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

