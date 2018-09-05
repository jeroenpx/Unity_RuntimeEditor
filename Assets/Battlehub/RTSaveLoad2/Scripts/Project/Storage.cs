using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    


    public delegate void StorageEventHandler(Error error);
    public delegate void StorageEventHandler<T>(Error error, T data);

    public interface IStorage
    {
        void GetProject(string projectPath, StorageEventHandler<ProjectInfo> callback);
        void GetProjectTree(string projectPath, StorageEventHandler<ProjectItem> callback);
        void GetPreviews(string projectPath, string[] folderPath, StorageEventHandler<Preview[][]> callback);
        void Save(string projectPath, string folderPath, AssetItem assetItem, PersistentObject persistentObject, ProjectInfo projectInfo, StorageEventHandler callback);
        void Load(string projectPath, string[] assetPaths, Type[] types, StorageEventHandler<PersistentObject[]> callback);
    }

    public class FileSystemStorage : IStorage
    {
        private const string MetaExt = ".rtmeta";
        private const string PreviewExt = ".rtview";

        private string RootPath
        {
            get { return Application.persistentDataPath; }
        }

        private string FullPath(string path)
        {
            return Path.Combine(RootPath, path);
        }

        private string AssetsFolderPath(string path)
        {
            return Path.Combine(Path.Combine(RootPath, path), "Assets");
        }

        public void GetProject(string projectPath, StorageEventHandler<ProjectInfo> callback)
        {
            projectPath = Path.Combine(FullPath(projectPath), "Project.rtmeta");
            ProjectInfo projectInfo;
            Error error = new Error();
            ISerializer serializer = RTSL2Deps.Get.Serializer;
            if (!File.Exists(projectPath))
            {
                projectInfo = new ProjectInfo();
            }
            else
            {
                try
                {
                    using (FileStream fs = File.OpenRead(projectPath))
                    {
                        projectInfo = serializer.Deserialize<ProjectInfo>(fs);
                    }       
                }
                catch (Exception e)
                {
                    projectInfo = new ProjectInfo();
                    error.ErrorCode = Error.E_Exception;
                    error.ErrorText = e.ToString();
                }
            }
            callback(error, projectInfo);
        }

        public void GetProjectTree(string projectPath, StorageEventHandler<ProjectItem> callback)
        {
            projectPath = AssetsFolderPath(projectPath);

            ProjectItem assets = new ProjectItem();
            assets.ItemID = 0;
            assets.Children = new List<ProjectItem>();
            assets.Name = "Assets";

            GetProjectTree(projectPath, assets);

            callback(new Error(), assets);
        }

        private static T LoadItem<T>(ISerializer serializer, string path) where T : ProjectItem, new()
        {
            T item = Load<T>(serializer, path);
            item.Name = Path.GetFileNameWithoutExtension(path);
            item.Ext = Path.GetExtension(path);
            return item;
        }
       
        private static T Load<T>(ISerializer serializer, string path) where T : new()
        {
            string metaFile = path + MetaExt;
            T item;
            if (File.Exists(metaFile))
            {
                try
                {
                    using (FileStream fs = File.OpenRead(metaFile))
                    {
                        item = serializer.Deserialize<T>(fs);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Unable to read meta file: {0} -> got exception: {1} ", metaFile, e.ToString());
                    item = new T();
                }
            }
            else
            {
                item = new T();
            }
         
            return item;
        }

        private void GetProjectTree(string path, ProjectItem parent)
        {
            if(!Directory.Exists(path))
            {
                return;
            }

            ISerializer serializer = RTSL2Deps.Get.Serializer;
            string[] dirs = Directory.GetDirectories(path);
            for (int i = 0; i < dirs.Length; ++i)
            {
                string dir = dirs[i];
                ProjectItem projectItem = LoadItem<ProjectItem>(serializer, dir);

                projectItem.Parent = parent;
                projectItem.Children = new List<ProjectItem>();
                parent.Children.Add(projectItem);

                GetProjectTree(dir, projectItem);
            }

            string[] files = Directory.GetFiles(path, "*" + MetaExt);
            for(int i = 0; i < files.Length; ++i)
            {
                string file = files[i];
                AssetItem assetItem =  LoadItem<AssetItem>(serializer, file);

                assetItem.Parent = parent;
                parent.Children.Add(assetItem);
            }
        }

        public void GetPreviews(string projectPath, string[] folderPath, StorageEventHandler<Preview[][]> callback)
        {
            projectPath = AssetsFolderPath(projectPath);

            ISerializer serializer = RTSL2Deps.Get.Serializer;
            Preview[][] result = new Preview[folderPath.Length][];
            for (int i = 0; i < folderPath.Length; ++i)
            {
                string path = Path.Combine(projectPath, folderPath[i]);
                if (!Directory.Exists(path))
                {
                    continue;
                }

                string[] files = Directory.GetFiles(path, "*" + PreviewExt);
                Preview[] previews = new Preview[files.Length];
                for(int j = 0; j < files.Length; ++j)
                {
                    previews[j] = Load<Preview>(serializer, files[j]);
                }

                result[i] = previews;
            }

            callback(new Error(), result);
        }

        public void Save(string projectPath, string folderPath, AssetItem assetItem, PersistentObject persistentObject, ProjectInfo projectInfo, StorageEventHandler callback)
        {
            string projectInfoPath = Path.Combine(FullPath(projectPath), "Project.rtmeta");
            projectPath = AssetsFolderPath(projectPath);
            ISerializer serializer = RTSL2Deps.Get.Serializer;
            Error error = new Error(Error.OK);
            try
            {
                string path = Path.Combine(projectPath, folderPath);
                string previewPath = Path.Combine(path, assetItem.NameExt + PreviewExt);
                if (assetItem.Preview == null)
                {
                    File.Delete(previewPath);
                }
                else
                {
                    using (FileStream fs = File.OpenWrite(previewPath))
                    {
                        serializer.Serialize(assetItem.Preview, fs);
                    }
                }

                using (FileStream fs = File.OpenWrite(Path.Combine(path, assetItem.NameExt + MetaExt)))
                {
                    serializer.Serialize(assetItem, fs);
                }
                using (FileStream fs = File.OpenWrite(Path.Combine(path, assetItem.NameExt)))
                {
                    serializer.Serialize(persistentObject, fs);
                }
                using (FileStream fs = File.OpenWrite(projectInfoPath))
                {
                    serializer.Serialize(projectInfo, fs);
                }
            }
            catch(Exception e)
            {
                Debug.LogErrorFormat("Unable to create asset: {0} -> got exception: {1} ", assetItem.NameExt, e.ToString());
                error.ErrorCode = Error.E_Exception;
                error.ErrorText = e.ToString();
            }

            callback(error);
        }

        public void Load(string projectPath, string[] assetPaths, Type[] types, StorageEventHandler<PersistentObject[]> callback)
        {
            PersistentObject[] result = new PersistentObject[assetPaths.Length];
            for(int i = 0; i < assetPaths.Length; ++i)
            {
                string assetPath = assetPaths[i];
                assetPath = Path.Combine(FullPath(projectPath), assetPath);
                ISerializer serializer = RTSL2Deps.Get.Serializer;
                try
                {
                    if (File.Exists(assetPath))
                    {
                        using (FileStream fs = File.OpenRead(assetPath))
                        {
                            result[i] = (PersistentObject)serializer.Deserialize(fs, types[i]);
                        }
                    }
                    else
                    {
                        callback(new Error(Error.E_NotFound), new PersistentObject[0]);
                        return;
                    }

                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Unable to load asset: {0} -> got exception: {1} ", assetPath, e.ToString());
                    callback(new Error(Error.E_Exception) { ErrorText = e.ToString() }, new PersistentObject[0]);
                    return;
                }
            }

            callback(new Error(Error.OK), result);
        }
    }
}
