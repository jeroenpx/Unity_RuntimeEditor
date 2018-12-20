using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using Battlehub.RTCommon;
using Battlehub.RTCommon.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTCommon.Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentExposeToEditor : PersistentMonoBehaviour
    {
        [ProtoMember(256)]
        public PersistentExposeToEditorUnityEvent Selected;

        [ProtoMember(257)]
        public PersistentExposeToEditorUnityEvent Unselected;

        [ProtoMember(258)]
        public long BoundsObject;

        [ProtoMember(259)]
        public BoundsType BoundsType;

        [ProtoMember(260)]
        public PersistentBounds CustomBounds;

        [ProtoMember(261)]
        public bool CanSelect;

        [ProtoMember(262)]
        public bool CanSnap;

        [ProtoMember(263)]
        public bool AddColliders;

        [ProtoMember(269)]
        public long[] Colliders;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ExposeToEditor uo = (ExposeToEditor)obj;
            Selected = uo.Selected;
            Unselected = uo.Unselected;
            BoundsObject = ToID(uo.BoundsObject);
            BoundsType = uo.BoundsType;
            CustomBounds = uo.CustomBounds;
            CanSelect = uo.CanSelect;
            CanSnap = uo.CanSnap;
            AddColliders = uo.AddColliders;
            Colliders = ToID(uo.Colliders);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ExposeToEditor uo = (ExposeToEditor)obj;
            uo.Selected = Selected;
            uo.Unselected = Unselected;
            uo.BoundsObject = FromID(BoundsObject, uo.BoundsObject);
            uo.BoundsType = BoundsType;
            uo.CustomBounds = CustomBounds;
            uo.CanSelect = CanSelect;
            uo.CanSnap = CanSnap;
            uo.AddColliders = AddColliders;
            uo.Colliders = FromID(Colliders, uo.Colliders);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            base.GetDepsImpl(context);
            AddSurrogateDeps(Selected, context);
            AddSurrogateDeps(Unselected, context);
            AddDep(BoundsObject, context);
            AddDep(Colliders, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            ExposeToEditor uo = (ExposeToEditor)obj;
            AddSurrogateDeps(uo.Selected, v_ => (PersistentExposeToEditorUnityEvent)v_, context);
            AddSurrogateDeps(uo.Unselected, v_ => (PersistentExposeToEditorUnityEvent)v_, context);
            AddDep(uo.BoundsObject, context);
            AddDep(uo.Colliders, context);
        }

        public static implicit operator ExposeToEditor(PersistentExposeToEditor surrogate)
        {
            if(surrogate == null) return default(ExposeToEditor);
            return (ExposeToEditor)surrogate.WriteTo(new ExposeToEditor());
        }
        
        public static implicit operator PersistentExposeToEditor(ExposeToEditor obj)
        {
            PersistentExposeToEditor surrogate = new PersistentExposeToEditor();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

