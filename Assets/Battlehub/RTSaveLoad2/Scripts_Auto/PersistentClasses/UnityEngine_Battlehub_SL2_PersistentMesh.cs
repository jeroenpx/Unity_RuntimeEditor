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
        [ProtoMember(262)]
        public Vector3[] vertices;

        [ProtoMember(263)]
        public Vector3[] normals;

        [ProtoMember(264)]
        public Vector4[] tangents;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Mesh uo = (Mesh)obj;
            vertices = uo.vertices;
            normals = uo.normals;
            tangents = uo.tangents;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Mesh uo = (Mesh)obj;
            uo.vertices = vertices;
            uo.normals = normals;
            uo.tangents = tangents;
            return obj;
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

