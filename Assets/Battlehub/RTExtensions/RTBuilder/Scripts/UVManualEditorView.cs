using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.UIControls.DockPanels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class UVManualEditorView : RuntimeWindow
    {
        public IProBuilderTool Tool
        {
            get;
            set;
        }

        private List<Transform> m_extraComponents;
        private IRuntimeSelection m_uvSelection;
        private DockPanel m_parentDockPanel;

        protected override void AwakeOverride()
        {
            ForceUseRenderTextures = true;
            base.AwakeOverride();
            Tool = IOC.Resolve<IProBuilderTool>();

            m_extraComponents = new List<Transform>();

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            Transform[] children = transform.OfType<Transform>().ToArray();
            for (int i = 0; i < children.Length; ++i)
            {
                Transform component = children[i];
                if (!(component is RectTransform))
                {
                    component.gameObject.SetActive(false);
                    component.transform.SetParent(wm.ComponentsRoot, false);

                    m_extraComponents.Add(component);
                }
            }

            m_uvSelection = new RuntimeSelection(Editor);
        }

        protected virtual void OnEnable()
        {
            m_parentDockPanel = GetComponentInParent<DockPanel>();
            m_parentDockPanel.RegionMaximized += OnRegionMaximized;

            SetComponentsActive(true);

            IRuntimeSceneComponent scene = IOCContainer.Resolve<IRuntimeSceneComponent>();
            scene.CameraPosition = Vector3.up * 10;
            scene.Pivot = Vector3.zero;
            scene.Selection = m_uvSelection;
            m_uvSelection.activeObject = null;
            scene.IsOrthographic = true;
            scene.CanRotate = false;
            scene.CanFreeMove = false;
            scene.CanSelect = false;
            scene.CanSelectAll = false;
            scene.ChangeOrthographicSizeOnly = true;

            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();

            Camera.cullingMask = 0;
            Camera.backgroundColor = appearance.Colors.Secondary;
            Camera.clearFlags = CameraClearFlags.SolidColor;

            EnableRaycasts();
        }

        protected virtual void OnDisable()
        {
            if(m_parentDockPanel != null)
            {
                m_parentDockPanel.RegionMaximized -= OnRegionMaximized;
            }

            SetComponentsActive(false);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            for (int i = 0; i < m_extraComponents.Count; ++i)
            {
                if (m_extraComponents[i] != null)
                {
                    Destroy(m_extraComponents[i].gameObject);
                }
            }
        }

        private void SetComponentsActive(bool active)
        {
            for (int i = 0; i < m_extraComponents.Count; ++i)
            {
                if (m_extraComponents[i] != null)
                {
                    m_extraComponents[i].gameObject.SetActive(active);
                }
            }
        }

        private void OnRegionMaximized(Region region, bool value)
        {
            
        }

    }
}
