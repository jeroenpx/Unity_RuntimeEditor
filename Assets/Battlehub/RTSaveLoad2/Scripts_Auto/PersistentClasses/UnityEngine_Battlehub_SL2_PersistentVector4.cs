using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentVector4 : PersistentSurrogate
    {
        
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

        public static implicit operator Vector4(PersistentVector4 surrogate)
        {
            return (Vector4)surrogate.WriteTo(new Vector4());
        }
        
        public static implicit operator PersistentVector4(Vector4 obj)
        {
            PersistentVector4 surrogate = new PersistentVector4();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

