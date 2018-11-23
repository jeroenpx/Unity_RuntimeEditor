using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Battlehub.RTSaveLoad2.Interface;
using Battlehub.RTCommon;
using System.Linq;

namespace Battlehub.RTSaveLoad2
{
    public delegate void StorageEventHandler(Error error);
    public delegate void StorageEventHandler<T>(Error error, T data);
    public delegate void StorageEventHandler<T, T2>(Error error, T data, T2 data2);

    public interface IStorage
    {
        void GetProject(string projectPath, StorageEventHandler<ProjectInfo, AssetBundleInfo[]> callback);
        void GetProjectTree(string projectPath, StorageEventHandler<ProjectItem> callback);
        void GetPreviews(string projectPath, string[] folderPath, StorageEventHandler<Preview[][]> callback);
        void Save(string projectPath, string[] folderPaths, AssetItem[] assetItems, PersistentObject[] persistentObjects, ProjectInfo projectInfo, StorageEventHandler callback);
        void Save(string projectPath, AssetBundleInfo assetBundleInfo, ProjectInfo project, StorageEventHandler callback);
        void Load(string projectPath, string[] assetPaths, Type[] types, StorageEventHandler<PersistentObject[]> callback);
        void Load(string projectPath, string bundleName, StorageEventHandler<AssetBundleInfo> callback);
        void Delete(string projectPath, string[] paths, StorageEventHandler callback);
        void Move(string projectPath, string[] paths, string[] names, string targetPath, StorageEventHandler callback);
        void Rename(string projectPath, string[] paths, string[] oldNames, string[] names, StorageEventHandler callback);
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

        public void GetProject(string projectPath, StorageEventHandler<ProjectInfo, AssetBundleInfo[]> callback)
        {
            projectPath = FullPath(projectPath) + "/Project.rtmeta";
            ProjectInfo projectInfo;
            Error error = new Error();
            ISerializer serializer = IOC.Resolve<ISerializer>();
            AssetBundleInfo[] result = new AssetBundleInfo[0];
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

                    string[] files = Directory.GetFiles(projectPath).Where(fn => fn.EndsWith(".rtbundle")).ToArray();
                    result = new AssetBundleInfo[files.Length];

                    for (int i = 0; i < result.Length; ++i)
                    {
                        using (FileStream fs = File.OpenRead(files[i]))
                        {
                            result[i] = serializer.Deserialize<AssetBundleInfo>(fs);
                        }
                    }
                }
                catch (Exception e)
                {
                    projectInfo = new ProjectInfo();
                    error.ErrorCode = Error.E_Exception;
                    error.ErrorText = e.ToString();
                }
            }

            callback(error, projectInfo, result);
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
            projectPath = FullPath(projectPath);

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
                    if(!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

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

        public void Save(string projectPath, AssetBundleInfo assetBundleInfo, ProjectInfo projectInfo, StorageEventHandler callback)
        {
            projectPath = FullPath(projectPath);
            string projectInfoPath = projectPath + "/Project.rtmeta";

            string assetBundlePath = assetBundleInfo.UniqueName.Replace("/", "_").Replace("\\", "_");
            assetBundlePath += ".rtbundle";
            assetBundlePath = projectPath + "/" + assetBundlePath;

            ISerializer serializer = IOC.Resolve<ISerializer>();

            using (FileStream fs = File.OpenWrite(assetBundlePath))
            {
                serializer.Serialize(assetBundleInfo, fs);
            }

            using (FileStream fs = File.OpenWrite(projectInfoPath))
            {
                serializer.Serialize(projectInfo, fs);
            }

            callback(new Error(Error.OK));
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

        public void Load(string projectPath, string bundleName, StorageEventHandler<AssetBundleInfo> callback)
        {
            string assetBundleInfoPath = bundleName.Replace("/", "_").Replace("\\", "_");
            assetBundleInfoPath += ".rtbundle";
            assetBundleInfoPath = projectPath + "/" + assetBundleInfoPath;

            ISerializer serializer = IOC.Resolve<ISerializer>();
            if (File.Exists(assetBundleInfoPath))
            {
                using (FileStream fs = File.OpenRead(assetBundleInfoPath))
                {
                    callback(new Error(Error.OK), serializer.Deserialize<AssetBundleInfo>(fs));
                }
            }
            else
            {
                callback(new Error(Error.E_NotFound), null);
                return;
            }
        }



        public void Delete(string projectPath, string[] paths, StorageEventHandler callback)
        {
            string fullPath = FullPath(projectPath);
            for(int i = 0; i < paths.Length; ++i)
            {
                string path = fullPath + paths[i];
                if(File.Exists(path))
                {
                    File.Delete(path);
                    if(File.Exists(path + MetaExt))
                    {
                        File.Delete(path + MetaExt);
                    }
                    if(File.Exists(path + PreviewExt))
                    {
                        File.Delete(path + PreviewExt);
                    }
                }
                else if(Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }

            callback(new Error(Error.OK));
        }

        public void Rename(string projectPath, string[] paths, string[] oldNames, string[] names, StorageEventHandler callback)
        {
            string fullPath = FullPath(projectPath);
            for (int i = 0; i < paths.Length; ++i)
            {
                string path = fullPath + paths[i] + "/" + oldNames[i];
                if (File.Exists(path))
                {
                    File.Move(path, fullPath + paths[i] + "/" + names[i]);
                    if (File.Exists(path + MetaExt))
                    {
                        File.Move(path + MetaExt, fullPath + paths[i] + "/" + names[i] + MetaExt);
                    }
                    if (File.Exists(path + PreviewExt))
                    {
                        File.Move(path + PreviewExt, fullPath + paths[i] + "/" + names[i] + PreviewExt);
                    }
                }
                else if (Directory.Exists(path))
                {
                    Directory.Move(path, fullPath + paths[i] + "/" + names[i]);
                }
            }

            callback(new Error(Error.OK));
        }

        public void Move(string projectPath, string[] paths, string[] names, string targetPath, StorageEventHandler callback)
        {
            string fullPath = FullPath(projectPath);
            for (int i = 0; i < paths.Length; ++i)
            {
                string path = fullPath + paths[i] + "/" + names[i];
                if (File.Exists(path))
                {
                    File.Move(path, fullPath + targetPath + "/" + names[i]);
                    if (File.Exists(path + MetaExt))
                    {
                        File.Move(path + MetaExt, fullPath + targetPath + "/" + names[i] + MetaExt);
                    }
                    if (File.Exists(path + PreviewExt))
                    {
                        File.Move(path + PreviewExt, fullPath + targetPath + "/" + names[i] + PreviewExt);
                    }
                }
                else if (Directory.Exists(path))
                {
                    Directory.Move(path, fullPath + targetPath + "/" + names[i]);
                }
            }

            callback(new Error(Error.OK));
        }
    }
}
