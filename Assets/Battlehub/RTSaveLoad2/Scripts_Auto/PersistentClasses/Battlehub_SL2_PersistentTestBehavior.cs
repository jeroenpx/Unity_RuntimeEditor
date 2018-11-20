using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
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
        public List<Test> Values2;

        [ProtoMember(258)]
        public long[] Values3;

        [ProtoMember(259)]
        public UnityEvent Event;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TestBehavior uo = (TestBehavior)obj;
            Values = uo.Values;
            Values2 = uo.Values2;
            Values3 = ToID(uo.Values3);
            Event = uo.Event;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TestBehavior uo = (TestBehavior)obj;
            uo.Values = Values;
            uo.Values2 = Values2;
            uo.Values3 = FromID(Values3, uo.Values3);
            uo.Event = Event;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext context)
        {
            base.GetDepsImpl(context);
            AddDep(Values3, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            TestBehavior uo = (TestBehavior)obj;
            AddDep(uo.Values3, context);
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

