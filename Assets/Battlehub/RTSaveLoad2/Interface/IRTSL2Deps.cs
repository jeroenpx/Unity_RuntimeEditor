using UnityEngine;

namespace Battlehub.RTSaveLoad2.Interface
{
    public interface IRTSL2Deps
    {
        IProject Project
        {
            get;
        }
    }

    [DefaultExecutionOrder(-100)]
    public class RTSL2Deps : MonoBehaviour, IRTSL2Deps
    {
        private IProject m_project;
        public IProject Project
        {
            get { return m_project; }
        }

        public static IRTSL2Deps Get
        {
            get;
            private set;
        }

        protected virtual void Awake()
        {
            if (Get != null)
            {
                Destroy(((MonoBehaviour)Get).gameObject);
                Debug.LogWarning("Another instance of RTSL2Deps exist");
            }
            Get = this;
            m_project = FindObjectOfType<Project>();
        }

        protected virtual void OnDestroy()
        {
            if (this == (MonoBehaviour)Get)
            {
                Get = null;
                m_project = null;
            }
        }
    }
}

