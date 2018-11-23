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
    public partial class PersistentBehaviour : PersistentComponent
    {
        [ProtoMember(256)]
        public bool enabled;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Behaviour uo = (Behaviour)obj;
            enabled = uo.enabled;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Behaviour uo = (Behaviour)obj;
            uo.enabled = enabled;
            return uo;
        }

        public static implicit operator Behaviour(PersistentBehaviour surrogate)
        {
            if(surrogate == null) return default(Behaviour);
            return (Behaviour)surrogate.WriteTo(new Behaviour());
        }
        
        public static implicit operator PersistentBehaviour(Behaviour obj)
        {
            PersistentBehaviour surrogate = new PersistentBehaviour();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

