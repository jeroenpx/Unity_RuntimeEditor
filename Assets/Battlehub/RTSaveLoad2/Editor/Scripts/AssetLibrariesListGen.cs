using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    public class AssetLibrariesListGen
    {
        public static void CreateList()
        {
            string dir = RTSL2Path.UserRoot;
            string dataPath = Application.dataPath;

            if (!Directory.Exists(dataPath + dir))
            {
                Directory.CreateDirectory(dataPath + dir);
            }

            if (!Directory.Exists(dataPath + dir + "/" + RTSL2Path.LibrariesFolder))
            {
                AssetDatabase.CreateFolder("Assets" + dir, RTSL2Path.LibrariesFolder);
            }

            dir = dir + "/" + RTSL2Path.LibrariesFolder;
            if (!Directory.Exists(dataPath + dir + "/Resources"))
            {
                AssetDatabase.CreateFolder("Assets" + dir, "Resources");
            }

            dir = dir + "/Resources";
            string path = "Assets" + RTSL2Path.UserRoot + "/" + RTSL2Path.LibrariesFolder + "/Resources/RTSL2AssetLibrariesList.asset";

            AssetLibrariesListAsset asset = Create(path);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static AssetLibrariesListAsset Create(string path)
        {
            AssetLibrariesListAsset asset = AssetDatabase.LoadAssetAtPath<AssetLibrariesListAsset>(path);
            if(asset == null)
            {
                asset = ScriptableObject.CreateInstance<AssetLibrariesListAsset>();
                asset.List = new List<AssetLibraryListEntry>();
            }

            Dictionary<AssetLibraryAsset, int> assetLibToOrdinal = new Dictionary<AssetLibraryAsset, int>();
            for(int i = 0; i < asset.List.Count; ++i)
            {
                AssetLibraryListEntry entry = asset.List[i];
                if(entry.Library != null)
                {
                    if(!assetLibToOrdinal.ContainsKey(entry.Library))
                    {
                        assetLibToOrdinal.Add(entry.Library, entry.Ordinal);
                    }
                }
            }
            
            string[] assetLibraries = AssetDatabase.FindAssets("t:AssetLibraryAsset");
            for(int i = 0; i < assetLibraries.Length; ++i)
            {
                //string assetLib = assetLibraries[i];

                //assetLib = AssetDatabase.GUIDToAssetPath(assetLib);

                //if(assetLib.StartsWith("Assets" + RTSL2Path.UserRoot + "/" + RTSL2Path.LibrariesFolder + "/Resources/Scenes/"))
                //{
                //    continue;
                //}

                //if (assetLib.StartsWith("Assets" + RTSL2Path.UserRoot + "/" + RTSL2Path.LibrariesFolder + "/Resources/BuiltInAssets"))
                //{
                //    continue;
                //}

                //if(assetLib.Contains("/Resources/"))
                //{
                //    Debug.LogWarning("Move " + assetLib + " to Resources folder");
                //    continue;
                //}

                //int index = assetLib.IndexOf("/Resources/");
                //assetLib = assetLib.Remove(0, index + 11);

                

                //asset.List.Add(entry);

                //Debug.Log(assetLib);
            }

            return asset;
        }
    }
}
