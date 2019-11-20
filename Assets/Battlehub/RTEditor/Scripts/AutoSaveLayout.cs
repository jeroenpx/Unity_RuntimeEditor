using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [DefaultExecutionOrder(-91)]
    public class AutoSaveLayout : EditorOverride
    {
        private const string SavedLayoutName = "Saved_Layout";

        private IWindowManager m_wm;

        protected override void OnEditorCreated(object obj)
        {    
            OverrideDefaultLayout();
            m_wm = IOC.Resolve<IWindowManager>();
        }

        protected override void OnEditorExist()
        {
            OverrideDefaultLayout();

            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            if (editor.IsOpened)
            {
                m_wm = IOC.Resolve<IWindowManager>();
                m_wm.SetLayout(DefaultLayout, RuntimeWindowType.Scene.ToString());
            }
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            m_wm.SaveLayout(SavedLayoutName);
        }

        private void OnApplicationQuit()
        {
            m_wm.SaveLayout(SavedLayoutName);
        }

        private void OverrideDefaultLayout()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.OverrideDefaultLayout(DefaultLayout, RuntimeWindowType.Scene.ToString());
        }

        static LayoutInfo DefaultLayout(IWindowManager wm)
        {
            if (wm.LayoutExist(SavedLayoutName))
            {
                LayoutInfo layout = wm.GetLayout(SavedLayoutName);
                return layout;
            }

            return wm.GetBuiltInDefaultLayout();
        }
    }

}
