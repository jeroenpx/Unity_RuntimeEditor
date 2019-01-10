using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.Utils;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class SceneView : RuntimeWindow
    {
        private Plane m_dragPlane;
        private IProject m_project;
        private GameObject m_prefabInstance;
     
        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Scene;
            base.AwakeOverride();
            m_project = IOC.Resolve<IProject>();

            if(!GetComponent<SceneViewInput>())
            {
                gameObject.AddComponent<SceneViewInput>();
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            Debug.Log("On SceneView activated");
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Debug.Log("On SceneView deactivated");
        }

        public override void DragEnter(object[] dragObjects, PointerEventData eventData)
        {
            base.DragEnter(dragObjects, eventData);
            if(Editor.DragDrop.DragObjects[0] is AssetItem)
            {
                AssetItem assetItem = (AssetItem)Editor.DragDrop.DragObjects[0];
                if (m_project.ToType(assetItem) == typeof(GameObject))
                {
                    Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
                    Editor.IsBusy = true;
                    m_project.Load(new[] { assetItem }, (error, obj) =>
                    {
                        Editor.IsBusy = false;
                        if(IsPointerOver)
                        {
                            if (obj[0] is GameObject)
                            {
                                IScenePivot scenePivot = IOCContainer.Resolve<IScenePivot>();
                                Vector3 up = Vector3.up;
                                if(Mathf.Abs(Vector3.Dot(Camera.transform.up, Vector3.up)) > Mathf.Cos(Mathf.Deg2Rad))
                                {
                                    up = Vector3.Cross(Camera.transform.right, Vector3.up);
                                }
                                else
                                {
                                    up = Vector3.up;
                                }
                                m_dragPlane = new Plane(up, scenePivot.SecondaryPivot.position);
                        
                                GameObject prefab = (GameObject)obj[0];
                                bool wasPrefabEnabled = prefab.activeSelf;
                                prefab.SetActive(false);

                                m_prefabInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

                                prefab.SetActive(wasPrefabEnabled);

                                ExposeToEditor exposeToEditor = m_prefabInstance.GetComponent<ExposeToEditor>();
                                if (exposeToEditor == null)
                                {
                                    exposeToEditor = m_prefabInstance.AddComponent<ExposeToEditor>();
                                }

                                exposeToEditor.SetName(obj[0].name);
                                m_prefabInstance.SetActive(true);
                            }
                        }
                    });
                }
            }
        }

        public override void Drag(object[] dragObjects, PointerEventData eventData)
        {
            base.Drag(dragObjects, eventData);

            if(m_prefabInstance != null)
            {
                Vector3 point;
                if (GetPointOnDragPlane(out point))
                {
                    m_prefabInstance.transform.position = point;
                    
                    RaycastHit hit = Physics.RaycastAll(Pointer).Where(h => h.transform != m_prefabInstance.transform).FirstOrDefault();
                    if (hit.transform != null)
                    {
                        m_prefabInstance.transform.position = hit.point;
                    }
                }
            }   
        }

        public override void Drop(object[] dragObjects, PointerEventData eventData)
        {
            if(m_prefabInstance != null)
            {
                RecordUndo();
                m_prefabInstance = null;
            }
        }

        private void RecordUndo()
        {
            Editor.Undo.BeginRecord();
            Editor.Undo.RecordSelection();
            Editor.Undo.BeginRegisterCreateObject(m_prefabInstance);
            Editor.Undo.EndRecord();

            bool isEnabled = Editor.Undo.Enabled;
            Editor.Undo.Enabled = false;
            Editor.Selection.activeGameObject = m_prefabInstance;
            Editor.Undo.Enabled = isEnabled;

            Editor.Undo.BeginRecord();
            Editor.Undo.RegisterCreatedObject(m_prefabInstance);
            Editor.Undo.RecordSelection();
            Editor.Undo.EndRecord();
        }

        public override void DragLeave(PointerEventData eventData)
        {
            base.DragLeave(eventData);
            Debug.Log("Drag Leave");
            Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);

            if(m_prefabInstance != null)
            {
                Destroy(m_prefabInstance);
                m_prefabInstance = null;
            }
        }

        private bool GetPointOnDragPlane(out Vector3 point)
        {
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
            float distance;
            if (m_dragPlane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = Vector3.zero;
            return false;
        }

        public void SelectAll()
        {
            Editor.Selection.objects = Editor.Object.Get(false).Select(exposed => exposed.gameObject).ToArray();
        }
    }
}
