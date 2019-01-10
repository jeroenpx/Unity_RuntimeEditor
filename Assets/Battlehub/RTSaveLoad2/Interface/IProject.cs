using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2.Interface
{
    public delegate void ProjectEventHandler(Error error);
    public delegate void ProjectEventHandler<T>(Error error, T result);
    public delegate void ProjectEventHandler<T, T2>(Error error, T result, T2 result2);

    public interface IProject
    {
        event ProjectEventHandler NewSceneCreated;
        event ProjectEventHandler<ProjectInfo> CreateProjectCompleted;
        event ProjectEventHandler<ProjectInfo> OpenProjectCompleted;
        event ProjectEventHandler<string> DeleteProjectCompleted;
        event ProjectEventHandler<ProjectInfo[]> ListProjectsCompleted;
        event ProjectEventHandler CloseProjectCompleted;

        event ProjectEventHandler<ProjectItem[]> GetAssetItemsCompleted;
        event ProjectEventHandler<AssetItem[]> CreateCompleted;
        event ProjectEventHandler<AssetItem[]> SaveCompleted;
        event ProjectEventHandler<UnityObject[]> LoadCompleted;
        event ProjectEventHandler UnloadCompleted;
        event ProjectEventHandler<AssetItem[]> ImportCompleted;
        event ProjectEventHandler<ProjectItem[]> DeleteCompleted;
        event ProjectEventHandler<ProjectItem[], ProjectItem> MoveCompleted;
        event ProjectEventHandler<ProjectItem> RenameCompleted;

        bool IsBusy
        {
            get;
        }

        ProjectItem Root
        {
            get;
        }

        AssetItem LoadedScene
        {
            get;
            set;
        }

        string[] AssetLibraries
        {
            get;
        }

        bool IsStatic(ProjectItem projectItem);
        bool IsScene(ProjectItem projectItem);
        Type ToType(AssetItem assetItem);
        Guid ToGuid(Type type);
        long ToID(UnityObject obj);
        T FromID<T>(long id) where T : UnityObject;
        AssetItem ToAssetItem(UnityObject obj);
        AssetItem[] GetDependantAssetItems(AssetItem[] assetItems);

        string GetExt(object obj);
        string GetExt(Type type);
        
        bool IsOpened
        {
            get;
        }

        void CreateNewScene();
        ProjectAsyncOperation<ProjectInfo> CreateProject(string project, ProjectEventHandler<ProjectInfo> callback = null);
        ProjectAsyncOperation<ProjectInfo> OpenProject(string project, ProjectEventHandler<ProjectInfo> callback = null);
        ProjectAsyncOperation<ProjectInfo[]> GetProjects(ProjectEventHandler<ProjectInfo[]> callback = null);
        ProjectAsyncOperation<string> DeleteProject(string project, ProjectEventHandler<string> callback = null);
        void CloseProject();

        ProjectAsyncOperation<AssetItem[]> GetAssetItems(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null); /*no events raised*/
        ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback = null); /*GetAssetItemsCompleted raised*/

        ProjectAsyncOperation<object[]> GetDependencies(object obj, bool exceptMappedObject = false, ProjectEventHandler<object[]> callback = null); /*no events raised*/
        ProjectAsyncOperation<AssetItem[]> Create(ProjectItem parent, byte[][] previewData, object[] obj, string[] nameOverrides, ProjectEventHandler<AssetItem[]> callback = null);
        ProjectAsyncOperation<AssetItem[]> Save(AssetItem[] assetItems, object[] objects, ProjectEventHandler<AssetItem[]> callback = null);
        ProjectAsyncOperation<AssetItem[]> SavePreview(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null); 
        ProjectAsyncOperation<UnityObject[]> Load(AssetItem[] assetItems, ProjectEventHandler<UnityObject[]> callback = null);
        AsyncOperation Unload(ProjectEventHandler completedCallback = null);

        ProjectAsyncOperation<ProjectItem> LoadImportItems(string path, bool isBuiltIn, ProjectEventHandler<ProjectItem> callback = null);
        void UnloadImportItems(ProjectItem importItemsRoot);
        ProjectAsyncOperation<AssetItem[]> Import(ImportItem[] importItems, ProjectEventHandler<AssetItem[]> callback = null);

        ProjectAsyncOperation<ProjectItem> Rename(ProjectItem projectItem, string oldName, ProjectEventHandler<ProjectItem> callback = null);
        ProjectAsyncOperation<ProjectItem[], ProjectItem> Move(ProjectItem[] projectItems, ProjectItem target, ProjectEventHandler<ProjectItem[], ProjectItem> callback = null);
        ProjectAsyncOperation<ProjectItem[]> Delete(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback = null);

        ProjectAsyncOperation<string[]> GetAssetBundles(ProjectEventHandler<string[]> callback = null);
    }

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

    public class ProjectAsyncOperation<T, T2> : ProjectAsyncOperation<T>
    {
        public T2 Result2
        {
            get;
            set;
        }
    }
}
