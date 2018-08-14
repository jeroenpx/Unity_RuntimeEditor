using System;
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

        void AddReference(string assetLibrary);

        void RemoveReference(string assetLibrary); 
    }

    public delegate void ProjectEventHandler(Error error);
    public delegate void ProjectEventHandler<T>(Error error, T result);

    public class Project : MonoBehaviour, IProject
    {
        private IStorage m_storage;

        [SerializeField]
        private ProjectInfo m_project;

        public ProjectItem Root
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private void Awake()
        {
            m_storage = RTSL2Deps.Get.Storage;   
        }

        public void AddReference(string assetLibrary)
        {
            AssetLibraryReference reference = new AssetLibraryReference
            {
                Ordinal = m_project.IdentityCounter++,
                Path = assetLibrary
            };

            if(m_project.References == null)
            {
                m_project.References = new AssetLibraryReference[1];
            }
            else
            {
                Array.Resize(ref m_project.References, m_project.References.Length + 1);
            }

            m_project.References[m_project.References.Length - 1] = reference;
        }

        public void RemoveReference(string assetLibrary)
        {
            if (m_project.References == null)
            {
                return;
            }

            m_project.References = m_project.References.Where(r => r != null && r.Path != assetLibrary).ToArray();
        }

#warning Project does not require any of AssetBundles loaded. Editor window could use PersistentObject for editing. Only Scene will load required asset bundles and unload when done.
        //Editing of prefabs or objects which does not have constructor and cannot be instantiated will cause asset library lazy loading. Will be unloaded if will not be used during several minutes (or as result cleanup unuse resources call)

        public void Open(string project, ProjectEventHandler callback)
        {
            m_storage.GetFolders(project, (error, folders) =>
            {
                if(error.HasError)
                {
                    callback(error);
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

