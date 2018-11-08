using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentBoxCollider : PersistentCollider
    {
        [ProtoMember(256)]
        public Vector3 center;

        [ProtoMember(257)]
        public Vector3 size;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            BoxCollider uo = (BoxCollider)obj;
            center = uo.center;
            size = uo.size;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            BoxCollider uo = (BoxCollider)obj;
            uo.center = center;
            uo.size = size;
            return uo;
        }

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

