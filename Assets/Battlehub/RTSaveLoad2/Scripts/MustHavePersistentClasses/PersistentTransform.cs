using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentTransform : PersistentComponent
    {
        [ProtoMember(256)]
        public Vector3 position;

        [ProtoMember(263)]
        public Quaternion rotation;

        [ProtoMember(265)]
        public Vector3 localScale;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Transform uo = (Transform)obj;
            position = uo.position;
            rotation = uo.rotation;
            localScale = uo.localScale;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Transform uo = (Transform)obj;
            uo.position = position;
            uo.rotation = rotation;
            uo.localScale = localScale;
            return obj;
        }
    }
}

