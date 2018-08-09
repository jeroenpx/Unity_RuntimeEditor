using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    
    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentScene : PersistentPrefab
    {
        protected override void ReadFromImpl(object obj)
        {
            Scene scene = (Scene)obj;
            
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            List<PersistentObject> data = new List<PersistentObject>();
            List<long> identifiers = new List<long>();    
            List<PersistentDescriptor> descriptors = new List<PersistentDescriptor>(rootGameObjects.Length);
            GetDepsContext getDepsCtx = new GetDepsContext();
            HashSet<int> usings = new HashSet<int>();
            for(int i = 0; i < rootGameObjects.Length; ++i)
            {
                GameObject rootGO = rootGameObjects[i];
                PersistentDescriptor descriptor = CreateDescriptorAndData(rootGO, data, identifiers, usings, getDepsCtx);
                if(descriptor != null)
                {
                    descriptors.Add(descriptor);
                }
            }

            Descriptors = descriptors.ToArray();
            Identifiers = identifiers.ToArray();
            Data = data.ToArray();

            Dependencies = getDepsCtx.Dependencies.ToArray();
            DependenciesToUsings(Dependencies, usings);
            Usings = usings.ToArray();
        }

        protected override object WriteToImpl(object obj)
        {
            if (Descriptors == null && Data == null)
            {
                return obj;
            }

            if (Descriptors == null && Data != null || Data != null && Descriptors == null)
            {
                throw new ArgumentException("data is corrupted", "scene");
            }

            if (Descriptors.Length == 0)
            {
                return obj;
            }

            if(Identifiers == null || Identifiers.Length != Data.Length)
            {
                throw new ArgumentException("data is corrupted", "scene");
            }
      
            Scene scene = (Scene)obj;
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                GameObject rootGO = rootGameObjects[i];
                if (rootGO.GetComponent<PersistentIgnore>())
                {
                    continue;
                }

                UnityObject.Destroy(rootGO);
            }

            Dictionary<int, UnityObject> idToUnityObj = new Dictionary<int, UnityObject>();
            for (int i = 0; i < Descriptors.Length; ++i)
            {
                PersistentDescriptor descriptor = Descriptors[i];
                if(descriptor != null)
                {
                    CreateGameObjectWithComponents(m_typeMap, descriptor, idToUnityObj);
                }
            }

            m_assetDB.RegisterSceneObjects(idToUnityObj);
            RestoreDataAndResolveDependencies();

            return scene;
        }

        public void Unload()
        {
            m_assetDB.UnregisterSceneObjects();
        }

        /// <summary>
        /// Create GameObjects hierarchy and Add Components recursively
        /// </summary>
        /// <param name="descriptor">PersistentObject descriptor (initially root descriptor)</param>
        /// <param name="idToObj">Dictionary instanceId->UnityObject which will be populated with GameObjects and Components</param>
        private void CreateGameObjectWithComponents(ITypeMap typeMap, PersistentDescriptor descriptor, Dictionary<int, UnityObject> idToObj, List<GameObject> createdGameObjects = null, Dictionary<long, UnityObject> decomposition = null)
        {
            UnityObject objGo;
            GameObject go;
            if (idToObj.TryGetValue(m_assetDB.ToInt32(descriptor.PersistentID), out objGo))
            {
                throw new ArgumentException(string.Format("duplicate object descriptor found in descriptors hierarchy. {0}", descriptor.ToString()), "descriptor");
            }
            else
            {
                go = new GameObject();
                idToObj.Add(m_assetDB.ToInt32(descriptor.PersistentID), go);
            }

            if (decomposition != null)
            {
                if (!decomposition.ContainsKey(descriptor.PersistentID))
                {
                    decomposition.Add(descriptor.PersistentID, go);
                }
            }

            if(createdGameObjects != null)
            {
                createdGameObjects.Add(go);
            }
            
            go.SetActive(false);

            if (descriptor.Parent != null)
            {
                UnityObject parentGO;
                if (!idToObj.TryGetValue(m_assetDB.ToInt32(descriptor.Parent.PersistentID), out parentGO))
                {
                    throw new ArgumentException(string.Format("objects dictionary is supposed to have object with PersistentID {0} at this stage. Descriptor {1}", descriptor.Parent.PersistentID, descriptor, "descriptor"));
                }

                if (parentGO == null)
                {
                    throw new ArgumentException(string.Format("object with PersistentID {0} should have GameObject type. Descriptor {1}", descriptor.Parent.PersistentID, descriptor, "descriptor"));
                }
                go.transform.SetParent(((GameObject)parentGO).transform, false);
            }

            if (descriptor.Components != null)
            {
                Dictionary<Type, bool> requirements = new Dictionary<Type, bool>();
                for (int i = 0; i < descriptor.Components.Length; ++i)
                {
                    PersistentDescriptor componentDescriptor = descriptor.Components[i];
                    
                    Type persistentComponentType = Type.GetType(componentDescriptor.AssemblyQualifiedName);
                    if (persistentComponentType == null)
                    {
                        Debug.LogWarningFormat("Unknown type {0} associated with component Descriptor {1}", componentDescriptor.AssemblyQualifiedName, componentDescriptor.ToString());
                        continue;
                    }
                    Type componentType = typeMap.ToUnityType(persistentComponentType);
                    if(componentType == null)
                    {
                        Debug.LogWarningFormat("There is no mapped type for " + persistentComponentType.FullName + " in TypeMap");
                        continue;
                    }

                    if (!componentType.IsSubclassOf(typeof(Component)))
                    {
                        Debug.LogErrorFormat("{0} is not subclass of {1}", componentType.FullName, typeof(Component).FullName);
                        continue;
                    }

                    UnityObject obj;
                    if (idToObj.TryGetValue(m_assetDB.ToInt32(componentDescriptor.PersistentID), out obj))
                    {
                        if (obj != null && !(obj is Component))
                        {
                            Debug.LogError("Invalid Type. Component " + obj.name + " " + obj.GetType() + " " + obj.GetInstanceID() + " " + descriptor.AssemblyQualifiedName + " " + componentDescriptor.AssemblyQualifiedName);
                        }
                    }
                    else
                    {
                        obj = AddComponent(idToObj, go, requirements, componentDescriptor, componentType);
                    }

                    if (decomposition != null)
                    {
                        if (!decomposition.ContainsKey(componentDescriptor.PersistentID))
                        {
                            decomposition.Add(componentDescriptor.PersistentID, obj);
                        }
                    }
                }
            }

            if (descriptor.Children != null)
            {
                for (int i = 0; i < descriptor.Children.Length; ++i)
                {
                    PersistentDescriptor childDescriptor = descriptor.Children[i];
                    CreateGameObjectWithComponents(typeMap, childDescriptor, idToObj, createdGameObjects, decomposition);
                }
            }
        }

        /// <summary>
        /// Add  dependencies here to let AddComponent method to figure out which components automatically added
        /// for example ParticleSystemRenderer should be added automatically if ParticleSystem component exists 
        /// </summary>
        public readonly static Dictionary<Type, HashSet<Type>> ComponentDependencies = new Dictionary<Type, HashSet<Type>>
            {
                //type depends on <- { types }
                { typeof(ParticleSystemRenderer), new HashSet<Type> { typeof(ParticleSystem) } }
            };

        private UnityObject AddComponent(Dictionary<int, UnityObject> idToObj, GameObject go, Dictionary<Type, bool> requirements, PersistentDescriptor componentDescriptor, Type componentType)
        {
            Component component;
            bool isReqFulfilled = requirements.ContainsKey(componentType) && requirements[componentType];
            bool maybeComponentAlreadyAdded =
                !isReqFulfilled ||
                componentType.IsSubclassOf(typeof(Transform)) ||
                componentType == typeof(Transform) ||
                componentType.IsDefined(typeof(DisallowMultipleComponent), true) ||
                ComponentDependencies.ContainsKey(componentType) && ComponentDependencies[componentType].Any(d => go.GetComponent(d) != null);

            if (maybeComponentAlreadyAdded)
            {
                component = go.GetComponent(componentType);
                if (component == null)
                {
                    component = go.AddComponent(componentType);
                }
                if (!isReqFulfilled)
                {
                    requirements[componentType] = true;
                }
            }
            else
            {
                component = go.AddComponent(componentType);
                if (component == null)
                {
                    component = go.GetComponent(componentType);
                }
            }
            if (component == null)
            {
                Debug.LogErrorFormat("Unable to add or get component of type {0}", componentType);
            }
            else
            {
                object[] requireComponents = component.GetType().GetCustomAttributes(typeof(RequireComponent), true);
                for (int j = 0; j < requireComponents.Length; ++j)
                {
                    RequireComponent requireComponent = requireComponents[j] as RequireComponent;
                    if (requireComponent != null)
                    {
                        if (requireComponent.m_Type0 != null && !requirements.ContainsKey(requireComponent.m_Type0))
                        {
                            bool fulfilled = go.GetComponent(requireComponent.m_Type0);
                            requirements.Add(requireComponent.m_Type0, fulfilled);
                        }
                        if (requireComponent.m_Type1 != null && !requirements.ContainsKey(requireComponent.m_Type1))
                        {
                            bool fulfilled = go.GetComponent(requireComponent.m_Type1);
                            requirements.Add(requireComponent.m_Type1, fulfilled);
                        }
                        if (requireComponent.m_Type2 != null && !requirements.ContainsKey(requireComponent.m_Type2))
                        {
                            bool fulfilled = go.GetComponent(requireComponent.m_Type2);
                            requirements.Add(requireComponent.m_Type2, fulfilled);
                        }
                    }
                }
                idToObj.Add(m_assetDB.ToInt32(componentDescriptor.PersistentID), component);
            }

            return component;
        }

        private void RestoreDataAndResolveDependencies()
        {
            List<GameObject> goList = new List<GameObject>();
            List<bool> goActivationList = new List<bool>();
            
            for (int i = 0; i < Data.Length; ++i)
            {
                PersistentObject data = Data[i];
                long id = Identifiers[i];

                UnityObject obj = FromID<UnityObject>(id);
                if (obj == null)
                {
                    Debug.LogWarningFormat("objects does not have object with instance id {0} however PersistentData of type {1} is present", id, data.GetType());
                    continue;
                }

                data.WriteTo(obj);
                if (obj is GameObject)
                {
                    goList.Add((GameObject)obj);
                    PersistentGameObject goData = (PersistentGameObject)data;
                    goActivationList.Add(goData.ActiveSelf);
                }
            }

            for (int i = 0; i < goList.Count; ++i)
            {
                bool activeSelf = goActivationList[i];
                GameObject go = goList[i];
                go.SetActive(activeSelf);
            }
        }
    }
}


