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

        void Create(string path, Action callback);

        void Open(string path, Action callback);

        void Load(ProjectItem project, Action<object> callback);
    }


    public class Project : MonoBehaviour, IProject
    {
        private ProjectInfo m_project;

        public ProjectItem Root
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Create(string path, Action callback)
        {
            throw new NotImplementedException();
        }

        public void Load(ProjectItem project, Action<object> callback)
        {
            throw new NotImplementedException();
        }

        public void Open(string path, Action callback)
        {
            throw new NotImplementedException();
        }
    }
}

