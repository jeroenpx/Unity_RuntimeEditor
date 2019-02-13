using ProtoBuf.Meta;
using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Battlehub.SL2;
using UnityEngine.SceneManagement;
using UnityEngine.SceneManagement.Battlehub.SL2;
using Battlehub.RTSaveLoad2.Battlehub.SL2;
using Battlehub.RTCommon;
using Battlehub.RTCommon.Battlehub.SL2;
using Battlehub.RTEditor;
using Battlehub.RTEditor.Battlehub.SL2;
using Battlehub.SL2;
using UnityEngine.Events.Battlehub.SL2;
using UnityEngine.Events;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
   public static partial class TypeModelCreator
   {
       static partial void RegisterAutoTypes(RuntimeTypeModel model)
       {
            model.Add(typeof(PersistentObject), true)
                .AddSubType(1025, typeof(PersistentGameObject))
                .AddSubType(1027, typeof(PersistentRuntimePrefab))
                .AddSubType(1028, typeof(PersistentMaterial))
                .AddSubType(1029, typeof(PersistentComponent))
                .AddSubType(1030, typeof(PersistentMesh))
                .AddSubType(1031, typeof(PersistentShader))
                .AddSubType(1032, typeof(PersistentTexture))
                .AddSubType(1033, typeof(PersistentFlare))
                .AddSubType(1034, typeof(PersistentPhysicMaterial))
                .AddSubType(1035, typeof(PersistentAvatar))
                .AddSubType(1036, typeof(PersistentRuntimeAnimatorController))
                .AddSubType(1037, typeof(PersistentSprite));
            model.Add(typeof(PersistentGameObject), true);
            model.Add(typeof(PersistentRenderer), true)
                .AddSubType(1025, typeof(PersistentMeshRenderer))
                .AddSubType(1026, typeof(PersistentSkinnedMeshRenderer))
                .AddSubType(1027, typeof(PersistentParticleSystemRenderer))
                .AddSubType(1028, typeof(PersistentSpriteRenderer));
            model.Add(typeof(PersistentMeshRenderer), true);
            model.Add(typeof(PersistentMeshFilter), true);
            model.Add(typeof(PersistentSkinnedMeshRenderer), true);
            model.Add(typeof(PersistentMesh), true);
            model.Add(typeof(PersistentMaterial), true);
            model.Add(typeof(PersistentRigidbody), true);
            model.Add(typeof(PersistentBoxCollider), true);
            model.Add(typeof(PersistentSphereCollider), true);
            model.Add(typeof(PersistentCapsuleCollider), true);
            model.Add(typeof(PersistentMeshCollider), true);
            model.Add(typeof(PersistentCamera), true);
            model.Add(typeof(PersistentLight), true);
            model.Add(typeof(PersistentAnimator), true);
            model.Add(typeof(PersistentAvatar), true);
            model.Add(typeof(PersistentBehaviour), true)
                .AddSubType(1025, typeof(PersistentCamera))
                .AddSubType(1026, typeof(PersistentLight))
                .AddSubType(1027, typeof(PersistentMonoBehaviour))
                .AddSubType(1028, typeof(PersistentAnimator));
            model.Add(typeof(PersistentCollider), true)
                .AddSubType(1025, typeof(PersistentBoxCollider))
                .AddSubType(1026, typeof(PersistentSphereCollider))
                .AddSubType(1027, typeof(PersistentCapsuleCollider))
                .AddSubType(1028, typeof(PersistentMeshCollider));
            model.Add(typeof(PersistentComponent), true)
                .AddSubType(1025, typeof(PersistentRenderer))
                .AddSubType(1026, typeof(PersistentTransform))
                .AddSubType(1027, typeof(PersistentMeshFilter))
                .AddSubType(1028, typeof(PersistentBehaviour))
                .AddSubType(1029, typeof(PersistentRigidbody))
                .AddSubType(1030, typeof(PersistentCollider))
                .AddSubType(1031, typeof(PersistentParticleSystem))
                .AddSubType(1032, typeof(PersistentParticleSystemForceField));
            model.Add(typeof(PersistentFlare), true);
            model.Add(typeof(PersistentMonoBehaviour), true)
                .AddSubType(1026, typeof(PersistentExposeToEditor))
                .AddSubType(1027, typeof(PersistentTestBehaviour))
                .AddSubType(1028, typeof(PersistentGameViewCamera));
            model.Add(typeof(PersistentParticleSystem), true);
            model.Add(typeof(PersistentParticleSystemRenderer), true);
            model.Add(typeof(PersistentPhysicMaterial), true);
            model.Add(typeof(PersistentRenderTexture), true);
            model.Add(typeof(PersistentRuntimeAnimatorController), true);
            model.Add(typeof(PersistentShader), true);
            model.Add(typeof(PersistentSprite), true);
            model.Add(typeof(PersistentSpriteRenderer), true);
            model.Add(typeof(PersistentTexture), true)
                .AddSubType(1025, typeof(PersistentRenderTexture))
                .AddSubType(1026, typeof(PersistentTexture2D))
                .AddSubType(1027, typeof(PersistentTexture3D));
            model.Add(typeof(PersistentTexture2D), true);
            model.Add(typeof(PersistentTexture3D), true);
            model.Add(typeof(PersistentTransform), true);
            model.Add(typeof(PersistentRuntimePrefab), true)
                .AddSubType(1025, typeof(PersistentRuntimeScene));
            model.Add(typeof(PersistentRuntimeScene), true);
            model.Add(typeof(PersistentExposeToEditor), true);
            model.Add(typeof(PersistentParticleSystemForceField), true);
            model.Add(typeof(PersistentGameViewCamera), true);
            model.Add(typeof(PersistentTestBehaviour), true);
            model.Add(typeof(Vector2), false).SetSurrogate(typeof(PersistentVector2));
            model.Add(typeof(Vector3), false).SetSurrogate(typeof(PersistentVector3));
            model.Add(typeof(Vector4), false).SetSurrogate(typeof(PersistentVector4));
            model.Add(typeof(Color), false).SetSurrogate(typeof(PersistentColor));
            model.Add(typeof(Matrix4x4), false).SetSurrogate(typeof(PersistentMatrix4x4));
            model.Add(typeof(AnimationCurve), false).SetSurrogate(typeof(PersistentAnimationCurve));
            model.Add(typeof(BoneWeight), false).SetSurrogate(typeof(PersistentBoneWeight));
            model.Add(typeof(Bounds), false).SetSurrogate(typeof(PersistentBounds));
            model.Add(typeof(Gradient), false).SetSurrogate(typeof(PersistentGradient));
            model.Add(typeof(GradientAlphaKey), false).SetSurrogate(typeof(PersistentGradientAlphaKey));
            model.Add(typeof(GradientColorKey), false).SetSurrogate(typeof(PersistentGradientColorKey));
            model.Add(typeof(Keyframe), false).SetSurrogate(typeof(PersistentKeyframe));
            model.Add(typeof(LayerMask), false).SetSurrogate(typeof(PersistentLayerMask));
            model.Add(typeof(LightBakingOutput), false).SetSurrogate(typeof(PersistentLightBakingOutput));
            model.Add(typeof(ParticleSystem.CollisionModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedCollisionModule));
            model.Add(typeof(ParticleSystem.ColorBySpeedModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedColorBySpeedModule));
            model.Add(typeof(ParticleSystem.ColorOverLifetimeModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedColorOverLifetimeModule));
            model.Add(typeof(ParticleSystem.CustomDataModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedCustomDataModule));
            model.Add(typeof(ParticleSystem.EmissionModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedEmissionModule));
            model.Add(typeof(ParticleSystem.ExternalForcesModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedExternalForcesModule));
            model.Add(typeof(ParticleSystem.ForceOverLifetimeModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedForceOverLifetimeModule));
            model.Add(typeof(ParticleSystem.InheritVelocityModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedInheritVelocityModule));
            model.Add(typeof(ParticleSystem.LightsModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedLightsModule));
            model.Add(typeof(ParticleSystem.LimitVelocityOverLifetimeModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedLimitVelocityOverLifetimeModule));
            model.Add(typeof(ParticleSystem.MainModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedMainModule));
            model.Add(typeof(ParticleSystem.MinMaxCurve), false).SetSurrogate(typeof(PersistentParticleSystemNestedMinMaxCurve));
            model.Add(typeof(ParticleSystem.MinMaxGradient), false).SetSurrogate(typeof(PersistentParticleSystemNestedMinMaxGradient));
            model.Add(typeof(ParticleSystem.NoiseModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedNoiseModule));
            model.Add(typeof(ParticleSystem.RotationBySpeedModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedRotationBySpeedModule));
            model.Add(typeof(ParticleSystem.RotationOverLifetimeModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedRotationOverLifetimeModule));
            model.Add(typeof(ParticleSystem.ShapeModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedShapeModule));
            model.Add(typeof(ParticleSystem.SizeBySpeedModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedSizeBySpeedModule));
            model.Add(typeof(ParticleSystem.SizeOverLifetimeModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedSizeOverLifetimeModule));
            model.Add(typeof(ParticleSystem.SubEmittersModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedSubEmittersModule));
            model.Add(typeof(ParticleSystem.TextureSheetAnimationModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedTextureSheetAnimationModule));
            model.Add(typeof(ParticleSystem.TrailModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedTrailModule));
            model.Add(typeof(ParticleSystem.TriggerModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedTriggerModule));
            model.Add(typeof(ParticleSystem.VelocityOverLifetimeModule), false).SetSurrogate(typeof(PersistentParticleSystemNestedVelocityOverLifetimeModule));
            model.Add(typeof(Quaternion), false).SetSurrogate(typeof(PersistentQuaternion));
            model.Add(typeof(Rect), false).SetSurrogate(typeof(PersistentRect));
            model.Add(typeof(Scene), false).SetSurrogate(typeof(PersistentScene));
            model.Add(typeof(ExposeToEditorUnityEvent), false).SetSurrogate(typeof(PersistentExposeToEditorUnityEvent));
            model.Add(typeof(UnityEvent), false).SetSurrogate(typeof(PersistentUnityEvent));
            model.Add(typeof(PersistentUnityEventBase), true)
                .AddSubType(1025, typeof(PersistentUnityEvent))
                .AddSubType(1026, typeof(PersistentExposeToEditorUnityEvent));
            model.Add(typeof(ParticleSystem.Burst), false).SetSurrogate(typeof(PersistentParticleSystemNestedBurst));
            
       }
   }
}
namespace UnityEngine.Battlehub.SL2 {}
namespace System.Battlehub.SL2 {}
namespace UnityEngine.Rendering.Battlehub.SL2 {}
namespace UnityEngine.SceneManagement.Battlehub.SL2 {}
namespace Battlehub.RTSaveLoad2.Battlehub.SL2 {}
namespace Battlehub.RTCommon.Battlehub.SL2 {}
namespace Battlehub.RTEditor.Battlehub.SL2 {}
namespace Battlehub.SL2 {}
namespace UnityEngine.Events.Battlehub.SL2 {}


