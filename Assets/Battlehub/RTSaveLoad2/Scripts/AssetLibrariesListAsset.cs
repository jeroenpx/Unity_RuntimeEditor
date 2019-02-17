using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [Serializable]
    public struct AssetLibraryListEntry
    {
        public int Ordinal;
        public AssetLibraryAsset Library;
    }

    public class AssetLibrariesListAsset : ScriptableObject
    {
        [SerializeField]
        public int Identity;

        [SerializeField]
        public List<AssetLibraryListEntry> List;
    }
}

