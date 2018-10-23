using UnityEngine;

using Battlehub.RTCommon;

namespace Battlehub.RTHandles
{
    /// <summary>
    /// Draws bounding box of selected object
    /// </summary>
    public class SelectionGizmo : MonoBehaviour, IGL
    {
        public bool DrawRay = true;
        public RuntimeHandlesComponent Appearance;
        private ExposeToEditor m_exposeToEditor;

        private void Awake()
        {
            m_exposeToEditor = GetComponent<ExposeToEditor>();
            RuntimeHandlesComponent.InitializeIfRequired(ref Appearance);
        }

        private void Start()
        {
            if (GLRenderer.Instance == null)
            {
                GameObject glRenderer = new GameObject();
                glRenderer.name = "GLRenderer";
                glRenderer.AddComponent<GLRenderer>();
            }

            if (m_exposeToEditor != null)
            {
                GLRenderer.Instance.Add(this);
            }

            if (!m_exposeToEditor.Editor.Selection.IsSelected(gameObject))
            {
                Destroy(this);
            }
        }

        private void OnEnable()
        {
            if (m_exposeToEditor != null)
            {
                if (GLRenderer.Instance != null)
                {
                    GLRenderer.Instance.Add(this);
                }
            }
        }

        private void OnDisable()
        {
            if (GLRenderer.Instance != null)
            {
                GLRenderer.Instance.Remove(this);
            }
        }

        public void Draw(int cullingMask)
        {
            if (m_exposeToEditor.Editor.Tools.ShowSelectionGizmos)
            {
                RTLayer layer = RTLayer.SceneView;
                if ((cullingMask & (int)layer) == 0)
                {
                    return;
                }

                Bounds bounds = m_exposeToEditor.Bounds;
                Transform trform = m_exposeToEditor.BoundsObject.transform;
                Appearance.DrawBounds(ref bounds, trform.position, trform.rotation, trform.lossyScale);
            }
        }
    }
}
