using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentLight : PersistentBehaviour
    {
        [ProtoMember(256)]
        public LightShadows shadows;

        [ProtoMember(257)]
        public float shadowStrength;

        [ProtoMember(258)]
        public LightShadowResolution shadowResolution;

        [ProtoMember(261)]
        public float[] layerShadowCullDistances;

        [ProtoMember(262)]
        public float cookieSize;

        [ProtoMember(263)]
        public long cookie;

        [ProtoMember(264)]
        public LightRenderMode renderMode;

        [ProtoMember(266)]
        public Vector2 areaSize;

        [ProtoMember(267)]
        public LightmapBakeType lightmapBakeType;

        [ProtoMember(271)]
        public LightType type;

        [ProtoMember(272)]
        public float spotAngle;

        [ProtoMember(273)]
        public Color color;

        [ProtoMember(274)]
        public float colorTemperature;

        [ProtoMember(275)]
        public float intensity;

        [ProtoMember(276)]
        public float bounceIntensity;

        [ProtoMember(277)]
        public int shadowCustomResolution;

        [ProtoMember(278)]
        public float shadowBias;

        [ProtoMember(279)]
        public float shadowNormalBias;

        [ProtoMember(280)]
        public float shadowNearPlane;

        [ProtoMember(281)]
        public float range;

        [ProtoMember(282)]
        public long flare;

        [ProtoMember(283)]
        public LightBakingOutput bakingOutput;

        [ProtoMember(284)]
        public int cullingMask;

        [ProtoMember(285)]
        public LightShadowCasterMode lightShadowCasterMode;

        [ProtoMember(286)]
        public float shadowRadius;

        [ProtoMember(287)]
        public float shadowAngle;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Light uo = (Light)obj;
            shadows = uo.shadows;
            shadowStrength = uo.shadowStrength;
            shadowResolution = uo.shadowResolution;
            layerShadowCullDistances = uo.layerShadowCullDistances;
            cookieSize = uo.cookieSize;
            cookie = ToID(uo.cookie);
            renderMode = uo.renderMode;
            areaSize = uo.areaSize;
            lightmapBakeType = uo.lightmapBakeType;
            type = uo.type;
            spotAngle = uo.spotAngle;
            color = uo.color;
            colorTemperature = uo.colorTemperature;
            intensity = uo.intensity;
            bounceIntensity = uo.bounceIntensity;
            shadowCustomResolution = uo.shadowCustomResolution;
            shadowBias = uo.shadowBias;
            shadowNormalBias = uo.shadowNormalBias;
            shadowNearPlane = uo.shadowNearPlane;
            range = uo.range;
            flare = ToID(uo.flare);
            bakingOutput = uo.bakingOutput;
            cullingMask = uo.cullingMask;
            lightShadowCasterMode = uo.lightShadowCasterMode;
            shadowRadius = uo.shadowRadius;
            shadowAngle = uo.shadowAngle;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Light uo = (Light)obj;
            uo.shadows = shadows;
            uo.shadowStrength = shadowStrength;
            uo.shadowResolution = shadowResolution;
            uo.layerShadowCullDistances = layerShadowCullDistances;
            uo.cookieSize = cookieSize;
            uo.cookie = FromID<Texture>(cookie);
            uo.renderMode = renderMode;
            uo.areaSize = areaSize;
            uo.lightmapBakeType = lightmapBakeType;
            uo.type = type;
            uo.spotAngle = spotAngle;
            uo.color = color;
            uo.colorTemperature = colorTemperature;
            uo.intensity = intensity;
            uo.bounceIntensity = bounceIntensity;
            uo.shadowCustomResolution = shadowCustomResolution;
            uo.shadowBias = shadowBias;
            uo.shadowNormalBias = shadowNormalBias;
            uo.shadowNearPlane = shadowNearPlane;
            uo.range = range;
            uo.flare = FromID<Flare>(flare);
            uo.bakingOutput = bakingOutput;
            uo.cullingMask = cullingMask;
            uo.lightShadowCasterMode = lightShadowCasterMode;
            uo.shadowRadius = shadowRadius;
            uo.shadowAngle = shadowAngle;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            base.GetDepsImpl(context);
            AddDep(cookie, context);
            AddDep(flare, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Light uo = (Light)obj;
            AddDep(uo.cookie, context);
            AddDep(uo.flare, context);
        }

        public static implicit operator Light(PersistentLight surrogate)
        {
            return (Light)surrogate.WriteTo(new Light());
        }
        
        public static implicit operator PersistentLight(Light obj)
        {
            PersistentLight surrogate = new PersistentLight();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

