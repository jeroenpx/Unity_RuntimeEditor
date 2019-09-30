using Battlehub.RTCommon;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public interface IOutlineManager
    {
        bool ContainsRenderer(Renderer renderer);
        void AddRenderers(Renderer[] renderers);
        void RemoveRenderers(Renderer[] renderers);
        void RecreateCommandBuffer();
    }

    public class OutlineManager : MonoBehaviour, IOutlineManager
    {
        private IRTE m_editor;
        private RuntimeWindow m_sceneWindow;
        private OutlineEffect m_outlineEffect;

        public Camera Camera
        {
            private get;
            set;
        }

        private void Start()
        {
            m_outlineEffect =  Camera.gameObject.AddComponent<OutlineEffect>();

            m_editor = IOC.Resolve<IRTE>();

            TryToAddRenderers();
            m_editor.Selection.SelectionChanged += OnSelectionChanged;

            RTEComponent rteComponent = GetComponentInParent<RTEComponent>();
            if(rteComponent != null)
            {
                m_sceneWindow = rteComponent.Window;
                if(m_sceneWindow != null)
                {
                    m_sceneWindow.IOCContainer.RegisterFallback<IOutlineManager>(this);
                }
            }
        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }

            if(m_outlineEffect != null)
            {
                Destroy(m_outlineEffect);
            }

            if (m_sceneWindow != null)
            {
                m_sceneWindow.IOCContainer.UnregisterFallback<IOutlineManager>(this);
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            if (unselectedObjects != null)
            {
                Renderer[] renderers = unselectedObjects.Select(go => go as GameObject).Where(go => go != null).SelectMany(go => go.GetComponentsInChildren<Renderer>(true)).ToArray();
                m_outlineEffect.RemoveRenderers(renderers);
            }

            TryToAddRenderers();
        }

        private void TryToAddRenderers()
        {
            if (m_editor.Selection.gameObjects != null)
            {
                Renderer[] renderers = m_editor.Selection.gameObjects.Where(go => go != null).Select(go => go.GetComponent<ExposeToEditor>()).Where(e => e != null && e.ShowSelectionGizmo).SelectMany(e => e.GetComponentsInChildren<Renderer>().Where(r => (r.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0)).ToArray();
                m_outlineEffect.AddRenderers(renderers);
            }
        }

        public bool ContainsRenderer(Renderer renderer)
        {
            return m_outlineEffect.ContainsRenderer(renderer);
        }

        public void AddRenderers(Renderer[] renderers)
        {
            m_outlineEffect.AddRenderers(renderers);
        }

        public void RemoveRenderers(Renderer[] renderers)
        {
            m_outlineEffect.RemoveRenderers(renderers);
        }

        public void RecreateCommandBuffer()
        {
            m_outlineEffect.RecreateCommandBuffer();
        }
    }
}

