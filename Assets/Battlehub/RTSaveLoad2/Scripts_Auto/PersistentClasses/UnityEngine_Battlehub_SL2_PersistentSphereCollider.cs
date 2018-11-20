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
    public partial class PersistentSphereCollider : PersistentCollider
    {
        [ProtoMember(256)]
        public PersistentVector3 center;

        [ProtoMember(257)]
        public float radius;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            SphereCollider uo = (SphereCollider)obj;
            center = uo.center;
            radius = uo.radius;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            SphereCollider uo = (SphereCollider)obj;
            uo.center = center;
            uo.radius = radius;
            return uo;
        }

        public static implicit operator SphereCollider(PersistentSphereCollider surrogate)
        {
            return (SphereCollider)surrogate.WriteTo(new SphereCollider());
        }
        
        public static implicit operator PersistentSphereCollider(SphereCollider obj)
        {
            PersistentSphereCollider surrogate = new PersistentSphereCollider();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

