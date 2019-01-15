using System;
using System.Linq;
using UnityEngine;

using Battlehub.RTCommon;
using UnityObject = UnityEngine.Object;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class InspectorView : RuntimeWindow
    {
        [SerializeField]
        private GameObject m_gameObjectEditor = null;

        [SerializeField]
        private GameObject m_materialEditor = null;

        [SerializeField]
        private Transform m_panel = null;

        [SerializeField]
        private GameObject m_addComponentRoot = null;

        [SerializeField]
        private AddComponentControl m_addComponentControl = null;

        private GameObject m_editor;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Hierarchy;
            base.AwakeOverride();

            if (m_gameObjectEditor == null)
            {
                Debug.LogError("GameObjectEditor is not set");
            }
            if (m_materialEditor == null)
            {
                Debug.LogError("MaterialEditor is not set");
            }

            Editor.Selection.SelectionChanged += OnRuntimeSelectionChanged;
        }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();
            UnityObject obj = Editor.Selection.activeObject;
            if(obj == null)
            {
                DestroyEditor();
            }
        }

        private void OnEnable()
        {
            CreateEditor();
        }

        private void OnDisable()
        {
            DestroyEditor();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if(Editor != null)
            {
                Editor.Selection.SelectionChanged -= OnRuntimeSelectionChanged;
            }
        }

        private void OnRuntimeSelectionChanged(UnityObject[] unselectedObjects)
        {
            if (m_editor != null &&  unselectedObjects != null && unselectedObjects.Length > 0)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                if(editor.IsDirty)
                {
                    editor.IsDirty = false;
                    editor.SaveAsset(unselectedObjects[0], result =>
                    {
                        CreateEditor();
                    });
                }
                else
                {
                    CreateEditor();
                }
            }
            else
            {
                CreateEditor();
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            if (editor.IsDirty && editor.Selection.activeObject != null)
            {
                editor.IsDirty = false;
                editor.SaveAsset(editor.Selection.activeObject, result =>
                {
                });
            }
        }

        private void DestroyEditor()
        {
            if (m_editor != null)
            {
                Destroy(m_editor);
            }

            if(m_addComponentRoot != null)
            {
                m_addComponentRoot.SetActive(false);
            }

            if (m_addComponentControl != null)
            {
                m_addComponentControl.ComponentSelected -= OnAddComponent;
            }
        }

        private void CreateEditor()
        {
            DestroyEditor();

            if (Editor.Selection.activeObject == null)
            {
                return;
            }

            UnityObject[] selectedObjects = Editor.Selection.objects.Where(o => o != null).ToArray();
            if (selectedObjects.Length != 1)
            {
                return;
            }

            Type objType = selectedObjects[0].GetType();
            for (int i = 1; i < selectedObjects.Length; ++i)
            {
                if (objType != selectedObjects[i].GetType())
                {
                    return;
                }
            }

            GameObject editorPrefab;

#if !UNITY_WEBGL && PROC_MATERIAL
            if (objType == typeof(Material) || objType == typeof(ProceduralMaterial))    
#else
            if (objType == typeof(Material))
#endif
            {
                Material mat = selectedObjects[0] as Material;
                if (mat.shader == null)
                {
                    return;
                }

                editorPrefab = EditorsMap.GetMaterialEditor(mat.shader);
            }
            else
            {
                if (!EditorsMap.IsObjectEditorEnabled(objType))
                {
                    return;
                }
                editorPrefab = EditorsMap.GetObjectEditor(objType);
            }

            if (editorPrefab != null)
            {
                m_editor = Instantiate(editorPrefab);
                m_editor.transform.SetParent(m_panel, false);
                m_editor.transform.SetAsFirstSibling();
            }

            bool isExposedToEditor = Editor.Selection.activeGameObject != null && Editor.Selection.activeGameObject.GetComponent<ExposeToEditor>() != null;
            if (m_addComponentRoot != null && isExposedToEditor)
            {
                m_addComponentRoot.SetActive(true);
                if(m_addComponentControl != null)
                {
                    m_addComponentControl.ComponentSelected += OnAddComponent;
                }
            }
        }


        private class ComponentReference
        {
            public Component Component;
            public ComponentReference(Component component)
            {
                Component = component;
            }
        }

        private void OnAddComponent(Type type)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();

            ExposeToEditor exposeToEditor = editor.Selection.activeGameObject.GetComponent<ExposeToEditor>();

            Component component = AddComponent(type, exposeToEditor);
            if (component != null)
            {
                editor.Undo.RecordObject(exposeToEditor, new ComponentReference(component), record =>
                {
                    //Does not work properly. 
                    ComponentReference state = (ComponentReference)record.State;
                    if(state.Component != null)
                    {
                        Destroy(state.Component);
                        state.Component = null;
                    }
                    else
                    {
                        state.Component = AddComponent(type, exposeToEditor);
                    }
                   
                    return true;
                });
            }
        }

        private static Component AddComponent(Type type, ExposeToEditor exposeToEditor)
        {
            Component component = exposeToEditor.AddComponent(type);
            if (component is Rigidbody)
            {
                Rigidbody rb = (Rigidbody)component;
                rb.isKinematic = true;
            }
            return component;
        }
    }
}
