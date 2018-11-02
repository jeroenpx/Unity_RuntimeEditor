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
    public partial class PersistentCollider : PersistentComponent
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public bool isTrigger;

        [ProtoMember(258)]
        public float contactOffset;

        [ProtoMember(259)]
        public long sharedMaterial;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Collider uo = (Collider)obj;
            enabled = uo.enabled;
            isTrigger = uo.isTrigger;
            contactOffset = uo.contactOffset;
            sharedMaterial = ToID(uo.sharedMaterial);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Collider uo = (Collider)obj;
            uo.enabled = enabled;
            uo.isTrigger = isTrigger;
            uo.contactOffset = contactOffset;
            uo.sharedMaterial = FromID<PhysicMaterial>(sharedMaterial);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            AddDep(sharedMaterial, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            Collider uo = (Collider)obj;
            AddDep(uo.sharedMaterial, context);
        }

        public static implicit operator Collider(PersistentCollider surrogate)
        {
            return (Collider)surrogate.WriteTo(new Collider());
        }
        
        public static implicit operator PersistentCollider(Collider obj)
        {
            PersistentCollider surrogate = new PersistentCollider();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

