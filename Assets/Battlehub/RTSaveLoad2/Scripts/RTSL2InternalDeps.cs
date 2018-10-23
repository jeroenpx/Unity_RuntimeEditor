using UnityEngine;

using Battlehub.RTSaveLoad2.Interface;

namespace Battlehub.RTSaveLoad2
{
    public interface IRTSL2InternalDeps : IRTSL2Deps
    {
        IIDMap IDMap
        {
            get;
        }

        IAssetDB AssetDB
        {
            get;
        }

      
        IUnityObjectFactory UnityObjFactory
        {
            get;
        }

        ISerializer Serializer
        {
            get;
        }

        IStorage Storage
        {
            get;
        }

        ITypeMap TypeMap
        {
            get;
        }
    }

    [DefaultExecutionOrder(-1)]
    public class RTSL2InternalDeps : RTSL2Deps, IRTSL2InternalDeps
    {
        public new static IRTSL2InternalDeps Get
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

        private ITypeMap m_typeMap;
        public ITypeMap TypeMap
        {
            get { return m_typeMap; }
        }

        private IUnityObjectFactory m_unityObjFactory;
        public IUnityObjectFactory UnityObjFactory
        {
            get { return m_unityObjFactory; }
        }

        private ISerializer m_serializer;
        public ISerializer Serializer
        {
            get { return m_serializer; }
        }

        private IStorage m_storage;
        public IStorage Storage
        {
            get { return m_storage; }
        }

        protected override void Awake()
        {
            base.Awake();

            Get = this;
            m_assetDB = new AssetDB();
            m_typeMap = new TypeMap();
            m_unityObjFactory = new UnityObjectFactory();
            m_serializer = new ProtobufSerializer();
            m_storage = new FileSystemStorage();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(this == (MonoBehaviour)Get)
            {
                Get = null;
                m_assetDB = null;
                m_typeMap = null;
                m_unityObjFactory = null;
                m_serializer = null;
                m_storage = null;
            }
        }


    }
}