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
        private AssetLibraryVisible[] m_references;

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



#warning Project does not require any of AssetLibraries loaded. Editor window could use PersistentObject for editing. Only Scene will load required asset libraries and unload when done.
        //Editing of prefabs or objects which does not have constructor and cannot be instantiated will cause asset library lazy loading. Will be unloaded if will not be used during several minutes (or as result cleanup unuse resources call)

        private void BuildTree(ProjectItem[] items)
        {
            Dictionary<long, ProjectItem> idToProjectItem = new Dictionary<long, ProjectItem>();
            for (int i = 0; i < items.Length; ++i)
            {
                ProjectItem projectItem = items[i];
                if (!idToProjectItem.ContainsKey(projectItem.ItemID))
                {
                    idToProjectItem.Add(projectItem.ItemID, projectItem);
                }
                else
                {
                    Debug.LogErrorFormat("Duplicate Item {0} with ItemID {1} found", projectItem.Name, projectItem.ItemID);
                }
            }

            for (int i = 0; i < items.Length; ++i)
            {
                ProjectItem projectItem = items[i];

                ProjectItem parentItem;
                if (idToProjectItem.TryGetValue(projectItem.ParentItemID, out parentItem))
                {
                    if (parentItem.Children == null)
                    {
                        parentItem.Children = new List<ProjectItem>();
                    }
                    parentItem.Children.Add(projectItem);
                    projectItem.Parent = parentItem;
                }
            }
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
                if(ordinal == m_assetDB.ToOrdinal(id))
                {
                    item.Children.RemoveAt(i);
                }
                else
                {
                    CleanupTree(childItem, ordinal);
                }
            }
        }

        public void MergeAssetLibrary(AssetLibraryVisible asset)
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

            AssetFolderInfo rootFolder = assetLibrary.Folders.Where(folder => folder.depth == -1).First();
            //MergeAssetLibrary(rootFolder, m_root);
        }

      

        private void MergeAssetLibrary(int ordinal, AssetFolderInfo from, ProjectItem to)
        {
            for(int i = 0; i < from.children.Count; ++i)
            {
                AssetFolderInfo childFrom = (AssetFolderInfo)from.children[i];
                
                if(to.Children == null)
                {
                    to.Children = new List<ProjectItem>();
                }

                long exposedFolderID = m_assetDB.ToExposedFolderID(ordinal, childFrom.id);
                ProjectItem childTo = to.Children.Where(item => item.ItemID == exposedFolderID).FirstOrDefault();
                if (childTo == null)
                {
                    childTo = new ProjectItem();
                    childTo.ItemID = exposedFolderID;
                    to.Children.Add(childTo);
                }

                childTo.Name = childFrom.name;
                childTo.ItemID = m_projectInfo.IdentitiyCounter++;
                childTo.ParentItemID = to.ItemID;
                childTo.Parent = to;
            }
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

        private void GetFolders(string project, ProjectEventHandler callback)
        {
            m_storage.GetFolders(project, (error, folders) =>
            {
                if (error.HasError)
                {
                    callback(error);
                    return;
                }

                BuildTree(folders);
                IEnumerable<ProjectItem> rootItems = folders.Where(f => f.Parent == null);
                int rootItemsCount = rootItems.Count();
                if (rootItemsCount == 0)
                {
                    throw new InvalidOperationException("Unable to open project. Root items count = " + rootItemsCount);
                }
                m_root = rootItems.First();
                for (int i = 0; i < m_references.Length; ++i)
                {
                    AssetLibraryVisible assetLibrary = m_references[i];
                    assetLibrary.Ordinal = i;
                    CleanupTree(m_root, i);
                    MergeAssetLibrary(assetLibrary);
                }
            });
        }

     
     
        public void Exists(string project, ProjectEventHandler<bool> callback)
        {
            throw new NotImplementedException();
        }

        public void Create(string project, Action callback)
        {
            throw new NotImplementedException();
        }

        public void Create(string project, string description, Action callback)
        {
            throw new NotImplementedException();
        }

      

        public void Load(ProjectItem project, Action<object> callback)
        {
            throw new NotImplementedException();
        }
    }
}

