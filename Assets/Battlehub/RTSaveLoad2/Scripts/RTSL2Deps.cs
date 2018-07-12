using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    public interface IRTSL2Deps
    {
        IIDMap IDMap
        {
            get;
        }

        IAssetDB AssetDB
        {
            get;
        }
    }

    [DefaultExecutionOrder(-1)]
    public class RTSL2Deps : MonoBehaviour, IRTSL2Deps
    {
        public static IRTSL2Deps Get
        {
            get;
            private set;
        }

        private IAssetDB m_assetDB;
        public IIDMap IDMap
        {
            get { return m_assetDB; }
        }

        public IAssetDB AssetDB
        {
            get { return m_assetDB; }
        }
      
        protected virtual void Awake()
        {
            if(Get != null)
            {
                Destroy(((MonoBehaviour)Get).gameObject);
                Debug.LogWarning("Another instance of RTSL2Deps exist");
            }
            Get = this;
            m_assetDB = new AssetDB();
        }

        protected virtual void OnDestroy()
        {
            if(this == (MonoBehaviour)Get)
            {
                Get = null;
                m_assetDB = null;
            }
        }


    }
}