using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.Utils;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class SceneView : RuntimeWindow
    {
        private IProject m_project;
        private GameObject m_prefabInstance;
        private bool m_dropExecuted;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_project = IOC.Resolve<IProject>();
            
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
                    m_project.Load(assetItem, (error, obj) =>
                    {
                        Editor.IsBusy = false;
                        if(IsPointerOver)
                        {
                            if (obj is GameObject)
                            {
                                GameObject prefab = (GameObject)obj;
                                bool wasPrefabEnabled = prefab.activeSelf;
                                prefab.SetActive(false);

                                m_prefabInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

                                prefab.SetActive(wasPrefabEnabled);

                                ExposeToEditor exposeToEditor = m_prefabInstance.GetComponent<ExposeToEditor>();
                                if (exposeToEditor == null)
                                {
                                    exposeToEditor = m_prefabInstance.AddComponent<ExposeToEditor>();
                                }

                                exposeToEditor.SetName(obj.name);
                                m_prefabInstance.SetActive(true);

                                if(m_dropExecuted)
                                {
                                    RecordUndo();
                                    m_prefabInstance = null;
                                    m_dropExecuted = true;
                                }
                            }
                        }
                    });
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
            else
            {
                m_dropExecuted = true;
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
    }

}
