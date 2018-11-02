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
    public partial class PersistentSkinnedMeshRenderer : PersistentRenderer
    {
        [ProtoMember(256)]
        public SkinQuality quality;

        [ProtoMember(257)]
        public bool updateWhenOffscreen;

        [ProtoMember(258)]
        public long rootBone;

        [ProtoMember(259)]
        public long[] bones;

        [ProtoMember(260)]
        public long sharedMesh;

        [ProtoMember(261)]
        public bool skinnedMotionVectors;

        [ProtoMember(262)]
        public Bounds localBounds;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            SkinnedMeshRenderer uo = (SkinnedMeshRenderer)obj;
            quality = uo.quality;
            updateWhenOffscreen = uo.updateWhenOffscreen;
            rootBone = ToID(uo.rootBone);
            bones = ToID(uo.bones);
            sharedMesh = ToID(uo.sharedMesh);
            skinnedMotionVectors = uo.skinnedMotionVectors;
            localBounds = uo.localBounds;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            SkinnedMeshRenderer uo = (SkinnedMeshRenderer)obj;
            uo.quality = quality;
            uo.updateWhenOffscreen = updateWhenOffscreen;
            uo.rootBone = FromID<Transform>(rootBone);
            uo.bones = FromID<Transform>(bones);
            uo.sharedMesh = FromID<Mesh>(sharedMesh);
            uo.skinnedMotionVectors = skinnedMotionVectors;
            uo.localBounds = localBounds;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            AddDep(rootBone, context);
            AddDep(bones, context);
            AddDep(sharedMesh, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            SkinnedMeshRenderer uo = (SkinnedMeshRenderer)obj;
            AddDep(uo.rootBone, context);
            AddDep(uo.bones, context);
            AddDep(uo.sharedMesh, context);
        }

        public static implicit operator SkinnedMeshRenderer(PersistentSkinnedMeshRenderer surrogate)
        {
            return (SkinnedMeshRenderer)surrogate.WriteTo(new SkinnedMeshRenderer());
        }
        
        public static implicit operator PersistentSkinnedMeshRenderer(SkinnedMeshRenderer obj)
        {
            PersistentSkinnedMeshRenderer surrogate = new PersistentSkinnedMeshRenderer();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

