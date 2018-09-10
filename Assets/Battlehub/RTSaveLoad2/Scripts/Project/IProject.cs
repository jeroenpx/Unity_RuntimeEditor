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

        void Open(string project, ProjectEventHandler callback);
        void GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback);

        bool CanSave(ProjectItem parent, UnityObject obj);
        void Save(ProjectItem parent, byte[] previewData, object obj, ProjectEventHandler<ProjectItem> callback);
        void Save(AssetItem assetItem, object obj, ProjectEventHandler callback);
        void Load(AssetItem assetItem, ProjectEventHandler<UnityObject> callback);
        AsyncOperation Unload(Action<AsyncOperation> completedCallback = null);
    }


    public delegate void ProjectEventHandler(Error error);
    public delegate void ProjectEventHandler<T>(Error error, T result);
    public delegate void ProjectEventHandler<T, V>(Error error, T result, V result2);
}
