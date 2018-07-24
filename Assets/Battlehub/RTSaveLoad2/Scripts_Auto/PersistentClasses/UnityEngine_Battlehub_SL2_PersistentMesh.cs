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

        partial void OnBeforeReadFrom(object obj);
        partial void OnAfterReadFrom(object obj);
        public override void ReadFrom(object obj)
        {
            OnBeforeReadFrom(obj);
            ReadFrom(obj);
            OnAfterReadFrom(obj);
        }

        partial void OnBeforeWriteTo(ref object input);
        partial void OnAfterWriteTo(ref object input);
        public override object WriteTo(object obj)
        {
           OnBeforeWriteTo(ref obj);
           obj = WriteTo(obj);
           OnAfterWriteTo(ref obj);
           return obj;
        }

        partial void OnBeforeGetDeps(GetDepsContext context);
        partial void OnAfterGetDeps(GetDepsContext context);
        public override void GetDeps(GetDepsContext context)
        {
           OnBeforeGetDeps(context);
           GetDepsImpl(context);
           OnAfterGetDeps(context);
        }

        partial void OnBeforeGetDepsFrom(object obj, GetDepsFromContext context);
        partial void OnAfterGetDepsFrom(object obj, GetDepsFromContext context);
        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
           OnBeforeGetDepsFrom(obj, context);
           GetDepsFromImpl(obj, context);
           OnAfterGetDepsFrom(obj, context);
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

