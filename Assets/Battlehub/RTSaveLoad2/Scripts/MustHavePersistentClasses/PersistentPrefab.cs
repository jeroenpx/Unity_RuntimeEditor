using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    [ProtoContract]
    public class PersistentPrefab : PersistentObject
    {
        [ProtoMember(1)]
        public PersistentDescriptor[] m_descriptors;
        [ProtoMember(2)]
        public PersistentObject[] m_data;
        [ProtoMember(3)]
        public long[] m_identifiers;

        protected readonly ITypeMap m_typeMap;
        protected readonly IAssetDB m_assetDB;
        public PersistentPrefab()
        {
            m_assetDB = RTSL2Deps.Get.AssetDB;
        }

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            GameObject go = (GameObject)obj;

            List<PersistentObject> data = new List<PersistentObject>();
            List<long> identifiers = new List<long>();

            m_descriptors = new PersistentDescriptor[1];
            m_descriptors[0] = CreateDescriptorAndData(go, data, identifiers);
     
            m_identifiers = identifiers.ToArray();
            m_data = data.ToArray();
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            for(int i = 0; i < m_data.Length; ++i)
            {
                PersistentObject data = m_data[i];
                long id = m_identifiers[i];
 
                UnityObject unityObj = m_assetDB.FromID<UnityObject>(id);
                data.WriteTo(unityObj);
            }

            return obj;
        }

        protected PersistentDescriptor CreateDescriptorAndData(GameObject go, List<PersistentObject> persistentData, List<long> persistentIdentifiers, PersistentDescriptor parentDescriptor = null)
        {
            if (go.GetComponent<PersistentIgnore>())
            {
                //Do not save persistent ignore objects
                return null;
            }
            Type persistentType = m_typeMap.ToPersistentType(go.GetType());
            if (persistentType == null)
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
                    if (persistentComponentType == null)
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

                if (componentDescriptors.Count > 0)
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


