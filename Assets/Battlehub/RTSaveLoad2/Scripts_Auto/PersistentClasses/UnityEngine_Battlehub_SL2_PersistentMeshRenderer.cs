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
    public partial class PersistentMeshRenderer : PersistentRenderer
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
            return uo;
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

