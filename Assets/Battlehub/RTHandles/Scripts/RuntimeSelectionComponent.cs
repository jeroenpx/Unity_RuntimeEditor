using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTHandles
{
    public delegate void UnityEditorToolChanged();
    public class UnityEditorToolsListener
    {
        public static event UnityEditorToolChanged ToolChanged;

        #if UNITY_EDITOR
        private static UnityEditor.Tool m_tool;
        static UnityEditorToolsListener()
        {
            m_tool = UnityEditor.Tools.current;
        }
        #endif

        public static void Update()
        {
            #if UNITY_EDITOR
            if (m_tool != UnityEditor.Tools.current)
            {
                if (ToolChanged != null)
                {
                    ToolChanged();
                }
                m_tool = UnityEditor.Tools.current;
            }
            #endif
        }
    }

    public class RuntimeSelectionComponent : RTEBehaviour
    {
        [SerializeField]
        private PositionHandle m_positionHandle;
        [SerializeField]
        private RotationHandle m_rotationHandle;
        [SerializeField]
        private ScaleHandle m_scaleHandle;
        [SerializeField]
        private BoxSelection m_boxSelection;

        public BoxSelection BoxSelection
        {
            get { return m_boxSelection; }
        }

        private bool m_isUISelected;
        public bool IsUISelected
        {
            get { return m_isUISelected; }
            private set
            {
                m_isUISelected = value;
                if (m_boxSelection != null)
                {
                    m_boxSelection.enabled = value;
                }
            }
        }

        protected virtual void Start()
        {
            if (m_boxSelection != null)
            {
                m_boxSelection.Filtering += OnBoxSelectionFiltering;
            }

            if (m_positionHandle != null)
            {
                m_positionHandle.gameObject.SetActive(true);
                m_positionHandle.gameObject.SetActive(false);
            }

            if (m_rotationHandle != null)
            {
                m_rotationHandle.gameObject.SetActive(true);
                m_rotationHandle.gameObject.SetActive(false);
            }

            if (m_scaleHandle != null)
            {
                m_scaleHandle.gameObject.SetActive(true);
                m_scaleHandle.gameObject.SetActive(false);
            }
        
            Editor.Selection.SelectionChanged += OnRuntimeSelectionChanged;
            Editor.Tools.ToolChanged += OnRuntimeToolChanged;

            if (GetComponent<RuntimeSelectionInputBase>() == null)
            {
                gameObject.AddComponent<RuntimeSelectionInput>();
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
        
            if(m_boxSelection != null)
            {
                m_boxSelection.Filtering -= OnBoxSelectionFiltering;
            }

            Editor.Tools.Current = RuntimeTool.None;
            Editor.Tools.ToolChanged -= OnRuntimeToolChanged;
            Editor.Selection.SelectionChanged -= OnRuntimeSelectionChanged;
        }

        protected override void OnWindowRegistered(RuntimeWindow window)
        {
            base.OnWindowRegistered(window);
            RuntimeSelectionComponentUI ui = window.GetComponentInChildren<RuntimeSelectionComponentUI>(true);
            if(ui == null && !Editor.IsVR)
            {
                GameObject runtimeSelectionComponentUI = new GameObject("SelectionComponentUI");
                runtimeSelectionComponentUI.transform.SetParent(window.transform, false);

                ui = runtimeSelectionComponentUI.AddComponent<RuntimeSelectionComponentUI>();
                RectTransform rt = runtimeSelectionComponentUI.GetComponent<RectTransform>();
                rt.SetSiblingIndex(0);
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMax = new Vector2(0, 0);
                rt.offsetMin = new Vector2(0, 0);
            }

            if(ui != null)
            {
                ui.Selected += OnUISelected;
                ui.Unselected += OnUIUnselected;
                if (window == Editor.ActiveWindow)
                {
                    IsUISelected = ui.IsSelected;
                }
            }
        }

        protected override void OnWindowUnregistered(RuntimeWindow window)
        {
            base.OnWindowUnregistered(window);
            RuntimeSelectionComponentUI ui = window.GetComponentInChildren<RuntimeSelectionComponentUI>(true);
            if(ui != null)
            {
                ui.Selected -= OnUISelected;
                ui.Unselected -= OnUIUnselected;
            }
        }

        protected override void OnWindowActivated()
        {
            base.OnWindowActivated();
            RuntimeSelectionComponentUI ui = ActiveWindow.GetComponentInChildren<RuntimeSelectionComponentUI>(true);
            if (ui != null)
            {
                IsUISelected = ui.IsSelected;
            }
        }

        protected virtual void OnUISelected(object sender, System.EventArgs e)
        {
            IsUISelected = true;
        }

        protected virtual void OnUIUnselected(object sender, System.EventArgs e)
        {
            IsUISelected = false;
        }

        public virtual void SelectGO(bool rangeSelect, bool multiselect)
        {
            Ray ray = ActiveWindow.Pointer;
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, float.MaxValue))
            {
                GameObject hitGO = hitInfo.collider.gameObject;
                bool canSelect = CanSelect(hitGO);
                if (canSelect)
                {
                    hitGO = hitGO.GetComponentInParent<ExposeToEditor>().gameObject;
                    if (multiselect)
                    {
                        List<Object> selection;
                        if (Editor.Selection.objects != null)
                        {
                            selection = Editor.Selection.objects.ToList();
                        }
                        else
                        {
                            selection = new List<Object>();
                        }

                        if (selection.Contains(hitGO))
                        {
                            selection.Remove(hitGO);
                            if (rangeSelect)
                            {
                                selection.Insert(0, hitGO);
                            }
                        }
                        else
                        {
                            selection.Insert(0, hitGO);
                        }
                        Editor.Selection.Select(hitGO, selection.ToArray());
                    }
                    else
                    {
                        Editor.Selection.activeObject = hitGO;
                    }
                }
                else
                {
                    if (!multiselect)
                    {
                        Editor.Selection.activeObject = null;
                    }
                }
            }
            else
            {
                if (!multiselect)
                {
                    Editor.Selection.activeObject = null;
                }
            }
        }

        public virtual void SelectAll()
        {
            IEnumerable<GameObject> filtered = Editor.IsPlaying ?
                ExposeToEditor.FindAll(Editor, ExposeToEditorObjectType.PlayMode) :
                ExposeToEditor.FindAll(Editor, ExposeToEditorObjectType.EditorMode);
            Editor.Selection.objects = filtered.ToArray();
        }


        private void OnRuntimeToolChanged()
        {
            if (Editor.Selection.activeTransform == null)
            {
                return;
            }

            if (m_positionHandle != null)
            {
                m_positionHandle.gameObject.SetActive(false);
                if (Editor.Tools.Current == RuntimeTool.Move)
                {
                    m_positionHandle.transform.position = Editor.Selection.activeTransform.position;
                    m_positionHandle.Targets = GetTargets();
                    m_positionHandle.gameObject.SetActive(m_positionHandle.Targets.Length > 0);
                }
            }
            if (m_rotationHandle != null)
            {
                m_rotationHandle.gameObject.SetActive(false);
                if (Editor.Tools.Current == RuntimeTool.Rotate)
                {
                    m_rotationHandle.transform.position = Editor.Selection.activeTransform.position;
                    m_rotationHandle.Targets = GetTargets();
                    m_rotationHandle.gameObject.SetActive(m_rotationHandle.Targets.Length > 0);
                }
            }
            if (m_scaleHandle != null)
            {
                m_scaleHandle.gameObject.SetActive(false);
                if (Editor.Tools.Current == RuntimeTool.Scale)
                {
                    m_scaleHandle.transform.position = Editor.Selection.activeTransform.position;
                    m_scaleHandle.Targets = GetTargets();
                    m_scaleHandle.gameObject.SetActive(m_scaleHandle.Targets.Length > 0);
                }
            }

#if UNITY_EDITOR
            switch (Editor.Tools.Current)
            {
                case RuntimeTool.None:
                    UnityEditor.Tools.current = UnityEditor.Tool.None;
                    break;
                case RuntimeTool.Move:
                    UnityEditor.Tools.current = UnityEditor.Tool.Move;
                    break;
                case RuntimeTool.Rotate:
                    UnityEditor.Tools.current = UnityEditor.Tool.Rotate;
                    break;
                case RuntimeTool.Scale:
                    UnityEditor.Tools.current = UnityEditor.Tool.Scale;
                    break;
                case RuntimeTool.View:
                    UnityEditor.Tools.current = UnityEditor.Tool.View;
                    break;
            }
#endif
        }

        private void OnBoxSelectionFiltering(object sender, FilteringArgs e)
        {
            if (e.Object == null)
            {
                e.Cancel = true;
            }

            ExposeToEditor exposeToEditor = e.Object.GetComponent<ExposeToEditor>();
            if (!exposeToEditor || !exposeToEditor.CanSelect)
            {
                e.Cancel = true;
            }
        }

        private void OnRuntimeSelectionChanged(Object[] unselected)
        {
            if (unselected != null)
            {
                for (int i = 0; i < unselected.Length; ++i)
                {
                    GameObject unselectedObj = unselected[i] as GameObject;
                    if (unselectedObj != null)
                    {
                        SelectionGizmo selectionGizmo = unselectedObj.GetComponent<SelectionGizmo>();
                        if (selectionGizmo != null)
                        {
                            DestroyImmediate(selectionGizmo);
                        }

                        ExposeToEditor exposeToEditor = unselectedObj.GetComponent<ExposeToEditor>();
                        if (exposeToEditor)
                        {
                            if (exposeToEditor.Unselected != null)
                            {
                                exposeToEditor.Unselected.Invoke(exposeToEditor);
                            }
                        }
                    }
                }
            }

            GameObject[] selected = Editor.Selection.gameObjects;
            if (selected != null)
            {
                for (int i = 0; i < selected.Length; ++i)
                {
                    GameObject selectedObj = selected[i];
                    ExposeToEditor exposeToEditor = selectedObj.GetComponent<ExposeToEditor>();
                    if (exposeToEditor && exposeToEditor.CanSelect && !selectedObj.IsPrefab() && !selectedObj.isStatic)
                    {
                        SelectionGizmo selectionGizmo = selectedObj.GetComponent<SelectionGizmo>();
                        if (selectionGizmo == null)
                        {
                            selectionGizmo = selectedObj.AddComponent<SelectionGizmo>();
                        }
                        
                        if (exposeToEditor.Selected != null)
                        {
                            exposeToEditor.Selected.Invoke(exposeToEditor);
                        }
                    }
                }
            }

            if (Editor.Selection.activeGameObject == null || Editor.Selection.activeGameObject.IsPrefab())
            {
                if (m_positionHandle != null)
                {
                    m_positionHandle.gameObject.SetActive(false);
                }
                if (m_rotationHandle != null)
                {
                    m_rotationHandle.gameObject.SetActive(false);
                }
                if (m_scaleHandle != null)
                {
                    m_scaleHandle.gameObject.SetActive(false);
                }
            }
            else
            {
                OnRuntimeToolChanged();
            }
        }

        protected virtual bool CanSelect(GameObject go)
        {
            return go.GetComponentInParent<ExposeToEditor>();
        }

        protected virtual Transform[] GetTargets()
        {
            return Editor.Selection.gameObjects.Select(g => g.transform).OrderByDescending(g => Editor.Selection.activeTransform == g).ToArray();
        }
    }
}
