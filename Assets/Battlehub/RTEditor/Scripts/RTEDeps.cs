using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [DefaultExecutionOrder(-100)]
    public class RTEDeps : MonoBehaviour
    {
        private IResourcePreviewUtility m_resourcePreview;
        private IRTEAppearance m_rteAppearance;
        private IRTE m_rte;

        protected virtual IResourcePreviewUtility ResourcePreview
        {
            get
            {
                IResourcePreviewUtility resourcePreviewUtility = FindObjectOfType<ResourcePreviewUtility>();
                if (resourcePreviewUtility == null)
                {
                    resourcePreviewUtility = gameObject.AddComponent<ResourcePreviewUtility>();
                }
                return resourcePreviewUtility;
            }
        }

        protected virtual IRTEAppearance RTEAppearance
        {
            get
            {
                IRTEAppearance rteAppearance = FindObjectOfType<RTEAppearance>();
                if(rteAppearance == null)
                {
                    rteAppearance = gameObject.AddComponent<RTEAppearance>();
                }
                return rteAppearance;
            }
        }

        protected virtual IRTE RTE
        {
            get
            {
                IRTE rte = FindObjectOfType<RuntimeEditor>();
                if(rte == null)
                {
                    rte = gameObject.AddComponent<RuntimeEditor>();
                }
                return rte;
            }
        }


        private void Awake()
        {
            if (m_instance != null)
            {
                Debug.LogWarning("AnotherInstance of RTSL2 exists");
            }
            m_instance = this;
            AwakeOverride();
        }

        protected virtual void AwakeOverride()
        {
            m_rte = RTE;
            m_resourcePreview = ResourcePreview;
            m_rteAppearance = RTEAppearance;
        }

        private void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
            }

            OnDestroyOverride();
            m_resourcePreview = null;
            m_rteAppearance = null;
            m_rte = null;
        }

        protected virtual void OnDestroyOverride()
        {

        }


        private static RTEDeps m_instance;
        private static RTEDeps Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<RTEDeps>();
                    if(m_instance == null)
                    {
                        GameObject go = new GameObject("RTEDeps");
                        go.AddComponent<RTEDeps>();
                    }
                }
                return m_instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            IOC.Register(() => Instance.m_rte);
            IOC.RegisterFallback(() => Instance.m_resourcePreview);
            IOC.RegisterFallback(() => Instance.m_rteAppearance);
        }
    }
}

