using Battlehub.RTCommon.EditorTreeView;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    [SerializeField]
    public class AssetInfo : TreeElement
    {
        public UnityObject Object;
        [ReadOnly]
        public int PersistentId;
    }

    [Serializable]
    public class AssetFolderInfo : TreeElement
    {
        public List<AssetInfo> Assets;
        public bool IsEnabled;

        public AssetFolderInfo()
        {

        }

        public AssetFolderInfo(string name, int depth, int id) : base(name, depth, id)
        {
        }
    }

    [Serializable]
    public class AssetLibraryInfo : TreeElement
    {
        public List<AssetFolderInfo> Folders;
    }
}
