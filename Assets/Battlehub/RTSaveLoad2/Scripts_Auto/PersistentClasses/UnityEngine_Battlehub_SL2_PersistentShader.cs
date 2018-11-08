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
    public partial class PersistentShader : PersistentObject
    {
        [ProtoMember(256)]
        public int maximumLOD;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Shader uo = (Shader)obj;
            maximumLOD = uo.maximumLOD;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Shader uo = (Shader)obj;
            uo.maximumLOD = maximumLOD;
            return uo;
        }
    }
}

