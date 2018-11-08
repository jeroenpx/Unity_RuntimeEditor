using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{     
    public interface IIDMap
    {
        long NullID { get; }

        bool IsNullID(long id);
        bool IsInstanceID(long id);
        bool IsStaticResourceID(long id);
        bool IsStaticFolderID(long id);
        bool IsDynamicResourceID(long id);
        bool IsDynamicFolderID(long id);
        bool IsSceneID(long id);
        bool IsResourceID(long id);

        int ToOrdinal(long id);
        int ToOrdinal(int id);

        long ToStaticResourceID(int ordinal, int id);
        long ToStaticFolderID(int ordinal, int id);
        long ToDynamicResourceID(int ordinal, int id);
        long ToDynamicFolderID(int ordinal, int id);
        long ToSceneID(int ordinal, int id);

        long ToID(UnityObject uo);
        long[] ToID(UnityObject[] uo);

        bool IsMapped(long id);
        T FromID<T>(long id) where T : UnityObject;
        T[] FromID<T>(long[] id) where T : UnityObject;
    }

    //public interface IIDMapManager : IIDMap
    //{
    //    void LoadMapping(int ordinal, bool IIDtoPID, bool PIDtoObj);
    //    void UnloadMapping(int ordinal);
    //    void LoadMapping(bool IIDtoPID, bool PIDtoObj);
    //}

    public interface IAssetDB : IIDMap
    {
        void RegisterSceneObjects(Dictionary<int, UnityObject> idToObj);
        void UnregisterSceneObjects();

        void RegisterDynamicResource(int persistentID, UnityObject obj);
        void RegisterDynamicResources(Dictionary<int, UnityObject> idToObj);
        void UnregisterDynamicResources();
        
        bool IsLibraryLoaded(int ordinal);
        bool AddLibrary(AssetLibraryAsset library, int ordinal);
        void RemoveLibrary(int ordinal);
        bool LoadLibrary(string assetLibrary, int ordinal);
        void UnloadLibraries();
        
        AsyncOperation UnloadUnusedAssets(Action<AsyncOperation> completedCallback = null);   
    }

    public class AssetDB : IAssetDB
    {
        private readonly HashSet<AssetLibraryAsset> m_loadedLibraries = new HashSet<AssetLibraryAsset>();
        private readonly Dictionary<int, AssetLibraryAsset> m_ordinalToLib = new Dictionary<int, AssetLibraryAsset>();
        private MappingInfo m_mapping = new MappingInfo();

        private Dictionary<int, UnityObject> m_persistentIDToSceneObject;
        private Dictionary<int, int> m_sceneObjectIDToPersistentID;

        private readonly Dictionary<int, UnityObject> m_persistentIDToDynamicResource = new Dictionary<int, UnityObject>();
        private readonly Dictionary<int, int> m_dynamicResourceIDToPersistentID = new Dictionary<int, int>();

        public void RegisterSceneObjects(Dictionary<int, UnityObject> idToObj)
        {
            if(m_persistentIDToSceneObject != null)
            {
                Debug.LogWarning("scene objects were not unregistered");
            }
            m_persistentIDToSceneObject = idToObj;
            m_sceneObjectIDToPersistentID = m_persistentIDToSceneObject.ToDictionary(kvp => kvp.Value.GetInstanceID(), kvp => kvp.Key);
        }

        public void UnregisterSceneObjects()
        {
            m_persistentIDToSceneObject = null;
            m_sceneObjectIDToPersistentID = null;
        }

        public void RegisterDynamicResource(int persistentID, UnityObject obj)
        {
            m_persistentIDToDynamicResource[persistentID] = obj;
            if (obj != null)
            {
                m_dynamicResourceIDToPersistentID[obj.GetInstanceID()] = persistentID;
            }

        }
        public void RegisterDynamicResources(Dictionary<int, UnityObject> idToObj)
        {
            foreach(KeyValuePair<int, UnityObject> kvp in idToObj)
            {
                m_persistentIDToDynamicResource[kvp.Key] = kvp.Value;
                if (kvp.Value != null)
                {
                    m_dynamicResourceIDToPersistentID[kvp.Value.GetInstanceID()] = kvp.Key;
                }
            }
        }

        public void UnregisterDynamicResources()
        {
            m_persistentIDToDynamicResource.Clear();
            m_dynamicResourceIDToPersistentID.Clear();
        }

        public bool IsLibraryLoaded(int ordinal)
        {
            return m_ordinalToLib.ContainsKey(ordinal);
        }

        public bool AddLibrary(AssetLibraryAsset assetLib, int ordinal)
        {
            if (m_ordinalToLib.ContainsKey(ordinal))
            {
                Debug.LogWarningFormat("Asset Library with ordinal {0} already loadeded", assetLib.Ordinal);
                return false;
            }

            if (m_loadedLibraries.Contains(assetLib))
            {
                Debug.LogWarning("Asset Library already added");
                return false;
            }

            assetLib.Ordinal = ordinal;
            m_loadedLibraries.Add(assetLib);
            m_ordinalToLib.Add(ordinal, assetLib);
            LoadMapping(ordinal, true, true);

            return true;
        }

        public bool LoadLibrary(string assetLibrary, int ordinal)
        {
            if (m_ordinalToLib.ContainsKey(ordinal))
            {
                Debug.LogWarningFormat("Asset Library {0} with this same ordinal {1} already loaded", m_ordinalToLib[ordinal].name, ordinal);
                return false;
            }

            AssetLibraryAsset assetLib = Resources.Load<AssetLibraryAsset>(assetLibrary);
            if (assetLib == null)
            {
                Debug.LogWarningFormat("Asset Library not found", assetLibrary);
                return false;
            }
            return AddLibrary(assetLib, ordinal);
        }     

        public void RemoveLibrary(int ordinal)
        {
            AssetLibraryAsset assetLib;
            if(m_ordinalToLib.TryGetValue(ordinal, out assetLib))
            {
                m_loadedLibraries.Remove(assetLib);
                m_ordinalToLib.Remove(ordinal);
                UnloadMapping(ordinal);
            }
        }

        public void UnloadLibraries()
        {
            foreach (AssetLibraryAsset assetLibrary in m_loadedLibraries)
            {
                Resources.UnloadAsset(assetLibrary);
            }
            m_ordinalToLib.Clear();
            m_loadedLibraries.Clear();
            UnloadMappings();
        }

        public AsyncOperation UnloadUnusedAssets(Action<AsyncOperation> completedCallback = null)
        {
            AsyncOperation operation = Resources.UnloadUnusedAssets();

            if(completedCallback != null)
            {
                if(operation.isDone)
                {
                    completedCallback(operation);
                }
                else
                {
                    Action<AsyncOperation> onCompleted = null;
                    onCompleted = ao =>
                    {
                        operation.completed -= onCompleted;
                        completedCallback(operation);
                    };
                    operation.completed += onCompleted;
                }
            }
           
            return operation;
        }

        private void LoadMapping(int ordinal, bool IIDtoPID, bool PIDtoObj)
        {
            AssetLibraryAsset assetLib;
            if(m_ordinalToLib.TryGetValue(ordinal, out assetLib))
            {
                assetLib.LoadIDMappingTo(m_mapping, IIDtoPID, PIDtoObj);
            }
            else
            {
                throw new ArgumentException(string.Format("Unable to find assetLibrary with ordinal = {0}", ordinal), "ordinal");
            }
        }

        private void UnloadMapping(int ordinal)
        {
            AssetLibraryAsset assetLib;
            if (m_ordinalToLib.TryGetValue(ordinal, out assetLib))
            {
                assetLib.UnloadIDMappingFrom(m_mapping);
            }
            else
            {
                throw new ArgumentException(string.Format("Unable to find assetLibrary with ordinal = {0}", ordinal), "ordinal");
            }
        }


        private void UnloadMappings()
        {
            m_mapping = new MappingInfo();
        }

        private const long m_nullID = 1L << 32;
        private const long m_instanceIDMask = 1L << 33;
        private const long m_staticResourceIDMask = 1L << 34;
        private const long m_staticFolderIDMask = 1L << 35;
        private const long m_dynamicResourceIDMask = 1L << 36;
        private const long m_dynamicFolderIDMask = 1L << 37;
        private const long m_sceneIDMask = 1L << 38;

        public long NullID { get { return m_nullID; } }

        public bool IsNullID(long id)
        {
            return (id & m_nullID) != 0;
        }

        public bool IsInstanceID(long id)
        {
            return (id & m_instanceIDMask) != 0;
        }

        public bool IsStaticResourceID(long id)
        {
            return (id & m_staticResourceIDMask) != 0;
        }

        public bool IsStaticFolderID(long id)
        {
            return (id & m_staticFolderIDMask) != 0;
        }
        
        public bool IsDynamicResourceID(long id)
        {
            return (id & m_dynamicResourceIDMask) != 0;
        }

        public bool IsDynamicFolderID(long id)
        {
            return (id & m_dynamicFolderIDMask) != 0;
        }

        public bool IsSceneID(long id)
        {
            return (id & m_sceneIDMask) != 0;
        }

        public bool IsResourceID(long id)
        {
            return IsStaticResourceID(id) || IsDynamicResourceID(id);
        }

        public long ToStaticResourceID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_staticResourceIDMask);
        }

        public long ToStaticFolderID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_staticFolderIDMask);
        }

        public long ToDynamicResourceID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_dynamicResourceIDMask);
        }

        public long ToDynamicFolderID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_dynamicFolderIDMask);
        }

        public long ToSceneID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_sceneIDMask);
        }

        private static long ToID(int ordinal, int id, long mask)
        {
            if (id > AssetLibraryInfo.ORDINAL_MASK)
            {
                throw new ArgumentException("id > AssetLibraryInfo.ORDINAL_MASK");
            }

            id = (ordinal << AssetLibraryInfo.ORDINAL_OFFSET) | (AssetLibraryInfo.ORDINAL_MASK & id);
            return mask | (0x00000000FFFFFFFFL & id);
        }

        public int ToOrdinal(long id)
        {
            int intId = (int)(0x00000000FFFFFFFFL & id);
            return (intId >> AssetLibraryInfo.ORDINAL_OFFSET) & AssetLibraryInfo.ORDINAL_MASK;
            
        }
        public int ToOrdinal(int id)
        {
            return (id >> AssetLibraryInfo.ORDINAL_OFFSET) & AssetLibraryInfo.ORDINAL_MASK;
        }

        public long ToID(UnityObject uo)
        {
            if(uo == null)
            {
                return m_nullID;
            }

            int instanceID = uo.GetInstanceID();
            int persistentID;
            if(m_mapping.InstanceIDtoPID.TryGetValue(instanceID, out persistentID))
            {
                return m_staticResourceIDMask | (0x00000000FFFFFFFFL & persistentID);
            }
            
            if(m_sceneObjectIDToPersistentID != null && m_sceneObjectIDToPersistentID.TryGetValue(instanceID, out persistentID))
            {
                return m_instanceIDMask | (0x00000000FFFFFFFFL & persistentID);
            }

            if(m_dynamicResourceIDToPersistentID.TryGetValue(instanceID, out persistentID))
            {
                return m_dynamicResourceIDMask | (0x00000000FFFFFFFFL & persistentID);
            }

            return m_instanceIDMask | (0x00000000FFFFFFFFL & instanceID);
        }

        public long[] ToID(UnityObject[] uo)
        {
            if(uo == null)
            {
                return null;
            }
            long[] ids = new long[uo.Length];
            for(int i = 0; i < uo.Length; ++i)
            {
                ids[i] = ToID(uo[i]);
            }
            return ids;
        }

        public bool IsMapped(long id)
        {
            if (IsNullID(id))
            {
                return true;
            }
            if (IsStaticFolderID(id))
            {
                return true;
            }
            if (IsDynamicFolderID(id))
            {
                return true;
            }
            if (IsInstanceID(id))
            {
                int persistentID = unchecked((int)id);
                return m_persistentIDToSceneObject.ContainsKey(persistentID);
            }
            if (IsStaticResourceID(id))
            {
                int persistentID = unchecked((int)id);
                return m_mapping.PersistentIDtoObj.ContainsKey(persistentID);
            }
            if(IsDynamicResourceID(id))
            {
                int persistentID = unchecked((int)id);
                return m_persistentIDToDynamicResource.ContainsKey(persistentID);
            }
            return false;
        }

        public T FromID<T>(long id) where T : UnityObject
        {
            if(IsNullID(id))
            {
                return null;
            }

            if(IsStaticResourceID(id))
            {
                UnityObject obj;
                int persistentID = unchecked((int)id);
                if (m_mapping.PersistentIDtoObj.TryGetValue(persistentID, out obj))
                {
                    return obj as T;
                }
            }
            else if(IsInstanceID(id))
            {
                UnityObject obj;
                int persistentID = unchecked((int)id);
                if(m_persistentIDToSceneObject.TryGetValue(persistentID, out obj))
                {
                    return obj as T;
                }
            }
            else if(IsDynamicResourceID(id))
            {
                UnityObject obj;
                int persistentID = unchecked((int)id);
                if(m_persistentIDToDynamicResource.TryGetValue(persistentID, out obj))
                {
                    return obj as T;
                }
            }
            return null;
        }

        public T[] FromID<T>(long[] id) where T : UnityObject
        {
            if(id == null)
            {
                return null;
            }

            T[] objs = new T[id.Length];
            for(int i = 0; i < id.Length; ++i)
            {
                objs[i] = FromID<T>(id[i]);
            }
            return objs;
        }
    }
}
