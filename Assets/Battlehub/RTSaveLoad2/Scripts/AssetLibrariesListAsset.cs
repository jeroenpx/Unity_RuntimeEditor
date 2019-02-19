using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [Serializable]
    public struct AssetLibraryListEntry
    {
        public string Library;
        public int Ordinal;
    }

    public class AssetLibrariesListAsset : ScriptableObject
    {
        [SerializeField]
        public int Identity;

        [SerializeField]
        public List<AssetLibraryListEntry> List;
    }
}

