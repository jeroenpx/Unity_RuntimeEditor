/*
using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System;
using System.Collections;
using System.IO;
using TriLib;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class FbxImporterExample : FileImporter
    {
        public override string FileExt
        {
            get { return ".fbx"; }
        }

        public override string IconPath
        {
            get { return "Importers/Fbx"; }
        }

        private bool m_loaded;

        public override IEnumerator Import(string filePath, string targetPath)
        {
            m_loaded = false;

            using (var assetLoader = new AssetLoaderAsync())
            {
                try
                {
                    var assetLoaderOptions = AssetLoaderOptions.CreateInstance();
                    assetLoaderOptions.RotationAngles = new Vector3(90f, 180f, 0f);
                    assetLoaderOptions.AutoPlayAnimations = true;
                    assetLoader.LoadFromFile(filePath, assetLoaderOptions, null, delegate (GameObject loadedGameObject)
                    {
                        IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                        IProject project = IOC.Resolve<IProject>();
                        ProjectItem targetFolder = project.GetFolder(Path.GetDirectoryName(targetPath));

                        loadedGameObject.SetActive(false);
                        loadedGameObject.name = Path.GetFileName(targetPath);

                        ExposeToEditor exposeToEditor = loadedGameObject.AddComponent<ExposeToEditor>();
                        editor.CreatePrefab(targetFolder, exposeToEditor, true, assetItems =>
                        {
                            m_loaded = true;
                            UnityEngine.Object.Destroy(loadedGameObject);
                        });
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }

            yield return new WaitUntil(() => m_loaded);
        }
    }
}
*/
