using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentQuaternion : PersistentSurrogate
    {
        [ProtoMember(256)]
        public float x;

        [ProtoMember(257)]
        public float y;

        [ProtoMember(258)]
        public float z;

        [ProtoMember(259)]
        public float w;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Quaternion uo = (Quaternion)obj;
            x = uo.x;
            y = uo.y;
            z = uo.z;
            w = uo.w;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Quaternion uo = (Quaternion)obj;
            uo.x = x;
            uo.y = y;
            uo.z = z;
            uo.w = w;
            return uo;
        }

        public static implicit operator Quaternion(PersistentQuaternion surrogate)
        {
            return (Quaternion)surrogate.WriteTo(new Quaternion());
        }
        
        public static implicit operator PersistentQuaternion(Quaternion obj)
        {
            PersistentQuaternion surrogate = new PersistentQuaternion();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

