
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battlehub.RTCommon;
using System;
using Battlehub.RTSaveLoad2.Interface;

namespace Battlehub.RTEditor
{
    public class GameObjectEditor : MonoBehaviour
    {
        [SerializeField]
        private Toggle TogEnableDisable = null;
        [SerializeField]
        private InputField InputName = null;
        [SerializeField]
        private Transform ComponentsPanel = null;

        private IRuntimeEditor m_editor;

        private void Start()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_editor.Object.ComponentAdded += OnComponentAdded;

            GameObject go = m_editor.Selection.activeGameObject;
            HashSet<Component> ignoreComponents = IgnoreComponents(go);
            InputName.text = go.name;
            TogEnableDisable.isOn = go.activeSelf;

            InputName.onEndEdit.AddListener(OnEndEditName);
            TogEnableDisable.onValueChanged.AddListener(OnEnableDisable);
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                Component component = components[i];
                CreateComponentEditor(go, component, ignoreComponents);
            }
        }

        private static HashSet<Component> IgnoreComponents(GameObject go)
        {
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            HierarchyItem hierarchyItem = go.GetComponent<HierarchyItem>();
            HashSet<Component> ignoreComponents = new HashSet<Component>();
            if (exposeToEditor != null)
            {
                if (exposeToEditor.Colliders != null)
                {
                    for (int i = 0; i < exposeToEditor.Colliders.Length; ++i)
                    {
                        Collider collider = exposeToEditor.Colliders[i];
                        if (!ignoreComponents.Contains(collider))
                        {
                            ignoreComponents.Add(collider);
                        }
                    }
                }

                ignoreComponents.Add(exposeToEditor);
            }

            if (hierarchyItem != null)
            {
                ignoreComponents.Add(hierarchyItem);
            }

            return ignoreComponents;
        }

        private bool CreateComponentEditor(GameObject go, Component component, HashSet<Component> ignoreComponents)
        {
            if (component == null)
            {
                return false;
            }

            if (ignoreComponents.Contains(component))
            {
                return false;
            }

            if ((component.hideFlags & HideFlags.HideInInspector) != 0)
            {
                return false;
            }

            if (EditorsMap.IsObjectEditorEnabled(component.GetType()))
            {
                GameObject editorPrefab = EditorsMap.GetObjectEditor(component.GetType());
                if (editorPrefab != null)
                {
                    ComponentEditor componentEditorPrefab = editorPrefab.GetComponent<ComponentEditor>();
                    if (componentEditorPrefab != null)
                    {
                        ComponentEditor editor = Instantiate(componentEditorPrefab);
                        editor.EndEditCallback = () =>
                        {
                            m_editor.UpdatePreview(go);
                            m_editor.IsDirty = true;
                        };
                        editor.transform.SetParent(ComponentsPanel, false);
                        editor.Component = component;
                        return true;
                    }
                    else
                    {
                        Debug.LogErrorFormat("editor prefab {0} does not have ComponentEditor script", editorPrefab.name);
                        return false;
                    }
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            if (InputName != null)
            {
                InputName.onEndEdit.RemoveListener(OnEndEditName);
            }
            if(TogEnableDisable != null)
            {
                TogEnableDisable.onValueChanged.RemoveListener(OnEnableDisable);
            }

            if(m_editor != null && m_editor.Object != null)
            {
                m_editor.Object.ComponentAdded -= OnComponentAdded;
            }
        }

        private void OnEnableDisable(bool enable)
        {
            GameObject go = m_editor.Selection.activeGameObject;
            go.SetActive(enable);
        }

        private void OnEndEditName(string name)
        {
            GameObject go = m_editor.Selection.activeGameObject;
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            if(exposeToEditor != null)
            {
                exposeToEditor.SetName(name);
            }
            else
            {
                go.name = name;
            }
        }

        private void OnComponentAdded(ExposeToEditor obj, Component component)
        {
            if(component == null)
            {
                IWindowManager wnd = IOC.Resolve<IWindowManager>();
                wnd.MessageBox("Unable to add component", "Component was not added");
            }
            else
            {
                HashSet<Component> components = IgnoreComponents(obj.gameObject);
                if (!CreateComponentEditor(obj.gameObject, component, components))
                {

                }
            }
        }
    }
}

