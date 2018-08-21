using System;

namespace Battlehub.RTSaveLoad2
{
    


    public delegate void StorageEventHandler(Error error);
    public delegate void StorageEventHandler<T>(Error error, T data);

    public interface IStorage
    {
        void GetProject(string path, StorageEventHandler<ProjectInfo> callback);
        void GetFolders(string path, StorageEventHandler<ProjectItem[]> callback);
        
    }

    public class FileSystemStorage : IStorage
    {
        public void GetProject(string path, StorageEventHandler<ProjectInfo> callback)
        {
            throw new NotImplementedException();
        }
        public void GetFolders(string path, StorageEventHandler<ProjectItem[]> callback)
        {
            throw new NotImplementedException();
        }
    }
}
