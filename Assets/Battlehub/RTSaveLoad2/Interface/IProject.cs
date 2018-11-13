using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2.Interface
{
    public interface IProject
    {
        ProjectItem Root
        {
            get;
        }

        string[] AssetLibraries
        {
            get;
        }

        bool IsStatic(ProjectItem projectItem);
        Type ToType(AssetItem assetItem);
        Guid ToGuid(Type type);
        long ToID(UnityObject obj);
        T FromID<T>(long id) where T : UnityObject;

        string GetExt(object obj);
        string GetExt(Type type);

        ProjectAsyncOperation Open(string project, ProjectEventHandler callback = null);
        ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback = null);

        bool CanSave(ProjectItem parent, UnityObject obj);
        ProjectAsyncOperation<AssetItem> Save(ProjectItem parent, byte[] previewData, object obj, string nameOverride, ProjectEventHandler<AssetItem> callback = null);
        ProjectAsyncOperation Save(AssetItem[] assetItems, object[] objects, ProjectEventHandler callback);
        ProjectAsyncOperation<UnityObject> Load(AssetItem assetItem, ProjectEventHandler<UnityObject> callback = null);
        AsyncOperation Unload(ProjectEventHandler completedCallback = null);

        ProjectAsyncOperation<ProjectItem> LoadAssetLibrary(int index, ProjectEventHandler<ProjectItem> callback = null);
        ProjectAsyncOperation Import(ImportItem[] assetItems, ProjectEventHandler callback);
        ProjectAsyncOperation Delete(ProjectItem[] projectItems, ProjectEventHandler callback);
        ProjectAsyncOperation Move(ProjectItem[] projectItems, ProjectItem target, bool modifyTree, ProjectEventHandler callback);
        ProjectAsyncOperation Rename(ProjectItem projectItem, string oldName, ProjectEventHandler callback);
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
