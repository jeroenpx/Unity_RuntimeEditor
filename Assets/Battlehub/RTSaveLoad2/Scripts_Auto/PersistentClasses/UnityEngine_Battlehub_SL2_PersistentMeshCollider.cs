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
    public partial class PersistentMeshCollider : PersistentCollider
    {
        [ProtoMember(256)]
        public long sharedMesh;

        [ProtoMember(257)]
        public bool convex;

        [ProtoMember(258)]
        public bool inflateMesh;

        [ProtoMember(259)]
        public MeshColliderCookingOptions cookingOptions;

        [ProtoMember(260)]
        public float skinWidth;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            MeshCollider uo = (MeshCollider)obj;
            sharedMesh = ToID(uo.sharedMesh);
            convex = uo.convex;
            inflateMesh = uo.inflateMesh;
            cookingOptions = uo.cookingOptions;
            skinWidth = uo.skinWidth;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            MeshCollider uo = (MeshCollider)obj;
            uo.sharedMesh = FromID(sharedMesh, uo.sharedMesh);
            uo.convex = convex;
            uo.inflateMesh = inflateMesh;
            uo.cookingOptions = cookingOptions;
            uo.skinWidth = skinWidth;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            base.GetDepsImpl(context);
            AddDep(sharedMesh, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            MeshCollider uo = (MeshCollider)obj;
            AddDep(uo.sharedMesh, context);
        }

        public static implicit operator MeshCollider(PersistentMeshCollider surrogate)
        {
            if(surrogate == null) return default(MeshCollider);
            return (MeshCollider)surrogate.WriteTo(new MeshCollider());
        }
        
        public static implicit operator PersistentMeshCollider(MeshCollider obj)
        {
            PersistentMeshCollider surrogate = new PersistentMeshCollider();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

