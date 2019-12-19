using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.MeshDeformer3
{
    [MenuDefinition(-1)]
    public class MeshDeformerInit : EditorOverride
    {
        [SerializeField]
        private GameObject m_meshDeformerWindow = null;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            if(FindObjectOfType<MeshDeformerTool>() == null)
            {
                gameObject.AddComponent<MeshDeformerTool>();
            }

            Register();
        }

        private void Register()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            if (m_meshDeformerWindow != null)
            {
                RegisterWindow(wm, "MeshDeformer", "Deformer",
                    Resources.Load<Sprite>("meshdeformer-24"), m_meshDeformerWindow, false);
            }
        }

        private void RegisterWindow(IWindowManager wm, string typeName, string header, Sprite icon, GameObject prefab, bool isDialog)
        {
            wm.RegisterWindow(new CustomWindowDescriptor
            {
                IsDialog = isDialog,
                TypeName = typeName,
                Descriptor = new WindowDescriptor
                {
                    Header = header,
                    Icon = icon,
                    MaxWindows = 1,
                    ContentPrefab = prefab
                }
            });
        }

        [MenuCommand("MenuWindow/MeshDeformer")]
        public static void OpenMeshDeformer()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow("MeshDeformer");
        }
    }
}


