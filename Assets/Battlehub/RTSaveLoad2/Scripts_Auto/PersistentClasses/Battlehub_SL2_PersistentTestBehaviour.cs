using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.SL2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentTestBehaviour : PersistentMonoBehaviour
    {
        [ProtoMember(256)]
        public PersistentColor m_color;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TestBehaviour uo = (TestBehaviour)obj;
            m_color = GetPrivate<Color>(uo, "m_color");
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TestBehaviour uo = (TestBehaviour)obj;
            SetPrivate(uo, "m_color", m_color);
            return uo;
        }

        public static implicit operator TestBehaviour(PersistentTestBehaviour surrogate)
        {
            if(surrogate == null) return default(TestBehaviour);
            return (TestBehaviour)surrogate.WriteTo(new TestBehaviour());
        }
        
        public static implicit operator PersistentTestBehaviour(TestBehaviour obj)
        {
            PersistentTestBehaviour surrogate = new PersistentTestBehaviour();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

