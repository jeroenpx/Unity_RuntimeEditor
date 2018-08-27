using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    public interface ITypeMap
    {
        Type ToPersistentType(Type unityType);
        Type ToUnityType(Type persistentType);
        Type ToType(Guid typeGuid);
        Guid ToGuid(Type type);
    }

    public partial class TypeMap : ITypeMap
    {
        protected Dictionary<Type, Type> m_toPeristentType = new Dictionary<Type, Type>();
        protected Dictionary<Type, Type> m_toUnityType = new Dictionary<Type, Type>();

        protected Dictionary<Type, Guid> m_toGuid = new Dictionary<Type, Guid>();
        protected Dictionary<Guid, Type> m_toType = new Dictionary<Guid, Type>();

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

        public Type ToType(Guid typeGuid)
        {
            Type type;
            if (m_toType.TryGetValue(typeGuid, out type))
            {
                return type;
            }
            return null;
        }

        public Guid ToGuid(Type type)
        {
            Guid guid;
            if(m_toGuid.TryGetValue(type, out guid))
            {
                return guid;
            }

            return Guid.Empty;
        }
    }
}


