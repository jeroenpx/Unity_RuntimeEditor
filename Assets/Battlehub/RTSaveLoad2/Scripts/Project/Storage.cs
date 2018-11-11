using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Battlehub.RTSaveLoad2.Interface;
using Battlehub.RTCommon;

namespace Battlehub.RTSaveLoad2
{
    public delegate void StorageEventHandler(Error error);
    public delegate void StorageEventHandler<T>(Error error, T data);

    public interface IStorage
    {
        void GetProject(string projectPath, StorageEventHandler<ProjectInfo> callback);
        void GetProjectTree(string projectPath, StorageEventHandler<ProjectItem> callback);
        void GetPreviews(string projectPath, string[] folderPath, StorageEventHandler<Preview[][]> callback);
        void Save(string projectPath, string[] folderPaths, AssetItem[] assetItems, PersistentObject[] persistentObjects, ProjectInfo projectInfo, StorageEventHandler callback);
        void Load(string projectPath, string[] assetPaths, Type[] types, StorageEventHandler<PersistentObject[]> callback);
    }

    public class FileSystemStorage : IStorage
    {
        private const string MetaExt = ".rtmeta";
        private const string PreviewExt = ".rtview";

        private string RootPath
        {
            get { return Application.persistentDataPath + "/"; }
        }

        private string FullPath(string path)
        {
            return RootPath + path;
        }

        private string AssetsFolderPath(string path)
        {
            return RootPath + path + "/Assets";
        }

        public FileSystemStorage()
        {
            Debug.LogFormat("RootPath : {0}", RootPath);
        }

        public void GetProject(string projectPath, StorageEventHandler<ProjectInfo> callback)
        {
            projectPath = FullPath(projectPath) + "/Project.rtmeta";
            ProjectInfo projectInfo;
            Error error = new Error();
            ISerializer serializer = IOC.Resolve<ISerializer>();
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
            if(!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }
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

            string fileNameWithoutMetaExt = Path.GetFileNameWithoutExtension(path);
            item.Name = Path.GetFileNameWithoutExtension(fileNameWithoutMetaExt);
            item.Ext = Path.GetExtension(fileNameWithoutMetaExt);

            return item;
        }
       
        private static T Load<T>(ISerializer serializer, string path) where T : new()
        {
            string metaFile = path;
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

            ISerializer serializer = IOC.Resolve<ISerializer>();
            string[] dirs = Directory.GetDirectories(path);
            for (int i = 0; i < dirs.Length; ++i)
            {
                string dir = dirs[i];
                ProjectItem projectItem = LoadItem<ProjectItem>(serializer, dir + MetaExt);

                projectItem.Parent = parent;
                projectItem.Children = new List<ProjectItem>();
                parent.Children.Add(projectItem);

                GetProjectTree(dir, projectItem);
            }

            string[] files = Directory.GetFiles(path, "*" + MetaExt);
            for(int i = 0; i < files.Length; ++i)
            {
                string file = files[i];
                if(!File.Exists(file.Replace(MetaExt, string.Empty)))
                {
                    continue;
                }

                AssetItem assetItem =  LoadItem<AssetItem>(serializer, file);
                assetItem.Parent = parent;
                parent.Children.Add(assetItem);
            }
        }

        public void GetPreviews(string projectPath, string[] folderPath, StorageEventHandler<Preview[][]> callback)
        {
            projectPath = AssetsFolderPath(projectPath);

            ISerializer serializer = IOC.Resolve<ISerializer>();
            Preview[][] result = new Preview[folderPath.Length][];
            for (int i = 0; i < folderPath.Length; ++i)
            {
                string path = projectPath + folderPath[i];
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

        public void Save(string projectPath, string[] folderPaths, AssetItem[] assetItems, PersistentObject[] persistentObjects, ProjectInfo projectInfo, StorageEventHandler callback)
        {
            if(assetItems.Length != persistentObjects.Length || persistentObjects.Length != folderPaths.Length)
            {
                throw new ArgumentException("assetItems");
            }

            projectPath = FullPath(projectPath);
            string projectInfoPath = projectPath + "/Project.rtmeta";
            ISerializer serializer = IOC.Resolve<ISerializer>();
            Error error = new Error(Error.OK);
            for(int i = 0; i < assetItems.Length; ++i)
            {
                string folderPath = folderPaths[i];
                AssetItem assetItem = assetItems[i];
                PersistentObject persistentObject = persistentObjects[i];
                try
                {
                    string path = projectPath + folderPath;
                    string previewPath = path + "/" + assetItem.NameExt + PreviewExt;
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

                    using (FileStream fs = File.OpenWrite(path + "/" + assetItem.NameExt + MetaExt))
                    {
                        serializer.Serialize(assetItem, fs);
                    }
                    using (FileStream fs = File.OpenWrite(path + "/" + assetItem.NameExt))
                    {
                        serializer.Serialize(persistentObject, fs);
                    }
                    using (FileStream fs = File.OpenWrite(projectInfoPath))
                    {
                        serializer.Serialize(projectInfo, fs);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Unable to create asset: {0} -> got exception: {1} ", assetItem.NameExt, e.ToString());
                    error.ErrorCode = Error.E_Exception;
                    error.ErrorText = e.ToString();
                    break;
                }
            }

            callback(error);
        }

        public void Load(string projectPath, string[] assetPaths, Type[] types, StorageEventHandler<PersistentObject[]> callback)
        {
            PersistentObject[] result = new PersistentObject[assetPaths.Length];
            for(int i = 0; i < assetPaths.Length; ++i)
            {
                string assetPath = assetPaths[i];
                assetPath = FullPath(projectPath) + assetPath;
                ISerializer serializer = IOC.Resolve<ISerializer>();
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
