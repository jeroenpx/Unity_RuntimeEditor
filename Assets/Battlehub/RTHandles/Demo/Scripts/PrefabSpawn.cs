using Battlehub.RTCommon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTHandles.Demo
{
    public class PrefabSpawn : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private GameObject m_prefab = null;

        [SerializeField]
        private Image m_preview = null;

        [SerializeField]
        private Text m_prefabName = null;

        private Texture2D m_texture;
        private Sprite m_sprite;

        private GameObject m_prefabInstance;
        private HashSet<Transform> m_prefabInstanceTransforms;
        private Plane m_dragPlane;
        private Vector3 m_point;
        
        private IRTE m_editor;
        private RuntimeWindow m_scene;
        
        public void Start()
        {
            if(m_prefab == null)
            {
                Debug.LogWarning("m_prefab is not set");
                return;
            }

            m_editor = IOC.Resolve<IRTE>();
            m_scene = m_editor.GetWindow(RuntimeWindowType.Scene);

            IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
            m_texture = resourcePreview.TakeSnapshot(m_prefab);
            if (m_preview != null)
            {
                m_preview.sprite = Sprite.Create(m_texture, new Rect(0, 0, m_texture.width, m_texture.height), new Vector2(0.5f, 0.5f));
                m_preview.color = Color.white;
            }

            if(m_prefabName != null)
            {
                m_prefabName.text = m_prefab.name;
            }
        }

        private void OnDestroy()
        {
            if(m_texture != null)
            {
                Destroy(m_texture);
                m_texture = null;
            }
        }

        private bool GetPointOnDragPlane(out Vector3 point)
        {
            Ray ray = m_scene.Pointer;
            float distance;
            if (m_dragPlane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = Vector3.zero;
            return false;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(m_prefab == null)
            {
                return;
            }

            IScenePivot scenePivot = m_scene.IOCContainer.Resolve<IScenePivot>();
            Camera camera = m_scene.Camera;
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(camera.transform.up, Vector3.up)) > Mathf.Cos(Mathf.Deg2Rad))
            {
                up = Vector3.Cross(camera.transform.right, Vector3.up);
            }
            else
            {
                up = Vector3.up;
            }
            m_dragPlane = new Plane(up, scenePivot.SecondaryPivot.position);

            bool wasPrefabEnabled = m_prefab.activeSelf;
            m_prefab.SetActive(false);

            Vector3 point;
            if (GetPointOnDragPlane(out point))
            {
                m_prefabInstance = Instantiate(m_prefab, point, Quaternion.identity);
            }
            else
            {
                m_prefabInstance = Instantiate(m_prefab, Vector3.zero, Quaternion.identity);
            }

            m_prefabInstanceTransforms = new HashSet<Transform>(m_prefabInstance.GetComponentsInChildren<Transform>(true));

            m_prefab.SetActive(wasPrefabEnabled);

            ExposeToEditor exposeToEditor = m_prefabInstance.GetComponent<ExposeToEditor>();
            if (exposeToEditor == null)
            {
                exposeToEditor = m_prefabInstance.AddComponent<ExposeToEditor>();
            }

            exposeToEditor.SetName(m_prefab.name);
            m_prefabInstance.SetActive(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 point;
            if (GetPointOnDragPlane(out point))
            {
                m_point = point;
                if (m_prefabInstance != null)
                {
                    m_prefabInstance.transform.position = m_point;
                    RaycastHit hit = Physics.RaycastAll(m_scene.Pointer).Where(h => !m_prefabInstanceTransforms.Contains(h.transform)).FirstOrDefault();
                    if (hit.transform != null)
                    {
                        m_prefabInstance.transform.position = hit.point;
                    }
                }
            }
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            if (m_prefabInstance != null)
            {
                ExposeToEditor exposeToEditor = m_prefabInstance.GetComponent<ExposeToEditor>();
                m_editor.Undo.BeginRecord();
                m_editor.Undo.RegisterCreatedObjects(new[] { exposeToEditor });
                m_editor.Selection.activeObject = m_prefabInstance;
                m_editor.Undo.EndRecord();
            }

            m_prefabInstance = null;
            m_prefabInstanceTransforms = null;
        }
    }
}



