using Battlehub.RTSaveLoad2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ProjectItemView : MonoBehaviour
    {
        private IProject m_project;

        [SerializeField]
        private Image m_imgPreview;

        [SerializeField]
        public Sprite m_folder;
        [SerializeField]
        public Sprite m_readonlyFolder;
        [SerializeField]
        public Sprite m_scene;
        [SerializeField]
        public Sprite m_defaultPrefab;
        [SerializeField]
        public Sprite m_none;

        private Texture2D m_texture;

        private ProjectItem m_projectItem;
        public ProjectItem ProjectItem
        {
            get { return m_projectItem; }
            set
            {
                if(m_projectItem != null)
                {
                    if(m_projectItem is AssetItem)
                    {
                        AssetItem assetItem = (AssetItem)m_projectItem;
                        assetItem.PreviewDataChanged -= OnPreviewDataChanged;
                    }
                }

                m_projectItem = value;
                UpdateImage();

                if (m_projectItem != null)
                {
                    if (m_projectItem is AssetItem)
                    {
                        AssetItem assetItem = (AssetItem)m_projectItem;
                        assetItem.PreviewDataChanged += OnPreviewDataChanged;
                    }
                }
            }
        }

        private void OnPreviewDataChanged(object sender, System.EventArgs e)
        {
            UpdateImage();
        }

        private void UpdateImage()
        {
            if (m_texture != null)
            {
                Destroy(m_texture);
                m_texture = null;
            }
            if (m_projectItem == null)
            {
                m_imgPreview.sprite = null;
                
            }
            else if (m_projectItem is AssetItem)
            {
                AssetItem assetItem = (AssetItem)m_projectItem;
                if(assetItem.Preview == null || assetItem.Preview.PreviewData == null)
                {
                    m_imgPreview.sprite = m_none;
                }
                else if(assetItem.Preview.PreviewData.Length == 0)
                {
                    m_imgPreview.sprite = m_defaultPrefab;
                }
                else
                {
                    m_texture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                    m_texture.LoadImage(assetItem.Preview.PreviewData);
                    m_imgPreview.sprite = Sprite.Create(m_texture, new Rect(0, 0, m_texture.width, m_texture.height), new Vector2(0.5f, 0.5f));
                }
            }
            else if (m_projectItem.IsFolder)
            {
                if(m_project.IsStatic(m_projectItem))
                {
                    m_imgPreview.sprite = m_readonlyFolder;
                }
                else
                {
                    m_imgPreview.sprite = m_folder;
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void Awake()
        {
            m_project = RTSL2Deps.Get.Project;
        }

        private void OnDestroy()
        {
            if(m_texture != null)
            {
                Destroy(m_texture);
                m_texture = null;
            }
            if (m_projectItem != null)
            {
                if (m_projectItem is AssetItem)
                {
                    AssetItem assetItem = (AssetItem)m_projectItem;
                    assetItem.PreviewDataChanged -= OnPreviewDataChanged;
                }
            }
        }
    }
}

