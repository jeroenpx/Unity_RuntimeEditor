using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentArgumentCache
    {
        [ProtoMember(1)]
        public bool m_BoolArgument;
        [ProtoMember(2)]
        public float m_FloatArgument;
        [ProtoMember(3)]
        public int m_IntArgument;
        [ProtoMember(4)]
        public string m_StringArgument;
        [ProtoMember(5)]
        public long m_ObjectArgument; //instanceId
        [ProtoMember(6)]
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

    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentPersistentCall
    {
        [ProtoMember(1)]
        public PersistentArgumentCache m_Arguments;
        [ProtoMember(2)]
        public UnityEventCallState m_CallState;
        [ProtoMember(3)]
        public string m_MethodName;
        [ProtoMember(4)]
        public PersistentListenerMode m_Mode;
        [ProtoMember(5)]
        public long m_Target; //instanceId
        [ProtoMember(6)]
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

    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentUnityEvent : PersistentUnityEventBase
    {
        public static implicit operator UnityEvent(PersistentUnityEvent surrogate)
        {
            return (UnityEvent)surrogate.WriteTo(new UnityEvent());
        }

        public static implicit operator PersistentUnityEvent(UnityEvent obj)
        {
            PersistentUnityEvent surrogate = new PersistentUnityEvent();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }

    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentUnityEventBase : PersistentSurrogate
    {
        private static FieldInfo m_persistentCallGroupInfo;
        private static FieldInfo m_callsInfo;
        private static Type m_callType;

        [ProtoMember(1)]
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