using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    public delegate void LoadAssetBundleHandler(AssetBundle bundle);
    public delegate void ListAssetBundlesHandler(string[] bundleNames);
    public interface IAssetBundleLoader
    {
        void ListAssetBundles(ListAssetBundlesHandler assetBundles);
        void Load(string name, LoadAssetBundleHandler callback);
    }

    public class AssetBundleLoader : IAssetBundleLoader
    {
        public void ListAssetBundles(ListAssetBundlesHandler callback)
        {
            List<string> result = new List<string>();

            string[] manifestFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.manifest");
            for(int i = 0; i < manifestFiles.Length; ++i)
            {
                string assetBundleFile = Path.GetDirectoryName(manifestFiles[i]) + "/" + Path.GetFileNameWithoutExtension(manifestFiles[i]);
                if(File.Exists(assetBundleFile))
                {
                    result.Add(Path.GetFileName(assetBundleFile));
                }
            }

            callback(result.ToArray());
        }

        public void Load(string name, LoadAssetBundleHandler callback)
        {
            if (!File.Exists(Application.streamingAssetsPath + "/" + name))
            {
                callback(null);
                return;
            }
            AssetBundle bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + name);
            if (callback != null)
            {
                callback(bundle);
            }
        }
    }
}