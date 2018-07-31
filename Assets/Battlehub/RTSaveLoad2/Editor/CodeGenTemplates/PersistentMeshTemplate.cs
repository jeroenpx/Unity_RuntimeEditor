//#define RTSL2_COMPILE_TEMPLATES
#if RTSL2_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using ProtoBuf;
using UnityEngine;
//<TEMPLATE_USINGS_END>
#endif

namespace Battlehub.RTSaveLoad2.Internal
{
    [PersistentTemplate("UnityEngine.Mesh", "vertices", "subMeshCount")]
    public class PersistentMeshTemplate : PersistentSurrogateTemplate
    {
#if RTSL2_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>
        [ProtoMember(1)]
        public Vector3[] vertices;

        [ProtoMember(2)]
        public int subMeshCount;

        [ProtoMember(3)]
        public IntArray[] m_tris;

        public override object WriteTo(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            Mesh o = (Mesh)obj;
            o.vertices = vertices;
            o.subMeshCount = subMeshCount;
            if (m_tris != null)
            {
                for (int i = 0; i < subMeshCount; ++i)
                {
                    o.SetTriangles(m_tris[i].Array, i);
                }
            }
            return  base.WriteTo(obj); 
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }
            Mesh o = (Mesh)obj;
            subMeshCount = o.subMeshCount;
            vertices = o.vertices;
            m_tris = new IntArray[subMeshCount];
            for (int i = 0; i < subMeshCount; ++i)
            {
                m_tris[i] = new IntArray();
                m_tris[i].Array = o.GetTriangles(i);
            }
        }
        //<TEMPLATE_BODY_END>
#endif
    }
}


