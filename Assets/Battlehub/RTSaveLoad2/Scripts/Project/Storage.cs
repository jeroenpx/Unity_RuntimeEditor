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
        void GetProject(string path, StorageEventHandler<ProjectInfo> callback);
        void GetFolders(string path, StorageEventHandler<ProjectItem> callback);
        
    }

    public class FileSystemStorage : IStorage
    {
        private string RootPath
        {
            get { return Application.persistentDataPath; }
        }

        private string FullPath(string path)
        {
            return Path.Combine(RootPath, path);
        }

        public void GetProject(string path, StorageEventHandler<ProjectInfo> callback)
        {
            path = Path.Combine(FullPath(path), "Project.rtmeta");
            ProjectInfo projectInfo;
            Error error = new Error();
            ISerializer serializer = RTSL2Deps.Get.Serializer;
            if (!File.Exists(path))
            {
                projectInfo = new ProjectInfo();
            }
            else
            {
                try
                {
                    using (FileStream fs = File.OpenRead(path))
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

        public void GetFolders(string path, StorageEventHandler<ProjectItem> callback)
        {
            path = FullPath(path);
            path = Path.Combine(path, "Assets");
            ProjectItem assets = new ProjectItem();
            assets.ItemID = 0;
            assets.Children = new List<ProjectItem>();
            assets.Name = "Assets";

            GetFolders(path, assets);

            callback(new Error(), assets);
        }

        private void GetFolders(string path, ProjectItem parent)
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
                string metaFile = dir + ".rtmeta";
                ProjectItem projectItem;
                if (File.Exists(metaFile))
                {
                    try
                    {
                        using (FileStream fs = File.OpenRead(metaFile))
                        {
                            projectItem = serializer.Deserialize<ProjectItem>(fs);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogErrorFormat("Unable to read meta file: {0} -> got exception: {1} ", metaFile, e.ToString());
                        projectItem = new ProjectItem();
                    }
                }
                else
                {
                    projectItem = new ProjectItem();
                }

                projectItem.Parent = parent;
                projectItem.Children = new List<ProjectItem>();
                projectItem.Name = Path.GetFileName(dir);
                parent.Children.Add(projectItem);

                GetFolders(dir, projectItem);
            }
        }
    }
}
