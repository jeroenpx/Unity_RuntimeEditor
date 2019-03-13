using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.UIControls.DockPanels;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class ScenesSetupExample : EditorOverride
    {
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
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                wm.SetLayout(DefaultLayout, RuntimeWindowType.Scene.ToString());
            }
        }

        private void OverrideDefaultLayout()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.OverrideDefaultLayout(DefaultLayout, RuntimeWindowType.Scene.ToString());
        }

        private LayoutInfo DefaultLayout(IWindowManager wm)
        {
            bool isDialog;

            WindowDescriptor sceneWd;
            GameObject sceneContent;
            wm.CreateWindow(RuntimeWindowType.Scene.ToString(), out sceneWd, out sceneContent, out isDialog);

            WindowDescriptor sceneXWd;
            GameObject sceneXContent;
            RuntimeWindow xWindow = wm.CreateWindow(RuntimeWindowType.Scene.ToString(), out sceneXWd, out sceneXContent, out isDialog).GetComponent<RuntimeWindow>();
           // xWindow.CanActivate = false;

            RunNextFrame(() =>
            {
                IScenePivot xPivot = xWindow.IOCContainer.Resolve<IScenePivot>();
                xPivot.Pivot.position = new Vector3(5, 0, 0);
                
                xWindow.Camera.transform.position = Vector3.right * 20;
                xWindow.Camera.transform.LookAt(xPivot.Pivot);
                xWindow.Camera.orthographic = true;

                PositionHandle positionHandle = wm.GetComponents(xWindow.transform).SelectMany(c => c.GetComponentsInChildren<PositionHandle>(true)).FirstOrDefault();
                positionHandle.GridSize = 2;

                RotationHandle rotationHandle = wm.GetComponents(xWindow.transform).SelectMany(c => c.GetComponentsInChildren<RotationHandle>(true)).FirstOrDefault();
                rotationHandle.GridSize = 5;

                Tab tab = Region.FindTab(xWindow.transform);
                tab.IsCloseButtonVisible = false;
            });
            
            WindowDescriptor gameWd;
            GameObject gameContent;
            wm.CreateWindow(RuntimeWindowType.Game.ToString(), out gameWd, out gameContent, out isDialog);

            WindowDescriptor inspectorWd;
            GameObject inspectorContent;
            wm.CreateWindow(RuntimeWindowType.Inspector.ToString(), out inspectorWd, out inspectorContent, out isDialog);

            WindowDescriptor hierarchyWd;
            GameObject hierarchyContent;
            wm.CreateWindow(RuntimeWindowType.Hierarchy.ToString(), out hierarchyWd, out hierarchyContent, out isDialog);

            LayoutInfo layout = new LayoutInfo(false,
                new LayoutInfo(true,
                    new LayoutInfo(
                        new LayoutInfo(sceneContent.transform, sceneWd.Header, sceneWd.Icon),
                        new LayoutInfo(gameContent.transform, gameWd.Header, gameWd.Icon)),
                    new LayoutInfo(sceneXContent.transform, sceneXWd.Header, sceneXWd.Icon), 
                    0.5f),
                new LayoutInfo(true,
                    new LayoutInfo(inspectorContent.transform, inspectorWd.Header, inspectorWd.Icon),
                    new LayoutInfo(hierarchyContent.transform, hierarchyWd.Header, hierarchyWd.Icon),
                    0.5f),
                0.75f);
            return layout;
        }
    }
}
