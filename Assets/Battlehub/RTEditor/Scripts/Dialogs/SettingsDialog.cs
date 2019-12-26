using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.Utils;
using System.Collections;
using UnityEngine;

namespace Battlehub.RTEditor
{
     public class SettingsDialog : RuntimeWindow
    {
        [SerializeField]
        private HeaderLabel m_sceneSettingsHeader = null;

        [SerializeField]
        private BoolEditor m_isGridVisibleEditor = null;

        [SerializeField]
        private BoolEditor m_gridZTest = null;

        [SerializeField]
        private BoolEditor m_snapToGridEditor = null;

        [SerializeField]
        private RangeEditor m_gridSizeEditor = null;

        [SerializeField]
        private HeaderLabel m_uiSettingsHeader = null;

        [SerializeField]
        private BoolEditor m_uiAutoScaleEditor = null;

        [SerializeField]
        private RangeEditor m_uiScaleEditor = null;

        [SerializeField]
        private HeaderLabel m_sceneNavigationSettingsHeader = null;

        [SerializeField]
        private RangeEditor m_zoomSpeedEditor = null;

        [SerializeField]
        private BoolEditor m_constantZoomSpeedEditor = null;

        public bool UIAutoScale
        {
            get { return m_settings.UIAutoScale; }
            set
            {
                m_settings.UIAutoScale = value;
                if (m_uiScaleEditor != null)
                {
                    m_uiScaleEditor.gameObject.SetActive(!UIAutoScale);
                }
                if(value)
                {
                    m_settings.UIScale = 1.0f;
                }
            }
        }

        private ILocalization m_localization;
        private ISettingsComponent m_settings;
        private Dialog m_parentDialog;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Custom;
            base.AwakeOverride();
            m_settings = IOC.Resolve<ISettingsComponent>();
            m_localization = IOC.Resolve<ILocalization>();
        }
        
        protected virtual void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            if(m_parentDialog != null)
            {
                m_parentDialog.IsOkVisible = true;
                m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SettingsDialog_Reset", "Reset");
                m_parentDialog.Ok += OnResetClick;
                m_parentDialog.IsCancelVisible = true;
                m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_SettingsDialog_Cancel", "Cancel");
            }

            if(m_uiSettingsHeader != null)
            {
                m_uiSettingsHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_UISettings", "UI Settings"));
            }

            if (m_sceneSettingsHeader != null)
            {
                m_sceneSettingsHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_SceneSettings", "Scene Settings"));
            }

            if (m_sceneNavigationSettingsHeader != null)
            {
                m_sceneNavigationSettingsHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_SceneNavigationSettings", "Scene Navigation Settings"));
            }

            if (m_isGridVisibleEditor != null)
            {
                m_isGridVisibleEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.IsGridVisible), null, m_localization.GetString("ID_RTEditor_SettingsDialog_IsGridVisible", "Is Grid Visible"));
            }

            if(m_gridZTest != null)
            {
                m_gridZTest.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.GridZTest), null, m_localization.GetString("ID_RTEditor_SettingsDialog_GridZTest", "Grid Z Test"));
            }

            if(m_snapToGridEditor != null)
            {
                m_snapToGridEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.IsGridEnabled), null, m_localization.GetString("ID_RTEditor_SettingsDialog_SnapToGrid", "Snap To Grid"));
            }

            if(m_gridSizeEditor != null)
            {
                m_gridSizeEditor.Min = 0.1f;
                m_gridSizeEditor.Max = 8;
                m_gridSizeEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.GridSize), null, m_localization.GetString("ID_RTEditor_SettingsDialog_GridSize", "Grid Size"));
            }

            if(m_uiAutoScaleEditor != null)
            {
                m_uiAutoScaleEditor.Init(this, this, Strong.PropertyInfo((SettingsDialog x) => x.UIAutoScale), null, m_localization.GetString("ID_RTEditor_SettingsDialog_UIAutoScale", "UI Auto Scale"));
            }

            if(m_uiScaleEditor != null)
            {
                m_uiScaleEditor.Min = 0.5f;
                m_uiScaleEditor.Max = 3;
                m_uiScaleEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.UIScale), null, m_localization.GetString("ID_RTEditor_SettingsDialog_UIScale", "UI Scale"),
                    () => { },
                    () => { },
                    () => 
                    {
                        m_settings.EndEditUIScale();
                        if(m_parentDialog != null)
                        {
                            StartCoroutine(CoEndEditUIScale());
                        }
                    });
                if (UIAutoScale)
                {
                    m_uiScaleEditor.gameObject.SetActive(false);
                }
            }

            if (m_zoomSpeedEditor != null)
            {
                m_zoomSpeedEditor.Min = 1.0f;
                m_zoomSpeedEditor.Max = 100.0f;
                m_zoomSpeedEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.ZoomSpeed), null, m_localization.GetString("ID_RTEditor_SettingsDialog_ZoomSpeed", "Zoom Speed"));
            }

            if(m_constantZoomSpeedEditor != null)
            {
                m_constantZoomSpeedEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.ConstantZoomSpeed), null, m_localization.GetString("ID_RTEditor_SettingsDialog_ConstantZoomSpeed", "Constant Zoom Speed"));
            }
        }

        private IEnumerator CoEndEditUIScale()
        {
            yield return new WaitForEndOfFrame();
            m_parentDialog.ParentRegion.Fit();  
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if(m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnResetClick;
            }
        }

        private void OnResetClick(Dialog sender, DialogCancelArgs args)
        {
            args.Cancel = true;

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.Confirmation("Reset to defaults confirmation", "Are you sure you want to reset to default settings?",
                (dialog, yes) => 
                {
                    m_settings.ResetToDefaults();
                    if (m_uiScaleEditor != null)
                    {
                        m_uiScaleEditor.gameObject.SetActive(!UIAutoScale);
                    }
                },
                (dialog, no) => { },
                "Yes", "No");
        }
    }
}
