using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;

namespace UnityEngine.Battlehub.SL2
{ }
namespace Battlehub.RTSaveLoad2
{
    public class CustomImplementationAttribute : Attribute
    {
    }

    [ProtoContract]
    public class IntArray
    {
        [ProtoMember(1)]
        public int[] Array;
    }

    public interface IPersistentSurrogate
    {
        void ReadFrom(object obj);

        object WriteTo(object obj);

        void GetDeps(GetDepsContext context);

        void GetDepsFrom(object obj, GetDepsFromContext context);
    }

    public class GetDepsContext
    {
        public readonly HashSet<long> Dependencies = new HashSet<long>();
        public readonly HashSet<object> VisitedObjects = new HashSet<object>();

        public void Clear()
        {
            Dependencies.Clear();
            VisitedObjects.Clear();
        }
    }

    public class GetDepsFromContext
    {
        public readonly HashSet<object> Dependencies = new HashSet<object>();
        public readonly HashSet<object> VisitedObjects = new HashSet<object>();

        public void Clear()
        {
            Dependencies.Clear();
            VisitedObjects.Clear();
        }
    }

    public abstract class PersistentSurrogate : IPersistentSurrogate
    {
        private IIDMap m_idMap;
        protected PersistentSurrogate()
        {
            m_idMap = RTSL2Deps.Get.IDMap;
        }

        protected virtual void ReadFromImpl(object obj) { }
        protected virtual object WriteToImpl(object obj) { return obj; }
        protected virtual void GetDepsImpl(GetDepsContext context) { }
        protected virtual void GetDepsFromImpl(object obj, GetDepsFromContext context) { }


        public virtual void ReadFrom(object obj)
        {
            ReadFromImpl(obj);
        }

        public virtual object WriteTo(object obj)
        {
            obj = WriteToImpl(obj);
            return obj;
        }

        public virtual void GetDeps(GetDepsContext context)
        {
            if (context.VisitedObjects.Contains(this))
            {
                return;
            }
            context.VisitedObjects.Add(this);
            GetDepsImpl(context);
        }

        public virtual void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            if (context.VisitedObjects.Contains(this))
            {
                return;
            }
            context.VisitedObjects.Add(this);
            GetDepsFromImpl(obj, context);
        }

        protected void AddDep(long depenency, GetDepsContext context)
        {
            if (depenency > 0 && !context.Dependencies.Contains(depenency))
            {
                context.Dependencies.Add(depenency);
            }
        }

        protected void AddDep(long[] depenencies, GetDepsContext context)
        {
            for (int i = 0; i < depenencies.Length; ++i)
            {
                AddDep(depenencies[i], context);
            }
        }

        protected void AddDep(object obj, GetDepsFromContext context)
        {
            if (obj != null && !context.Dependencies.Contains(obj))
            {
                context.Dependencies.Add(obj);
            }
        }

        protected void AddDep<T>(T[] dependencies, GetDepsFromContext context)
        {
            for (int i = 0; i < dependencies.Length; ++i)
            {
                AddDep(dependencies[i], context);
            }
        }

        protected void AddSurrogateDeps(PersistentSurrogate surrogate, GetDepsContext context)
        {
            surrogate.GetDeps(context);
        }

        protected void AddSurrogateDeps<T>(T[] surrogateArray, GetDepsContext context) where T : PersistentSurrogate
        {
            for (int i = 0; i < surrogateArray.Length; ++i)
            {
                PersistentSurrogate surrogate = surrogateArray[i];
                surrogate.GetDeps(context);
            }
        }

        protected void AddSurrogateDeps(object obj, GetDepsFromContext context)
        {
            if (obj != null)
            {
                PersistentSurrogate surrogate = (PersistentSurrogate)obj;
                surrogate.GetDepsFrom(obj, context);
            }
        }

        protected void AddSurrogateDeps<T>(T[] objArray, GetDepsFromContext context)
        {
            for (int i = 0; i < objArray.Length; ++i)
            {
                object obj = objArray[i];
                if (obj != null)
                {
                    PersistentSurrogate surrogate = (PersistentSurrogate)obj;
                    surrogate.GetDepsFrom(obj, context);
                }
            }
        }

        protected long ToID(UnityObject uo)
        {
            return m_idMap.ToID(uo);
        }

        protected long[] ToID(UnityObject[] uo)
        {
            return m_idMap.ToID(uo);
        }

        public T FromID<T>(long id) where T : UnityObject
        {
            return m_idMap.FromID<T>(id);
        }

        public T[] FromID<T>(long[] id) where T : UnityObject
        {
            return m_idMap.FromID<T>(id);
        }
    }
}

