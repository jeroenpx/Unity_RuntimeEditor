using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentMonoBehaviour : PersistentBehaviour
    {
        [ProtoMember(256)]
        public bool useGUILayout;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            MonoBehaviour uo = (MonoBehaviour)obj;
            useGUILayout = uo.useGUILayout;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            MonoBehaviour uo = (MonoBehaviour)obj;
            uo.useGUILayout = useGUILayout;
            return uo;
        }
    }
}

