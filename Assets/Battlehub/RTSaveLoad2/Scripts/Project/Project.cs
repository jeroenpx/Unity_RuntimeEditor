using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

using Battlehub.RTSaveLoad2.Interface;
using Battlehub.RTCommon;
using System.IO;
using Battlehub.RTCommon.EditorTreeView;

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
        //[SerializeField]
        //private AssetLibraryAsset[] m_staticReferences = new AssetLibraryAsset[0];
        //public AssetLibraryAsset[] StaticReferences
        //{
        //    get { return m_staticReferences; }
        //}

        [SerializeField]
        private string[] m_assetLibaries;
        public string[] AssetLibraries
        {
            get { return m_assetLibaries; }
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
        //private MappingInfo m_staticReferencesMapping;

        private void Awake()
        {
            m_storage = IOC.Resolve<IStorage>();
            m_assetDB = IOC.Resolve<IAssetDB>();
            m_typeMap = IOC.Resolve<ITypeMap>();
            m_factory = IOC.Resolve<IUnityObjectFactory>();

            if (m_dynamicPrefabsRoot == null)
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
            //m_staticReferencesMapping = null;
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

        /*
        private void CleanupTree(ProjectItem item, int ordinal)
        {
            if (item.Children == null)
            {
                return;
            }

            for (int i = item.Children.Count - 1; i >= 0; --i)
            {
                ProjectItem childItem = item.Children[i];

                int id = unchecked((int)childItem.ItemID);
                if (childItem.ItemID == 0 || ordinal == m_assetDB.ToOrdinal(id))
                {
                    item.Children.RemoveAt(i);
                }

                CleanupTree(childItem, ordinal);
            }
        }*/

        /*
        private void MergeAssetLibrary(AssetLibraryAsset asset, int ordinal)
        {
            if (!asset.KeepRuntimeProjectInSync)
            {
                return;
            }

            AssetLibraryInfo assetLibrary = asset.AssetLibrary;
            if (assetLibrary == null)
            {
                return;
            }

            assetLibrary.BuildTree();

            AssetFolderInfo rootFolder = assetLibrary.Folders.Where(folder => folder.depth == 0).First();
            MergeFolders(rootFolder, m_root, ordinal);
        }*/

        /*
        private void MergeFolders(AssetFolderInfo from, ProjectItem to, int ordinal)
        {
            if (from.hasChildren)
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
        }*/

        /*
        private void MergeAssets(AssetFolderInfo from, ProjectItem to, int ordinal)
        {
            List<AssetInfo> fromAssets = from.Assets;
            if (fromAssets == null)
            {
                return;
            }

            if (to.Children == null)
            {
                to.Children = new List<ProjectItem>();
            }

            for (int i = 0; i < fromAssets.Count; ++i)
            {
                AssetInfo assetFrom = fromAssets[i];
                if (assetFrom.Object == null)
                {
                    continue;
                }
                AssetItem assetTo = to.Children.OfType<AssetItem>().Where(item => item.Name == assetFrom.name).FirstOrDefault();
                if (assetTo == null)
                {
                    assetTo = new AssetItem();
                    to.Children.Add(assetTo);
                }

                assetTo.Name = assetFrom.name;
                assetTo.ItemID = m_assetDB.ToStaticResourceID(ordinal, assetFrom.PersistentID);
                assetTo.Parent = to;
                assetTo.TypeGuid = m_typeMap.ToGuid(assetFrom.Object.GetType());
                assetTo.Preview = null; //must rebuild preview

                if (assetFrom.PrefabParts != null)
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
        */

        private void GetProjectTree(string project, ProjectAsyncOperation ao, ProjectEventHandler callback)
        {
            m_storage.GetProjectTree(project, (error, rootFolder) =>
            {
                if (error.HasError)
                {
                    if (callback != null)
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

            /*
            for (int i = 0; i < m_staticReferences.Length; ++i)
            {
                AssetLibraryAsset assetLibrary = m_staticReferences[i];
                if (assetLibrary == null)
                {
                    continue;
                }
                assetLibrary.Ordinal = i;
                m_assetDB.AddLibrary(assetLibrary, i);
                
                CleanupTree(m_root, i);
                MergeAssetLibrary(assetLibrary, i);
            }
            */
            AssetItem[] assetItems = m_root.Flatten(true).OfType<AssetItem>().ToArray();
            m_idToAssetItem = assetItems.ToDictionary(item => item.ItemID);
            for (int i = 0; i < assetItems.Length; ++i)
            {
                AssetItem assetItem = assetItems[i];
                if (assetItem.Parts != null)
                {
                    for (int j = 0; j < assetItem.Parts.Length; ++j)
                    {
                        PrefabPart prefabPart = assetItem.Parts[j];
                        if (prefabPart != null)
                        {
                            m_idToAssetItem.Add(prefabPart.PartID, assetItem);
                        }
                    }
                }
            }
            if (callback != null)
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

        //public bool TryGetFromStaticReferences(AssetItem assetItem, out UnityObject obj)
        //{
        //    return m_staticReferencesMapping.PersistentIDtoObj.TryGetValue(unchecked((int)assetItem.ItemID), out obj);
        //}

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
            if (obj == null)
            {
                return null;
            }
            if (obj is Scene)
            {
                return ".rtscene";
            }
            if (obj is GameObject)
            {
                return ".rtprefab";
            }
            if (obj is ScriptableObject)
            {
                return ".rtasset";
            }
            if (obj is Material)
            {
                return ".rtmat";
            }
            if (obj is Mesh)
            {
                return ".rtmesh";
            }
            if (obj is Shader)
            {
                return ".rtshader";
            }
            return ".rt" + obj.GetType().Name.ToLower().Substring(0, 3);
        }

        public string GetExt(Type type)
        {
            if (type == null)
            {
                return null;
            }
            if (type == typeof(Scene))
            {
                return ".rtscene";
            }
            if (type == typeof(GameObject))
            {
                return ".rtprefab";
            }
            if (type == typeof(ScriptableObject))
            {
                return ".rtasset";
            }
            if (type == typeof(Material))
            {
                return ".rtmat";
            }
            if (type == typeof(Mesh))
            {
                return ".rtmesh";
            }
            if (type == typeof(Shader))
            {
                return ".rtshader";
            }
            return ".rt" + type.Name.ToLower().Substring(0, 3);
        }

        public ProjectAsyncOperation Open(string project, ProjectEventHandler callback)
        {
            //m_staticReferencesMapping = new MappingInfo();
            /*
            for (int i = 0; i < StaticReferences.Length; ++i)
            {
                AssetLibraryAsset reference = StaticReferences[i];
                if (reference != null)
                {
                    reference.LoadIDMappingTo(m_staticReferencesMapping, false, true);
                }
            }
            */

            UnloadUnregisterDestroy();

            m_projectInfo = null;
            m_root = null;

            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            m_projectPath = project;

            m_storage.GetProject(m_projectPath, (error, projectInfo) =>
            {
                if (error.HasError)
                {
                    if (callback != null)
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
                    if (callback != null)
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

                        if (m_idToAssetItem.TryGetValue(preview.ItemID, out assetItem))
                        {
                            if (assetItem.Parent == null)
                            {
                                Debug.LogErrorFormat("asset item {0} parent is null", assetItem.ToString());
                                continue;
                            }

                            if (assetItem.Parent.ItemID != folder.ItemID)
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

            if (parent.Children != null && parent.Children.Any(c => c.NameExt == obj.name + GetExt(obj)))
            {
                return false;
            }

            return true;
        }

        private void PersistentDescriptorsToPrefabPartItems(PersistentDescriptor[] descriptors, List<PrefabPart> prefabParts)
        {
            if (descriptors == null)
            {
                return;
            }

            for (int i = 0; i < descriptors.Length; ++i)
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
                    if (type == null)
                    {
                        Debug.LogWarningFormat("Unable to get unity type from persistent type {1}", type.FullName);
                        continue;
                    }

                    Guid typeGuid = m_typeMap.ToGuid(type);
                    if (typeGuid == Guid.Empty)
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
            if (m_root == null)
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

            if (persistentType == typeof(PersistentGameObject))
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
                for (int i = 0; i < transforms.Length; ++i)
                {
                    Transform tf = transforms[i];
                    if (tf.gameObject != go)
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
                    for (int j = 0; j < components.Length; ++j)
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
            else if (obj is UnityObject)
            {
                m_assetDB.RegisterDynamicResource((int)m_assetDB.ToDynamicResourceID(rootOrdinal, rootId), (UnityObject)obj);
            }

            PersistentObject persistentObject = (PersistentObject)Activator.CreateInstance(persistentType);
            persistentObject.ReadFrom(obj);

            if (!string.IsNullOrEmpty(nameOverride))
            {
                persistentObject.name = nameOverride;
            }

            AssetItem assetItem = new AssetItem();
            if (obj is Scene)
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

            m_storage.Save(m_projectPath, new[] { parent.ToString() }, new[] { assetItem }, new[] { persistentObject }, m_projectInfo, error =>
            {
                if (!error.HasError)
                {
                    if (!(obj is Scene))
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

        public ProjectAsyncOperation Save(AssetItem[] assetItems, object[] objects, ProjectEventHandler callback)
        {
            if (m_root == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            if (objects == null)
            {
                throw new ArgumentNullException("objects");
            }

            PersistentObject[] persistentObjects = new PersistentObject[assetItems.Length];
            for(int i = 0; i < persistentObjects.Length; ++i)
            {
                object obj = objects[i];
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
                persistentObject.ReadFrom(objects[i]);

                if (persistentObject is PersistentRuntimePrefab)
                {
                    PersistentRuntimePrefab persistentPrefab = (PersistentRuntimePrefab)persistentObject;
                    if (persistentPrefab.Descriptors != null)
                    {
                        List<PrefabPart> prefabParts = new List<PrefabPart>();
                        PersistentDescriptorsToPrefabPartItems(persistentPrefab.Descriptors, prefabParts);
                        assetItems[i].Parts = prefabParts.ToArray();
                    }
                }

                GetDepsContext getDepsCtx = new GetDepsContext();
                persistentObject.GetDeps(getDepsCtx);
                assetItems[i].Dependencies = getDepsCtx.Dependencies.ToArray();
            }

            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            m_storage.Save(m_projectPath, assetItems.Select(ai => ai.Parent.ToString()).ToArray(), assetItems, persistentObjects, m_projectInfo, error =>
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
                        if (m_idToAssetItem.TryGetValue(dep, out dependencyAssetItem))
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
            if (type == null)
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
            for (int i = 0; i < persistentTypes.Length; ++i)
            {
                if (persistentTypes[i] == typeof(PersistentGameObject))
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

#warning Implement caching (populate cache onLoad, update cache on save)
        private void OnLoadCompleted(AssetItem rootItem, AssetItem[] assetItems, PersistentObject[] persistentObjects, ProjectAsyncOperation<UnityObject> ao, ProjectEventHandler<UnityObject> callback)
        {
            for (int i = 0; i < assetItems.Length; ++i)
            {
                AssetItem assetItem = assetItems[i];
                if (!m_assetDB.IsMapped(assetItem.ItemID))
                {
                    if (m_assetDB.IsStaticResourceID(assetItem.ItemID))
                    {
                        int ordinal = m_assetDB.ToOrdinal(assetItem.ItemID);
                        if (!m_assetDB.IsLibraryLoaded(ordinal))
                        {
                            //AssetLibraryAsset assetLibrary = m_staticReferences.Where(r => r.Ordinal == ordinal).FirstOrDefault();
                            AssetLibraryReferenceInfo reference = m_projectInfo.References.FirstOrDefault(r => r.Ordinal == ordinal);
                            if (reference != null)
                            {
                                m_assetDB.LoadLibrary(reference.AssetLibrary, reference.Ordinal);
                            }
                        }
                    }
                    else if (m_assetDB.IsDynamicResourceID(assetItem.ItemID))
                    {
                        PersistentObject persistentObject = persistentObjects[i];
                        if (persistentObject != null)
                        {
                            if (persistentObject is PersistentRuntimeScene)
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
                    if (m_assetDB.IsSceneID(assetItems[i].ItemID))
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


        public ProjectAsyncOperation<ProjectItem> LoadAssetLibrary(int index, ProjectEventHandler<ProjectItem> callback = null)
        {
            ProjectAsyncOperation<ProjectItem> pao = new ProjectAsyncOperation<ProjectItem>();
            if(m_root == null)
            {
                Error error = new Error(Error.E_InvalidOperation);
                if (callback != null)
                {
                    error.ErrorText = "Unable to load asset library. Open project first";
                    callback(error, null);
                }
                pao.Error = error;
                pao.IsCompleted = true;
                return pao;
            }
            
            ResourceRequest request = Resources.LoadAsync<AssetLibraryAsset>(AssetLibraries[index]);
            Action<AsyncOperation> completed = null;
            completed = ao =>
            {
                request.completed -= completed;

                ProjectItem result = new ProjectItem();

                AssetLibraryAsset asset = (AssetLibraryAsset)request.asset;

                Error error = new Error(Error.OK);
                if (asset == null)
                {
                    error.ErrorCode = Error.E_NotFound;
                    error.ErrorText = "Asset Library " + AssetLibraries[index] + " does not exist";
                    if(callback != null)
                    {
                        callback(error, result);
                    }

                    pao.Error = error;
                    pao.Result = null;
                    pao.IsCompleted = true;
                    return;
                }

                TreeModel<AssetFolderInfo> model = new TreeModel<AssetFolderInfo>(asset.AssetLibrary.Folders);
                BuildTree(result, (AssetFolderInfo)model.root.children[0], index);

                if (callback != null)
                {
                    callback(error, result);
                }

                pao.Result = result;
                pao.IsCompleted = true;

                Resources.UnloadAsset(asset);
            };
            request.completed += completed;
            return pao;
        }

        private void BuildTree(ProjectItem projectItem, AssetFolderInfo folder,  int ordinal)
        {
            projectItem.Name = folder.name;

            if (folder.hasChildren)
            {
                projectItem.Children = new List<ProjectItem>();
                for (int i = 0; i < folder.children.Count; ++i)
                {
                    ProjectItem child = new ProjectItem();
                    projectItem.AddChild(child);
                    BuildTree(child, (AssetFolderInfo)folder.children[i], ordinal);
                }
            }

            if (folder.Assets != null && folder.Assets.Count > 0)
            {
                if (projectItem.Children == null)
                {
                    projectItem.Children = new List<ProjectItem>();
                }

                List<string> existingNames = new List<string>();
                for (int i = 0; i < folder.Assets.Count; ++i)
                {
                    AssetInfo assetInfo = folder.Assets[i];
                    if (assetInfo.Object != null)
                    {
                        ImportStatus status = ImportStatus.New;
                        string ext = GetExt(assetInfo.Object);
                        string name = PathHelper.GetUniqueName(assetInfo.name, ext, existingNames);
                        long itemID = m_assetDB.ToStaticResourceID(ordinal, assetInfo.PersistentID);
                        Guid typeGuid = m_typeMap.ToGuid(assetInfo.Object.GetType());
                        if(typeGuid == Guid.Empty)
                        {
                            continue;
                        }

                        ImportItem importItem = new ImportItem
                        {
                            Name = name,
                            Ext = ext,
                            Object = assetInfo.Object,
                            TypeGuid = typeGuid,
                            ItemID = itemID
                        };

                        if(assetInfo.PrefabParts != null)
                        {
                            List<PrefabPart> parts = new List<PrefabPart>();
                            for(int j = 0; j < assetInfo.PrefabParts.Count; ++j)
                            {
                                PrefabPartInfo partInfo = assetInfo.PrefabParts[j];
                                
                                if (partInfo.Object != null)
                                {
                                    Guid partTypeGuid = m_typeMap.ToGuid(partInfo.Object.GetType());
                                    if(partTypeGuid == Guid.Empty)
                                    {
                                        continue;
                                    }
                                    PrefabPart part = new PrefabPart
                                    {
                                        Name = partInfo.Object.name,
                                        PartID = m_assetDB.ToStaticResourceID(ordinal, partInfo.PersistentID),
                                        ParentID = m_assetDB.ToStaticResourceID(ordinal, partInfo.PersistentID),
                                        TypeGuid = partTypeGuid,
                                    };

                                    if(m_idToAssetItem.ContainsKey(part.PartID))
                                    {
                                        status = ImportStatus.Conflict;
                                    }

                                    parts.Add(part);
                                }
                            }
                            importItem.Parts = parts.ToArray();
                        }

                        if(status != ImportStatus.Conflict)
                        {
                            AssetItem exisitingItem;
                            if (m_idToAssetItem.TryGetValue(itemID, out exisitingItem))
                            {
                                if (exisitingItem.TypeGuid == typeGuid)
                                {
                                    status = ImportStatus.Overwrite;
                                }
                                else
                                {
                                    status = ImportStatus.Conflict;
                                }
                            }
                            else
                            {
                                status = ImportStatus.New;
                            }
                        }
                        
                        importItem.Status = status;

                        projectItem.AddChild(importItem);
                        existingNames.Add(importItem.NameExt);
                    }
                }
            }
        }

        public ProjectAsyncOperation ImportAssets(ImportItem[] importItems, ProjectEventHandler callback)
        {
            ProjectAsyncOperation pao = new ProjectAsyncOperation();
            if (m_root == null)
            {
                Error error = new Error(Error.E_InvalidOperation);
                error.ErrorText = "Unable to load asset library. Open project first";
                ImportAssetsCompleted(error, callback, pao);
                return pao;
            }

            for (int i = 0; i < importItems.Length; ++i)
            {
                ImportItem importItem = importItems[i];
                if (m_typeMap.ToType(importItem.TypeGuid) == null)
                {
                    Error error = new Error(Error.E_InvalidOperation);
                    error.ErrorText = "Type of import item is invalid";
                    ImportAssetsCompleted(error, callback, pao);
                    return pao;
                }

                if (importItem.Parts != null)
                {
                    for (int p = 0; p < importItem.Parts.Length; ++p)
                    {
                        PrefabPart part = importItem.Parts[p];
                        if (m_typeMap.ToType(part.TypeGuid) == null)
                        {
                            Error error = new Error(Error.E_InvalidOperation);
                            error.ErrorText = "Type of import item part is invalid";
                            ImportAssetsCompleted(error, callback, pao);
                            return pao;
                        }
                    }
                }
            }


            AssetItem[] assetItems = new AssetItem[importItems.Length];
            object[] objects = new object[importItems.Length];
            for (int i = 0; i < importItems.Length; ++i)
            {
                ImportItem importItem = importItems[i];
                ProjectItem parent = null;
                AssetItem assetItem;
                if (m_idToAssetItem.TryGetValue(importItem.ItemID, out assetItem))
                {
                    parent = assetItem.Parent;
                    RemoveAssetItem(assetItem);
                }

                if (importItem.Parts != null)
                {
                    for (int p = 0; p < importItem.Parts.Length; ++p)
                    {
                        PrefabPart part = importItem.Parts[p];
                        if (m_idToAssetItem.TryGetValue(importItem.ItemID, out assetItem))
                        {
                            RemoveAssetItem(assetItem);
                        }
                    }
                }

                assetItem = new AssetItem
                {
                    Name = PathHelper.GetUniqueName(importItem.Name, importItem.Ext, importItem.Parent.Children.Select(child => child.NameExt).ToList()),
                    Ext = importItem.Ext,
                    ItemID = importItem.ItemID,
                    Parts = importItem.Parts,
                    TypeGuid = importItem.TypeGuid,
                    Preview = importItem.Preview,
                };

                m_idToAssetItem.Add(assetItem.ItemID, assetItem);
                if (assetItem.Parts != null)
                {
                    for (int p = 0; p < assetItem.Parts.Length; ++p)
                    {
                        PrefabPart part = assetItem.Parts[p];
                        m_idToAssetItem.Add(part.PartID, assetItem);
                    }
                }

                if (parent == null)
                {
                    parent = m_root.Get(importItem.Parent.ToString(), true);
                }

                parent.AddChild(assetItem);
                assetItems[i] = assetItem;
                objects[i] = importItem.Object;
            }

            Save(assetItems, objects, saveError =>
            {
                ImportAssetsCompleted(saveError, callback, pao);
            });

            return pao;
        }

        private static void ImportAssetsCompleted(Error error, ProjectEventHandler callback, ProjectAsyncOperation pao)
        {
            if (callback != null)
            {
                callback(error);
            }
            pao.Error = error;
            pao.IsCompleted = true;
        }

        private void RemoveAssetItem(AssetItem assetItem)
        {
            if (assetItem.Parent != null)
            {
                assetItem.Parent.RemoveChild(assetItem);
            }
            m_idToAssetItem.Remove(assetItem.ItemID);
            if (assetItem.Parts != null)
            {
                for (int p = 0; p < assetItem.Parts.Length; ++p)
                {
                    AssetItem partAssetItem;
                    if (m_idToAssetItem.TryGetValue(assetItem.Parts[p].PartID, out partAssetItem))
                    {
                        Debug.Assert(assetItem == partAssetItem);
                        m_idToAssetItem.Remove(assetItem.Parts[p].PartID);
                    }
                }
            }
        }
    }

    public static class PathHelper
    {
        public static bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static string RemoveInvalidFineNameCharacters(string name)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidChars.Length; ++i)
            {
                name = name.Replace(invalidChars[i].ToString(), string.Empty);
            }
            return name;
        }

        public static string GetUniqueName(string desiredName, string ext, List<string> existingNames)
        {
            if (existingNames == null || existingNames.Count == 0)
            {
                return desiredName;
            }

            for (int i = 0; i < existingNames.Count; ++i)
            {
                existingNames[i] = existingNames[i].ToLower();
            }

            HashSet<string> existingNamesHS = new HashSet<string>(existingNames);
            if (string.IsNullOrEmpty(ext))
            {
                if (!existingNamesHS.Contains(desiredName.ToLower()))
                {
                    return desiredName;
                }
            }
            else
            {
                if (!existingNamesHS.Contains(string.Format("{0}{1}", desiredName.ToLower(), ext)))
                {
                    return desiredName;
                }
            }

            string[] parts = desiredName.Split(' ');
            string lastPart = parts[parts.Length - 1];
            int number;
            if (!int.TryParse(lastPart, out number))
            {
                number = 1;
            }
            else
            {
                desiredName = desiredName.Substring(0, desiredName.Length - lastPart.Length).TrimEnd(' ');
            }

            const int maxAttempts = 10000;
            for (int i = 0; i < maxAttempts; ++i)
            {
                string uniqueName;
                if (string.IsNullOrEmpty(ext))
                {
                    uniqueName = string.Format("{0} {1}", desiredName, number);
                }
                else
                {
                    uniqueName = string.Format("{0} {1}{2}", desiredName, number, ext);
                }

                if (!existingNamesHS.Contains(uniqueName.ToLower()))
                {
                    return uniqueName;
                }

                number++;
            }

            if (string.IsNullOrEmpty(ext))
            {
                return string.Format("{0} {1}", desiredName, Guid.NewGuid().ToString("N"));
            }
            return string.Format("{0} {1}{2}", desiredName, Guid.NewGuid().ToString("N"), ext);
        }

        public static string GetUniqueName(string desiredName, List<string> existingNames)
        {
            return GetUniqueName(desiredName, null, existingNames);
        }
    }
}
