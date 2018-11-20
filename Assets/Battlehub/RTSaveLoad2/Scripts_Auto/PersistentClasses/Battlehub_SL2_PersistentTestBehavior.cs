using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.Events.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentTestBehavior : PersistentMonoBehaviour
    {
        [ProtoMember(256)]
        public List<int> Values;

        [ProtoMember(257)]
        public List<PersistentTest> Values2;

        [ProtoMember(258)]
        public PersistentTest[] Values25;

        [ProtoMember(259)]
        public PersistentVector3[] Values26;

        [ProtoMember(260)]
        public long[] Values3;

        [ProtoMember(261)]
        public PersistentUnityEvent Event;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TestBehavior uo = (TestBehavior)obj;
            Values = uo.Values;
            Values2 = Assign(uo.Values2, v_ => (PersistentTest)v_);
            Values25 = Assign(uo.Values25, v_ => (PersistentTest)v_);
            Values26 = Assign(uo.Values26, v_ => (PersistentVector3)v_);
            Values3 = ToID(uo.Values3);
            Event = uo.Event;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TestBehavior uo = (TestBehavior)obj;
            uo.Values = Values;
            uo.Values2 = Assign(Values2, v_ => (Test)v_);
            uo.Values25 = Assign(Values25, v_ => (Test)v_);
            uo.Values26 = Assign(Values26, v_ => (Vector3)v_);
            uo.Values3 = FromID(Values3, uo.Values3);
            uo.Event = Event;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            base.GetDepsImpl(context);
            AddDep(Values3, context);
            AddSurrogateDeps(Event, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            TestBehavior uo = (TestBehavior)obj;
            AddDep(uo.Values3, context);
            AddSurrogateDeps(uo.Event, v_ => (PersistentUnityEvent)v_, context);
        }

        public static implicit operator TestBehavior(PersistentTestBehavior surrogate)
        {
            return (TestBehavior)surrogate.WriteTo(new TestBehavior());
        }
        
        public static implicit operator PersistentTestBehavior(TestBehavior obj)
        {
            PersistentTestBehavior surrogate = new PersistentTestBehavior();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

