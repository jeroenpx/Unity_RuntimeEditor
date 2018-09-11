using System;
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

        string GetExt(object obj);
        string GetExt(Type type);

        ProjectAsyncOperation Open(string project, ProjectEventHandler callback = null);
        ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback = null);

        bool CanSave(ProjectItem parent, UnityObject obj);
        ProjectAsyncOperation<AssetItem> Save(ProjectItem parent, byte[] previewData, object obj, ProjectEventHandler<AssetItem> callback = null);
        ProjectAsyncOperation Save(AssetItem assetItem, object obj, ProjectEventHandler callback = null);
        ProjectAsyncOperation<UnityObject> Load(AssetItem assetItem, ProjectEventHandler<UnityObject> callback = null);
        AsyncOperation Unload(ProjectEventHandler completedCallback = null);
    }

    public delegate void ProjectEventHandler(Error error);
    public delegate void ProjectEventHandler<T>(Error error, T result);
    
    public class ProjectAsyncOperation : CustomYieldInstruction
    {
        public Error Error
        {
            get;
            set;
        }
        public bool IsCompleted
        {
            get;
            set;
        }
        public override bool keepWaiting
        {
            get { return !IsCompleted; }
        }
    }

    public class ProjectAsyncOperation<T> : ProjectAsyncOperation
    {
        public T Result
        {
            get;
            set;
        }
    }
}
