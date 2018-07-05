using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [CreateAssetMenu(fileName = "AssetLibrary", menuName = "RT Asset Library", order = 1)]
    public class AssetLibraryAsset : ScriptableObject
    {
        [SerializeField] AssetLibraryInfo m_assetLibrary;

        public AssetLibraryInfo AssetLibrary
        {
            get { return m_assetLibrary; }
            set { m_assetLibrary = value; }
        }

        private void Awake()
        {
            if (m_assetLibrary == null || m_assetLibrary.Folders.Count == 0)
            {
                AssetFolderInfo assetsFolder = new AssetFolderInfo
                {
                    name = "Assets",
                    depth = -1,
                };

                m_assetLibrary = new AssetLibraryInfo
                {
                    name = "Root",
                    depth = -1,
                    Folders = new List<AssetFolderInfo>
                    {
                        assetsFolder
                    }
                };
            }
          
        }
    }
}
