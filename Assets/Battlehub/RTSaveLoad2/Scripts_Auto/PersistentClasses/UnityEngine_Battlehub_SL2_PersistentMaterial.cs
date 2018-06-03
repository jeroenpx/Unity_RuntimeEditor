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
    public class PersistentMaterial : PersistentSurrogate
    {
        [ProtoMember(256)]
        public string[] shaderKeywords;

        [ProtoMember(257)]
        public int shader;

        [ProtoMember(258)]
        public Color color;

        [ProtoMember(259)]
        public int mainTexture;

        [ProtoMember(260)]
        public Vector2 mainTextureOffset;

        [ProtoMember(261)]
        public Vector2 mainTextureScale;

        [ProtoMember(262)]
        public int renderQueue;

        [ProtoMember(263)]
        public MaterialGlobalIlluminationFlags globalIlluminationFlags;

        [ProtoMember(264)]
        public bool doubleSidedGI;

        [ProtoMember(265)]
        public bool enableInstancing;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            Material uo = (Material)obj;
            shaderKeywords = uo.shaderKeywords;
            shader = ToId(uo.shader);
            color = uo.color;
            mainTexture = ToId(uo.mainTexture);
            mainTextureOffset = uo.mainTextureOffset;
            mainTextureScale = uo.mainTextureScale;
            renderQueue = uo.renderQueue;
            globalIlluminationFlags = uo.globalIlluminationFlags;
            doubleSidedGI = uo.doubleSidedGI;
            enableInstancing = uo.enableInstancing;
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            Material uo = (Material)obj;
            uo.shaderKeywords = shaderKeywords;
            uo.shader = FromId<Shader>(shader);
            uo.color = color;
            uo.mainTexture = FromId<Texture>(mainTexture);
            uo.mainTextureOffset = mainTextureOffset;
            uo.mainTextureScale = mainTextureScale;
            uo.renderQueue = renderQueue;
            uo.globalIlluminationFlags = globalIlluminationFlags;
            uo.doubleSidedGI = doubleSidedGI;
            uo.enableInstancing = enableInstancing;
            return obj;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            AddDep(shader, context);
            AddDep(mainTexture, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            Material uo = (Material)obj;
            AddDep(uo.shader, context);
            AddDep(uo.mainTexture, context);
        }
    }
}

