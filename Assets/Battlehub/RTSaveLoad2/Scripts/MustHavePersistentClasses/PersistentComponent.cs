using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract]
    public partial class PersistentComponent : PersistentObject
    {
        public static implicit operator PersistentComponent(Component obj)
        {
            PersistentComponent surrogate = new PersistentComponent();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

