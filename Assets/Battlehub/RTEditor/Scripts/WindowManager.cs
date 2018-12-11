using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using System;
using UnityEngine;
namespace Battlehub.RTEditor
{
    public interface IWindowManager
    {
        void CreateWindow(RuntimeWindowType windowType, bool focusIfExists = true);
    }

    [Serializable]
    public struct WindowDescriptor
    {
        public Sprite Icon;
        public string Hader;
        public GameObject ContentPrefab;
    }

    public class WindowManager : MonoBehaviour, IWindowManager
    {
        [SerializeField]
        private WindowDescriptor m_sceneWindow;

        [SerializeField]
        private WindowDescriptor m_gameWindow;

        [SerializeField]
        private WindowDescriptor m_hierarchyWindow;

        [SerializeField]
        private WindowDescriptor m_inspectorWindow;

        [SerializeField]
        private WindowDescriptor m_projectWindow;

        [SerializeField]
        private WindowDescriptor m_consoleWindow;

        [SerializeField]
        private DockPanelsRoot m_dockPanels;

        private void Awake()
        {
            if(m_dockPanels == null)
            {
                m_dockPanels = FindObjectOfType<DockPanelsRoot>();
            }
        }

        public void CreateWindow(RuntimeWindowType windowType, bool focusIfExists = true)
        {
            Debug.Log("Create Window " + windowType + " FocusIfExists ? " + focusIfExists);
        }
    }
}

