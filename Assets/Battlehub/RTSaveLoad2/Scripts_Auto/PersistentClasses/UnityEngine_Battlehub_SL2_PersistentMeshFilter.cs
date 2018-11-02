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
    public partial class PersistentMeshFilter : PersistentComponent
    {
        [ProtoMember(256)]
        public long sharedMesh;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            MeshFilter uo = (MeshFilter)obj;
            sharedMesh = ToID(uo.sharedMesh);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            MeshFilter uo = (MeshFilter)obj;
            uo.sharedMesh = FromID<Mesh>(sharedMesh);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            AddDep(sharedMesh, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            MeshFilter uo = (MeshFilter)obj;
            AddDep(uo.sharedMesh, context);
        }

        public static implicit operator MeshFilter(PersistentMeshFilter surrogate)
        {
            return (MeshFilter)surrogate.WriteTo(new MeshFilter());
        }
        
        public static implicit operator PersistentMeshFilter(MeshFilter obj)
        {
            PersistentMeshFilter surrogate = new PersistentMeshFilter();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

