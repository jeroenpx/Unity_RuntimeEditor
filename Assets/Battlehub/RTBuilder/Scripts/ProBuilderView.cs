using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public interface IProBuilderCmd
    {
        void Run();
        bool Validate();
    }
   
    public class ProBuilderView : RuntimeWindow
    {
        private class ProBuilderCmd : IProBuilderCmd
        {
            private Action m_action;
            private Func<bool> m_validate;
            public string Text;
            public bool CanDrag;

            public ProBuilderCmd(string text, Action action, bool canDrag = false) : this(text, action, () => true, canDrag)
            {
            }

            public ProBuilderCmd(string text, Action action, Func<bool> validate, bool canDrag = false)
            {
                Text = text;
                m_action = action;
                m_validate = validate;
                CanDrag = canDrag;
            }

            public void Run()
            {
                m_action();
            }

            public bool Validate()
            {
                return m_validate();
            }
        }


        [SerializeField]
        private VirtualizingTreeView m_commandsList = null;

        [SerializeField]
        private ProBuilderToolbar m_proBuilderToolbarPrefab = null;
        private ProBuilderCmd[] m_commands;
        private GameObject m_proBuilderToolGO;
        private IProBuilderTool m_proBuilderTool;

        private bool m_isProBuilderMeshSelected = false;
        private bool m_isNonProBuilderMeshSelected = false;
        
        private IWindowManager m_wm;
        
        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Custom;
            base.AwakeOverride();

            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.WindowCreated += OnWindowCreated;
            m_wm.WindowDestroyed += OnWindowDestroyed;

            m_proBuilderToolGO = new GameObject("ProBuilderTool");
            m_proBuilderToolGO.transform.SetParent(Editor.Root, false);
            m_proBuilderTool = m_proBuilderToolGO.AddComponent<ProBuilderTool>();
            
            CreateToolbar();

            Editor.Undo.Store();

            Editor.Selection.SelectionChanged += OnSelectionChanged;

            m_commandsList.ItemClick += OnItemClick;
            m_commandsList.ItemDataBinding += OnItemDataBinding;
            m_commandsList.ItemBeginDrag += OnItemBeginDrag;
            m_commandsList.ItemDrop += OnItemDrop;
            m_commandsList.ItemDragEnter += OnItemDragEnter;
            m_commandsList.ItemDragExit += OnItemDragExit;
            m_commandsList.ItemEndDrag += OnItemEndDrag;

            m_commandsList.CanEdit = false;
            m_commandsList.CanReorder = false;
            m_commandsList.CanReparent = false;
            m_commandsList.CanSelectAll = false;
            m_commandsList.CanUnselectAll = true;
            m_commandsList.CanRemove = false;
        }

        protected virtual void Start()
        {
            m_commands = new[]
            {
                new ProBuilderCmd("New Shape", OnNewShape, true),
                new ProBuilderCmd("ProBuilderize", OnProBuilderize, CanProBuilderize),
                new ProBuilderCmd("Extrude Face", OnExtrudeFace, CanExtrudeFace),
                new ProBuilderCmd("Delete Face", OnDeleteFace, CanDeleteFace)
            };
            m_commandsList.Items = m_commands;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
                m_wm.WindowDestroyed -= OnWindowDestroyed;
                DestroyToolbar();
            }

            if(m_proBuilderToolGO != null)
            {
                Destroy(m_proBuilderToolGO);
            }

            Editor.Undo.Restore();
            
            if(Editor != null)
            {
                Editor.Selection.SelectionChanged -= OnSelectionChanged;
            }

            if (m_commandsList != null)
            {
                m_commandsList.ItemClick -= OnItemClick;
                m_commandsList.ItemDataBinding -= OnItemDataBinding;
            }
        }

        private void OnSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            GameObject[] selected = Editor.Selection.gameObjects;
            if (selected != null && selected.Length > 0)
            {
                m_isProBuilderMeshSelected = selected.Where(go => go.GetComponent<PBMesh>() != null).Any();
                m_isNonProBuilderMeshSelected = selected.Where(go => go.GetComponent<PBMesh>() == null).Any();
            }
            else
            {
                m_isProBuilderMeshSelected = false;
                m_isNonProBuilderMeshSelected = false;
            }

            int index = m_commandsList.VisibleItemIndex;
            int count = m_commandsList.VisibleItemsCount;
            for (int i = 0; i < count; ++i)
            {
                m_commandsList.DataBindItem(m_commands[i]);
            }
        }
        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>();
            ProBuilderCmd cmd = (ProBuilderCmd)e.Item;
            text.text = cmd.Text;

            bool isValid = cmd.Validate();
            Color color = text.color;
            color.a = isValid ? 1 : 0.5f;
            text.color = color;
          
            e.CanDrag = cmd.CanDrag;
        }

        private void OnItemClick(object sender, ItemArgs e)
        {
            ProBuilderCmd cmd = (ProBuilderCmd)e.Items[0];
            if(cmd.Validate())
            {
                cmd.Run();
            }
        }

        private void OnNewShape()
        {
            GameObject go = PBMesh.Create().gameObject;
            Editor.Undo.Enabled = false;
            Editor.Selection.Select(go, new[] { go });
            Editor.Undo.Enabled = true;
        }

        private bool CanProBuilderize()
        {
            return m_isNonProBuilderMeshSelected;
        }

        private void OnProBuilderize()
        {
            GameObject[] gameObjects = Editor.Selection.gameObjects;
            if(gameObjects == null)
            {
                return;
            }

            for(int i = 0; i < gameObjects.Length; ++i)
            {
                MeshFilter[] filters = gameObjects[i].GetComponentsInChildren<MeshFilter>();
                for(int j = 0; j < filters.Length; ++j)
                {
                    PBMesh.ProBuilderize(filters[j].gameObject);
                }
            }
        }

        private bool CanExtrudeFace()
        {
            return m_proBuilderTool.Mode == ProBuilderToolMode.Face && m_proBuilderTool.HasSelection;
        }

        private void OnExtrudeFace()
        {
            m_proBuilderTool.Extrude(1);
        }

        private bool CanDeleteFace()
        {
            return m_isProBuilderMeshSelected;
        }

        private void OnDeleteFace()
        {
            Debug.Log("OnDeleteFace");
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseBeginDrag(this, e.Items, e.PointerEventData);
        }

        private void OnItemDragEnter(object sender, ItemDropCancelArgs e)
        {
            Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
            e.Cancel = true;
        }

        private void OnItemDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrag(e.PointerEventData);
        }

        private void OnItemDragExit(object sender, EventArgs e)
        {
            Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        private void CreateToolbar()
        {
            Transform[] scenes = m_wm.GetWindows(RuntimeWindowType.Scene.ToString());
            for(int i = 0; i < scenes.Length; ++i)
            {
                RuntimeWindow window = scenes[i].GetComponent<RuntimeWindow>();
                CreateToolbar(scenes[i], window);
            }
        }

        private void DestroyToolbar()
        {
            Transform[] scenes = m_wm.GetWindows(RuntimeWindowType.Scene.ToString());
            for(int i = 0; i < scenes.Length; ++i)
            {
                RuntimeWindow window = scenes[i].GetComponent<RuntimeWindow>();
                DestroyToolbar(scenes[i], window);
            }
        }

        private void OnWindowCreated(Transform windowTransform)
        {
            RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
            CreateToolbar(windowTransform, window);
        }

        private void CreateToolbar(Transform windowTransform, RuntimeWindow window)
        {
            if (window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                if (m_proBuilderToolbarPrefab != null)
                {
                    RectTransform rt = (RectTransform)Instantiate(m_proBuilderToolbarPrefab, windowTransform, false).transform;
                    rt.Stretch();
                }
            }
        }

        private void OnWindowDestroyed(Transform windowTransform)
        {
            RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
            DestroyToolbar(windowTransform, window);
        }

        private void DestroyToolbar(Transform windowTransform, RuntimeWindow window)
        {
            if (window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                if (m_proBuilderToolbarPrefab != null)
                {
                    ProBuilderToolbar toolbar = windowTransform.GetComponentInChildren<ProBuilderToolbar>();
                    if (toolbar != null)
                    {
                        Destroy(toolbar.gameObject);
                    }
                }
            }
        }

    }
}


