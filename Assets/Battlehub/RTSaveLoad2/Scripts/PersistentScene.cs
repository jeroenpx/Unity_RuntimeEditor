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
    public class PersistentScene : PersistentObject
    {
        [ProtoMember(1)]
        public PersistentDescriptor[] m_descriptors;
        [ProtoMember(2)]
        public PersistentObject[] m_data;
        [ProtoMember(3)]
        public long[] m_identifiers;
        
        private readonly ITypeMap m_typeMap;
        private readonly IAssetDB m_assetDB;
        protected PersistentScene()
        {
            m_typeMap = RTSL2Deps.Get.TypeMap;
            m_assetDB = RTSL2Deps.Get.AssetDB;
        }

        public override void ReadFrom(object obj)
        {
            Scene scene = (Scene)obj;
            GameObject[] rootGameObjects = scene.GetRootGameObjects();

            List<PersistentObject> data = new List<PersistentObject>();
            List<long> identifiers = new List<long>(); 

            m_descriptors = new PersistentDescriptor[rootGameObjects.Length];
            for(int i = 0; i < m_descriptors.Length; ++i)
            {
                GameObject rootGO = rootGameObjects[i];
                m_descriptors[i] = CreateDescriptorAndData(rootGO, data, identifiers);
            }

            m_identifiers = identifiers.ToArray();
            m_data = data.ToArray();
        }

        public override object WriteTo(object obj)
        {
            if (m_descriptors == null && m_data == null)
            {
                return obj;
            }

            if (m_descriptors == null && m_data != null || m_data != null && m_descriptors == null)
            {
                throw new ArgumentException("data is corrupted", "scene");
            }

            if (m_descriptors.Length == 0)
            {
                return obj;
            }

            if(m_identifiers == null || m_identifiers.Length != m_data.Length)
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

            return scene;
        }

        private PersistentDescriptor CreateDescriptorAndData(GameObject go, List<PersistentObject> persistentData, List<long> persistentIdentifiers, PersistentDescriptor parentDescriptor = null)
        {
            if (go.GetComponent<PersistentIgnore>())
            {
                //Do not save persistent ignore objects
                return null;
            }
            Type persistentType = m_typeMap.ToPersistentType(go.GetType());
            if(persistentType == null)
            {
                return null;
            }

            long persistentID = ToID(go);
            PersistentDescriptor descriptor = new PersistentDescriptor(persistentType, persistentID);
            descriptor.Parent = parentDescriptor;

            PersistentObject goData = (PersistentObject)Activator.CreateInstance(persistentType);
            goData.ReadFrom(go);
            persistentData.Add(goData);
            persistentIdentifiers.Add(persistentID);

            Component[] components = go.GetComponents<Component>().Where(c => c != null).ToArray();
            if (components.Length > 0)
            {
                List<PersistentDescriptor> componentDescriptors = new List<PersistentDescriptor>();
                for (int i = 0; i < components.Length; ++i)
                {
                    Component component = components[i];
                    Type persistentComponentType = m_typeMap.ToPersistentType(component.GetType());
                    if(persistentComponentType == null)
                    {
                        continue;
                    }

                    long componentID = ToID(component);
                    PersistentDescriptor componentDescriptor = new PersistentDescriptor(persistentComponentType, componentID);
                    componentDescriptor.Parent = descriptor;
                    componentDescriptors.Add(componentDescriptor);

                    PersistentObject componentData = (PersistentObject)Activator.CreateInstance(persistentComponentType);
                    componentData.ReadFrom(component);
                    persistentData.Add(componentData);
                    persistentIdentifiers.Add(componentID);
                }

                if(componentDescriptors.Count > 0)
                {
                    descriptor.Components = componentDescriptors.ToArray();
                }   
            }

            Transform transform = go.transform;
            if (transform.childCount > 0)
            {
                List<PersistentDescriptor> children = new List<PersistentDescriptor>();
                foreach (Transform child in transform)
                {
                    PersistentDescriptor childDescriptor = CreateDescriptorAndData(child.gameObject, persistentData, persistentIdentifiers, descriptor);
                    if (childDescriptor != null)
                    {
                        children.Add(childDescriptor);
                    }
                }

                descriptor.Children = children.ToArray();
            }

            return descriptor;
        }
    }

}


