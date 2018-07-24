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
    public partial class PersistentQuaternion : PersistentSurrogate
    {
        [ProtoMember(256)]
        public float x;

        [ProtoMember(257)]
        public float y;

        [ProtoMember(258)]
        public float z;

        [ProtoMember(259)]
        public float w;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Quaternion uo = (Quaternion)obj;
            x = uo.x;
            y = uo.y;
            z = uo.z;
            w = uo.w;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Quaternion uo = (Quaternion)obj;
            uo.x = x;
            uo.y = y;
            uo.z = z;
            uo.w = w;
            return obj;
        }

        partial void OnBeforeReadFrom(object obj);
        partial void OnAfterReadFrom(object obj);
        public override void ReadFrom(object obj)
        {
            OnBeforeReadFrom(obj);
            ReadFrom(obj);
            OnAfterReadFrom(obj);
        }

        partial void OnBeforeWriteTo(ref object input);
        partial void OnAfterWriteTo(ref object input);
        public override object WriteTo(object obj)
        {
           OnBeforeWriteTo(ref obj);
           obj = WriteTo(obj);
           OnAfterWriteTo(ref obj);
           return obj;
        }

        partial void OnBeforeGetDeps(GetDepsContext context);
        partial void OnAfterGetDeps(GetDepsContext context);
        public override void GetDeps(GetDepsContext context)
        {
           OnBeforeGetDeps(context);
           GetDepsImpl(context);
           OnAfterGetDeps(context);
        }

        partial void OnBeforeGetDepsFrom(object obj, GetDepsFromContext context);
        partial void OnAfterGetDepsFrom(object obj, GetDepsFromContext context);
        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
           OnBeforeGetDepsFrom(obj, context);
           GetDepsFromImpl(obj, context);
           OnAfterGetDepsFrom(obj, context);
        }

        public static implicit operator Quaternion(PersistentQuaternion surrogate)
        {
            return (Quaternion)surrogate.WriteTo(new Quaternion());
        }
        
        public static implicit operator PersistentQuaternion(Quaternion obj)
        {
            PersistentQuaternion surrogate = new PersistentQuaternion();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

