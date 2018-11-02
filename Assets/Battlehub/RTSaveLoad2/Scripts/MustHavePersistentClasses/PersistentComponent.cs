using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract(AsReferenceDefault = true)]
    public partial class PersistentComponent : PersistentObject
    {
        public static implicit operator Component(PersistentComponent surrogate)
        {
            return (Component)surrogate.WriteTo(new Component());
        }
        
        public static implicit operator PersistentComponent(Component obj)
        {
            PersistentComponent surrogate = new PersistentComponent();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

