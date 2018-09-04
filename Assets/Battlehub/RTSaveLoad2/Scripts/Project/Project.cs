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
        void GetAssets(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback);

        bool CanSave(ProjectItem parent, UnityObject obj);
        void Save(ProjectItem parent, byte[] previewData, UnityObject obj, ProjectEventHandler<ProjectItem> callback);
        void Save(AssetItem assetItem, UnityObject obj, ProjectEventHandler callback);
        void Load(AssetItem assetItem, ProjectEventHandler<UnityObject> callback);
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

        private void PopulateIdToAssetItem(ProjectItem item)
        {
            if(item == null)
            {
                return;
            }

            AssetItem assetItem = item as AssetItem;
            if(assetItem != null)
            {
                m_idToAssetItem.Add(assetItem.ItemID, assetItem);
                if(assetItem.Parts != null)
                {
                    for(int i = 0; i < assetItem.Parts.Length; ++i)
                    {
                        PrefabPartItem part = assetItem.Parts[i];
                        if(part != null)
                        {
                            m_idToAssetItem.Add(part.PartID, assetItem);
                        }
                    }
                }
            }

            if(item.Children != null)
            {
                for(int i = 0; i < item.Children.Count; ++i)
                {
                    PopulateIdToAssetItem(item.Children[i]);
                }
            }
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
                if (childItem.ItemID != 0 && ordinal == m_assetDB.ToOrdinal(id))
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
                assetTo.PreviewData = null; //must rebuild preview

                if(assetFrom.PrefabParts != null)
                {
                    assetTo.Parts = new PrefabPartItem[assetFrom.PrefabParts.Count];
                    for (int j = 0; j < assetFrom.PrefabParts.Count; ++j)
                    {
                        PrefabPartInfo prefabPart = assetFrom.PrefabParts[j];
                        Guid typeGuid = m_typeMap.ToGuid(prefabPart.Object.GetType());
                        if (prefabPart != null && prefabPart.Object != null && typeGuid != Guid.Empty)
                        {
                            assetTo.Parts[i] = new PrefabPartItem
                            {
                                Name = prefabPart.Object.name,
                                ParentID = prefabPart.ParentPersistentID,
                                PartID = prefabPart.PersistentID,
                                TypeGuid = typeGuid
                            };
                        }
                        else
                        {
                            assetTo.Parts[i] = null;
                        }
                    }
                }                
            }
        }

        private void SetIdForFoldersWithNoId(ProjectItem projectItem)
        {
            if(projectItem.ItemID == 0)
            {
                if(projectItem.IsFolder)
                {
                    int ordinal, id;
                    if(!GetOrdinalAndId(m_projectInfo.FolderIdentifier, out ordinal, out id))
                    {
                        if(projectItem.Parent != null)
                        {
                            projectItem.Parent.RemoveChild(projectItem);
                        }  
                        return;
                    }

                    projectItem.ItemID = m_assetDB.ToRuntimeFolderID(ordinal, id);
                    m_projectInfo.FolderIdentifier++;

                    if(projectItem.Children != null)
                    {
                        for(int i = 0; i < projectItem.Children.Count; ++i)
                        {
                            SetIdForFoldersWithNoId(projectItem);
                        }
                    }
                }
            }
        }

        private void GetFolderTree(string project, ProjectEventHandler callback)
        {
            m_storage.GetFolderTree(project, (error, rootFolder) =>
            {
                if (error.HasError)
                {
                    callback(error);
                    return;
                }

                OnGetFoldersCompleted(error, rootFolder, callback);
            });
        }

        private void OnGetFoldersCompleted(Error error, ProjectItem rootFolder, ProjectEventHandler callback)
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
                CleanupTree(m_root, i);
                MergeAssetLibrary(assetLibrary, i);
            }

            SetIdForFoldersWithNoId(m_root);

           

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
            GetFolderTree(project, callback);
        }

        public void GetAssets(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback)
        {
            m_storage.GetAssets(m_projectPath, folders.Select(f => f.ToString()).ToArray(), (error, result) =>
            {
                if (error.HasError)
                {
                    callback(error, new ProjectItem[0]);
                    return;
                }
                OnGetAssetsCompleted(folders, callback, error, result);
            });
        }

        private void OnGetAssetsCompleted(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback, Error error, ProjectItem[][] result)
        {
            for (int i = 0; i < result.Length; ++i)
            {
                ProjectItem folder = folders[i];
                ProjectItem[] items = result[i];
                if (items != null && items.Length > 0)
                {
                    if(folder.Children == null)
                    {
                        folder.Children = new List<ProjectItem>();
                    }
                    
                    for (int j = 0; j < items.Length; ++j)
                    {
                        ProjectItem item = items[j];
                        if(item.ItemID == 0)
                        {
                            items[j] = null;
                            continue;
                        }
                        
                        if (!folder.Children.Any(child => child.ItemID == item.ItemID))
                        {
                            item.Parent = folder;
                            folder.Children.Add(item);
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

        private void PersistentDescriptorsToPrefabPartItems(PersistentDescriptor[] descriptors, List<PrefabPartItem> prefabParts)
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

                    PrefabPartItem prefabPartItem = new PrefabPartItem
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
            assetItem.PreviewData = previewData;

            if(persistentObject is PersistentPrefab)
            {
                PersistentPrefab persistentPrefab = (PersistentPrefab)persistentObject;
                if(persistentPrefab.Descriptors != null)
                {
                    List<PrefabPartItem> prefabParts = new List<PrefabPartItem>();
                    PersistentDescriptorsToPrefabPartItems(persistentPrefab.Descriptors, prefabParts);
                    assetItem.Parts = prefabParts.ToArray();
                }
            }

            m_storage.Save(m_projectPath, parent.ToString(), assetItem, persistentObject, m_projectInfo, error =>
            {
                if(!error.HasError)
                {
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
                    List<PrefabPartItem> prefabParts = new List<PrefabPartItem>();
                    PersistentDescriptorsToPrefabPartItems(persistentPrefab.Descriptors, prefabParts);
                    assetItem.Parts = prefabParts.ToArray();
                }
            }

            m_storage.Save(m_projectPath, assetItem.Parent.ToString(), assetItem, persistentObject, m_projectInfo, error =>
            {
                callback(error);
            });
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

            m_storage.Load(m_projectPath, assetItem.ToString(), persistentType, (error, persistentObject) =>
            {
                if (error.HasError)
                {
                    callback(error, null);
                    return;
                }

                OnLoadCompleted(assetItem, persistentObject, callback);
            });
        }

        private void OnLoadCompleted(AssetItem assetItem, PersistentObject persistentObject, ProjectEventHandler<UnityObject> callback)
        {
            UnityObject obj = m_assetDB.FromID<UnityObject>(assetItem.ItemID);
            if(obj == null)
            {
                Type type = m_typeMap.ToType(assetItem.TypeGuid);
                try
                {
                    obj = m_factory.CreateInstance(type);
                }
                catch(Exception e)
                {
                    Debug.LogErrorFormat("Unable to load asset: {0} -> got exception: {1} ", assetItem.ToString(), e.ToString());
                    Error error = new Error();
                    error.ErrorCode = Error.E_Exception;
                    error.ErrorText = e.ToString();
                    callback(error, null);
                    return;
                }
            }

            if(persistentObject is PersistentPrefab)
            {
                PersistentPrefab persistentPrefab = (PersistentPrefab)persistentObject;
                long[] deps = persistentPrefab.Dependencies;

                for(int i = 0; i < deps.Length; ++i)
                {
                    long dep = deps[i];
                    if(!m_assetDB.IsLoaded(dep))
                    {
                        if (m_assetDB.IsStaticResourceID(dep))
                        {
                            int ordinal = m_assetDB.ToOrdinal(dep);
                            if (!m_assetDB.IsLibraryLoaded(ordinal))
                            {
                                AssetLibraryReferenceInfo reference = m_projectInfo.References.FirstOrDefault(r => r.Ordinal == ordinal);
                                if(reference != null)
                                {
                                    m_assetDB.LoadLibrary(reference.AssetLibrary, reference.Ordinal);
                                }       
                            }
                        }
                        else if(m_assetDB.IsDynamicResourceID(dep))
                        {
                            //m_root.F
                        }
                    }
                }
                
                
            }

            obj = (UnityObject)persistentObject.WriteTo(obj);
            callback(new Error(), obj);
        }
    }
}

