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
    public class PersistentRuntimeScene : PersistentRuntimePrefab
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
                if (rootGO.GetComponent<RTSL2Ignore>())
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

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            if(!(obj is Scene))
            {
                return;
            }

            Scene scene = (Scene)obj;
            GameObject[] gameObjects = scene.GetRootGameObjects();

            for(int i = 0; i < gameObjects.Length; ++i)
            {
                base.GetDepsFromImpl(gameObjects[i], context);
            }
        }
    }
}


