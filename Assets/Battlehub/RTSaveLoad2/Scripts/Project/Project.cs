using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    public interface IProject
    {
        ProjectItem Root
        {
            get;
        }

        AssetLibraryReference[] StaticReferences
        {
            get;
        }

        bool IsStatic(ProjectItem projectItem);

        string GetExt(UnityObject obj);
        string GetExt(Type type);
        
        void Open(string project, ProjectEventHandler callback);
        void GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback);

        bool CanSave(ProjectItem parent, UnityObject obj);
        void Save(ProjectItem parent, byte[] previewData, UnityObject obj, ProjectEventHandler<ProjectItem> callback);
        void Save(AssetItem assetItem, UnityObject obj, ProjectEventHandler callback);
        void Load(AssetItem assetItem, ProjectEventHandler<UnityObject> callback);

        AsyncOperation UnloadUnusedAssets(Action<AsyncOperation> completedCallback = null);
    }
      

    public delegate void ProjectEventHandler(Error error);
    public delegate void ProjectEventHandler<T>(Error error, T result);

    public class Project : MonoBehaviour, IProject
    {
        private IStorage m_storage;
        private IAssetDB m_assetDB;
        private ITypeMap m_typeMap;
        private IUnityObjectFactory m_factory;

        /// <summary>
        /// Important!!!
        /// Do not remove and do not reorder items from this array. 
        /// If you want to remove reference, just set to null corresponding array element.
        /// Append new references to the end of m_references array.
        /// </summary>
        [SerializeField]
        private AssetLibraryReference[] m_staticReferences;
        public AssetLibraryReference[] StaticReferences
        {
            get { return m_staticReferences; }
        }

        private ProjectInfo m_projectInfo;
        private string m_projectPath;

        private ProjectItem m_root;
        public ProjectItem Root
        {
            get { return m_root; }
        }


        /// <summary>
        /// For fast access when resolving dependencies.
        /// </summary>
        private Dictionary<long, AssetItem> m_idToAssetItem = new Dictionary<long, AssetItem>();

        private void Awake()
        {
            m_storage = RTSL2Deps.Get.Storage;
            m_assetDB = RTSL2Deps.Get.AssetDB;
            m_typeMap = RTSL2Deps.Get.TypeMap;
            m_factory = RTSL2Deps.Get.UnityObjFactory;
        }

        private void OnDestroy()
        {
            m_assetDB.UnloadLibraries();
            m_assetDB.UnregisterSceneObjects();
            m_assetDB.UnregisterDynamicResources();
        }

        private bool IsDynamicOrdinal(int ordinal)
        {
            return ordinal >= AssetLibraryInfo.ORDINAL_MASK / 2;
        }

        private bool GetOrdinalAndId(int identifier, out int ordinal, out int id)
        {
            ordinal = AssetLibraryInfo.ORDINAL_MASK / 2 + m_assetDB.ToOrdinal(identifier);
            if (ordinal > AssetLibraryInfo.ORDINAL_MASK)
            {
                Debug.LogError("Unable to generate identifier. Allotted Identifiers range was exhausted");
                id = 0;
                return false;
            }

            id = identifier & AssetLibraryInfo.ORDINAL_MASK;
            return true;
        }

        private void CleanupTree(ProjectItem item, int ordinal)
        {
            if(item.Children == null)
            {
                return;
            }

            for(int i = item.Children.Count - 1; i >= 0; --i)
            {
                ProjectItem childItem = item.Children[i];

                int id = unchecked((int)childItem.ItemID);
                if (childItem.ItemID == 0 || ordinal == m_assetDB.ToOrdinal(id))
                {
                    item.Children.RemoveAt(i);
                }

                CleanupTree(childItem, ordinal);
            }
        }

        private void MergeAssetLibrary(AssetLibraryReference asset, int ordinal)
        {
            if(!asset.KeepRuntimeProjectInSync)
            {
                return;
            }

            AssetLibraryInfo assetLibrary = asset.AssetLibrary;
            if(assetLibrary == null)
            {
                return;
            }

            assetLibrary.BuildTree();

            AssetFolderInfo rootFolder = assetLibrary.Folders.Where(folder => folder.depth == -1).First();
            MergeFolders(rootFolder, m_root, ordinal);
        }

        private void MergeFolders(AssetFolderInfo from, ProjectItem to, int ordinal)
        {
            if(from.hasChildren)
            {
                for (int i = 0; i < from.children.Count; ++i)
                {
                    AssetFolderInfo childFrom = (AssetFolderInfo)from.children[i];

                    if (to.Children == null)
                    {
                        to.Children = new List<ProjectItem>();
                    }

                    ProjectItem childTo = to.Children.Where(item => item.Name == childFrom.name && !(item is AssetItem)).FirstOrDefault();
                    if (childTo == null)
                    {
                        childTo = new ProjectItem();
                        to.Children.Add(childTo);
                    }
                    childTo.Name = childFrom.name;
                    childTo.ItemID = m_assetDB.ToExposedFolderID(ordinal, childFrom.id);
                    childTo.Parent = to;

                    MergeFolders(childFrom, childTo, ordinal);
                }
            }

            MergeAssets(from, to, ordinal);
        }

        private void MergeAssets(AssetFolderInfo from, ProjectItem to, int ordinal)
        {
            List<AssetInfo> fromAssets = from.Assets;
            if(fromAssets == null)
            {
                return;
            }

            if(to.Children == null)
            {
                to.Children = new List<ProjectItem>();
            }

            for(int i = 0; i < fromAssets.Count; ++i)
            {
                AssetInfo assetFrom = fromAssets[i];
                if(assetFrom.Object == null)
                {
                    continue;
                }
                AssetItem assetTo = to.Children.OfType<AssetItem>().Where(item => item.Name == assetFrom.name).FirstOrDefault();
                if(assetTo == null)
                {
                    assetTo = new AssetItem();
                    to.Children.Add(assetTo);
                }

                assetTo.Name = assetFrom.name;
                assetTo.ItemID = m_assetDB.ToExposedResourceID(ordinal, assetFrom.PersistentID);
                assetTo.Parent = to;
                assetTo.TypeGuid = m_typeMap.ToGuid(assetFrom.Object.GetType());
                assetTo.Preview = null; //must rebuild preview

                if(assetFrom.PrefabParts != null)
                {
                    assetTo.Parts = new PrefabPart[assetFrom.PrefabParts.Count];
                    for (int j = 0; j < assetFrom.PrefabParts.Count; ++j)
                    {
                        PrefabPartInfo prefabPart = assetFrom.PrefabParts[j];
                        Guid typeGuid = m_typeMap.ToGuid(prefabPart.Object.GetType());
                        if (prefabPart != null && prefabPart.Object != null && typeGuid != Guid.Empty)
                        {
                            assetTo.Parts[j] = new PrefabPart
                            {
                                Name = prefabPart.Object.name,
                                ParentID = prefabPart.ParentPersistentID,
                                PartID = prefabPart.PersistentID,
                                TypeGuid = typeGuid
                            };
                        }
                        else
                        {
                            assetTo.Parts[j] = null;
                        }
                    }
                }                
            }
        }

        private void GetProjectTree(string project, ProjectEventHandler callback)
        {
            m_storage.GetProjectTree(project, (error, rootFolder) =>
            {
                if (error.HasError)
                {
                    callback(error);
                    return;
                }

                OnGetProjectTreeCompleted(error, rootFolder, callback);
            });
        }

        private void OnGetProjectTreeCompleted(Error error, ProjectItem rootFolder, ProjectEventHandler callback)
        {
            m_root = rootFolder;
            for (int i = 0; i < m_staticReferences.Length; ++i)
            {
                AssetLibraryReference assetLibrary = m_staticReferences[i];
                if (assetLibrary == null)
                {
                    continue;
                }
                assetLibrary.Ordinal = i;
                m_assetDB.AddLibrary(assetLibrary, i);
                
                CleanupTree(m_root, i);
                MergeAssetLibrary(assetLibrary, i);
            }

            AssetItem[] assetItems = m_root.Flatten(true).OfType<AssetItem>().ToArray();
            m_idToAssetItem = assetItems.ToDictionary(item => item.ItemID);
            for(int i = 0; i < assetItems.Length; ++i)
            {
                AssetItem assetItem = assetItems[i];
                if(assetItem.Parts != null)
                {
                    for(int j = 0; j < assetItem.Parts.Length; ++j)
                    {
                        PrefabPart prefabPart = assetItem.Parts[j];
                        m_idToAssetItem.Add(prefabPart.PartID, assetItem);
                    }
                }
            }
            callback(error);
        }

        public bool IsStatic(ProjectItem projectItem)
        {
            return m_assetDB.IsStaticFolderID(projectItem.ItemID) || m_assetDB.IsStaticResourceID(projectItem.ItemID);
        }

        public string GetExt(UnityObject obj)
        {
            return ".rto";
        }

        public string GetExt(Type type)
        {
            return ".rto";
        }

        public void Open(string project, ProjectEventHandler callback)
        {
            m_projectPath = project;

            m_assetDB.UnloadLibraries();
      
            m_storage.GetProject(m_projectPath, (error, projectInfo) =>
            {
                if (error.HasError)
                {
                    callback(error);
                    return;
                }

                OnOpened(project, callback, projectInfo);
            });
        }

        private void OnOpened(string project, ProjectEventHandler callback, ProjectInfo projectInfo)
        {
            if (projectInfo == null)
            {
                projectInfo = new ProjectInfo();
            }

            m_projectInfo = projectInfo;
            GetProjectTree(project, callback);
        }

        public void GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback)
        {
            m_storage.GetPreviews(m_projectPath, folders.Select(f => f.ToString()).ToArray(), (error, result) =>
            {
                if (error.HasError)
                {
                    callback(error, new AssetItem[0]);
                    return;
                }
                OnGetPreviewsCompleted(folders, callback, error, result);
            });
        }

        private void OnGetPreviewsCompleted(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback, Error error, Preview[][] result)
        {
            for (int i = 0; i < result.Length; ++i)
            {
                ProjectItem folder = folders[i];
                Preview[] previews = result[i];
                if (previews != null && previews.Length > 0)
                {                    
                    for (int j = 0; j < previews.Length; ++j)
                    {
                        Preview preview = previews[j];
                        AssetItem assetItem;

                        if(m_idToAssetItem.TryGetValue(preview.ItemID, out assetItem))
                        {
                            if(assetItem.Parent == null)
                            {
                                Debug.LogErrorFormat("asset item {0} parent is null", assetItem.ToString());
                                continue;
                            }

                            if(assetItem.Parent.ItemID != folder.ItemID)
                            {
                                Debug.LogErrorFormat("asset item {0} with wrong parent selected. Expected parent {1}. Actual parent {2}", folder.ToString(), assetItem.Parent.ToString());
                                continue;
                            }

                            assetItem.Preview = preview;
                        }
                        else
                        {
                            Debug.LogWarningFormat("AssetItem with ItemID {0} does not exists", preview.ItemID);
                        }
                    }
                }
            }

            callback(error, folders.Where(f => f.Children != null).SelectMany(f => f.Children).ToArray());
        }

        public bool CanSave(ProjectItem parent, UnityObject obj)
        {
            Type persistentType = m_typeMap.ToPersistentType(obj.GetType());
            if (persistentType == null)
            {
                return false;
            }

            if(parent.Children != null && parent.Children.Any(c => c.NameExt == obj.name + GetExt(obj)))
            {
                return false;
            }

            return true;
        }

        private void PersistentDescriptorsToPrefabPartItems(PersistentDescriptor[] descriptors, List<PrefabPart> prefabParts)
        {
            if(descriptors == null)
            {
                return;
            }

            for(int i = 0; i < descriptors.Length; ++i)
            {
                PersistentDescriptor descriptor = descriptors[i];
                if (descriptor != null)
                {
                    Type persistentType = m_typeMap.ToType(descriptor.PersistentTypeGuid);
                    if (persistentType == null)
                    {
                        Debug.LogWarningFormat("Unable to resolve type with guid {0}", descriptor.PersistentTypeGuid);
                        continue;
                    }

                    Type type = m_typeMap.ToUnityType(persistentType);
                    if(type == null)
                    {
                        Debug.LogWarningFormat("Unable to get unity type from persistent type {1}", type.FullName);
                        continue;
                    }

                    Guid typeGuid = m_typeMap.ToGuid(type);
                    if(typeGuid == Guid.Empty)
                    {
                        Debug.LogWarningFormat("Unable convert type {0} to guid", type.FullName);
                        continue;
                    }

                    PrefabPart prefabPartItem = new PrefabPart
                    {
                        Name = descriptor.Name,
                        ParentID = descriptor.Parent != null ? descriptor.Parent.PersistentID : m_assetDB.NullID,
                        PartID = descriptor.PersistentID,
                        TypeGuid = typeGuid
                    };

                    prefabParts.Add(prefabPartItem);

                    PersistentDescriptorsToPrefabPartItems(descriptor.Children, prefabParts);
                    PersistentDescriptorsToPrefabPartItems(descriptor.Components, prefabParts);
                }
            }
        }

        public void Save(ProjectItem parent, byte[] previewData, UnityObject obj, ProjectEventHandler<ProjectItem> callback)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            Type persistentType = m_typeMap.ToPersistentType(obj.GetType());
            if (persistentType == null)
            {
                throw new ArgumentException(string.Format("PersistentClass for {0} does not exist", obj.GetType()), "obj");
            }

            if (parent == null)
            {
                parent = Root;
            }

            if (!parent.IsFolder)
            {
                throw new ArgumentException("parent is not folder", "parent");
            }

            int ordinal, id;
            if (!GetOrdinalAndId(m_projectInfo.AssetIdentifier, out ordinal, out id))
            {
                return;
            }
            
            PersistentObject persistentObject = (PersistentObject)Activator.CreateInstance(persistentType);
            persistentObject.ReadFrom(obj);           

            AssetItem assetItem = new AssetItem();
            assetItem.ItemID = m_assetDB.ToRuntimeResourceID(ordinal, id);
            assetItem.Name = persistentObject.name;
            assetItem.Ext = GetExt(obj);
            assetItem.TypeGuid = m_typeMap.ToGuid(obj.GetType());
            assetItem.Preview = new Preview
            {
                ItemID = assetItem.ItemID,
                PreviewData = previewData
            };

            if(persistentObject is PersistentPrefab)
            {
                PersistentPrefab persistentPrefab = (PersistentPrefab)persistentObject;
                if(persistentPrefab.Descriptors != null)
                {
                    List<PrefabPart> prefabParts = new List<PrefabPart>();
                    PersistentDescriptorsToPrefabPartItems(persistentPrefab.Descriptors, prefabParts);
                    assetItem.Parts = prefabParts.ToArray();
                }
            }

            GetDepsContext getDepsCtx = new GetDepsContext();
            persistentObject.GetDeps(getDepsCtx);

            assetItem.Dependencies = getDepsCtx.Dependencies.ToArray();

            m_storage.Save(m_projectPath, parent.ToString(), assetItem, persistentObject, m_projectInfo, error =>
            {
                if(!error.HasError)
                {
                    m_idToAssetItem.Add(assetItem.ItemID, assetItem);
                    if(assetItem.Parts != null)
                    {
                        for (int i = 0; i < assetItem.Parts.Length; ++i)
                        {
                            m_idToAssetItem.Add(assetItem.Parts[i].PartID, assetItem);
                        }
                    }
                   
                    parent.AddChild(assetItem);
                }

                callback(error, assetItem);
            });
        }

        public void Save(AssetItem assetItem, UnityObject obj, ProjectEventHandler callback)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            Type persistentType = m_typeMap.ToPersistentType(obj.GetType());
            if (persistentType == null)
            {
                throw new ArgumentException(string.Format("PersistentClass for {0} does not exist", obj.GetType()), "obj");
            }

            PersistentObject persistentObject = (PersistentObject)Activator.CreateInstance(persistentType);
            persistentObject.ReadFrom(obj);

            if (persistentObject is PersistentPrefab)
            {
                PersistentPrefab persistentPrefab = (PersistentPrefab)persistentObject;
                if (persistentPrefab.Descriptors != null)
                {
                    List<PrefabPart> prefabParts = new List<PrefabPart>();
                    PersistentDescriptorsToPrefabPartItems(persistentPrefab.Descriptors, prefabParts);
                    assetItem.Parts = prefabParts.ToArray();
                }
            }

            GetDepsContext getDepsCtx = new GetDepsContext();
            persistentObject.GetDeps(getDepsCtx);

            assetItem.Dependencies = getDepsCtx.Dependencies.ToArray();

            m_storage.Save(m_projectPath, assetItem.Parent.ToString(), assetItem, persistentObject, m_projectInfo, error =>
            {
                callback(error);
            });
        }

        public void GetAssetItemsToLoad(AssetItem assetItem, HashSet<AssetItem> loadHs)
        {
            Type type = m_typeMap.ToType(assetItem.TypeGuid);
            if (type == null)
            {
                return;
            }
            Type persistentType = m_typeMap.ToPersistentType(type);
            if (persistentType == null)
            {
                return;
            }

            if (!loadHs.Contains(assetItem) && !m_assetDB.IsMapped(assetItem.ItemID))
            {
                loadHs.Add(assetItem);
                if (assetItem.Dependencies != null)
                {
                    for (int i = 0; i < assetItem.Dependencies.Length; ++i)
                    {
                        long dep = assetItem.Dependencies[i];

                        AssetItem dependencyAssetItem;
                        if(m_idToAssetItem.TryGetValue(dep, out dependencyAssetItem))
                        {
                            GetAssetItemsToLoad(assetItem, loadHs);
                        }
                        else
                        {
                            Debug.LogWarningFormat("AssetItem with ItemID {0} does not exists", dep);
                        }
                    }
                }
            }
        }

        public void Load(AssetItem assetItem, ProjectEventHandler<UnityObject> callback)
        {
            Type type = m_typeMap.ToType(assetItem.TypeGuid);
            if(type == null)
            {
                throw new ArgumentException("assetItem", string.Format("Unable to resolve type using TypeGuid {0}", assetItem.TypeGuid));
            }

            Type persistentType = m_typeMap.ToPersistentType(type);
            if (persistentType == null)
            {
                throw new ArgumentException(string.Format("PersistentClass for {0} does not exist", type), "obj");
            }

            HashSet<AssetItem> loadAssetItemsHs = new HashSet<AssetItem>();
            GetAssetItemsToLoad(assetItem, loadAssetItemsHs);

            m_storage.Load(m_projectPath, loadAssetItemsHs.Select(item => item.ToString()).ToArray(), loadAssetItemsHs.Select(item => m_typeMap.ToType(item.TypeGuid)).ToArray(), (error, persistentObjects) =>
            {
                if (error.HasError)
                {
                    callback(error, null);
                    return;
                }

                OnLoadCompleted(assetItem, loadAssetItemsHs.ToArray(), persistentObjects, callback);
            });
        }

        private void OnLoadCompleted(AssetItem rootItem, AssetItem[] assetItems, PersistentObject[] persistentObjects, ProjectEventHandler<UnityObject> callback)
        {
            for(int i = 0; i < assetItems.Length; ++i)
            {
                AssetItem assetItem = assetItems[i];
                if(!m_assetDB.IsMapped(assetItem.ItemID))
                {
                    if(m_assetDB.IsStaticResourceID(assetItem.ItemID))
                    {
                        int ordinal = m_assetDB.ToOrdinal(assetItem.ItemID);
                        if (m_assetDB.IsLibraryRefLoaded(ordinal))
                        {
                            m_assetDB.RemoveLibrary(ordinal);
                        }

                        if(!m_assetDB.IsLibraryLoaded(ordinal))
                        {
                            AssetLibraryReferenceInfo reference = m_projectInfo.References.FirstOrDefault(r => r.Ordinal == ordinal);
                            if (reference != null)
                            {
                                m_assetDB.LoadLibrary(reference.AssetLibrary, reference.Ordinal);
                            }
                        }
                    }
                    else if(m_assetDB.IsDynamicResourceID(assetItem.ItemID))
                    {
                        PersistentObject persistentObject = persistentObjects[i];
                        if(persistentObject != null)
                        {
                            if (persistentObject is PersistentPrefab)
                            {
                                PersistentPrefab persistentPrefab = (PersistentPrefab)persistentObject;
                                Dictionary<int, UnityObject> idToObj = new Dictionary<int, UnityObject>();
                                persistentPrefab.CreateGameObjectWithComponents(m_typeMap, persistentPrefab.Descriptors[0], idToObj);
                                m_assetDB.RegisterDynamicResources(idToObj);
                            }
                            else
                            {
                                Type type = m_typeMap.ToType(assetItem.TypeGuid);
                                UnityObject instance = m_factory.CreateInstance(type);
                                m_assetDB.RegisterDynamicResource(unchecked((int)assetItem.ItemID), instance);
                            }
                        }   
                    }
                }                
            }

            for (int i = 0; i < persistentObjects.Length; ++i)
            {
                PersistentObject persistentObject = persistentObjects[i];
                if (persistentObject != null)
                {
                    UnityObject obj = m_assetDB.FromID<UnityObject>(assetItems[i].ItemID);
                    Debug.Assert(obj != null);
                    if(obj != null)
                    {
                        persistentObject.WriteTo(obj);
                    }   
                }
            }

            UnityObject result = m_assetDB.FromID<UnityObject>(rootItem.ItemID);
            callback(new Error(Error.OK), result);
        }

        public AsyncOperation UnloadUnusedAssets(Action<AsyncOperation> completedCallback = null)
        {
            return m_assetDB.UnloadUnusedAssets(completedCallback);
        }
    }
}

