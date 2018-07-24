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
    public partial class PersistentMeshRenderer : PersistentObject
    {
        [ProtoMember(256)]
        public long additionalVertexStreams;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            MeshRenderer uo = (MeshRenderer)obj;
            additionalVertexStreams = ToID(uo.additionalVertexStreams);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            MeshRenderer uo = (MeshRenderer)obj;
            uo.additionalVertexStreams = FromID<Mesh>(additionalVertexStreams);
            return obj;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            AddDep(additionalVertexStreams, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            MeshRenderer uo = (MeshRenderer)obj;
            AddDep(uo.additionalVertexStreams, context);
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

        public static implicit operator MeshRenderer(PersistentMeshRenderer surrogate)
        {
            return (MeshRenderer)surrogate.WriteTo(new MeshRenderer());
        }
        
        public static implicit operator PersistentMeshRenderer(MeshRenderer obj)
        {
            PersistentMeshRenderer surrogate = new PersistentMeshRenderer();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

