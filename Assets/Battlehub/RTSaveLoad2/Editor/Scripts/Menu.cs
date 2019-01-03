using Battlehub.RTCommon;
using Battlehub.RTCommon.EditorTreeView;
using Battlehub.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    public static class Menu
    {
        private static HashSet<UnityObject> ReadFromAssetLibraries(Scene scene, out int index, out AssetLibraryAsset asset, out AssetFolderInfo folder)
        {
            HashSet<UnityObject> hs = new HashSet<UnityObject>();

            string[] guids = AssetDatabase.FindAssets("", new[] { "Assets/" + BHPath.Root + "/RTSaveLoad2/Resources_Auto/Resources/" + scene.name });

            List<AssetLibraryAsset> assetLibraries = new List<AssetLibraryAsset>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                AssetLibraryAsset assetLibrary = AssetDatabase.LoadAssetAtPath<AssetLibraryAsset>(path);
                if (assetLibrary != null)
                {
                    assetLibrary.Foreach(assetInfo =>
                    {
                        if (assetInfo.Object != null)
                        {
                            if (!hs.Contains(assetInfo.Object))
                            {
                                hs.Add(assetInfo.Object);
                            }

                            if(assetInfo.PrefabParts != null)
                            {
                                foreach (PrefabPartInfo prefabPart in assetInfo.PrefabParts)
                                {
                                    if (prefabPart.Object != null)
                                    {
                                        if (!hs.Contains(prefabPart.Object))
                                        {
                                            hs.Add(prefabPart.Object);
                                        }
                                    }
                                }
                            }
                        }
                        return true;
                    });

                    assetLibraries.Add(assetLibrary);
                }
            }

            if(assetLibraries.Count == 0)
            {
                asset = ScriptableObject.CreateInstance<AssetLibraryAsset>();
                index = 0;
            }
            else
            {
                asset = assetLibraries.OrderBy(a => a.AssetLibrary.Identity).FirstOrDefault();
                index = assetLibraries.Count - 1;
            }

            folder = asset.AssetLibrary.Folders.Where(f => f.depth == 0).First();
            if(folder.Assets == null)
            {
                folder.Assets = new List<AssetInfo>();
            }
            return hs;
        }

        private static void CreateAssetLibraryFromScene(Scene scene, int index, AssetLibraryAsset asset, AssetFolderInfo folder, HashSet<UnityObject> hs)
        {
            TypeMap typeMap = new TypeMap();
            AssetDB assetDB = new AssetDB();

            IOC.Register<ITypeMap>(typeMap);
            IOC.Register<IAssetDB>(assetDB);

            PersistentRuntimeScene rtScene = new PersistentRuntimeScene();

            GetDepsFromContext ctx = new GetDepsFromContext();
            rtScene.GetDepsFrom(scene, ctx);

            IOC.Unregister<ITypeMap>(typeMap);
            IOC.Unregister<IAssetDB>(assetDB);

            int identity = asset.AssetLibrary.Identity;
            
            foreach (UnityObject obj in ctx.Dependencies)
            {
                if (!obj)
                {
                    continue;
                }

                if (hs.Contains(obj))
                {
                    continue; 
                }

                if (obj is GameObject)
                {
                    GameObject go = (GameObject)obj;
                    if (go.IsPrefab())
                    {
                        AssetInfo assetInfo = new AssetInfo(go.name, 0, identity);
                        assetInfo.Object = go;
                        identity++;

                        List<PrefabPartInfo> prefabParts = new List<PrefabPartInfo>();
                        AssetLibraryAssetsGUI.CreatePefabParts(go, ref identity, prefabParts);
                        if (prefabParts.Count >= AssetLibraryInfo.MAX_ASSETS - AssetLibraryInfo.INITIAL_ID)
                        {
                            EditorUtility.DisplayDialog("Unable Create AssetLibrary", string.Format("Max 'Indentity' value reached. 'Identity' ==  {0}", AssetLibraryInfo.MAX_ASSETS), "OK");
                            return;
                        }

                        if (identity >= AssetLibraryInfo.MAX_ASSETS)
                        {
                            SaveAssetLibrary(asset, scene, index);
                            index++;

                            asset = ScriptableObject.CreateInstance<AssetLibraryAsset>();
                            folder = asset.AssetLibrary.Folders.Where(f => f.depth == 0).First();
                            
                            identity = asset.AssetLibrary.Identity;
                        }

                        assetInfo.PrefabParts = prefabParts;
                        asset.AssetLibrary.Identity = identity;
                        folder.Assets.Add(assetInfo);
                    }
                }
                else 
                {
                    AssetInfo assetInfo = new AssetInfo(obj.name, 0, identity);
                    assetInfo.Object = obj;
                    identity++;

                    if (identity >= AssetLibraryInfo.MAX_ASSETS)
                    {
                        SaveAssetLibrary(asset, scene, index);
                        index++;

                        asset = ScriptableObject.CreateInstance<AssetLibraryAsset>();
                        folder = asset.AssetLibrary.Folders.Where(f => f.depth == 0).First();
                        identity = asset.AssetLibrary.Identity;
                    }

                    asset.AssetLibrary.Identity = identity;
                    folder.Assets.Add(assetInfo);
                }
            }

            SaveAssetLibrary(asset, scene, index);
            index++;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static void SaveAssetLibrary(AssetLibraryAsset asset, Scene scene, int index)
        {
            string dir = BHPath.Root + "/RTSaveLoad2";
            string dataPath = Application.dataPath + "/";

            if (!Directory.Exists(dataPath + dir + "/Resources_Auto"))
            {
                AssetDatabase.CreateFolder("Assets/" + dir, "Resources_Auto");
            }

            dir = dir + "/Resources_Auto";
            if (!Directory.Exists(dataPath + dir + "/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/" + dir, "Resources");
            }

            dir = dir + "/Resources";
            if (!Directory.Exists(dataPath + dir + "/" + scene.name))
            {
                AssetDatabase.CreateFolder("Assets/" + dir, scene.name);
            }

            dir = dir + "/" + scene.name;

            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset)))
            {
                if (index == 0)
                {

                    AssetDatabase.CreateAsset(asset, "Assets/" + dir + "/AssetLibrary.asset");
                }
                else
                {
                    AssetDatabase.CreateAsset(asset, "Assets/" + dir + "/AssetLibrary" + (index + 1) + ".asset");
                }
            }
            
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }


        [MenuItem("Tools/Runtime SaveLoad2/Persistent Classes")]
        private static void ShowMenuItem()
        {
            PersistentClassMapperWindow.ShowWindow();
        }

        [MenuItem("Tools/Runtime SaveLoad2/Collect Scene Dependencies")]
        private static void AssetLibraryFromScene()
        {
            Scene scene = SceneManager.GetActiveScene();

            int index;
            AssetLibraryAsset asset;
            AssetFolderInfo folder;
            HashSet<UnityObject> hs = ReadFromAssetLibraries(scene, out index, out asset, out folder);

            CreateAssetLibraryFromScene(scene, index, asset, folder, hs);
        }

        [MenuItem("Tools/Runtime SaveLoad2/Create Shader Profiles")]
        private static void CreateShaderProfiles()
        {
            RuntimeShaderProfilesGen.CreateProfile();
        }
    }

}

