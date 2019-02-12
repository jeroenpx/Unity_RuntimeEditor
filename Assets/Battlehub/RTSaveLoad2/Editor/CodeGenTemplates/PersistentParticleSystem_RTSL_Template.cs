#define RTSL2_COMPILE_TEMPLATES
#if RTSL2_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using ProtoBuf;
using UnityEngine;
//<TEMPLATE_USINGS_END>
#else
using UnityEngine;
#endif

namespace Battlehub.RTSaveLoad2.Internal
{
    [PersistentTemplate("UnityEngine.ParticleSystem", new string[0],
        new [] {
           "UnityEngine.ParticleSystem+CollisionModule",
           "UnityEngine.ParticleSystem+ColorBySpeedModule",
           "UnityEngine.ParticleSystem+ColorOverLifetimeModule",
           "UnityEngine.ParticleSystem+CustomDataModule",
           "UnityEngine.ParticleSystem+EmissionModule",
           "UnityEngine.ParticleSystem+ExternalForcesModule",
           "UnityEngine.ParticleSystem+ForceOverLifetimeModule",
           "UnityEngine.ParticleSystem+InheritVelocityModule",
           "UnityEngine.ParticleSystem+LightsModule",
           "UnityEngine.ParticleSystem+LimitVelocityOverLifetimeModule",
           "UnityEngine.ParticleSystem+MainModule",
           "UnityEngine.ParticleSystem+NoiseModule",
           "UnityEngine.ParticleSystem+RotationBySpeedModule",
           "UnityEngine.ParticleSystem+RotationOverLifetimeModule",
           "UnityEngine.ParticleSystem+ShapeModule",
           "UnityEngine.ParticleSystem+SizeBySpeedModule",
           "UnityEngine.ParticleSystem+SizeOverLifetimeModule",
           "UnityEngine.ParticleSystem+SubEmittersModule",
           "UnityEngine.ParticleSystem+TextureSheetAnimationModule",
           "UnityEngine.ParticleSystem+TrailModule",
           "UnityEngine.ParticleSystem+TriggerModule",
           "UnityEngine.ParticleSystem+VelocityOverLifetimeModule",
     })]


    public partial class PersistentParticleSystem_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL2_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>
        [ProtoMember(1)]
        public PersistentCollisionModule_RTSL_Template m_collisionModule;

        [ProtoMember(2)]
        public ParticleSystem.ColorBySpeedModule m_colorBySpeedModule;

        [ProtoMember(3)]
        public ParticleSystem.ColorOverLifetimeModule m_colorOverLifetimeModule;

        [ProtoMember(4)]
        public ParticleSystem.CustomDataModule m_customDataModule;

        [ProtoMember(5)]
        public ParticleSystem.EmissionModule m_emissonModule;

        [ProtoMember(6)]
        public ParticleSystem.ExternalForcesModule m_externalForcesModule;

        [ProtoMember(7)]
        public ParticleSystem.ForceOverLifetimeModule m_forceOverlifeTimeModule;

        [ProtoMember(8)]
        public ParticleSystem.InheritVelocityModule m_inheritVelocityModule;

        [ProtoMember(9)]
        public ParticleSystem.LightsModule m_lightsModule;

        [ProtoMember(10)]
        public ParticleSystem.LimitVelocityOverLifetimeModule m_limitVelocityOverLifetimeModule;

        [ProtoMember(11)]
        public ParticleSystem.MainModule m_mainModule;

        [ProtoMember(12)]
        public ParticleSystem.NoiseModule m_noiseModule;

        [ProtoMember(13)]
        public ParticleSystem.RotationBySpeedModule m_rotationBySpeedModule;

        [ProtoMember(14)]
        public ParticleSystem.RotationOverLifetimeModule m_rotationOverlifetimeModule;

        [ProtoMember(15)]
        public ParticleSystem.ShapeModule m_shapeModule;

        [ProtoMember(16)]
        public ParticleSystem.SizeBySpeedModule m_sizeBySpeedModule;

        [ProtoMember(17)]
        public ParticleSystem.SizeOverLifetimeModule m_sizeOverlifeTimeModuel;

        [ProtoMember(18)]
        public ParticleSystem.SubEmittersModule m_subEmittersModule;

        [ProtoMember(19)]
        public ParticleSystem.TextureSheetAnimationModule m_textureSheetAnimationModule;

        [ProtoMember(20)]
        public ParticleSystem.TrailModule m_trialModule;

        [ProtoMember(21)]
        public ParticleSystem.TriggerModule m_triggerModule;

        [ProtoMember(22)]
        public ParticleSystem.VelocityOverLifetimeModule m_velocityOverLifetimeModule;

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            ParticleSystem o = (ParticleSystem)obj;
            WriteSurrogateTo(m_collisionModule, o.collision);

            return obj;
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }

            ParticleSystem o = (ParticleSystem)obj;
            m_collisionModule = new PersistentCollisionModule_RTSL_Template();
            m_collisionModule.ReadFrom(o.collision);
                
        }

        public override void GetDeps(GetDepsContext context)
        {
            base.GetDeps(context);
            AddSurrogateDeps(m_collisionModule, context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            if (obj == null)
            {
                return;
            }

            ParticleSystem o = (ParticleSystem)obj;
            AddSurrogateDeps(o.collision, v_ => (PersistentCollisionModule_RTSL_Template)v_, context);
        }

        //<TEMPLATE_BODY_END>
#endif
    }
}