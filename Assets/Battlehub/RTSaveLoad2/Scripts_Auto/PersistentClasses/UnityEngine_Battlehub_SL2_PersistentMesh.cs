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
        [ProtoMember(263)]
        public Vector3[] normals;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Mesh uo = (Mesh)obj;
            normals = uo.normals;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Mesh uo = (Mesh)obj;
            uo.normals = normals;
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

