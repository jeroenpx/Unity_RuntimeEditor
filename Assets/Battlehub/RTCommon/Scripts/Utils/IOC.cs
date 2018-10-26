using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.RTCommon
{
    public class IOC
    {
        private class Item
        {
            public object Instance;
            public MulticastDelegate Function;
            public Item(object instance)
            {
                Instance = instance;
            }

            public Item(MulticastDelegate function)
            {
                Function = function;
            }

            public T Resolve<T>()
            {
                if(Instance != null)
                {
                    return (T)Instance;
                }

                return ((Func<T>)Function)();
            }
        }

        private static Dictionary<Type, Item> m_registered = new Dictionary<Type, Item>();
        private static Dictionary<Type, Item> m_fallbacks = new Dictionary<Type, Item>();

        public static void Register<T>(Func<T> func)
        {
            if(func == null)
            {
                throw new ArgumentNullException("func");
            }
            if(m_registered.ContainsKey(typeof(T)))
            {
                Debug.LogWarning("type {0} already registered.");
                return;
            }

            m_registered.Add(typeof(T), new Item(func));
        }

        public static void Register<T>(T instance)
        {
            if(instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if(m_registered.ContainsKey(typeof(T)))
            {
                Debug.LogWarning("type {0} already registered.");
                return;
            }

            m_registered.Add(typeof(T), new Item(instance));
        }

        public static void Unregister<T>(Func<T> func)
        {
            Item item;
            if(m_registered.TryGetValue(typeof(T), out item))
            {
                if(item.Function != null && item.Function.Equals(func))
                {
                    m_registered.Remove(typeof(T));
                }
            }
        }

        public static void Unregister<T>(T instance)
        {
            Item item;
            if(m_registered.TryGetValue(typeof(T), out item))
            {
                if(ReferenceEquals(item, instance))
                {
                    m_registered.Remove(typeof(T));
                }
            }
        }

        public static void RegisterFallback<T>(Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            if (m_fallbacks.ContainsKey(typeof(T)))
            {
                Debug.LogWarning("fallback for type {0} already registered.");
                return;
            }
            m_fallbacks[typeof(T)] = new Item(func);
        }


        public static void RegisterFallback<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (m_fallbacks.ContainsKey(typeof(T)))
            {
                Debug.LogWarning("type {0} already registered.");
                return;
            }

            m_fallbacks.Add(typeof(T), new Item(instance));
        }

        public static void UnregisterFallback<T>(Func<T> func)
        {
            Item item;
            if (m_fallbacks.TryGetValue(typeof(T), out item))
            {
                if (item.Function != null && item.Equals(func))
                {
                    m_fallbacks.Remove(typeof(T));
                }
            }
        }

        public static void UnregisterFallback<T>(T instance)
        {
            Item item;
            if (m_fallbacks.TryGetValue(typeof(T), out item))
            {
                if (ReferenceEquals(item, instance))
                {
                    m_fallbacks.Remove(typeof(T));
                }
            }
        }

        public static T Resolve<T>()
        {
            Item item;
            if (m_registered.TryGetValue(typeof(T), out item))
            {
                return item.Resolve<T>();
            }
            else
            {
                if(m_fallbacks.TryGetValue(typeof(T), out item))
                {
                    return item.Resolve<T>();
                }
            }
            return default(T);
        }

        public static bool ClearOnSceneUnloaded = true;
        public static void Clear()
        {
            m_registered.Clear();
            m_fallbacks.Clear();  
        }
       
        static IOC()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private static void OnSceneUnloaded(Scene arg0)
        {
            if(ClearOnSceneUnloaded)
            {
                Clear();
            }
        }
    }

}

