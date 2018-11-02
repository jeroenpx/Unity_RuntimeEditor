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
    public partial class PersistentMaterial : PersistentObject
    {
        [ProtoMember(256)]
        public long shader;

        [ProtoMember(257)]
        public int renderQueue;

        [ProtoMember(258)]
        public MaterialGlobalIlluminationFlags globalIlluminationFlags;

        [ProtoMember(259)]
        public bool doubleSidedGI;

        [ProtoMember(260)]
        public bool enableInstancing;

        [ProtoMember(261)]
        public string[] shaderKeywords;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Material uo = (Material)obj;
            shader = ToID(uo.shader);
            renderQueue = uo.renderQueue;
            globalIlluminationFlags = uo.globalIlluminationFlags;
            doubleSidedGI = uo.doubleSidedGI;
            enableInstancing = uo.enableInstancing;
            shaderKeywords = uo.shaderKeywords;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Material uo = (Material)obj;
            uo.shader = FromID<Shader>(shader);
            uo.renderQueue = renderQueue;
            uo.globalIlluminationFlags = globalIlluminationFlags;
            uo.doubleSidedGI = doubleSidedGI;
            uo.enableInstancing = enableInstancing;
            uo.shaderKeywords = shaderKeywords;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            AddDep(shader, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            Material uo = (Material)obj;
            AddDep(uo.shader, context);
        }
    }
}

