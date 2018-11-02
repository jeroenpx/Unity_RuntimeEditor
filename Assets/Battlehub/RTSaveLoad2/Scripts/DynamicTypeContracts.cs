using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    public static class DictionaryExt
    {
        public static U Get<T, U>(this Dictionary<T, U> dict, T key)
        {
            U val;
            if (dict.TryGetValue(key, out val))
            {
                return val;
            }
            return default(U);
        }
    }

    [ProtoContract]
    public abstract class PrimitiveContract
    {
        public static PrimitiveContract<T> Create<T>(T value)
        {
            return new PrimitiveContract<T>(value);
        }

        public static PrimitiveContract Create(Type type)
        {
            Type d1 = typeof(PrimitiveContract<>);
            Type constructed = d1.MakeGenericType(type);
            return (PrimitiveContract)Activator.CreateInstance(constructed);
        }

        public object ValueBase
        {
            get { return ValueImpl; }
            set { ValueImpl = value; }
        }
        protected abstract object ValueImpl { get; set; }
        protected PrimitiveContract() { }
    }

    [ProtoContract]
    public class PrimitiveContract<T> : PrimitiveContract
    {
        public PrimitiveContract() { }
        public PrimitiveContract(T value) { Value = value; }
        [ProtoMember(1)]
        public T Value { get; set; }
        protected override object ValueImpl
        {
            get { return Value; }
            set { Value = (T)value; }
        }
    }

    //[ProtoContract(AsReferenceDefault = true)]
    //public class DataContract
    //{
    //    [ProtoMember(1, DynamicType = true)]
    //    public object Data
    //    {
    //        get;
    //        set;
    //    }

    //    public PrimitiveContract AsPrimitive
    //    {
    //        get { return Data as PrimitiveContract; }
    //    }

    //    public PersistentUnityEventBase AsUnityEvent
    //    {
    //        get { return Data as PersistentUnityEventBase; }
    //    }

    //    public PersistentSurrogate AsPersistentSurrogate
    //    {
    //        get { return Data as PersistentSurrogate; }
    //    }

    //    public DataContract() { }

    //    public DataContract(PersistentSurrogate data)
    //    {
    //        Data = data;
    //    }

    //    public DataContract(PrimitiveContract primitive)
    //    {
    //        Data = primitive;
    //    }

    //    public DataContract(PersistentUnityEventBase data)
    //    {
    //        Data = data;
    //    }
    //}

    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentArgumentCache
    {
        public bool m_BoolArgument;
        public float m_FloatArgument;
        public int m_IntArgument;
        public string m_StringArgument;
        public long m_ObjectArgument; //instanceId
        public string m_ObjectArgumentAssemblyTypeName;

        private static bool m_isFieldInfoInitialized;
        private static FieldInfo m_boolArgumentFieldInfo;
        private static FieldInfo m_floatArgumentFieldInfo;
        private static FieldInfo m_intArgumentFieldInfo;
        private static FieldInfo m_stringArgumentFieldInfo;
        private static FieldInfo m_objectArgumentFieldInfo;
        private static FieldInfo m_objectArgumentAssemblyTypeNameFieldInfo;

        private static void Initialize(Type type)
        {
            if (m_isFieldInfoInitialized)
            {
                return;
            }

            m_boolArgumentFieldInfo = type.GetField("m_BoolArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_boolArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_BoolArgument FieldInfo not found.");
            }

            m_floatArgumentFieldInfo = type.GetField("m_FloatArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_floatArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_FloatArgument FieldInfo not found.");
            }

            m_intArgumentFieldInfo = type.GetField("m_IntArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_intArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_IntArgument FieldInfo not found.");
            }

            m_stringArgumentFieldInfo = type.GetField("m_StringArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_stringArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_StringArgument FieldInfo not found.");
            }

            m_objectArgumentFieldInfo = type.GetField("m_ObjectArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_objectArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_ObjectArgument FieldInfo not found.");
            }

            m_objectArgumentAssemblyTypeNameFieldInfo = type.GetField("m_ObjectArgumentAssemblyTypeName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_objectArgumentAssemblyTypeNameFieldInfo == null)
            {
                throw new NotSupportedException("m_ObjectArgumentAssemblyTypeName FieldInfo not found.");
            }

            m_isFieldInfoInitialized = true;
        }

        public void ReadFrom(object obj, IIDMap idMap)
        {
            if (obj == null)
            {
                m_BoolArgument = false;
                m_FloatArgument = 0;
                m_IntArgument = 0;
                m_StringArgument = null;
                m_ObjectArgument = 0;
                m_ObjectArgumentAssemblyTypeName = null;
                return;
            }
            Initialize(obj.GetType());
            m_BoolArgument = (bool)m_boolArgumentFieldInfo.GetValue(obj);
            m_FloatArgument = (float)m_floatArgumentFieldInfo.GetValue(obj);
            m_IntArgument = (int)m_intArgumentFieldInfo.GetValue(obj);
            m_StringArgument = (string)m_stringArgumentFieldInfo.GetValue(obj);
            UnityObject uobjArgument = (UnityObject)m_objectArgumentFieldInfo.GetValue(obj);
            m_ObjectArgument = idMap.ToID(uobjArgument);
            m_ObjectArgumentAssemblyTypeName = (string)m_objectArgumentAssemblyTypeNameFieldInfo.GetValue(obj);
        }

        public void GetDependencies(object obj, Dictionary<long, UnityObject> dependencies, IIDMap idMap)
        {
            if (obj == null)
            {
                return;
            }

            Initialize(obj.GetType());
            UnityObject uobjArgument = (UnityObject)m_objectArgumentFieldInfo.GetValue(obj);
            AddDependency(uobjArgument, dependencies, idMap);
        }

        protected void AddDependency(UnityObject obj, Dictionary<long, UnityObject> dependencies, IIDMap idMap)
        {
            if (obj == null)
            {
                return;
            }

            long instanceId = idMap.ToID(obj);
            if (!dependencies.ContainsKey(instanceId))
            {
                dependencies.Add(instanceId, obj);
            }
        }

        public void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
        {
            AddDependency(m_ObjectArgument, dependencies, objects, allowNulls);
        }

        protected void AddDependency<T>(long id, Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
        {
            T obj = objects.Get(id);
            if (obj != null || allowNulls)
            {
                if (!dependencies.ContainsKey(id))
                {
                    dependencies.Add(id, obj);
                }
            }
        }

        public void WriteTo(object obj, Dictionary<long, UnityObject> objects)
        {
            if (obj == null)
            {
                return;
            }
            Initialize(obj.GetType());
            m_boolArgumentFieldInfo.SetValue(obj, m_BoolArgument);
            m_floatArgumentFieldInfo.SetValue(obj, m_FloatArgument);
            m_intArgumentFieldInfo.SetValue(obj, m_IntArgument);
            m_stringArgumentFieldInfo.SetValue(obj, m_StringArgument);
            m_objectArgumentFieldInfo.SetValue(obj, objects.Get(m_ObjectArgument));
            m_objectArgumentAssemblyTypeNameFieldInfo.SetValue(obj, m_ObjectArgumentAssemblyTypeName);
        }
    }

#if RT_USE_PROTOBUF
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
#endif
    [Serializable]
    public class PersistentPersistentCall
    {
        public PersistentArgumentCache m_Arguments;
        public UnityEventCallState m_CallState;
        public string m_MethodName;
        public PersistentListenerMode m_Mode;
        public long m_Target; //instanceId
        public string TypeName;

        private static bool m_isFieldInfoInitialized;
        private static FieldInfo m_argumentsFieldInfo;
        private static FieldInfo m_callStatFieldInfo;
        private static FieldInfo m_methodNameFieldInfo;
        private static FieldInfo m_modeFieldInfo;
        private static FieldInfo m_targetFieldInfo;

        private static void Initialize(Type type)
        {
            if (m_isFieldInfoInitialized)
            {
                return;
            }

            m_argumentsFieldInfo = type.GetField("m_Arguments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_argumentsFieldInfo == null)
            {
                throw new NotSupportedException("m_Arguments FieldInfo not found.");
            }
            m_callStatFieldInfo = type.GetField("m_CallState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_callStatFieldInfo == null)
            {
                throw new NotSupportedException("m_CallState FieldInfo not found.");
            }
            m_methodNameFieldInfo = type.GetField("m_MethodName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_methodNameFieldInfo == null)
            {
                throw new NotSupportedException("m_MethodName FieldInfo not found.");
            }
            m_modeFieldInfo = type.GetField("m_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_modeFieldInfo == null)
            {
                throw new NotSupportedException("m_Mode FieldInfo not found.");
            }
            m_targetFieldInfo = type.GetField("m_Target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_targetFieldInfo == null)
            {
                throw new NotSupportedException("m_Target FieldInfo not found.");
            }

            m_isFieldInfoInitialized = true;
        }

        public void ReadFrom(object obj, IIDMap idMap)
        {
            if (obj == null)
            {
                m_Arguments = default(PersistentArgumentCache);
                m_CallState = default(UnityEventCallState);
                m_MethodName = null;
                m_Mode = default(PersistentListenerMode);
                m_Target = 0;
                return;
            }

            Initialize(obj.GetType());
            m_Arguments = new PersistentArgumentCache();
            m_Arguments.ReadFrom(m_argumentsFieldInfo.GetValue(obj), idMap);
            m_CallState = (UnityEventCallState)m_callStatFieldInfo.GetValue(obj);
            m_MethodName = (string)m_methodNameFieldInfo.GetValue(obj);
            m_Mode = (PersistentListenerMode)m_modeFieldInfo.GetValue(obj);
            UnityObject target = (UnityObject)m_targetFieldInfo.GetValue(obj);
            m_Target = idMap.ToID(target);
        }


        public void GetDependencies(object obj, Dictionary<long, UnityObject> dependencies, IIDMap idMap)
        {
            if (obj == null)
            {
                return;
            }

            Initialize(obj.GetType());

            PersistentArgumentCache args = new PersistentArgumentCache();
            args.GetDependencies(m_argumentsFieldInfo.GetValue(obj), dependencies, idMap);

            UnityObject target = (UnityObject)m_targetFieldInfo.GetValue(obj);
            AddDependency(target, dependencies, idMap);
        }

        protected void AddDependency(UnityObject obj, Dictionary<long, UnityObject> dependencies, IIDMap idMap)
        {
            if (obj == null)
            {
                return;
            }

            long instanceId = idMap.ToID(obj);
            if (!dependencies.ContainsKey(instanceId))
            {
                dependencies.Add(instanceId, obj);
            }
        }

        public void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
        {
            if (m_Arguments != null)
            {
                m_Arguments.FindDependencies(dependencies, objects, allowNulls);
            }

            AddDependency(m_Target, dependencies, objects, allowNulls);
        }

        protected void AddDependency<T>(long id, Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
        {
            T obj = objects.Get(id);
            if (obj != null || allowNulls)
            {
                if (!dependencies.ContainsKey(id))
                {
                    dependencies.Add(id, obj);
                }
            }
        }

        public void WriteTo(object obj, Dictionary<long, UnityObject> objects)
        {
            if (obj == null)
            {
                return;
            }
            Initialize(obj.GetType());

            TypeName = obj.GetType().AssemblyQualifiedName;

            if (m_Arguments != null)
            {
                object arguments = Activator.CreateInstance(m_argumentsFieldInfo.FieldType);
                m_Arguments.WriteTo(arguments, objects);
                m_argumentsFieldInfo.SetValue(obj, arguments);
            }

            m_callStatFieldInfo.SetValue(obj, m_CallState);
            m_methodNameFieldInfo.SetValue(obj, m_MethodName);
            m_modeFieldInfo.SetValue(obj, m_Mode);
            m_targetFieldInfo.SetValue(obj, objects.Get(m_Target));
        }
    }



#if RT_USE_PROTOBUF
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
#endif
    [Serializable]
    public class PersistentUnityEventBase
    {
        private static FieldInfo m_persistentCallGroupInfo;
        private static FieldInfo m_callsInfo;
        private static Type m_callType;

        public PersistentPersistentCall[] m_calls;

        static PersistentUnityEventBase()
        {
            m_persistentCallGroupInfo = typeof(UnityEventBase).GetField("m_PersistentCalls", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (m_persistentCallGroupInfo == null)
            {
                throw new NotSupportedException("m_PersistentCalls FieldInfo not found.");
            }

            Type persistentCallsType = m_persistentCallGroupInfo.FieldType;
            m_callsInfo = persistentCallsType.GetField("m_Calls", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_callsInfo == null)
            {
                throw new NotSupportedException("m_Calls FieldInfo not found. ");
            }

            Type callsType = m_callsInfo.FieldType;
            if (!callsType.IsGenericType() || callsType.GetGenericTypeDefinition() != typeof(List<>))
            {
                throw new NotSupportedException("m_callsInfo.FieldType is not a generic List<>");
            }

            m_callType = callsType.GetGenericArguments()[0];
        }

        public void ReadFrom(UnityEventBase obj, IIDMap idMap)
        {
            if (obj == null)
            {
                return;
            }

            object persistentCalls = m_persistentCallGroupInfo.GetValue(obj);
            if (persistentCalls == null)
            {
                return;
            }

            object calls = m_callsInfo.GetValue(persistentCalls);
            if (calls == null)
            {
                return;
            }

            IList list = (IList)calls;
            m_calls = new PersistentPersistentCall[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                object call = list[i];
                PersistentPersistentCall persistentCall = new PersistentPersistentCall();
                persistentCall.ReadFrom(call, idMap);
                m_calls[i] = persistentCall;
            }
        }

        public void GetDependencies(UnityEventBase obj, Dictionary<long, UnityObject> dependencies, IIDMap idMap)
        {
            if (obj == null)
            {
                return;
            }

            object persistentCalls = m_persistentCallGroupInfo.GetValue(obj);
            if (persistentCalls == null)
            {
                return;
            }

            object calls = m_callsInfo.GetValue(persistentCalls);
            if (calls == null)
            {
                return;
            }

            IList list = (IList)calls;
            for (int i = 0; i < list.Count; ++i)
            {
                object call = list[i];
                PersistentPersistentCall persistentCall = new PersistentPersistentCall();
                persistentCall.GetDependencies(call, dependencies, idMap);
            }
        }

        public void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
        {
            if (m_calls == null)
            {
                return;
            }

            for (int i = 0; i < m_calls.Length; ++i)
            {
                PersistentPersistentCall persistentCall = m_calls[i];
                if (persistentCall != null)
                {
                    persistentCall.FindDependencies(dependencies, objects, allowNulls);
                }
            }
        }

        public void WriteTo(UnityEventBase obj, Dictionary<long, UnityObject> objects, IIDMap idMap)
        {
            if (obj == null)
            {
                return;
            }

            if (m_calls == null)
            {
                return;
            }

            object persistentCalls = Activator.CreateInstance(m_persistentCallGroupInfo.FieldType);
            object calls = Activator.CreateInstance(m_callsInfo.FieldType);

            IList list = (IList)calls;
            for (int i = 0; i < m_calls.Length; ++i)
            {
                PersistentPersistentCall persistentCall = m_calls[i];
                if (persistentCall != null)
                {
                    object call = Activator.CreateInstance(m_callType);
                    persistentCall.WriteTo(call, objects);
                    list.Add(call);
                }
                else
                {
                    list.Add(null);
                }
            }
            m_callsInfo.SetValue(persistentCalls, calls);
            m_persistentCallGroupInfo.SetValue(obj, persistentCalls);
        }
    }
}

