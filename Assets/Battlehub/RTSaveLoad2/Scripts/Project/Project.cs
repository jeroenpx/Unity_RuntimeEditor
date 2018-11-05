using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

using Battlehub.RTSaveLoad2.Interface;
using Battlehub.RTCommon;

namespace Battlehub.RTSaveLoad2
{
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
        private AssetLibraryReference[] m_staticReferences = new AssetLibraryReference[0];
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

        [SerializeField]
        private Transform m_dynamicPrefabsRoot;
        private readonly Dictionary<int, UnityObject> m_dynamicResources = new Dictionary<int, UnityObject>();
        private MappingInfo m_staticReferencesMapping;

        private void Awake()
        {
            m_storage = IOC.Resolve<IStorage>();
            m_assetDB = IOC.Resolve<IAssetDB>();
            m_typeMap = IOC.Resolve<ITypeMap>();
            m_factory = IOC.Resolve<IUnityObjectFactory>();

            if(m_dynamicPrefabsRoot == null)
            {
                m_dynamicPrefabsRoot = transform;
            }
        }

        private void OnDestroy()
        {
            UnloadUnregisterDestroy();
        }

        private void UnloadUnregisterDestroy()
        {
            m_assetDB.UnloadLibraries();
            m_assetDB.UnregisterSceneObjects();
            m_assetDB.UnregisterDynamicResources();
            foreach (UnityObject dynamicResource in m_dynamicResources.Values)
            {
                Destroy(dynamicResource);
            }
            m_dynamicResources.Clear();
            m_staticReferencesMapping = null;
            m_idToAssetItem = new Dictionary<long, AssetItem>();
        }

        private bool IsDynamicOrdinal(int ordinal)
        {
            return ordinal >= AssetLibraryInfo.ORDINAL_MASK / 2;
        }

        private bool GetOrdinalAndId(ref int identifier, out int ordinal, out int id)
        {
            ordinal = AssetLibraryInfo.ORDINAL_MASK / 2 + m_assetDB.ToOrdinal(identifier);
            if (ordinal > AssetLibraryInfo.ORDINAL_MASK)
            {
                Debug.LogError("Unable to generate identifier. Allotted Identifiers range was exhausted");
                id = 0;
                return false;
            }

            id = identifier & AssetLibraryInfo.ORDINAL_MASK;
            identifier++;
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
                    childTo.ItemID = m_assetDB.ToStaticFolderID(ordinal, childFrom.id);
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
                assetTo.ItemID = m_assetDB.ToStaticResourceID(ordinal, assetFrom.PersistentID);
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

        private void GetProjectTree(string project, ProjectAsyncOperation ao, ProjectEventHandler callback)
        {
            m_storage.GetProjectTree(project, (error, rootFolder) =>
            {
                if (error.HasError)
                {
                    if(callback != null)
                    {
                        callback(error);
                    }
                    
                    ao.Error = error;
                    ao.IsCompleted = true;
                    return;
                }

                OnGetProjectTreeCompleted(error, rootFolder, ao, callback);
            });
        }

        private void OnGetProjectTreeCompleted(Error error, ProjectItem rootFolder, ProjectAsyncOperation ao, ProjectEventHandler callback)
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
                        if(prefabPart != null)
                        {
                            m_idToAssetItem.Add(prefabPart.PartID, assetItem);
                        }   
                    }
                }
            }
            if(callback != null)
            {
                callback(error);
            }
            
            ao.Error = error;
            ao.IsCompleted = true;
        }

        public bool IsStatic(ProjectItem projectItem)
        {
            return m_assetDB.IsStaticFolderID(projectItem.ItemID) || m_assetDB.IsStaticResourceID(projectItem.ItemID);
        }

        public bool TryGetFromStaticReferences(AssetItem assetItem, out UnityObject obj)
        {
            return m_staticReferencesMapping.PersistentIDtoObj.TryGetValue(unchecked((int)assetItem.ItemID), out obj);
        }
        
        public Type ToType(AssetItem assetItem)
        {
            return m_typeMap.ToType(assetItem.TypeGuid);
        }

        public Guid ToGuid(Type type)
        {
            return m_typeMap.ToGuid(type);
        }

        public long ToID(UnityObject obj)
        {
            return m_assetDB.ToID(obj);
        }

        public T FromID<T>(long id) where T : UnityObject
        {
            return m_assetDB.FromID<T>(id);
        }

        public string GetExt(object obj)
        {
            if(obj is Scene)
            {
                return ".rtscene";
            }
            return ".rto";
        }

        public string GetExt(Type type)
        {
            if(type == typeof(Scene))
            {
                return ".rtscene";
            }
            return ".rto";
        }

        public ProjectAsyncOperation Open(string project, ProjectEventHandler callback)
        {
            m_staticReferencesMapping = new MappingInfo();
            for (int i = 0; i < StaticReferences.Length; ++i)
            {
                AssetLibraryReference reference = StaticReferences[i];
                if (reference != null)
                {
                    reference.LoadIDMappingTo(m_staticReferencesMapping, false, true);
                }
            }

            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            m_projectPath = project;

            m_assetDB.UnloadLibraries();
      
            m_storage.GetProject(m_projectPath, (error, projectInfo) =>
            {
                if (error.HasError)
                {
                    if(callback != null)
                    {
                        callback(error);
                    }
                    
                    ao.Error = error;
                    ao.IsCompleted = true;
                    return;
                }

                OnOpened(project, projectInfo, ao, callback);
            });
            return ao;
        }

        private void OnOpened(string project, ProjectInfo projectInfo, ProjectAsyncOperation ao, ProjectEventHandler callback)
        {
            if (projectInfo == null)
            {
                projectInfo = new ProjectInfo();
            }

            m_projectInfo = projectInfo;
            GetProjectTree(project, ao, callback);
        }

        public ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback)
        {
            ProjectAsyncOperation<ProjectItem[]> ao = new ProjectAsyncOperation<ProjectItem[]>();
            m_storage.GetPreviews(m_projectPath, folders.Select(f => f.ToString()).ToArray(), (error, result) =>
            {
                if (error.HasError)
                {
                    if(callback != null)
                    {
                        callback(error, new AssetItem[0]);
                    }
                    return;
                }
                OnGetPreviewsCompleted(folders, ao, callback, error, result);
            });
            return ao;
        }

        private void OnGetPreviewsCompleted(ProjectItem[] folders, ProjectAsyncOperation<ProjectItem[]> ao, ProjectEventHandler<ProjectItem[]> callback, Error error, Preview[][] result)
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

            ProjectItem[] projectItems = folders.Where(f => f.Children != null).SelectMany(f => f.Children).ToArray();
            if (callback != null)
            {
                callback(error, projectItems);
            }
            
            ao.Error = error;
            ao.Result = projectItems;
            ao.IsCompleted = true;
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

        public ProjectAsyncOperation<AssetItem> Save(ProjectItem parent, byte[] previewData, object obj, string nameOverride, ProjectEventHandler<AssetItem> callback)
        {
            if(m_root == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            Type objType = obj.GetType();
            Type persistentType = m_typeMap.ToPersistentType(objType);
            if (persistentType == null)
            {
                throw new ArgumentException(string.Format("PersistentClass for {0} does not exist", obj.GetType()), "obj");
            }

            ProjectAsyncOperation<AssetItem> ao = new ProjectAsyncOperation<AssetItem>();

            if(persistentType == typeof(PersistentGameObject))
            {
                persistentType = typeof(PersistentRuntimePrefab);
            }

            if (parent == null)
            {
                parent = Root;
            }

            if (!parent.IsFolder)
            {
                throw new ArgumentException("parent is not folder", "parent");
            }

            int assetIdBackup = m_projectInfo.AssetIdentifier;
            int rootOrdinal;
            int rootId;
            if (!GetOrdinalAndId(ref m_projectInfo.AssetIdentifier, out rootOrdinal, out rootId))
            {
                OnExhausted(callback, ao, assetIdBackup);
                return ao;
            }

            if (obj is GameObject)
            {
                Dictionary<int, UnityObject> idToObj = new Dictionary<int, UnityObject>();
                GameObject go = (GameObject)obj;
                idToObj.Add(unchecked((int)m_assetDB.ToDynamicResourceID(rootOrdinal, rootId)), go);

                Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
                for(int i = 0; i < transforms.Length; ++i)
                {
                    Transform tf = transforms[i];
                    if(tf.gameObject != go)
                    {
                        int ordinal;
                        int id;
                        if (!GetOrdinalAndId(ref m_projectInfo.AssetIdentifier, out ordinal, out id))
                        {
                            OnExhausted(callback, ao, assetIdBackup);
                            return ao;
                        }
                        idToObj.Add(unchecked((int)m_assetDB.ToDynamicResourceID(ordinal, id)), tf.gameObject);  
                    }
                  
                    Component[] components = tf.GetComponents<Component>();
                    for(int j = 0; j < components.Length; ++j)
                    {
                        Component comp = components[j];
                        int ordinal;
                        int id;
                        if (!GetOrdinalAndId(ref m_projectInfo.AssetIdentifier, out ordinal, out id))
                        {
                            OnExhausted(callback, ao, assetIdBackup);
                            return ao;
                        }
                        idToObj.Add(unchecked((int)m_assetDB.ToDynamicResourceID(ordinal, id)), comp);
                       
                    }
                }

                m_assetDB.RegisterDynamicResources(idToObj);
            }
            else if(obj is UnityObject)
            {
                m_assetDB.RegisterDynamicResource((int)m_assetDB.ToDynamicResourceID(rootOrdinal, rootId), (UnityObject)obj);
            }

            PersistentObject persistentObject = (PersistentObject)Activator.CreateInstance(persistentType);
            persistentObject.ReadFrom(obj);  

            if(!string.IsNullOrEmpty(nameOverride))
            {
                persistentObject.name = nameOverride;
            }

            AssetItem assetItem = new AssetItem();
            if(obj is Scene)
            {
                assetItem.ItemID = m_assetDB.ToSceneID(rootOrdinal, rootId);
            }
            else
            {
                assetItem.ItemID = m_assetDB.ToDynamicResourceID(rootOrdinal, rootId);
            }
            
            assetItem.Name = persistentObject.name;
            assetItem.Ext = GetExt(obj);
            assetItem.TypeGuid = m_typeMap.ToGuid(obj.GetType());
            assetItem.Preview = new Preview
            {
                ItemID = assetItem.ItemID,
                PreviewData = previewData
            };

            if(persistentObject is PersistentRuntimePrefab)
            {
                PersistentRuntimePrefab persistentPrefab = (PersistentRuntimePrefab)persistentObject;
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
                    if(!(obj is Scene))
                    {
                        if (assetItem.Parts != null)
                        {
                            for (int i = 0; i < assetItem.Parts.Length; ++i)
                            {
                                m_idToAssetItem.Add(assetItem.Parts[i].PartID, assetItem);
                            }
                        }
                        else
                        {
                            m_idToAssetItem.Add(assetItem.ItemID, assetItem);
                        }
                    }
                   
                    parent.AddChild(assetItem);
                }

                if (callback != null)
                {
                    callback(error, assetItem);
                }
                ao.Error = error;
                ao.Result = assetItem;
                ao.IsCompleted = true;
            });

            return ao;
        }

        private void OnExhausted(ProjectEventHandler<AssetItem> callback, ProjectAsyncOperation<AssetItem> ao, int assetIdBackup)
        {
            m_projectInfo.AssetIdentifier = assetIdBackup;
            Error error = new Error(Error.E_InvalidOperation);
            if (callback != null)
            {
                callback(error, null);
            }
            ao.Error = error;
            ao.Result = null;
            ao.IsCompleted = true;
        }

        public ProjectAsyncOperation Save(AssetItem assetItem, object obj, ProjectEventHandler callback)
        {
            if (m_root == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            Type persistentType = m_typeMap.ToPersistentType(obj.GetType());
            if (persistentType == null)
            {
                throw new ArgumentException(string.Format("PersistentClass for {0} does not exist", obj.GetType()), "obj");
            }

            if (persistentType == typeof(PersistentGameObject))
            {
                persistentType = typeof(PersistentRuntimePrefab);
            }

            PersistentObject persistentObject = (PersistentObject)Activator.CreateInstance(persistentType);
            persistentObject.ReadFrom(obj);

            if (persistentObject is PersistentRuntimePrefab)
            {
                PersistentRuntimePrefab persistentPrefab = (PersistentRuntimePrefab)persistentObject;
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
            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            m_storage.Save(m_projectPath, assetItem.Parent.ToString(), assetItem, persistentObject, m_projectInfo, error =>
            {
                if (callback != null)
                {
                    callback(error);
                }
                ao.Error = error;
                ao.IsCompleted = true;
            });
            return ao;
        }

        private void GetAssetItemsToLoad(AssetItem assetItem, HashSet<AssetItem> loadHs)
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


        public ProjectAsyncOperation<UnityObject> Load(AssetItem assetItem, ProjectEventHandler<UnityObject> callback)
        {
            Type type = m_typeMap.ToType(assetItem.TypeGuid);
            if(type == null)
            {
                throw new ArgumentException("assetItem", string.Format("Unable to resolve type using TypeGuid {0}", assetItem.TypeGuid));
            }

            if (type == typeof(Scene))
            {
                m_assetDB.UnregisterSceneObjects();
            }

            HashSet<AssetItem> loadAssetItemsHs = new HashSet<AssetItem>();
            GetAssetItemsToLoad(assetItem, loadAssetItemsHs);

            Type[] persistentTypes = loadAssetItemsHs.Select(item => m_typeMap.ToPersistentType(m_typeMap.ToType(item.TypeGuid))).ToArray();
            for(int i = 0; i < persistentTypes.Length; ++i)
            {
                if(persistentTypes[i] == typeof(PersistentGameObject))
                {
                    persistentTypes[i] = typeof(PersistentRuntimePrefab);
                }
            }

            ProjectAsyncOperation<UnityObject> ao = new ProjectAsyncOperation<UnityObject>();
            m_storage.Load(m_projectPath, loadAssetItemsHs.Select(item => item.ToString()).ToArray(), persistentTypes, (error, persistentObjects) =>
            {
                if (error.HasError)
                {
                    if (callback != null)
                    {
                        callback(error, null);
                    }
                    ao.Error = error;
                    return;
                }

                OnLoadCompleted(assetItem, loadAssetItemsHs.ToArray(), persistentObjects, ao, callback);
            });
            return ao;
        }

        private void OnLoadCompleted(AssetItem rootItem, AssetItem[] assetItems, PersistentObject[] persistentObjects, ProjectAsyncOperation<UnityObject> ao, ProjectEventHandler<UnityObject> callback)
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
                            if(persistentObject is PersistentRuntimeScene)
                            {
                                PersistentRuntimeScene persistentScene = (PersistentRuntimeScene)persistentObject;
                                Dictionary<int, UnityObject> idToObj = new Dictionary<int, UnityObject>();
                                persistentScene.CreateGameObjectWithComponents(m_typeMap, persistentScene.Descriptors[0], idToObj);
                            }
                            else if (persistentObject is PersistentRuntimePrefab)
                            {
                                PersistentRuntimePrefab persistentPrefab = (PersistentRuntimePrefab)persistentObject;
                                Dictionary<int, UnityObject> idToObj = new Dictionary<int, UnityObject>();
                                List<GameObject> createdGameObjects = new List<GameObject>();
                                persistentPrefab.CreateGameObjectWithComponents(m_typeMap, persistentPrefab.Descriptors[0], idToObj, createdGameObjects);
                                m_assetDB.RegisterDynamicResources(idToObj);
                                for (int j = 0; j < createdGameObjects.Count; ++j)
                                {
                                    GameObject createdGO = createdGameObjects[j];
                                    createdGO.transform.SetParent(createdGO.transform, false);
                                    m_dynamicResources.Add(unchecked((int)m_assetDB.ToID(createdGO)), createdGO);
                                }  
                            }
                            else
                            {
                                Type type = m_typeMap.ToType(assetItem.TypeGuid);
                                UnityObject instance = m_factory.CreateInstance(type);
                                m_assetDB.RegisterDynamicResource(unchecked((int)assetItem.ItemID), instance);
                                m_dynamicResources.Add(unchecked((int)assetItem.ItemID), instance);
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
                    if(m_assetDB.IsSceneID(assetItems[i].ItemID))
                    {
                        persistentObject.WriteTo(SceneManager.GetActiveScene());
                    }
                    else
                    {
                        UnityObject obj = m_assetDB.FromID<UnityObject>(assetItems[i].ItemID);
                        Debug.Assert(obj != null);
                        if (obj != null)
                        {
                            persistentObject.WriteTo(obj);
                        }
                    }    
                }
            }
            Error error = new Error(Error.OK);
            UnityObject result = m_assetDB.FromID<UnityObject>(rootItem.ItemID);
            if (callback != null)
            {
                callback(error, result);
            }
            ao.Error = error;
            ao.Result = result;
            ao.IsCompleted = true;
        }

        public AsyncOperation Unload(ProjectEventHandler callback = null)
        {
            UnloadUnregisterDestroy();
            return m_assetDB.UnloadUnusedAssets(ao =>
            {
                if (callback != null)
                {
                    callback(new Error());
                }
            });
        }
    }
}

