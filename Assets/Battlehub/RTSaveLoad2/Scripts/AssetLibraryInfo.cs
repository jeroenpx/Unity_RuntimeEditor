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
        public int PersistentID;
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
        public const int ORDINAL_OFFSET = 16;
        public const int MAX_ASSETS = 1 << ORDINAL_OFFSET;

        public int Identity = 0;

        public List<AssetFolderInfo> Folders;

        public bool Contains(UnityObject obj)
        {
            AssetFolderInfo folder;
            AssetInfo asset;
            return TryGetAssetInfo(obj, out folder, out asset);
        }

        public bool TryGetAssetInfo(UnityObject obj, out AssetFolderInfo resultFolder, out AssetInfo resultAsset)
        {
            for(int i = 0; i < Folders.Count; ++i)
            {
                AssetFolderInfo folder = Folders[i];
                if(folder != null && TryGetAssetInfo(folder, obj, out resultFolder, out resultAsset))
                {
                    return true;
                }
            }
            resultAsset = null;
            resultFolder = null;
            return false;
        }

        private bool TryGetAssetInfo(AssetFolderInfo folder, UnityObject obj, out AssetFolderInfo resultFolder, out AssetInfo resultAsset)
        {
            if(folder.Assets != null)
            {
                for(int i = 0; i < folder.Assets.Count; ++i)
                {
                    AssetInfo asset = folder.Assets[i];
                    if(asset.Object == obj)
                    {
                        resultFolder = folder;
                        resultAsset = asset;
                        return true;
                    }
                }
            }

            if(folder.hasChildren)
            {
                for(int i = 0; i < folder.children.Count; ++i)
                {
                    AssetFolderInfo subfolder = (AssetFolderInfo)folder.children[i];
                    if(TryGetAssetInfo(subfolder, obj, out resultFolder, out resultAsset))
                    {
                        return true;
                    }   
                }
            }

            resultAsset = null;
            resultFolder = null;
            return false;
        }
    }
}
