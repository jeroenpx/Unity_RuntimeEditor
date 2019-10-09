using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    [DefaultExecutionOrder(-90)]
    public class PersistentLayoutExample2 : EditorOverride
    {
        private const string SavedLayoutName = "AutoSavedLayout";

        private IWindowManager m_wm;

        protected override void OnEditorCreated(object obj)
        {
            OverrideDefaultLayout();
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
