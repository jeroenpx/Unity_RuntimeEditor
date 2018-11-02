using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentMesh : PersistentObject
    {
        [ProtoMember(257)]
        public BoneWeight[] boneWeights;

        [ProtoMember(258)]
        public Matrix4x4[] bindposes;

        [ProtoMember(259)]
        public Bounds bounds;

        [ProtoMember(260)]
        public Vector3[] normals;

        [ProtoMember(261)]
        public Vector4[] tangents;

        [ProtoMember(262)]
        public Vector2[] uv;

        [ProtoMember(263)]
        public Vector2[] uv2;

        [ProtoMember(264)]
        public Vector2[] uv3;

        [ProtoMember(265)]
        public Vector2[] uv4;

        [ProtoMember(266)]
        public Vector2[] uv5;

        [ProtoMember(267)]
        public Vector2[] uv6;

        [ProtoMember(268)]
        public Vector2[] uv7;

        [ProtoMember(269)]
        public Vector2[] uv8;

        [ProtoMember(270)]
        public Color[] colors;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Mesh uo = (Mesh)obj;
            boneWeights = uo.boneWeights;
            bindposes = uo.bindposes;
            bounds = uo.bounds;
            normals = uo.normals;
            tangents = uo.tangents;
            uv = uo.uv;
            uv2 = uo.uv2;
            uv3 = uo.uv3;
            uv4 = uo.uv4;
            uv5 = uo.uv5;
            uv6 = uo.uv6;
            uv7 = uo.uv7;
            uv8 = uo.uv8;
            colors = uo.colors;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Mesh uo = (Mesh)obj;
            uo.boneWeights = boneWeights;
            uo.bindposes = bindposes;
            uo.bounds = bounds;
            uo.normals = normals;
            uo.tangents = tangents;
            uo.uv = uv;
            uo.uv2 = uv2;
            uo.uv3 = uv3;
            uo.uv4 = uv4;
            uo.uv5 = uv5;
            uo.uv6 = uv6;
            uo.uv7 = uv7;
            uo.uv8 = uv8;
            uo.colors = colors;
            return uo;
        }

        public static implicit operator Mesh(PersistentMesh surrogate)
        {
            return (Mesh)surrogate.WriteTo(new Mesh());
        }
        
        public static implicit operator PersistentMesh(Mesh obj)
        {
            PersistentMesh surrogate = new PersistentMesh();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

