using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    public class MappingInfo
    {
        public readonly bool PopulateInstanceIDToPersistentID;
        public readonly bool PopulatePeristentIDToObj;

        public readonly int Offset;

        public readonly Dictionary<int, int> m_instanceIdToId = new Dictionary<int, int>();
        public readonly Dictionary<int, UnityObject> m_persistentIdToObj = new Dictionary<int, UnityObject>();

        public MappingInfo(int offset, bool populateInstanceIDToPersistentID, bool populatePersistentIDToObj)
        {
            Offset = offset;
            PopulateInstanceIDToPersistentID = populateInstanceIDToPersistentID;
            PopulatePeristentIDToObj = populatePersistentIDToObj;
        }
        
        public void Add(int instanceId, int mappedId)
        {
            try
            {
                m_instanceIdToId.Add(Offset + instanceId, mappedId);
            }
            catch(ArgumentException)
            {
                Debug.LogWarningFormat("An element with offset + instanceId = {0} already exists. mappedId = {1}", Offset + instanceId, mappedId);
                throw;
            }
            
        }

        public void Add(int persistentId, UnityObject obj)
        {
            try
            {
                m_persistentIdToObj.Add(persistentId, obj);
            }
            catch (ArgumentException)
            {
                UnityObject existingObj = m_persistentIdToObj[persistentId];
                string existingObjStr = existingObj != null ? existingObj.GetType().Name + ", name: " + existingObj.name + ", InstanceID: " + existingObj.GetInstanceID() : "null";
                string newObjStr = obj != null ? obj.GetType().Name + ", name: " + obj.name + ", InstanceID: " + obj.GetInstanceID() : "null";
                Debug.LogWarningFormat("An element with mappedId = {0} already exists. existing obj = {1}; new obj = {2}", persistentId, existingObjStr, newObjStr);
                throw;
            }

           
        }
    }

    [CreateAssetMenu(fileName = "AssetLibrary", menuName = "RT Asset Library", order = 1)]
    public class AssetLibraryAsset : ScriptableObject
    {
        [SerializeField] AssetLibraryInfo m_assetLibrary;

        public AssetLibraryInfo AssetLibrary
        {
            get { return m_assetLibrary; }
            set { m_assetLibrary = value; }
        }

        public void GetIDMapping(MappingInfo mapping)
        {
            if(!mapping.PopulatePeristentIDToObj && !mapping.PopulateInstanceIDToPersistentID)
            {
                return;
            }

            if(m_assetLibrary == null || m_assetLibrary.Folders == null || m_assetLibrary.Folders.Count == 0)
            {
                return;
            }

            for(int i = 0; i < m_assetLibrary.Folders.Count; ++i)
            {
                AssetFolderInfo folder = m_assetLibrary.Folders[i];
                if(folder != null)
                {
                    GetIDMapping(folder, mapping);
                }
            }
        }
         
        private void GetIDMapping(AssetFolderInfo folder, MappingInfo mapping)
        {
            if(folder == null)
            {
                return;
            }

            if(folder.Assets != null && folder.Assets.Count > 0)
            {
                for(int i = 0; i < folder.Assets.Count; ++i)
                {
                    AssetInfo asset = folder.Assets[i];
                    if(asset.Object != null)
                    {
                        if(mapping.PopulateInstanceIDToPersistentID)
                        {
                            mapping.Add(asset.Object.GetInstanceID(), asset.PersistentId);
                        }

                        if(mapping.PopulatePeristentIDToObj)
                        {
                            mapping.Add(asset.PersistentId, asset.Object);
                        }
                    }
                }
            }

            if(folder.hasChildren)
            {
                for(int i = 0; i < folder.children.Count; ++i)
                {
                    AssetFolderInfo subfolder = (AssetFolderInfo)folder.children[i];
                    if(subfolder != null)
                    {
                        GetIDMapping(subfolder, mapping);
                    }   
                }
            }
        }

        private void Awake()
        {
            if (m_assetLibrary == null || m_assetLibrary.Folders.Count == 0)
            {
                AssetFolderInfo assetsFolder = new AssetFolderInfo
                {
                    name = "Assets",
                    depth = -1,
                };

                m_assetLibrary = new AssetLibraryInfo
                {
                    name = "Root",
                    depth = -1,
                    Folders = new List<AssetFolderInfo>
                    {
                        assetsFolder
                    }
                };
            }
          
        }
    }
}
