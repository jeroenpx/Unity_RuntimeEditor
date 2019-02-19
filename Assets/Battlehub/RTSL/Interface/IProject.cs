using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSL.Interface
{
    public delegate void ProjectEventHandler(Error error);
    public delegate void ProjectEventHandler<T>(Error error, T result);
    public delegate void ProjectEventHandler<T, T2>(Error error, T result, T2 result2);

    public interface IProject
    {
        event ProjectEventHandler NewSceneCreating;
        event ProjectEventHandler NewSceneCreated;
        event ProjectEventHandler<ProjectInfo> CreateProjectCompleted;
        event ProjectEventHandler<ProjectInfo> OpenProjectCompleted;
        event ProjectEventHandler<string> DeleteProjectCompleted;
        event ProjectEventHandler<ProjectInfo[]> ListProjectsCompleted;
        event ProjectEventHandler CloseProjectCompleted;

        event ProjectEventHandler<ProjectItem[]> GetAssetItemsCompleted;
        event ProjectEventHandler<object[]> BeginSave;
        event ProjectEventHandler<AssetItem[], bool> SaveCompleted;
        event ProjectEventHandler<AssetItem[]> BeginLoad;
        event ProjectEventHandler<AssetItem[], UnityObject[]> LoadCompleted;
        event ProjectEventHandler<AssetItem[]> DuplicateCompleted;
        event ProjectEventHandler UnloadCompleted;
        event ProjectEventHandler<AssetItem[]> ImportCompleted;
        event ProjectEventHandler<ProjectItem[]> BeforeDeleteCompleted;
        event ProjectEventHandler<ProjectItem[]> DeleteCompleted;
        event ProjectEventHandler<ProjectItem[], ProjectItem[]> MoveCompleted;
        event ProjectEventHandler<ProjectItem> RenameCompleted;
        event ProjectEventHandler<ProjectItem> CreateCompleted;

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
        string GetUniqueName(string name, string[] names);
        
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

        ProjectAsyncOperation<AssetItem[]> Save(AssetItem[] assetItems, object[] obj, ProjectEventHandler<AssetItem[]> callback = null);
        ProjectAsyncOperation<AssetItem[]> Save(ProjectItem[] parents, byte[][] previewData, object[] obj, string[] nameOverrides, ProjectEventHandler<AssetItem[]> callback = null);
        ProjectAsyncOperation<AssetItem[]> SavePreview(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null);
        ProjectAsyncOperation<AssetItem[]> Duplicate(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null);

        ProjectAsyncOperation<UnityObject[]> Load(AssetItem[] assetItems, ProjectEventHandler<UnityObject[]> callback = null);
        ProjectAsyncOperation Unload(ProjectEventHandler completedCallback = null);

        ProjectAsyncOperation<ProjectItem> LoadImportItems(string path, bool isBuiltIn, ProjectEventHandler<ProjectItem> callback = null);
        void UnloadImportItems(ProjectItem importItemsRoot);
        ProjectAsyncOperation<AssetItem[]> Import(ImportItem[] importItems, ProjectEventHandler<AssetItem[]> callback = null);

        ProjectAsyncOperation<ProjectItem> CreateFolder(ProjectItem projectItem, ProjectEventHandler<ProjectItem> callback = null);
        ProjectAsyncOperation<ProjectItem> Rename(ProjectItem projectItem, string oldName, ProjectEventHandler<ProjectItem> callback = null);
        ProjectAsyncOperation<ProjectItem[], ProjectItem[]> Move(ProjectItem[] projectItems, ProjectItem target, ProjectEventHandler<ProjectItem[], ProjectItem[]> callback = null);
        ProjectAsyncOperation<ProjectItem[]> Delete(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback = null);

        ProjectAsyncOperation<string[]> GetAssetBundles(ProjectEventHandler<string[]> callback = null);
        Dictionary<int, string> GetStaticAssetLibraries();
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

    public static class IProjectExtensions
    {
        public static ProjectItem Get<T>(this IProject project, string path)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            return project.Root.Get(string.Format("{0}/{1}{2}", project.Root.Name, path, project.GetExt(typeof(T))));
        }

        public static bool Exist<T>(this IProject project, string path)
        {
            return project.Get<T>(path) != null;
        }

        public static ProjectAsyncOperation CreateFolder(this IProject project, string path)
        {
            ProjectItem folder = project.Root.Get(string.Format("{0}/{1}", project.Root.Name, path), true);
            return project.CreateFolder(folder);
        }

        public static ProjectAsyncOperation Save(this IProject project, string path, object obj)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            string name = Path.GetFileName(path);
            path = Path.GetDirectoryName(path).Replace(@"\", "/");
            path = string.Format("{0}/{1}", project.Root.Name, path);

            string ext = project.GetExt(obj.GetType());
            ProjectItem item = project.Root.Get(path + "/" + name + ext);
            if (item is AssetItem)
            {
                AssetItem assetItem = (AssetItem)item;
                return project.Save(new[] { assetItem }, new[] { obj });
            }

            ProjectItem folder = project.Root.Get(path);
            if (folder == null || !folder.IsFolder)
            {
                throw new ArgumentException("directory cannot be found", "path");
            }

            return project.Save(new[] { folder }, new[] { new byte[0] }, new[] { obj }, new[] { name });
        }

        public static ProjectAsyncOperation<UnityObject[]> Load<T>(this IProject project, string path)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            path = string.Format("{0}/{1}", project.Root.Name, path);

            AssetItem assetItem = project.Root.Get(path + project.GetExt(typeof(T))) as AssetItem;
            if (assetItem == null)
            {
                throw new ArgumentException("not found", "path");
            }

            return project.Load(new[] { assetItem });
        }
    }
}
