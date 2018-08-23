using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [CustomEditor(typeof(AssetLibraryAsset))]
    public class AssetLibraryEditor : Editor
    {
        private AssetLibraryProjectGUI m_projectGUI;
        private AssetLibraryAssetsGUI m_assetsGUI;
        private AssetLibraryAsset m_minAsset;

        private AssetLibraryAsset Asset
        {
            get { return (AssetLibraryAsset)target; }
        }

        private bool m_canRenderAssetsGUI;
        private bool m_isSyncRequired;

        private void OnEnable()
        {
            if (m_assetsGUI == null)
            {
                m_assetsGUI = new AssetLibraryAssetsGUI();
                m_assetsGUI.SetTreeAsset(Asset);      
            }

            if (m_projectGUI == null)
            {
                m_projectGUI = new AssetLibraryProjectGUI(m_assetsGUI);
                m_projectGUI.SetTreeAsset(Asset);
                m_projectGUI.SelectedFoldersChanged += OnSelectedFoldersChanged;

                m_isSyncRequired = Asset.IsSyncRequired();
            }

            m_assetsGUI.SetSelectedFolders(m_projectGUI.SelectedFolders);
            m_projectGUI.OnEnable();
            m_assetsGUI.OnEnable();
        }

        private void OnDisable()
        {
            if(Asset != null)
            {
                SaveAsset();
            }

            m_projectGUI.SelectedFoldersChanged -= OnSelectedFoldersChanged;
            m_projectGUI.OnDisable();
            m_assetsGUI.OnDisable();
        }

        private void SaveAsset()
        {
            AssetLibraryInfo assetLibrary = Asset.AssetLibrary;
            string assetLibraryPath = AssetDatabase.GetAssetPath(Asset);
            int assetExtIndex = assetLibraryPath.LastIndexOf(".asset");
            string proxyPath = assetLibraryPath.Remove(assetExtIndex) + "_Visible.asset";

            AssetLibraryVisible proxy = AssetDatabase.LoadAssetAtPath<AssetLibraryVisible>(proxyPath);
            if(proxy == null)
            {
                proxy = CreateInstance<AssetLibraryVisible>();
                AssetDatabase.CreateAsset(proxy, proxyPath);
            }

            proxy.AssetLibrary = assetLibrary.CloneVisible();
            proxy.AssetLibraryPath = assetLibraryPath;
            proxy.KeepRuntimeProjectInSync = Asset.KeepRuntimeProjectInSync;

            EditorUtility.SetDirty(proxy);
            EditorUtility.SetDirty(Asset);
            AssetDatabase.SaveAssets();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();
            if(m_isSyncRequired)
            {
                EditorGUILayout.HelpBox("One or more prefabs have been changed. AssetLibrary need to be synchronized.", MessageType.Warning);
                if (GUILayout.Button("Synchronize"))
                {
                    Asset.Sync();
                    m_assetsGUI = new AssetLibraryAssetsGUI();
                    m_assetsGUI.InitIfNeeded();
                    m_assetsGUI.SetSelectedFolders(m_projectGUI.SelectedFolders);
                    m_assetsGUI.OnEnable();
                    m_isSyncRequired = false;
                    SaveAsset();
                }
            }
            
            m_projectGUI.OnGUI();
            if(m_canRenderAssetsGUI)
            {
                m_assetsGUI.OnGUI();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            Asset.KeepRuntimeProjectInSync = EditorGUILayout.Toggle("Keep in sync",Asset.KeepRuntimeProjectInSync);
            EditorGUILayout.EndHorizontal();
            if (Asset.KeepRuntimeProjectInSync)
            {
                EditorGUILayout.HelpBox("Runtime project tree will be updated each time you launch runtime editor and will reflect all changes in this asset library", MessageType.Info);
            }
           
            if(EditorGUI.EndChangeCheck())
            {
                SaveAsset();
            }

            EditorGUILayout.EndVertical();
        }

        private void OnSelectedFoldersChanged(object sender, EventArgs e)
        {
            m_canRenderAssetsGUI = m_projectGUI.SelectedFolders != null && m_projectGUI.SelectedFolders.Length > 0;
            m_assetsGUI.SetSelectedFolders(m_projectGUI.SelectedFolders);
        }

    


       
    }
}
