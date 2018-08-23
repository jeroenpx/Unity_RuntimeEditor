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

        void Open(string project, ProjectEventHandler callback);
        bool IsReadOnly(ProjectItem projectItem);
    }
      

    public delegate void ProjectEventHandler(Error error);
    public delegate void ProjectEventHandler<T>(Error error, T result);

    public class Project : MonoBehaviour, IProject
    {
        private IStorage m_storage;
        private IAssetDB m_assetDB;

        /// <summary>
        /// Important!!!
        /// Do not remove and do not reorder items from this array. 
        /// If you want to remove reference, just set to null corresponding array element.
        /// Append new references to the end of m_references array.
        /// </summary>
        [SerializeField]
        public AssetLibraryVisible[] m_references;
        

        private ProjectInfo m_projectInfo;

        private ProjectItem m_root;
        public ProjectItem Root
        {
            get { return m_root; }
        }

        private void Awake()
        {
            m_storage = RTSL2Deps.Get.Storage;
            m_assetDB = RTSL2Deps.Get.AssetDB;
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

                int id = m_assetDB.ToInt32(childItem.ItemID);
                if (childItem.ItemID != 0 && ordinal == m_assetDB.ToOrdinal(id))
                {
                    item.Children.RemoveAt(i);
                }

                CleanupTree(childItem, ordinal);
            }
        }

        private void Merge(AssetLibraryVisible asset, int ordinal)
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
            if(!from.hasChildren)
            {
                return;
            }


            for(int i = 0; i < from.children.Count; ++i)
            {
                AssetFolderInfo childFrom = (AssetFolderInfo)from.children[i];
                
                if(to.Children == null)
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

            MergeAssets(from, to, ordinal);
        }

        private void MergeAssets(AssetFolderInfo from, ProjectItem to, int ordinal)
        {
            List<AssetInfo> fromAssets = from.Assets;
            if(fromAssets == null)
            {
                return;
            }

            for(int i = 0; i < fromAssets.Count; ++i)
            {
                AssetInfo assetFrom = fromAssets[i];
                AssetItem assetTo = to.Children.OfType<AssetItem>().Where(item => item.Name == assetFrom.name).FirstOrDefault();
                if(assetTo == null)
                {
                    assetTo = new AssetItem();
                    to.Children.Add(assetTo);
                }

                assetTo.Name = assetFrom.name;
                assetTo.ItemID = m_assetDB.ToExposedResourceID(ordinal, assetFrom.id);
                assetTo.Parent = to;
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

        private void OnGetFoldersCompleted(Error error, ProjectItem rootFolder, ProjectEventHandler callback)
        {
            m_root = rootFolder;
            for (int i = 0; i < m_references.Length; ++i)
            {
                AssetLibraryVisible assetLibrary = m_references[i];
                if (assetLibrary == null)
                {
                    continue;
                }
                assetLibrary.Ordinal = i;
                CleanupTree(m_root, i);
                Merge(assetLibrary, i);
            }
            
            SetIdForFoldersWithNoId(m_root);
            callback(error);
        }

        private void GetFolders(string project, ProjectEventHandler callback)
        {
            m_storage.GetFolders(project, (error, rootFolder) =>
            {
                if (error.HasError)
                {
                    callback(error);
                    return;
                }

                OnGetFoldersCompleted(error, rootFolder, callback);
            });
        }

        public void Open(string project, ProjectEventHandler callback)
        {
            m_storage.GetProject(project, (error, projectInfo) =>
            {
                if (error.HasError)
                {
                    callback(error);
                    return;
                }

                if(projectInfo == null)
                {
                    projectInfo = new ProjectInfo();
                }

                m_projectInfo = projectInfo;
                GetFolders(project, callback);
            });
        }

        public bool IsReadOnly(ProjectItem projectItem)
        {
            return  m_assetDB.IsExposedFolderID(projectItem.ItemID) || m_assetDB.IsExposedResourceID(projectItem.ItemID);
        }
    }
}

