using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Battlehub.RTSL
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
        protected readonly Dictionary<Type, Type> m_toPeristentType = new Dictionary<Type, Type>();
        protected readonly Dictionary<Type, Type> m_toUnityType = new Dictionary<Type, Type>();
        protected readonly Dictionary<Type, Guid> m_toGuid = new Dictionary<Type, Guid>();
        protected readonly Dictionary<Guid, Type> m_toType = new Dictionary<Guid, Type>();

        protected void OnConstructed()
        {
            m_toPeristentType[typeof(Scene)] = typeof(PersistentRuntimeScene);
            m_toUnityType[typeof(PersistentRuntimeScene)] = typeof(Scene);

            Guid sceneGuid = new Guid("d144fbe0-d2c0-4bcf-aa9f-251376262202");
            m_toGuid[typeof(Scene)] = sceneGuid;
            m_toType[sceneGuid] = typeof(Scene);
        }

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


