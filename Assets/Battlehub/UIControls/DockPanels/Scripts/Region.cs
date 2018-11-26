using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public enum RegionSplitType
    {
        Left,
        Top,
        Right,
        Bottom
    }

    public class Region : Selectable
    {
        [SerializeField]
        private ToggleGroup m_headerPanel;

        [SerializeField]
        private RectTransform m_contentPanel;

        [SerializeField]
        private Tab m_tabPrefab;

        [SerializeField]
        private Region m_regionPrefab;

        public void Add(Sprite icon, string header, RectTransform content)
        {
            Tab tab = Instantiate(m_tabPrefab, m_headerPanel.transform);
            tab.Icon = icon;
            tab.Text = header;
            tab.ToggleGroup = m_headerPanel;
            content.SetParent(m_contentPanel, false);

            tab.Toggle += OnTabToggle;
            tab.IsOn = true;
        }

        

        public void Split(RegionSplitType splitType)
        {
            switch (splitType)
            {
                case RegionSplitType.Left:
                    break;
                case RegionSplitType.Top:
                    break;
                case RegionSplitType.Right:
                    break;
                case RegionSplitType.Bottom:
                    break;
            }
        }

        private void OnTabToggle(Tab sender, bool args)
        {
            throw new System.NotImplementedException();
        }
    }
}

