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
    [PersistentTemplate("UnityEngine.ParticleSystem+CollisionModule")]
    public partial class PersistentCollisionModule_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL2_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            ParticleSystem.CollisionModule o = (ParticleSystem.CollisionModule)obj;
            return obj;
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }

            ParticleSystem.CollisionModule o = (ParticleSystem.CollisionModule)obj;

        }

        public override void GetDeps(GetDepsContext context)
        {
            base.GetDeps(context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            if (obj == null)
            {
                return;
            }

            ParticleSystem.CollisionModule o = (ParticleSystem.CollisionModule)obj;

        }

        //<TEMPLATE_BODY_END>
#endif
    }
}
