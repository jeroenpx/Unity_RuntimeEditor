using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentFlare : PersistentObject
    {
        
        public static implicit operator Flare(PersistentFlare surrogate)
        {
            return (Flare)surrogate.WriteTo(new Flare());
        }
        
        public static implicit operator PersistentFlare(Flare obj)
        {
            PersistentFlare surrogate = new PersistentFlare();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

