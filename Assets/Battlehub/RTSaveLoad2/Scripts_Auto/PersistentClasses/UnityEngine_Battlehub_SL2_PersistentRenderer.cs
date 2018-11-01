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
    public partial class PersistentRenderer : PersistentObject
    {
        [ProtoMember(281)]
        public long[] sharedMaterials;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Renderer uo = (Renderer)obj;
            sharedMaterials = ToID(uo.sharedMaterials);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Renderer uo = (Renderer)obj;
            uo.sharedMaterials = FromID<Material>(sharedMaterials);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            AddDep(sharedMaterials, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            Renderer uo = (Renderer)obj;
            AddDep(uo.sharedMaterials, context);
        }

        public static implicit operator Renderer(PersistentRenderer surrogate)
        {
            return (Renderer)surrogate.WriteTo(new Renderer());
        }
        
        public static implicit operator PersistentRenderer(Renderer obj)
        {
            PersistentRenderer surrogate = new PersistentRenderer();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

