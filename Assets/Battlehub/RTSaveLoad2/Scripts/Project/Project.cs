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
    }
      

    public delegate void ProjectEventHandler(Error error);
    public delegate void ProjectEventHandler<T>(Error error, T result);

    public class Project : MonoBehaviour, IProject
    {
        private IStorage m_storage;
        private IAssetDB m_assetDB;
        private ITypeMap m_typeMap;

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

        private void Awake()
        {
            m_storage = RTSL2Deps.Get.Storage;
            m_assetDB = RTSL2Deps.Get.AssetDB;
            m_typeMap = RTSL2Deps.Get.TypeMap;
        }

        private bool IsRuntimeOrdinal(int ordinal)
        {
            return ordinal >= AssetLibraryInfo.ORDINAL_MASK / 2;
        }

        private bool GetOrdinalAndId(out int ordinal, out int id)
        {
            ordinal = AssetLibraryInfo.ORDINAL_MASK / 2 + m_assetDB.ToOrdinal(m_projectInfo.IdentitiyCounter);
            if (ordinal > AssetLibraryInfo.ORDINAL_MASK)
            {
                Debug.LogError("Unable to generate identifier. Allotted Identifiers range was exhausted");
                id = 0;
                return false;
            }

            id = m_projectInfo.IdentitiyCounter & AssetLibraryInfo.ORDINAL_MASK;
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
            }
        }

        private void SetIdForFoldersWithNoId(ProjectItem projectItem)
        {
            if(projectItem.ItemID == 0)
            {
                if(projectItem.IsFolder)
                {
                    int ordinal, id;
                    if(!GetOrdinalAndId(out ordinal, out id))
                    {
                        if(projectItem.Parent != null)
                        {
                            projectItem.Parent.RemoveChild(projectItem);
                        }  
                        return;
                    }

                    projectItem.ItemID = m_assetDB.ToRuntimeFolderID(ordinal, id);
                    m_projectInfo.IdentitiyCounter++;

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
            return m_assetDB.IsExposedFolderID(projectItem.ItemID) || m_assetDB.IsExposedResourceID(projectItem.ItemID);
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

           // callback(error, result.Where(items => items != null).SelectMany(item => item).Where(item => item != null).ToArray());
        }
    }
}

