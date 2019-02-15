using System;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2.Interface
{
    public interface IUnityObjectFactory
    {
        UnityObject CreateInstance(Type type, PersistentSurrogate surrogate);
    }
}
