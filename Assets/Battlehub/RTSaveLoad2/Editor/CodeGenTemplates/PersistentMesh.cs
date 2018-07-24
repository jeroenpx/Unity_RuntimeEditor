#if true
using System.Collections.Generic;
using Battlehub.RTSaveLoad.PersistentObjects;
using UnityEngine;

namespace UnityEngine.Battlehub.NAMESPACE
{
    public partial class PersistentMesh : PersistentObject
    {
        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
        }

        public override object WriteTo(object obj, Dictionary<long, Object> objects)
        {
            return base.WriteTo(obj, objects);
        }
    }
}

#endif
