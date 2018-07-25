using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentVector3 : PersistentSurrogate
    {
        [ProtoMember(256)]
        public float x;

        [ProtoMember(257)]
        public float y;

        [ProtoMember(258)]
        public float z;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Vector3 uo = (Vector3)obj;
            x = uo.x;
            y = uo.y;
            z = uo.z;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Vector3 uo = (Vector3)obj;
            uo.x = x;
            uo.y = y;
            uo.z = z;
            return obj;
        }

        public static implicit operator Vector3(PersistentVector3 surrogate)
        {
            return (Vector3)surrogate.WriteTo(new Vector3());
        }
        
        public static implicit operator PersistentVector3(Vector3 obj)
        {
            PersistentVector3 surrogate = new PersistentVector3();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

