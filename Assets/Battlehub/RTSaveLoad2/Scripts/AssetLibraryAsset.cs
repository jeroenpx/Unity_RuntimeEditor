using System;
using System.Collections.Generic;
using UnityEngine;


using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    

    public class MappingInfo
    {
        public class IDLists
        {
            public readonly List<int> InstanceIDs = new List<int>();
            public readonly List<int> PersistentIDs = new List<int>();
            
            public IDLists(List<int> instanceIDs, List<int> persistentIDs)
            {
                InstanceIDs = instanceIDs;
                PersistentIDs = persistentIDs;
            }
        }
        
        public readonly Dictionary<int, int> InstanceIDtoPID = new Dictionary<int, int>();
        public readonly Dictionary<int, UnityObject> PersistentIDtoObj = new Dictionary<int, UnityObject>();
        public readonly Dictionary<AssetLibraryAsset, IDLists> LibToIDs = new Dictionary<AssetLibraryAsset, IDLists>();

        public void Add(AssetLibraryAsset lib, List<int> instanceIDs, List<int> persistentIDs)
        {
            try
            {
                LibToIDs.Add(lib, new IDLists(instanceIDs, persistentIDs));
            }
            catch (ArgumentException)
            {
                Debug.LogWarningFormat("Asset library {0} ordinal {1} already added.", lib.name, lib.Ordinal);
                throw;
            }
        }
        
        public void Add(int instanceID, int persistentID)
        {
            try
            {
                InstanceIDtoPID.Add(instanceID, persistentID);
            }
            catch(ArgumentException)
            {
                Debug.LogWarningFormat("An element with instanceId = {0} already exists. mappedId = {1}", instanceID, persistentID);
                throw;
            }
        }

        public void Add(int persistentID, UnityObject obj)
        {
            try
            {
                PersistentIDtoObj.Add(persistentID, obj);
            }
            catch (ArgumentException)
            {
                UnityObject existingObj = PersistentIDtoObj[persistentID];
                string existingObjStr = existingObj != null ? existingObj.GetType().Name + ", name: " + existingObj.name + ", InstanceID: " + existingObj.GetInstanceID() : "null";
                string newObjStr = obj != null ? obj.GetType().Name + ", name: " + obj.name + ", InstanceID: " + obj.GetInstanceID() : "null";
                Debug.LogWarningFormat("An element with mappedId = {0} already exists. existing obj = {1}; new obj = {2}", persistentID, existingObjStr, newObjStr);
                throw;
            }

           
        }
    }

    [CreateAssetMenu(fileName = "AssetLibrary", menuName = "RT Asset Library", order = 1)]
    public class AssetLibraryAsset : ScriptableObject
    {
        private int m_offset;
        private int m_ordinal;
        public int Ordinal
        {
            get { return m_ordinal; }
            set
            {
                m_ordinal = value;
                m_offset = m_ordinal << AssetLibraryInfo.ORDINAL_OFFSET;
            }
        }
        
        [SerializeField] AssetLibraryInfo m_assetLibrary;

        public AssetLibraryInfo AssetLibrary
        {
            get { return m_assetLibrary; }
            set { m_assetLibrary = value; }
        }

        public void UnloadIDMappingFrom(MappingInfo mapping)
        {
            MappingInfo.IDLists idLists;
            if (mapping.LibToIDs.TryGetValue(this, out idLists))
            {
                for(int i = 0; i < idLists.InstanceIDs.Count; ++i)
                {
                    int instanceID = idLists.InstanceIDs[i];
                    mapping.InstanceIDtoPID.Remove(instanceID);
                }

                for(int i = 0; i < idLists.PersistentIDs.Count; ++i)
                {
                    int persistentID = idLists.PersistentIDs[i];
                    mapping.PersistentIDtoObj.Remove(persistentID);
                }

                mapping.LibToIDs.Remove(this);
            }
        }

        public void LoadIDMappingTo(MappingInfo mapping, bool IIDtoPID, bool PIDtoObj)
        {
            if(!IIDtoPID && !PIDtoObj)
            {
                return;
            }

            if(m_assetLibrary == null || m_assetLibrary.Folders == null || m_assetLibrary.Folders.Count == 0)
            {
                return;
            }

            List<int> instanceIDs = new List<int>();
            List<int> persistentIDs = new List<int>();

            for (int i = 0; i < m_assetLibrary.Folders.Count; ++i)
            {
                AssetFolderInfo folder = m_assetLibrary.Folders[i];
                if(folder != null)
                {
                    LoadIDMappingTo(folder, mapping, instanceIDs, persistentIDs, IIDtoPID, PIDtoObj);
                }
            }

            mapping.Add(this, instanceIDs, persistentIDs);
        }
         
        private void LoadIDMappingTo(
            AssetFolderInfo folder, 
            MappingInfo mapping,
            List<int> instanceIDs,
            List<int> persistentIDs,
            bool IIDtoPID, bool PIDtoObj)
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
                        if(IIDtoPID)
                        {
                            int instanceID = asset.Object.GetInstanceID();
                            mapping.Add(instanceID, m_offset + asset.PersistentID);
                            instanceIDs.Add(instanceID);
                        }

                        if(PIDtoObj)
                        {
                            int persistentID = m_offset + asset.PersistentID;
                            mapping.Add(persistentID, asset.Object);
                            persistentIDs.Add(persistentID);
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
                        LoadIDMappingTo(subfolder, mapping, instanceIDs, persistentIDs, IIDtoPID, PIDtoObj);
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
