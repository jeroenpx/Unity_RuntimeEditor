using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.UIControls.DockPanels;
using Battlehub.Utils;
using System.Collections;
using UnityEngine;

namespace Battlehub.RTEditor
{
     public class SettingsDialog : RuntimeWindow
    {
        [SerializeField]
        private BoolEditor m_isGridVisibleEditor = null;

        [SerializeField]
        private BoolEditor m_snapToGridEditor = null;

        [SerializeField]
        private RangeEditor m_gridSizeEditor = null;

        [SerializeField]
        private BoolEditor m_uiAutoScaleEditor = null;

        [SerializeField]
        private RangeEditor m_uiScaleEditor = null;
        
        public bool IsGridVisible
        {
            get { return m_settings.IsGridVisible; }
            set { m_settings.IsGridVisible = value; }
        }

        public bool SnapToGrid
        {
            get { return m_settings.IsGridEnabled; }
            set { m_settings.IsGridEnabled = value; }
        }

        public float GridSize
        {
            get { return m_settings.GridSize; }
            set { m_settings.GridSize = value; }
        }

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
                    UIScale = 1.0f;
                }
            }
        }

        public float UIScale
        {
            get { return m_settings.UIScale; }
            set { m_settings.UIScale = value; }
        }

        private ISettingsComponent m_settings;
        private Dialog m_parentDialog;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Custom;
            base.AwakeOverride();
            m_settings = IOC.Resolve<ISettingsComponent>();
        }
        
        protected virtual void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            if(m_parentDialog != null)
            {
                m_parentDialog.IsOkVisible = true;
                m_parentDialog.OkText = "Reset";
                m_parentDialog.Ok += OnResetClick;
                m_parentDialog.IsCancelVisible = true;
                m_parentDialog.CancelText = "Close";
            }

            if(m_isGridVisibleEditor != null)
            {
                m_isGridVisibleEditor.Init(this, this, Strong.PropertyInfo((SettingsDialog x) => x.IsGridVisible), null, "Is Grid Visible");
            }

            if(m_snapToGridEditor != null)
            {
                m_snapToGridEditor.Init(this, this, Strong.PropertyInfo((SettingsDialog x) => x.SnapToGrid), null, "Snap To Grid");
            }

            if(m_gridSizeEditor != null)
            {
                m_gridSizeEditor.Min = 0.1f;
                m_gridSizeEditor.Max = 8;
                m_gridSizeEditor.Init(this, this, Strong.PropertyInfo((SettingsDialog x) => x.GridSize), null, "Grid Size");
            }

            if(m_uiAutoScaleEditor != null)
            {
                m_uiAutoScaleEditor.Init(this, this, Strong.PropertyInfo((SettingsDialog x) => x.UIAutoScale), null, "UI Auto Scale");
            }

            if(m_uiScaleEditor != null)
            {
                m_uiScaleEditor.Min = 0.5f;
                m_uiScaleEditor.Max = 3;
                m_uiScaleEditor.Init(this, this, Strong.PropertyInfo((SettingsDialog x) => x.UIScale), null, "UI Scale", 
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
