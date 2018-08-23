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
        public PersistentDescriptor[] Descriptors;
        [ProtoMember(2)]
        public PersistentObject[] Data;
        [ProtoMember(3)]
        public long[] Identifiers;

        //Asset library ordinals
        [ProtoMember(4)]
        public int[] Usings;

        //Identifiers of assets PersistentPrefab depends on
        [ProtoMember(5)]
        public long[] Dependencies;

        protected readonly ITypeMap m_typeMap;
        protected readonly IAssetDB m_assetDB;
        public PersistentPrefab()
        {
            m_typeMap = RTSL2Deps.Get.TypeMap;
            m_assetDB = RTSL2Deps.Get.AssetDB;
        }

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            GameObject go = (GameObject)obj;

            List<PersistentObject> data = new List<PersistentObject>();
            List<long> identifiers = new List<long>();
            HashSet<int> usings = new HashSet<int>();
            GetDepsContext getDepsCtx = new GetDepsContext();
            Descriptors = new PersistentDescriptor[1];
            Descriptors[0] = CreateDescriptorAndData(go, data, identifiers, usings, getDepsCtx);

            Identifiers = identifiers.ToArray();
            Data = data.ToArray();

            Dependencies = getDepsCtx.Dependencies.ToArray();
            DependenciesToUsings(Dependencies, usings);
            Usings = usings.ToArray();
        }

        protected void DependenciesToUsings(long[] dependencies, HashSet<int> usings)
        {
            for (int i = 0; i < dependencies.Length; ++i)
            {
                long dependency = dependencies[i];
                if (m_assetDB.IsResourceID(dependency))
                {
                    int ordinal = m_assetDB.ToOrdinal(dependency);
                    if (!usings.Contains(ordinal))
                    {
                        usings.Add(ordinal);
                    }
                }
            }
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            for(int i = 0; i < Data.Length; ++i)
            {
                PersistentObject data = Data[i];
                long id = Identifiers[i];
 
                UnityObject unityObj = m_assetDB.FromID<UnityObject>(id);
                data.WriteTo(unityObj);
            }

            return obj;
        }

        protected PersistentDescriptor CreateDescriptorAndData(GameObject go, List<PersistentObject> persistentData, List<long> persistentIdentifiers, HashSet<int> usings, GetDepsContext getDepsCtx, PersistentDescriptor parentDescriptor = null)
        {
            if (go.GetComponent<RTSL2Ignore>())
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
            if(m_assetDB.IsResourceID(persistentID))
            {
                int ordinal = m_assetDB.ToOrdinal(persistentID);
                usings.Add(ordinal);
            }
            
            PersistentDescriptor descriptor = new PersistentDescriptor(persistentType, persistentID);
            descriptor.Parent = parentDescriptor;

            PersistentObject goData = (PersistentObject)Activator.CreateInstance(persistentType);
            goData.ReadFrom(go);
            goData.GetDeps(getDepsCtx);
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
                    if (m_assetDB.IsResourceID(componentID))
                    {
                        int ordinal = m_assetDB.ToOrdinal(componentID);
                        usings.Add(ordinal);
                    }
                    PersistentDescriptor componentDescriptor = new PersistentDescriptor(persistentComponentType, componentID);
                    componentDescriptor.Parent = descriptor;
                    componentDescriptors.Add(componentDescriptor);

                    PersistentObject componentData = (PersistentObject)Activator.CreateInstance(persistentComponentType);
                    componentData.ReadFrom(component);
                    componentData.GetDeps(getDepsCtx);
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
                    PersistentDescriptor childDescriptor = CreateDescriptorAndData(child.gameObject, persistentData, persistentIdentifiers, usings, getDepsCtx, descriptor);
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


