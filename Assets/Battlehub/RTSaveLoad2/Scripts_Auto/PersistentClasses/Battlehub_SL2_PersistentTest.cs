using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentTest : PersistentSurrogate
    {
        [ProtoMember(256)]
        public int Value;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Test uo = (Test)obj;
            Value = uo.Value;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Test uo = (Test)obj;
            uo.Value = Value;
            return uo;
        }

        public static implicit operator Test(PersistentTest surrogate)
        {
            return (Test)surrogate.WriteTo(new Test());
        }
        
        public static implicit operator PersistentTest(Test obj)
        {
            PersistentTest surrogate = new PersistentTest();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

