using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Battlehub.RTSaveLoad2.Internal
{
    public class PersistentTemplateAttribute : Attribute
    {
        public readonly string ForType;
        public readonly string[] FieldNames;

        public PersistentTemplateAttribute(string forType, params string[] templateFields)
        {
            ForType = forType;
            FieldNames = templateFields;
        }
    }

    public class PersistentSurrogateTemplate : ScriptableObject
    {
        public virtual void ReadFrom(object obj)
        {
            throw new InvalidOperationException();
        }

        public virtual object WriteTo(object obj)
        {
            throw new InvalidOperationException();
        }

        public virtual void GetDeps(GetDepsContext context)
        {
            throw new InvalidOperationException();
        }

        public virtual void GetDepsFrom(object obj, GetDepsContext context)
        {
            throw new InvalidOperationException();
        }

        protected void AddDep(long depenency, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddDep(long[] depenencies, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddDep(object obj, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddDep<T>(T[] dependencies, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddSurrogateDeps(object surrogate, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddSurrogateDeps<T>(T[] surrogateArray, object context)
        {
            throw new InvalidOperationException();
        }

        protected long ToID(object uo)
        {
            throw new InvalidOperationException();
        }

        protected long[] ToID(object[] uo)
        {
            throw new InvalidOperationException();
        }

        public T FromID<T>(long id)
        {
            throw new InvalidOperationException();
        }

        public T[] FromID<T>(long[] id)
        {
            throw new InvalidOperationException();
        }
    }
}