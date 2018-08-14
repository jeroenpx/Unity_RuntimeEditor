using System;

namespace Battlehub.RTSaveLoad2
{
    


    public delegate void StorageEventHandler(Error error);
    public delegate void StorageEventHandler<T>(Error error, T data);

    public interface IStorage
    {
        void GetFolders(string path, StorageEventHandler<ProjectItem[]> callback);
    }

    public class FileSystemStorage : IStorage
    {
        public void GetFolders(string path, StorageEventHandler<ProjectItem[]> callback)
        {
            throw new NotImplementedException();
        }
    }
}
