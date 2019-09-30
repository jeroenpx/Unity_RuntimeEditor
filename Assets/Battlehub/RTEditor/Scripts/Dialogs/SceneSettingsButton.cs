using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class SceneSettingsButton : MonoBehaviour
    {
        [SerializeField]
        private Button m_sceneSettingsButton = null;

        private void Start()
        {
            m_sceneSettingsButton.onClick.AddListener(OnSceneSettings);
        }

        private void OnDestroy()
        {
            if(m_sceneSettingsButton != null)
            {
                m_sceneSettingsButton.onClick.RemoveListener(OnSceneSettings);
            }
        }

        private void OnSceneSettings()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            ISceneSettingsDialog sceneSettingsDialog = null;
            Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow("scenesettings", "Scene Settings",
                 (sender, args) => { }, (sender, args) => { }, 250, 160, 250, 160, false);
            sceneSettingsDialog = IOC.Resolve<ISceneSettingsDialog>();
            sceneSettingsDialog.Scene = GetComponent<RuntimeWindow>();
        }
    }
}

