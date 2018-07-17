using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    public interface ITypeMap
    {
        Type ToPersistentType(Type unityType);
        Type ToUnityType(Type persistentType);
    }

    public partial class TypeMap : ITypeMap
    {
        protected Dictionary<Type, Type> m_toPeristentType = new Dictionary<Type, Type>();
        protected Dictionary<Type, Type> m_toUnityType = new Dictionary<Type, Type>();

        public Type ToPersistentType(Type unityType)
        {
            Type persistentType;
            if(m_toPeristentType.TryGetValue(unityType, out persistentType))
            {
                return persistentType;
            }
            return null;
        }

        public Type ToUnityType(Type persistentType)
        {
            Type unityType;
            if(m_toUnityType.TryGetValue(persistentType, out unityType))
            {
                return unityType;
            }
            return null;
        }
    }
}


