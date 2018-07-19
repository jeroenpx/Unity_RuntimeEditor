using ProtoBuf;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract(AsReferenceDefault = true)]    
    public class PersistentObject : PersistentSurrogate
    {
        [ProtoMember(1)]
        public string name;

        [ProtoMember(2)]
        public int hideFlags;

        protected override void ReadFromImpl(object obj)
        {
            UnityObject uo = (UnityObject)obj;
            name = uo.name;
            hideFlags = (int)uo.hideFlags;
        }

        protected override object WriteToImpl(object obj)
        {
            UnityObject uo = (UnityObject)obj;
            uo.name = name;
            uo.hideFlags = (HideFlags)hideFlags;
            return obj;
        }       
    }

}
