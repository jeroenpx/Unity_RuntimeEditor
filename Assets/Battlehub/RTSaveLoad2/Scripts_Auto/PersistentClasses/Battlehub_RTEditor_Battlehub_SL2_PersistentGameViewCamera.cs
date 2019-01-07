using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using Battlehub.RTEditor;
using Battlehub.RTEditor.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTEditor.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentGameViewCamera : PersistentMonoBehaviour
    {
        [ProtoMember(258)]
        public PersistentRect Rect;

        [ProtoMember(259)]
        public int Depth;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            GameViewCamera uo = (GameViewCamera)obj;
            Rect = uo.Rect;
            Depth = uo.Depth;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            GameViewCamera uo = (GameViewCamera)obj;
            uo.Rect = Rect;
            uo.Depth = Depth;
            return uo;
        }

        public static implicit operator GameViewCamera(PersistentGameViewCamera surrogate)
        {
            if(surrogate == null) return default(GameViewCamera);
            return (GameViewCamera)surrogate.WriteTo(new GameViewCamera());
        }
        
        public static implicit operator PersistentGameViewCamera(GameViewCamera obj)
        {
            PersistentGameViewCamera surrogate = new PersistentGameViewCamera();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

