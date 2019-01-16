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
        private GameObject m_dropTarget;
        private AssetItem m_dragItem;
     
        protected override void AwakeOverride()
        {
            ActivateOnAnyKey = true;
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
                        //if (IsPointerOver)
                        {
                            if (obj[0] is GameObject)
                            {
                                IScenePivot scenePivot = IOCContainer.Resolve<IScenePivot>();
                                Vector3 up = Vector3.up;
                                if (Mathf.Abs(Vector3.Dot(Camera.transform.up, Vector3.up)) > Mathf.Cos(Mathf.Deg2Rad))
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
                    m_dragItem = null;
                }
                else if (m_project.ToType(assetItem) == typeof(Material))
                {
                    m_dragItem = assetItem;
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
            
            if(m_dragItem != null)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(Pointer, out hitInfo, float.MaxValue, Editor.CameraLayerSettings.RaycastMask))
                {
                    MeshRenderer renderer = hitInfo.collider.GetComponentInChildren<MeshRenderer>();
                    SkinnedMeshRenderer sRenderer = hitInfo.collider.GetComponentInChildren<SkinnedMeshRenderer>();

                    if (renderer != null || sRenderer != null)
                    {
                        Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
                        m_dropTarget = hitInfo.transform.gameObject;
                    }
                    else
                    {
                        Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
                        m_dropTarget = null;
                    }
                }
                else
                {
                    Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
                    m_dropTarget = null;
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

            if (m_dropTarget != null)
            {
                MeshRenderer renderer = m_dropTarget.GetComponentInChildren<MeshRenderer>();
                SkinnedMeshRenderer sRenderer = m_dropTarget.GetComponentInChildren<SkinnedMeshRenderer>();

                if (renderer != null || sRenderer != null)
                {
                    AssetItem assetItem = (AssetItem)Editor.DragDrop.DragObjects[0];
                    Editor.IsBusy = true;
                    m_project.Load(new[] { assetItem }, (error, material) =>
                    {
                        Editor.IsBusy = false;
                        Editor.Undo.BeginRecord();

                        if (renderer != null)
                        {
                            Editor.Undo.RecordValue(renderer, Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                            Material[] materials = renderer.sharedMaterials;
                            for (int i = 0; i < materials.Length; ++i)
                            {
                                materials[i] = (Material)material[0];
                            }
                            renderer.sharedMaterials = materials;
                        }

                        if (sRenderer != null)
                        {
                            Editor.Undo.RecordValue(sRenderer, Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                            Material[] materials = sRenderer.sharedMaterials;
                            for (int i = 0; i < materials.Length; ++i)
                            {
                                materials[i] = (Material)material[0];
                            }
                            sRenderer.sharedMaterials = materials;
                        }

                        if (renderer != null || sRenderer != null)
                        {
                            Editor.Undo.EndRecord();
                        }

                        if (renderer != null || sRenderer != null)
                        {
                            Editor.Undo.BeginRecord();
                        }

                        if (renderer != null)
                        {
                            Editor.Undo.RecordValue(renderer, Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                        }

                        if (sRenderer != null)
                        {
                            Editor.Undo.RecordValue(sRenderer, Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                        }

                        Editor.Undo.EndRecord();
                    });
                }

                m_dropTarget = null;
                m_dragItem = null;
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

            m_dragItem = null;
            m_dropTarget = null;
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
