using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface IRTEDeps
    {
        IResourcePreviewUtility ResourcePreview
        {
            get;
        }
    }

    [DefaultExecutionOrder(-100)]
    public class RTEDeps : MonoBehaviour, IRTEDeps
    {
        public static IRTEDeps Get
        {
            get;
            private set;
        }

        private IResourcePreviewUtility m_resourcePreview;
        public IResourcePreviewUtility ResourcePreview
        {
            get { return m_resourcePreview; }
        }

        protected virtual void Awake()
        {
            if (Get != null)
            {
                Destroy(((MonoBehaviour)Get).gameObject);
                Debug.LogWarning("Another instance of RTSL2Deps exist");
            }
            Get = this;
            m_resourcePreview = FindObjectOfType<ResourcePreviewUtility>();
        }

        protected virtual void OnDestroy()
        {
            if (this == (MonoBehaviour)Get)
            {
                Get = null;
                m_resourcePreview = null;
            }
        }
    }

}

