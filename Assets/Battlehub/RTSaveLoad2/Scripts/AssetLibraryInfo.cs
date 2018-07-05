using Battlehub.RTCommon.EditorTreeView;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    [Serializable]
    public class AssetInfo : TreeElement
    {
        [NonSerialized]
        [HideInInspector]
        public AssetFolderInfo Folder;
        public UnityObject Object;
        [ReadOnly]
        public int PersistentId;
        public bool IsEnabled;

        public AssetInfo()
        {

        }

        public AssetInfo(string name, int depth, int id) : base(name, depth, id)
        {
        }
    }

    [Serializable]
    public class AssetFolderInfo : TreeElement, ISerializationCallbackReceiver
    {
        public List<AssetInfo> Assets;
        public bool IsEnabled;

        public AssetFolderInfo()
        {

        }

        public AssetFolderInfo(string name, int depth, int id) : base(name, depth, id)
        {
        }

        public void OnAfterDeserialize()
        {
            for(int i = 0; i < Assets.Count; ++i)
            {
                AssetInfo assetInfo = Assets[i];
                if(assetInfo != null)
                {
                    assetInfo.Folder = this;
                }
            }
        }

        public void OnBeforeSerialize()
        {
            
        }
    }

    [Serializable]
    public class AssetLibraryInfo : TreeElement
    {
        public List<AssetFolderInfo> Folders;
    }
}
